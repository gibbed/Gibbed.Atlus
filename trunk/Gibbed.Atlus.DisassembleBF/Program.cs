﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.Atlus.FileFormats;
using Gibbed.Helpers;
using NDesk.Options;
using Instruction = Gibbed.Atlus.FileFormats.BfFile.Instruction;

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
            BfFile bf, uint index, BfFile.Op opcode)
        {
            switch (opcode.Instruction)
            {
                case Instruction.CallNative:
                {
                    return HintCall(opcode.Extra);
                }

                default:
                {
                    return null;
                }
            }
        }

        private static string DisassembleInstruction(
            BfFile bf, uint index, BfFile.Op opcode)
        {
            switch (opcode.Instruction)
            {
                case Instruction.EnterFunction:
                {
                    return string.Format("enter {0}",
                        bf.Functions[opcode.Extra].Name);
                }

                case Instruction.CallNative:
                {
                    return string.Format("calln {0}",
                        opcode.Extra);
                }

                case Instruction.Return:
                {
                    return "ret";
                }

                case Instruction.CallFunction:
                {
                    return string.Format("callf {0}",
                        bf.Functions[opcode.Extra].Name);
                }

                case Instruction.Jump:
                {
                    return String.Format("jmp @{0}",
                        bf.Labels[opcode.Extra].Name);
                }

                case Instruction.Add:
                {
                    return "add";
                }

                case Instruction.JumpZero:
                {
                    return String.Format("jz @{0}",
                        bf.Labels[opcode.Extra].Name);
                }

                case Instruction.PushWord:
                {
                    return string.Format("pushw {0}",
                        (short)opcode.Extra);
                }

                default:
                {
                    return "???";
                }
            }
        }

        private static void WriteInstruction(
            TextWriter output, BfFile bf, uint index, BfFile.Op opcode)
        {
            var labels = bf.Labels.Where(l => l.Offset == index).ToArray();
            var function = bf.Functions.SingleOrDefault(l => l.Offset == index);

            if (function != null)
            {
                if (index > 0)
                {
                    output.WriteLine();
                }

                output.WriteLine("[{0}]#{1}", function.Name, bf.Functions.IndexOf(function));
            }

            if (labels.Length > 0)
            {
                output.WriteLine();
            }

            if ((ushort)opcode.Instruction == 4 &&
                opcode.Extra != 0)
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
                String.Format("{0:D5} {1:X8} {2}:{3:X4} {4} {5}",
                    index,
                    ((uint)opcode).Swap(),
                    ((ushort)opcode).ToString().PadLeft(4),
                    opcode.Extra,
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

            var bf = new BfFile();
            bf.Deserialize(input);

            input.Close();

            var output = new StreamWriter(outputPath);

            output.WriteLine("# From: {0}", inputPath);

            int ep = (int)bf.Entrypoint;

            if (ep > bf.Functions.Count)
            {
                output.WriteLine("# Entrypoint: ??? {0}", ep);
            }
            else if (ep == bf.Functions.Count)
            {
                output.WriteLine("# Entrypoint: <none>");
            }
            else
            {
                output.WriteLine("# Entrypoint: {0}", bf.Functions[ep].Name);
            }

            output.WriteLine("# Functions: {0}",
                bf.Functions.Implode(f => f.Name, ", "));

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
