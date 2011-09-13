/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

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
