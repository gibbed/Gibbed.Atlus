using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats.ArchiveFormats
{
    public class NamedListArchiveFile : IArchiveFormat
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
            byte[] buffer = new byte[32];

            using (var input = File.Open(
                path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (input.Length < 4)
                {
                    error = new ArchiveValidateError(
                          "file not large enough to have header",
                          input);
                    return false;
                }

                var count = input.ReadValueU32();
                long atLeast = 4 + (count * (32 + 4));

                if (input.Length < atLeast)
                {
                    error = new ArchiveValidateError(string.Format(
                        "file not large enough to support {0} files",
                        count), input);
                    return false;
                }

                for (uint i = 0; i < count; i++)
                {
                    if (input.Position + 36 > input.Length)
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

                    if (input.Position + size > input.Length)
                    {
                        error = new ArchiveValidateError(
                            "end of file for data",
                            input);
                        return false;
                    }

                    input.Seek(size, SeekOrigin.Current);
                }

                if (input.Position != input.Length)
                {
                    error = new ArchiveValidateError(
                        "not at end of file",
                        input);
                    return false;
                }

                error = null;
                return true;
            }
        }

        public List<ArchiveEntry> GetEntries(string path)
        {
            byte[] buffer = new byte[32];
            var entries = new List<ArchiveEntry>();

            using (var input = File.Open(
                path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (input.Length < 4)
                {
                    return null;
                }

                var count = input.ReadValueU32();
                long atLeast = 4 + (count * (32 + 4));

                if (input.Length < atLeast)
                {
                    return null;
                }

                for (uint i = 0; i < count; i++)
                {
                    if (input.Position + 36 > input.Length)
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

                    if (input.Position + size > input.Length)
                    {
                        return null;
                    }

                    entries.Add(new ArchiveEntry()
                    {
                        Name = Encoding.ASCII.GetString(buffer, 0, length),
                        Offset = input.Position,
                        Size = size,
                    });

                    input.Seek(size, SeekOrigin.Current);
                }

                if (input.Position != input.Length)
                {
                    return null;
                }

                return entries;
            }
        }
    }
}
