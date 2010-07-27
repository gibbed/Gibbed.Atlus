using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NDesk.Options;

namespace Gibbed.Atlus.UnpackMany
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            bool verbose = false;
            bool overwriteFiles = false;
            bool listing = false;
            bool showHelp = false;
            string filter = null;

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

            if (extras.Count < 1 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_directory", GetExecutableName());
                Console.WriteLine("Unpack many archives.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var filePaths = new List<string>();

            foreach (var extra in extras)
            {
                string inputPath = extra;
                string outputPath =
                    Path.Combine(
                        Path.GetDirectoryName(inputPath),
                        Path.GetFileName(inputPath) + "_unpacked");

                foreach (var archivePath in GetArchivePaths(inputPath))
                {
                    string unpackPath = Path.ChangeExtension(archivePath, null);
                    unpackPath =
                        outputPath + unpackPath.Substring(inputPath.Length);

                    Console.WriteLine("Unpacking {0} to {1}",
                        archivePath, unpackPath);

                    var cmd = new List<string>();

                    cmd.Add("--nopause");

                    if (verbose == true)
                    {
                        cmd.Add("--verbose");
                    }

                    if (listing == true)
                    {
                        cmd.Add("--list");
                    }

                    if (overwriteFiles == true)
                    {
                        cmd.Add("--overwrite");
                    }

                    if (filter != null)
                    {
                        cmd.Add("--filter=" + filter);
                    }

                    cmd.Add(archivePath);
                    cmd.Add(unpackPath);

                    Unpack.Program.Main(cmd.ToArray());
                    //Console.ReadKey(true);
                }
            }
        }

        private static bool IsArchivePath(string inputPath)
        {
            var extension = Path.GetExtension(inputPath);
            return
                extension == ".bin" ||
                extension == ".abin" ||
                extension == ".pac" ||
                extension == ".fpc";
        }

        private static IEnumerable<string> GetArchivePaths(string basePath)
        {
            return Directory.GetFiles(basePath, "*", SearchOption.AllDirectories)
                .Where(IsArchivePath);
        }

        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }
    }
}
