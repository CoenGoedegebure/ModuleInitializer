using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace Injector
{
    /// <summary>
    /// Injector module
    /// </summary>
    internal class Injector
    {
        private AssemblyDefinition InjectionTargetAssembly { get; set; }

        /// <summary>
        /// Injects the specified injection target assembly path.
        /// </summary>
        /// <param name="injectionTargetAssemblyPath">The injection target assembly path.</param>
        /// <param name="className">The classname of the ModuleInitializer containing the Initialize-method</param>
        /// <param name="methodName">The methodname that should be called in the injected constructor (Initialize-method)</param>
        /// <param name="keyfile">The path to the keyfile.</param>
        /// <exception cref="InjectionException">
        /// Assembly '{0}' does not exist
        /// or
        /// The key file'{0}' does not exist
        /// or
        /// </exception>
        public void Inject(string injectionTargetAssemblyPath, string className, string methodName, string keyfile = null)
        {
            // Validate the preconditions
            if (!File.Exists(injectionTargetAssemblyPath))
            {
                throw new InjectionException($"Assembly '{injectionTargetAssemblyPath}' does not exist");
            }

            if (keyfile != null && !File.Exists(keyfile))
            {
                throw new InjectionException($"The key file '{keyfile}' does not exist");
            }

            try
            {
                // Read the injectionTarget
                ReadInjectionTargetAssembly(injectionTargetAssemblyPath);

                // Get a reference to the initializerMethod 
                var initializerMethod = GetModuleInitializerMethod(className, methodName);

                // Inject the Initializermethod into the assembly as a constructormethod
                InjectInitializer(initializerMethod);

                // Rewrite the assembly
                WriteAssembly(injectionTargetAssemblyPath, keyfile);
            }
            catch (Exception e)
            {
                throw new InjectionException(e.Message);
            }
        }

        /// <summary>
        /// Injects the initializer.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        /// <exception cref="InjectionException">No module class found</exception>
        private void InjectInitializer(MethodReference initializer)
        {
            if (initializer == null)
            {
                throw new InjectionException("InjectInitializer failed: initializer cannot be null");
            }

            var initializerReturnType = InjectionTargetAssembly.MainModule.Import(initializer.ReturnType);

            const MethodAttributes attributes = MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;

            // Create a new method .cctor (a static constructor) inside the Assembly  
            var cctor = new MethodDefinition(".cctor", attributes, initializerReturnType);
            var il = cctor.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Call, initializer));
            il.Append(il.Create(OpCodes.Ret));

            var moduleClass = InjectionTargetAssembly.MainModule.Types.FirstOrDefault(t => t.Name == "<Module>");

            if (moduleClass == null)
            {
                throw new InjectionException("No module class found");
            }

            moduleClass.Methods.Add(cctor);
        }

        /// <summary>
        /// Writes the changes to the assembly.
        /// </summary>
        /// <param name="assemblyFile">The assembly file.</param>
        /// <param name="keyfile">The keyfile.</param>
        private void WriteAssembly(string assemblyFile, string keyfile)
        {
            if (InjectionTargetAssembly == null)
            {
                throw new InjectionException("Unable to write the Injection TargetAssembly: InjectionTargetAssembly is null");
            }

            var writeParams = new WriterParameters();

            if (GetPdbFilePath(assemblyFile) != null)
            {
                writeParams.WriteSymbols = true;
                writeParams.SymbolWriterProvider = new PdbWriterProvider();
            }

            if (keyfile != null)
            {
                writeParams.StrongNameKeyPair = new StrongNameKeyPair(File.ReadAllBytes(keyfile));
            }

            InjectionTargetAssembly.Write(assemblyFile, writeParams);
        }

        /// <summary>
        /// Reads the injection target assembly.
        /// </summary>
        /// <param name="assemblyFile">The assembly file.</param>
        private void ReadInjectionTargetAssembly(string assemblyFile)
        {
            if (assemblyFile == null)
            {
                throw new InjectionException("Unable to read the Injection TargetAssembly: InjectionTargetAssembly is null");
            }

            var readParams = new ReaderParameters(ReadingMode.Immediate);

            if (GetPdbFilePath(assemblyFile) != null)
            {
                readParams.ReadSymbols = true;
                readParams.SymbolReaderProvider = new PdbReaderProvider();
            }

            InjectionTargetAssembly = AssemblyDefinition.ReadAssembly(assemblyFile, readParams);
        }

        /// <summary>
        /// Attempts to obtain a reference to the module initializer method within the assembly.
        /// </summary>
        /// <returns>If the method is found and valid, a reference to the ModuleInitializerMethod. Otherwise null</returns>
        /// <exception cref="InjectionException">
        /// No type found named 'ModuleInitializer', this type must exist or the ModuleInitializer parameter must be used
        /// or
        /// No method named '{0}' exists in the type '{1}'
        /// or
        /// Module initializer method must not have any parameters
        /// or
        /// Module initializer method may not be private or protected, use public or internal instead
        /// or
        /// Module initializer method must have 'void' as return type
        /// or
        /// Module initializer method must be static
        /// </exception>
        private MethodReference GetModuleInitializerMethod(string className, string methodName)
        {
            if (InjectionTargetAssembly == null)
            {
                throw new InjectionException("Unable to determine ModuleInitializer: InjectionTargetAssembly is null");
            }

            // Retrieve the ModuleInitializer Class
            var moduleInitializerClass = InjectionTargetAssembly.MainModule.Types.FirstOrDefault(t => t.Name == className);
            if (moduleInitializerClass == null)
            {
                throw new InjectionException("No type found named 'ModuleInitializer', this type must exist or the ModuleInitializer parameter must be used");
            }

            // Retrieve the ModuleInitializer method 
            var resultMethod = moduleInitializerClass.Methods.FirstOrDefault(m => m.Name == methodName);
            if (resultMethod == null)
            {
                throw new InjectionException($"No method named '{moduleInitializerClass.FullName}' exists in the type '{methodName}'");
            }

            // Validate the found method
            if (resultMethod.Parameters.Count > 0)
            {
                throw new InjectionException("Module initializer method must not have any parameters");
            }

            if (resultMethod.IsPrivate || resultMethod.IsFamily)
            {
                throw new InjectionException("Module initializer method may not be private or protected, use public or internal instead");
            }

            //Don't compare the types themselves, they might be from different CLR versions.
            if (!resultMethod.ReturnType.FullName.Equals(typeof(void).FullName))
            {
                throw new InjectionException("Module initializer method must have 'void' as return type");
            }

            if (!resultMethod.IsStatic)
            {
                throw new InjectionException("Module initializer method must be static");
            }

            return resultMethod;
        }

        /// <summary>
        /// Gets the PDB file path belonging to the assemblyfile.
        /// </summary>
        /// <param name="assemblyFilePath">The path to the assembly file.</param>
        /// <returns>If the pdb file exists, the path of the pdb-file. Otherwise null</returns>
        private static string GetPdbFilePath(string assemblyFilePath)
        {
            if (assemblyFilePath == null)
            {
                throw new InjectionException("Unable to determine pdb filepath: assemblyFilePath is null");
            }

            var path = Path.ChangeExtension(assemblyFilePath, ".pdb");
            return File.Exists(path) ? path : null;
        }
    }
}
