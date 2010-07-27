using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats.ArchiveFormats
{
    public class Named256ArchiveFile : IArchiveFormat
    {
        private static int CheckFileName(byte[] data)
        {
            int end = -1;
            for (int i = 0; i < data.Length; i++)
            {
                if (end >= 0)
                {
                    if (data[i] != 0)
                    {
                        return -1;
                    }
                }
                else
                {
                    if (data[i] == 0)
                    {
                        end = i;
                    }
                    else if (data[i] < 0x20 || data[i] > 0x7E)
                    {
                        return -1;
                    }
                }
            }

            return end;
        }

        public bool Validate(string path, out ArchiveValidateError error)
        {
            byte[] buffer = new byte[256 - 4];

            using (var input = File.Open(
                path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                while (input.Position < input.Length)
                {
                    if (input.Position + 256 > input.Length)
                    {
                        error = new ArchiveValidateError(
                            "end of file for next entry",
                            input);
                        return false;
                    }

                    input.Read(buffer, 0, buffer.Length);

                    int length = CheckFileName(buffer);
                    if (length < 0)
                    {
                        error = new ArchiveValidateError(
                            "invalid file name",
                            input);
                        return false;
                    }

                    uint size = input.ReadValueU32();
                    uint realSize = size.Align(64);

                    if (size == 0 && length == 0)
                    {
                        // this is the last entry
                        if (input.Position == input.Length)
                        {
                            break;
                        }

                        error = new ArchiveValidateError(
                            "null entry not at end of file",
                            input);
                        return false;
                    }

                    if (input.Position + realSize > input.Length)
                    {
                        error = new ArchiveValidateError(
                            "end of file for data",
                            input);
                        return false;
                    }

                    input.Seek(realSize, SeekOrigin.Current);
                }

                error = null;
                return true;
            }
        }

        public List<ArchiveEntry> GetEntries(string path)
        {
            byte[] buffer = new byte[256 - 4];
            var entries = new List<ArchiveEntry>();

            using (var input = File.Open(
                path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                while (input.Position < input.Length)
                {
                    if (input.Position + 256 > input.Length)
                    {
                        return null;
                    }

                    input.Read(buffer, 0, buffer.Length);

                    int length = CheckFileName(buffer);
                    if (length < 0)
                    {
                        return null;
                    }

                    uint size = input.ReadValueU32();
                    uint realSize = size.Align(64);

                    if (size == 0 && length == 0)
                    {
                        // this is the last entry
                        if (input.Position == input.Length)
                        {
                            break;
                        }

                        return null;
                    }

                    if (input.Position + realSize > input.Length)
                    {
                        return null;
                    }

                    entries.Add(new ArchiveEntry()
                    {
                        Name = GetUniqueName(Encoding.ASCII.GetString(buffer, 0, length), entries),
                        Offset = input.Position,
                        Size = size,
                    });

                    input.Seek(realSize, SeekOrigin.Current);
                }

                return entries;
            }
        }

        private static string GetUniqueName(string name, List<ArchiveEntry> entries)
        {
            name = name.Replace('/', '\\');

            string basePath = Path.GetDirectoryName(name);
            string baseName = Path.GetFileNameWithoutExtension(name);
            string extension = Path.GetExtension(name);
            string current = name;
            int counter = 0;
            
            while (entries.SingleOrDefault(e => e.Name == current) != null)
            {
                current = Path.Combine(basePath, Path.ChangeExtension(
                    string.Format("__DUPLICATE_{1}_{0}",
                        baseName, counter), extension));
                counter++;
            }

            return current;
        }
    }
}
