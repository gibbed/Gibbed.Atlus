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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using NDesk.Options;

/* *.idx, *.ndx, *.bin
 * 
 * Games
 *   Radiant Historia
 *   Devil Survivor
 *   Devil Survivor 2
 */

namespace Gibbed.Atlus.UnpackIDXNDXBIN
{
    public class Program
    {
        private static List<string> ReadNames(Stream input, string basePath)
        {
            var names = new List<string>();

            var queue = new Queue<KeyValuePair<string, uint>>();
            queue.Enqueue(new KeyValuePair<string, uint>(basePath, 0));

            while (queue.Count > 0)
            {
                var kv = queue.Dequeue();
                input.Seek(kv.Value, SeekOrigin.Begin);

                var count = input.ReadValueU16();
                for (int i = 0; i < count; i++)
                {
                    var length = input.ReadValueU16();
                    var name = input.ReadString(length, true, Encoding.ASCII);
                    var offset = input.ReadValueU32();

                    name = name.Replace('\\', '/');
                    name = name.ToLowerInvariant();

                    if (string.IsNullOrEmpty(kv.Key) == false)
                    {
                        name = kv.Key + '/' + name;
                    }

                    if (offset != 0)
                    {
                        queue.Enqueue(new KeyValuePair<string, uint>(name, offset));
                    }
                    else
                    {
                        names.Add(name);
                    }
                }
            }

            return names;
        }

        private static uint HashFileName(string text)
        {
            uint hash = 0;

            if (string.IsNullOrEmpty(text) == false)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    hash = (hash * 37) + (uint)text[i];
                }
            }

            return hash;
        }

        public static void Main(string[] args)
        {
            bool overwriteFiles = false;
            bool showHelp = false;
            bool onlyFilesWithExtensions = true;

            var options = new OptionSet()
            {
                {
                    "o|overwrite",
                    "overwrite files if they already exist", 
                    v => overwriteFiles = v != null
                },
                {
                    "a|all-files",
                    "when unpacking, try unpacking names that don't have an extension (this is a bad idea)", 
                    v => onlyFilesWithExtensions = v == null
                },
                {
                    "h|help",
                    "show this message and exit", 
                    v => showHelp = v != null
                },
            };

            List<string> extras;

            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (extras.Count < 1 || extras.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_archive [output_directory]", GetExecutableName());
                Console.WriteLine("Unpack specified archive.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string fileTablePath = Path.ChangeExtension(extras[0], ".idx");
            string nameTablePath = Path.ChangeExtension(extras[0], ".ndx");
            string dataPath = Path.ChangeExtension(extras[0], ".bin");
            string outputPath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(fileTablePath, null) + "_unpack";

            Console.WriteLine("Reading file names...");

            var names = new List<string>();
            using (var input = File.OpenRead(nameTablePath))
            {
                names = ReadNames(input, null);
            }

            Console.WriteLine("Building file table...");

            uint tableSize;
            var entries = new List<Entry>();
            using (var input = File.OpenRead(fileTablePath))
            {
                tableSize = input.ReadValueU32();
                var unknown04 = input.ReadValueU32();

                if (unknown04 != 0)
                {
                    throw new FormatException();
                }

                foreach (var name in names)
                {
                    if (onlyFilesWithExtensions == true &&
                        name.LastIndexOf('.') < 0)
                    {
                        continue;
                    }

                    var hash = HashFileName(name) & (tableSize - 1);
                    input.Seek(8 + (hash * 6), SeekOrigin.Begin);

                    ulong flags = 0;
                    flags |= ((ulong)input.ReadValueU32()) << 0;
                    flags |= ((ulong)input.ReadValueU16()) << 32;

                    var hasMultipleFiles = (flags & 1) != 0;
                    var offset = (uint)((flags & 0x0000000003FFFFFEUL) >> 1);
                    var size = (uint)((flags & 0x0000FFFFFC000000UL) >> 26);
                    
                    var found = false;
                    if (hasMultipleFiles == false)
                    {
                        found = true;

                        entries.Add(new Entry()
                        {
                            Name = name,
                            Offset = offset << 2,
                            Size = size,
                        });
                    }
                    else
                    {
                        input.Seek(offset, SeekOrigin.Begin);
                        var count = input.ReadValueU8();
                        for (int i = 0; i < count; i++)
                        {
                            var localOffset = input.ReadValueU32();
                            var localSize = i == 0 ? size : input.ReadValueU32();

                            var matches = new List<KeyValuePair<byte, byte>>();
                            while (true)
                            {
                                var match = input.ReadValueU8();
                                if (match == 0)
                                {
                                    break;
                                }
                                var index = input.ReadValueU8();
                                matches.Add(new KeyValuePair<byte, byte>(index, match));
                            }

                            var matched = true;
                            foreach (var match in matches)
                            {
                                if (match.Key >= name.Length ||
                                    name[match.Key] != (char)match.Value)
                                {
                                    matched = false;
                                    break;
                                }
                            }

                            if (matched == true)
                            {
                                found = true;
                                entries.Add(new Entry()
                                    {
                                        Name = name,
                                        Offset = localOffset << 2,
                                        Size = localSize,
                                    });
                                break;
                            }
                        }
                    }

                    if (found == false)
                    {
                        Console.WriteLine("could not find '{0}' (it's probably a directory)", name);
                    }
                }
            }

            Console.WriteLine("Unpacking...");

            var offsets = entries
                .Select(e => e.Offset)
                .Distinct()
                .OrderBy(o => o);
            foreach (var offset in offsets)
            {
                var matching = entries.Where(e => e.Offset == offset);
                if (matching.Count() > 1)
                {
                    Console.WriteLine("files with duplicate offset 0x{0:X8}: (this is not an error)", offset);
                    foreach (var match in matching)
                    {
                        Console.WriteLine("  [{0:X8} {1:X8}] {2} -> {3}",
                            HashFileName(match.Name),
                            HashFileName(match.Name) & (tableSize - 1),
                            match.Name,
                            match.Size);
                    }
                }
            }

            using (var input = File.OpenRead(dataPath))
            {
                foreach (var entry in entries.OrderBy(e => e.Offset))
                {
                    var entryName = entry.Name;

                    entryName = entryName.Replace('/', '\\');
                    if (entryName.StartsWith("\\") == true)
                    {
                        entryName = entryName.Substring(1);
                    }

                    var entryPath = Path.Combine(outputPath, entryName);
                    if (overwriteFiles == false &&
                        File.Exists(entryPath) == true)
                    {
                        continue;
                    }

                    Console.WriteLine(entryName);

                    Directory.CreateDirectory(Path.GetDirectoryName(entryPath));
                    using (var output = File.Create(entryPath))
                    {
                        input.Seek(entry.Offset, SeekOrigin.Begin);
                        output.WriteFromStream(input, entry.Size);
                    }
                }
            }
        }

        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }
    }
}
