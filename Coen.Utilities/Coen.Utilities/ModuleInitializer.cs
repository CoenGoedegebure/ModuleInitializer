using System;
using System.Linq;
using System.Reflection;

namespace Coen.Utilities
{
    internal class ModuleInitializer
    {
        private const string EmbeddedResourceNamespace = "Coen.Utilities.Embedded";

        public static void Initialize()
        {
            Console.WriteLine("[Coen.Utilities] ModuleInitializer.Initialize() method called. Subscribing to AssemblyResolve event");
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;
        }

        private static Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine($"Handling AssemblyResolve event for assembly {args.Name}");
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);

            if (assembly != null)
                return assembly;

            var resourceName = GetResourceName(args.Name);
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                Console.WriteLine($"Embedded resource {resourceName} found");

                if (stream != null)
                {
                    // Read the raw assembly from the resource
                    var buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);

                    // Load the assembly
                    return Assembly.Load(buffer);
                }
            }
            return null;
        }

        private static string GetResourceName(string assemblyName)
        {
            var name = new AssemblyName(assemblyName).Name;
            return $"{EmbeddedResourceNamespace}.{name}.dll";
        }
    }
}
