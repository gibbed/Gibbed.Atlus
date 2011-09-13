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
using NDesk.Options;

namespace Gibbed.Atlus.Unpack
{
    public class Program
    {
        private static List<FileFormats.IArchiveFormat> GetArchiveFormats()
        {
            var formats = new List<FileFormats.IArchiveFormat>();
            var iface = typeof(FileFormats.IArchiveFormat);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => iface.IsAssignableFrom(p) && p.IsClass == true);
            foreach (var type in types)
            {
                formats.Add(
                    (FileFormats.IArchiveFormat)
                        Activator.CreateInstance(type));
            }
            return formats;
        }

        public static void Main(string[] args)
        {
            bool verbose = false;
            bool overwriteFiles = false;
            bool listing = false;
            bool showHelp = false;
            string filter = null;
            bool pauseOnError = true;

            OptionSet options = new OptionSet()
            {
                {
                    "v|verbose",
                    "be verbose (list files)",
                    v => verbose = v != null
                },
                {
                    "np|nopause",
                    "don't pause on errors",
                    v => pauseOnError = v == null
                },
                {
                    "l|list",
                    "just list files (don't extract)", 
                    v => listing = v != null
                },
                {
                    "o|overwrite",
                    "overwrite files if they already exist", 
                    v => overwriteFiles = v != null
                },
                {
                    "f|filter=",
                    "extension filtering", 
                    v => filter = v
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

            string inputPath = extras[0];
            string outputPath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(inputPath, null);

            var formats = GetArchiveFormats();
            FileFormats.IArchiveFormat format = null;
            var failures
                = new Dictionary<FileFormats.IArchiveFormat, FileFormats.ArchiveValidateError>();
            foreach (var check in formats)
            {
                FileFormats.ArchiveValidateError error;

                try
                {
                    if (check.Validate(inputPath, out error) == true)
                    {
                        format = check;
                        break;
                    }
                    else
                    {
                        failures.Add(check, error);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception while validating with {0}:",
                        check.GetType().Name);
                    Console.WriteLine("  {0}", e.ToString());
                    Console.WriteLine();
                }
            }

            if (format == null)
            {
                Console.WriteLine("Failed to detect archive format.");
                Console.WriteLine("");

                foreach (var failure in failures)
                {
                    if (failure.Value.Position != -1)
                    {
                        Console.WriteLine("  {0}: {1} (@ {2})",
                            failure.Key.GetType().Name,
                            failure.Value.Message,
                            failure.Value.Position);
                    }
                    else
                    {
                        Console.WriteLine("  {0}: {1}",
                            failure.Key.GetType().Name,
                            failure.Value.Message);
                    }
                }

                if (pauseOnError == true)
                {
                    Console.ReadKey(true);
                }

                return;
            }

            var entries = format.GetEntries(inputPath);

            if (entries == null)
            {
                Console.WriteLine("Error when getting entries (inconsistency between validate and get entries?)");
                
                if (pauseOnError == true)
                {
                    Console.ReadKey(true);
                }

                return;
            }

            if (filter != null)
            {
                filter = filter.ToLowerInvariant();
                entries = entries
                    .Where(e => MatchesFilter(e.Name, filter)).ToList();
            }

            long total = entries.Count;
            long counter = 0;
            long skipped = 0;

            if (entries.Count > 0)
            {
                Stream input = File.OpenRead(inputPath);
                byte[] buffer = new byte[0x4000];

                if (listing == false)
                {
                    Directory.CreateDirectory(outputPath);
                }

                foreach (var entry in entries)
                {
                    counter++;

                    var entryName = entry.Name;

                    if (entryName.Contains("/") == true)
                    {
                        entryName = entryName.Replace("/", Path.DirectorySeparatorChar.ToString());
                    }

                    if (entryName.Contains("..\\") == true)
                    {
                        entryName = entryName.Replace("..\\", "__UP\\");
                    }

                    string entryPath = Path.Combine(outputPath, entryName);

                    if (overwriteFiles == false && File.Exists(entryPath) == true)
                    {
                        Console.WriteLine("{1:D5}/{2:D5} !! {0}", entryName, counter, total);
                        skipped++;
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("{1:D5}/{2:D5} => {0}", entryName, counter, total);
                    }

                    if (listing == false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(entryPath));
                        Stream output = File.Open(entryPath, FileMode.Create, FileAccess.Write, FileShare.Read);

                        input.Seek(entry.Offset, SeekOrigin.Begin);

                        int left = (int)entry.Size;
                        while (left > 0)
                        {
                            int read = input.Read(buffer, 0, Math.Min(left, buffer.Length));
                            if (read == 0)
                            {
                                break;
                            }
                            output.Write(buffer, 0, read);
                            left -= read;
                        }

                        output.Flush();
                        output.Close();
                    }
                }

                input.Close();
            }

            if (skipped > 0)
            {
                Console.WriteLine("{0} files not overwritten.", skipped);
                
                if (pauseOnError == true)
                {
                    Console.ReadKey(true);
                }
            }

            //Console.ReadKey(true);
        }

        private static bool MatchesFilter(string path, string filter)
        {
            string extension = Path.GetExtension(path);
            if (extension == null)
            {
                return false;
            }
            return extension.ToLowerInvariant() == filter;
        }

        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }
    }
}
