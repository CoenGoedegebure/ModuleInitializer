using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Injector
{
    class Program
    {
        static int Main(string[] args)
        {
            var injector = new Injector();

            // Validate arguments
            if (args.Length < 3 || args.Length > 4 || Regex.IsMatch(args[0], @"^((/|--?)(\?|h|help))$"))
            {
                PrintHelp();
                return 1;
            }

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("InjectModuleInitializer v{0}.{1}", version.Major, version.Minor);
            Console.WriteLine("");

            // Parse the arguments
            string keyfile = null;
            var assemblyFile = args[args.Length - 1];
            var methodName = args[args.Length - 2];
            var className = args[args.Length - 3];

            for (var i = 0; i < args.Length - 1; i++)
            {
                var keyMatch = Regex.Match(args[i], "^/k(eyfile)?:(.+)", RegexOptions.IgnoreCase);
                if (keyMatch.Success)
                {
                    keyfile = keyMatch.Groups[2].Value;
                }
            }

            // Start injecting the ModuleInitializer into the static constructor of the assembly
            try
            {
                injector.Inject(assemblyFile, className, methodName, keyfile);
                Console.WriteLine("Module Initializer successfully injected in assembly " + assemblyFile);
                return 0;
            }
            catch (InjectionException e)
            {
                Console.Error.WriteLine("error: " + e);
                return 1;
            }
        }

        /// <summary>
        /// Prints the help.
        /// </summary>
        private static void PrintHelp()
        {
            Console.Error.WriteLine(@"InjectModuleInitializer.exe [/k:<keyfile>] className methodName filename");
            Console.Error.WriteLine("");
            Console.Error.WriteLine(@"/k:<keyfile>                  A strong name key file that will be used to sign");
            Console.Error.WriteLine(@"/keyfile:<keyfile>            the assembly after the module initializer is");
            Console.Error.WriteLine(@"                              injected into it.");
            Console.Error.WriteLine(@"filename                      Name of the assembly file (exe or dll) to inject");
            Console.Error.WriteLine(@"                              a module initializer into.");
            Console.Error.WriteLine(@"/?                            Prints this help screen.");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("Note:");
            Console.Error.WriteLine(@"InjectModuleInitializer will search for the ModuleInitializer class with the Initialize method");
            Console.Error.WriteLine(@"This method must have the following properties:");
            Console.Error.WriteLine(@" - Initialize may not be private or protected");
            Console.Error.WriteLine(@" - Initialize must not have any parameters");
            Console.Error.WriteLine(@" - Initialize must have 'void' as return type");
            Console.Error.WriteLine(@" - Initialize must be static");
            Console.Error.WriteLine(@"Example: public static void Initialize()");
        }
    }
}
