using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Artemis_Pack_Tool
{
    static class PF8
    {
        public static void Create(string rootPath, string filePath)
        {
            var entries = new List<Entry>();

            // Fetch file list
            foreach (var path in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories))
            {
                entries.Add(new Entry
                {
                    LocalPath = path,
                    Path = Path.GetRelativePath(rootPath, path)
                });
            }

            using var writer = new BinaryWriter(File.Create(filePath));

            // "pf8"
            writer.Write(new byte[] { 0x70, 0x66, 0x38 });

            // index size
            writer.Write(0);

            // #
            var posIndexStart = writer.BaseStream.Position;

            // entry count
            writer.Write(entries.Count);

            // Write index
            foreach (var entry in entries)
            {
                var path = Encoding.UTF8.GetBytes(entry.Path);

                // path
                writer.Write(path.Length);
                writer.Write(path);

                // #
                entry.Position = writer.BaseStream.Position;

                // unknow
                writer.Write(0);

                // offset
                writer.Write(0);
                // length
                writer.Write(0);
            }

            // #
            var posOffsetTable = writer.BaseStream.Position;

            // entry count in offset table
            writer.Write(entries.Count + 1);

            // Write offset table
            foreach (var entry in entries)
            {
                writer.Write(Convert.ToUInt32(entry.Position - posIndexStart));
                writer.Write(0);
            }

            // End of table
            writer.Write(0);
            writer.Write(0);

            // table position
            writer.Write(Convert.ToUInt32(posOffsetTable - posIndexStart));

            // #
            var posIndexEnd = writer.BaseStream.Position;
            var dataOffset = posIndexEnd;

            // Prepare data placement
            foreach (var entry in entries)
            {
                var length = new FileInfo(entry.LocalPath).Length;

                entry.Offset = Convert.ToUInt32(dataOffset);
                entry.Length = Convert.ToUInt32(length);

                dataOffset += length;
            }

            // Write index
            foreach (var entry in entries)
            {
                writer.BaseStream.Position = entry.Position + 4;

                writer.Write(entry.Offset);
                writer.Write(entry.Length);
            }

            writer.Flush();

            // Calculate archive key
            var indexSize = Convert.ToInt32(posIndexEnd - posIndexStart);
            var indexData = new byte[indexSize];
            writer.BaseStream.Position = posIndexStart;
            writer.BaseStream.Read(indexData, 0, indexSize);
            byte[] key;
            using (var sha1 = SHA1.Create())
            {
                key = sha1.ComputeHash(indexData);
            }

            writer.BaseStream.Position = posIndexEnd;

            // Write data
            foreach (var entry in entries)
            {
                Console.WriteLine($"Adding \"{entry.Path}\"");

                var data = File.ReadAllBytes(entry.LocalPath);

                Debug.Assert(writer.BaseStream.Position == entry.Offset);
                Debug.Assert(data.Length == entry.Length);

                Encrypt(data, key);

                writer.Write(data);
            }

            // Write header
            writer.BaseStream.Position = 3;
            writer.Write(indexSize);

            writer.Flush();
        }

        class Entry
        {
            public string LocalPath;
            public string Path;
            public long Position;
            public uint Offset;
            public uint Length;
        }

        static void Encrypt(byte[] input, byte[] key)
        {
            for (int i = 0; i < input.Length; i++)
            {
                input[i] ^= key[i % key.Length];
            }
        }
    }
}
