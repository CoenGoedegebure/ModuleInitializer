using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Coen.Utilities
{
    public static class Zipper
    {
        /// <summary>
        /// This method creates a zipfile in memory. We use this simple method to trigger the AssemblyResolve event
        /// </summary>
        public static void CallSharpZipLib()
        {
            using (var dummyStream = new MemoryStream())
            {
                ZipFile.Create(dummyStream);
            }
        }
    }
}
