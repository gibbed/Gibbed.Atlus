using System.Collections.Generic;
using System.IO;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats.ArchiveFormats
{
    public class UnnamedListArchiveFile : IArchiveFormat
    {
        public bool Validate(string path, out ArchiveValidateError error)
        {
            using (var input = File.Open(
                path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (input.Length < 8)
                {
                    error = new ArchiveValidateError(
                        "file not large enough to have header",
                        input);
                    return false;
                }

                var magic = input.ReadValueU32();
                var count = input.ReadValueU32();

                if (magic != 100)
                {
                    error = new ArchiveValidateError(
                        "invalid magic",
                        input);
                    return false;
                }

                if (input.Length < 8 + (count * 8))
                {
                    error = new ArchiveValidateError(string.Format(
                        "file not large enough to support {0} files",
                        count), input);
                    return false;
                }

                long headerSize = 8 + (count * 8);

                for (uint i = 0; i < count; i++)
                {
                    uint offset = input.ReadValueU32();
                    uint size = input.ReadValueU32();

                    if (offset < headerSize)
                    {
                        error = new ArchiveValidateError(string.Format(
                            "entry offset in header",
                            count), input);
                        return false;
                    }

                    if ((long)offset + (long)size > input.Length)
                    {
                        error = new ArchiveValidateError(string.Format(
                            "entry exceeds file bounds",
                            count), input);
                        return false;
                    }
                }

                error = null;
                return true;
            }
        }

        public List<ArchiveEntry> GetEntries(string path)
        {
            var entries = new List<ArchiveEntry>();

            string baseName = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);

            using (var input = File.Open(
                path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (input.Length < 8)
                {
                    return null;
                }

                var magic = input.ReadValueU32();
                var count = input.ReadValueU32();

                if (magic != 100)
                {
                    return null;
                }

                if (input.Length < 8 + (count * 8))
                {
                    return null;
                }

                long headerSize = 8 + (count * 8);

                for (uint i = 0; i < count; i++)
                {
                    uint offset = input.ReadValueU32();
                    uint size = input.ReadValueU32();

                    if (offset < headerSize)
                    {
                        return null;
                    }

                    if ((long)offset + (long)size > input.Length)
                    {
                        return null;
                    }

                    entries.Add(new ArchiveEntry()
                        {
                            Name = string.Format("{0}_{1}", baseName, i),
                            Offset = offset,
                            Size = size,
                        });
                }

                return entries;
            }
        }
    }
}
