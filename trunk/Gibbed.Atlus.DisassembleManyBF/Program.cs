using System;
using System.Collections.Generic;
using System.IO;
using NDesk.Options;

namespace Gibbed.Atlus.DisassembleManyBF
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            bool showHelp = false;

            OptionSet options = new OptionSet()
            {
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
                Console.WriteLine("Disassembly many binary scripts.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var filePaths = new List<string>();

            foreach (var extra in extras)
            {
                string inputPath = extra;

                foreach (var scriptPath in Directory.GetFiles(inputPath, "*.bf", SearchOption.AllDirectories))
                {
                    Console.WriteLine("Disassembling {0}...", scriptPath);

                    var cmd = new List<string>();
                    cmd.Add(scriptPath);
                    DisassembleBF.Program.Main(cmd.ToArray());
                    //Console.ReadKey(true);
                }
            }
        }

        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }
    }
}
