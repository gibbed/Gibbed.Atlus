using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.Atlus.FileFormats;
using Gibbed.Helpers;
using NDesk.Options;
using Gibbed.Atlus.FileFormats.Script;

namespace Gibbed.Atlus.DisassembleBF
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        private static string HintCall(uint extra)
        {
            switch (extra)
            {
                case 0: return "dialogue";
                case 3: return "choices";
                case 7: return "get flag";
                case 8: return "set flag";
                case 9: return "clear flag";
            }

            return null;
        }

        private static string CommentInstruction(
            BinaryScriptFile bf, uint index, Opcode opcode)
        {
            switch (opcode.Instruction)
            {
                case Instruction.CallNative:
                {
                    return HintCall(opcode.Argument);
                }

                default:
                {
                    return null;
                }
            }
        }

        private static string DisassembleInstruction(
            BinaryScriptFile bf, uint index, Opcode opcode)
        {
            switch (opcode.Instruction)
            {
                case Instruction.BeginProcedure:
                {
                    return string.Format("enter {0}",
                        bf.Procedures[opcode.Argument].Name);
                }

                case Instruction.CallNative:
                {
                    return string.Format("calln {0}",
                        opcode.Argument);
                }

                case Instruction.Return:
                {
                    return "ret";
                }

                case Instruction.CallProcedure:
                {
                    return string.Format("callp {0}",
                        bf.Procedures[opcode.Argument].Name);
                }

                case Instruction.Jump:
                {
                    return String.Format("jmp @{0}",
                        bf.Labels[opcode.Argument].Name);
                }

                case Instruction.Add:
                {
                    return "add";
                }

                case Instruction.JumpZero:
                {
                    return String.Format("jz @{0}",
                        bf.Labels[opcode.Argument].Name);
                }

                case Instruction.PushWord:
                {
                    return string.Format("pushw {0}",
                        (short)opcode.Argument);
                }

                default:
                {
                    return "???";
                }
            }
        }

        private static void WriteInstruction(
            TextWriter output, BinaryScriptFile bf, uint index, Opcode opcode)
        {
            var labels = bf.Labels.Where(l => l.Offset == index).ToArray();
            var function = bf.Procedures.SingleOrDefault(l => l.Offset == index);

            if (function != null)
            {
                if (index > 0)
                {
                    output.WriteLine();
                }

                output.WriteLine("[{0}]#{1}", function.Name, bf.Procedures.IndexOf(function));
            }

            if (labels.Length > 0)
            {
                output.WriteLine();
            }

            if ((ushort)opcode.Instruction == 4 &&
                opcode.Argument != 0)
            {
                throw new Exception();
            }

            var line = DisassembleInstruction(bf, index, opcode);
            var comment = CommentInstruction(bf, index, opcode);

            if (comment != null)
            {
                line = line.PadRight(20);
            }

            output.Write(
                String.Format("{0:D5} {1} {2}:{3:X4} {4} {5}",
                    index,
                    opcode.ToCode(bf.LittleEndian),
                    ((ushort)opcode.Instruction).ToString().PadLeft(4),
                    opcode.Argument,
                    ((labels.Length > 0) ? ("@" + labels.Implode(l => l.Name, " +@")) : "").PadRight(16),
                    line));

            if (comment != null)
            {
                output.Write(" ; {0}", comment);
            }

            output.WriteLine();
        }

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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_bf output_code", GetExecutableName());
                Console.WriteLine("Disassemble an Atlus BF file.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string inputPath = extra[0];
            string outputPath = extra.Count > 1 ? extra[1] : Path.ChangeExtension(inputPath, ".code");

            var input = File.OpenRead(inputPath);

            var bf = new BinaryScriptFile();
            bf.Deserialize(input);

            input.Close();

            var output = new StreamWriter(outputPath);

            output.WriteLine("# From: {0}", inputPath);

            int ep = (int)bf.Entrypoint;

            if (ep > bf.Procedures.Count)
            {
                output.WriteLine("# Entrypoint: ??? {0}", ep);
            }
            else if (ep == bf.Procedures.Count)
            {
                output.WriteLine("# Entrypoint: <none>");
            }
            else
            {
                output.WriteLine("# Entrypoint: {0}", bf.Procedures[ep].Name);
            }

            output.WriteLine("# Functions: {0}",
                bf.Procedures.Implode(f => f.Name, ", "));

            output.WriteLine();

            for (uint i = 0; i < bf.Code.Length; i++)
            {
                WriteInstruction(output, bf, i, bf.Code[i]);
            }

            output.WriteLine();

            if (bf.Data != null)
            {
                for (int i = 0; i < bf.Data.Messages.Count; i++)
                {
                    var msg = bf.Data.Messages[i];

                    output.WriteLine("# {0}_{1}: {2}",
                        msg.GetType().Name,
                        i,
                        msg.Name);
                }

                output.WriteLine();
            }

            output.Close();
        }
    }
}
