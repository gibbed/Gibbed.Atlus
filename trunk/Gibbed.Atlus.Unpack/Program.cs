using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NDesk.Options;

namespace Gibbed.Atlus.Unpack
{
    internal class Program
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
            bool decompress = false;
            bool listing = false;
            bool showHelp = false;

            OptionSet options = new OptionSet()
            {
                {
                    "v|verbose",
                    "be verbose (list files)",
                    v => verbose = v != null
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
                    "h|help",
                    "show this message and exit", 
                    v => showHelp = v != null
                },
            };

            List<string> extra;

            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (extra.Count < 1 || extra.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_sarc [output_directory]", GetExecutableName());
                Console.WriteLine("Unpack specified small archive.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string inputPath = extra[0];
            string outputPath = extra.Count > 1 ? extra[1] : Path.ChangeExtension(inputPath, null);

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

                Console.ReadKey(true);
                return;
            }

            var entries = format.GetEntries(inputPath);

            if (entries == null)
            {
                Console.WriteLine("Error when getting entries (inconsistency between validate and get entries?)");
                Console.ReadKey(true);
                return;
            }

            Stream input = File.OpenRead(inputPath);
            byte[] buffer = new byte[0x4000];

            long total = entries.Count;
            long counter = 0;
            long skipped = 0;

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

            if (skipped > 0)
            {
                Console.WriteLine("{0} files not overwritten.", skipped);
            }

            Console.ReadKey(true);
        }

        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }
    }
}
