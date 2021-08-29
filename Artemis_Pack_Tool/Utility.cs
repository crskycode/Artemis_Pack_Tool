using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis_Pack_Tool
{
    static class Utility
    {
        public static bool PathIsDirectory(string path)
        {
            return new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory);
        }
    }
}
