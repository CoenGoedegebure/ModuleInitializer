using System;
using System.IO;
using Coen.Utilities;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Zipper.CallSharpZipLib();
                Console.WriteLine("SUCCESS: SharpZipLib successfully called");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("ERROR: Embedded ZipFile was not found. Use the Injector tool on the compiled Coen.Utilities library");
            }

            Console.WriteLine("end.");
            Console.ReadKey(true);
        }
    }
}
