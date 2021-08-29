using System;
using System.IO;

namespace Artemis_Pack_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Artemis Pack Tool created by CrskyCode");
                Console.WriteLine("Usage:");
                Console.WriteLine("  Artemis_Pack_Tool <root directory> <output file>");
                return;
            }

            var rootPath = args[0];
            var filePath = args[1];

            if (!Utility.PathIsDirectory(rootPath))
            {
                Console.WriteLine($"ERROR: \"{rootPath}\" not a directory");
                return;
            }

            PF8.Create(rootPath, filePath);
        }
    }
}
