using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.Atlus.FileFormats;
using Gibbed.Atlus.FileFormats.Script;
using Gibbed.Helpers;
using NDesk.Options;

namespace Gibbed.Atlus.DisassembleBF
{
    public class Program
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
                case 20499: return "is persona equipped";
            }

            return null;
        }

        private static string CommentInstruction(
            BinaryScriptFile bf, uint index, Opcode opcode)
        {
            switch (opcode.Instruction)
            {
                case Instruction.PushFloat:
                {
                    return "float";
                }

                /*
                case Instruction.PushWord:
                {
                    return "word";
                }
                */

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
                case Instruction.PushInt:
                {
                    uint dummy =
                        ((uint)bf.Code[index + 1].Instruction) |
                        ((uint)bf.Code[index + 1].Argument) << 16;
                    return string.Format("push {0}",
                        (int)dummy);
                }

                case Instruction.PushFloat:
                {
                    // this is some retarded way to do this
                    uint dummy =
                        ((uint)bf.Code[index + 1].Instruction) |
                        ((uint)bf.Code[index + 1].Argument) << 16;
                    byte[] data = BitConverter.GetBytes(dummy);
                    float value = BitConverter.ToSingle(data, 0);
                    return string.Format("push {0}",
                        value);
                }

                case Instruction.PushVariable:
                {
                    return string.Format("push var{0}",
                        opcode.Argument);
                }

                case Instruction.PushResult:
                {
                    return "push result";
                }

                case Instruction.PopVariable:
                {
                    return string.Format("pop var{0}",
                        opcode.Argument);
                }

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

                case Instruction.Subtract:
                {
                    return "sub";
                }

                case Instruction.Not:
                {
                    return "not";
                }

                case Instruction.JumpFalse:
                {
                    return String.Format("jf @{0}",
                        bf.Labels[opcode.Argument].Name);
                }

                case Instruction.PushShort:
                {
                    return string.Format("push {0}",
                        (short)opcode.Argument);
                }

                /*
                case Instruction.SetVariable:
                {
                    return string.Format("set var{0}",
                        opcode.Argument);
                }
                */

                default:
                {
                    return "???";
                }
            }
        }

        private static void WriteInstruction(
            TextWriter output,
            BinaryScriptFile bf,
            uint index,
            Opcode opcode,
            bool isData)
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

            string line = "";
            string raw = "";
            string comment = null;

            if (isData == false)
            {
                line = DisassembleInstruction(bf, index, opcode);
                raw = string.Format("{0}:{1:X4}",
                    (ushort)opcode.Instruction,
                    opcode.Argument);
                comment = CommentInstruction(bf, index, opcode);
            }
            else
            {
                raw = "(data)";
            }

            if (comment != null)
            {
                line = line.PadRight(20);
            }

            output.Write(
                String.Format("{0:D5} {1} {2} {3} {4}",
                    index,
                    GetOpcodeBytes(opcode, bf.LittleEndian),
                    (raw != null ? raw : "").PadLeft(11),
                    ((labels.Length > 0) ? ("@" + labels.Implode(l => l.Name, " +@")) : "").PadRight(16),
                    line));

            if (comment != null)
            {
                output.Write(" ; {0}", comment);
            }

            output.WriteLine();
        }

        private static string GetOpcodeBytes(Opcode opcode, bool littleEndian)
        {
            if (littleEndian == true)
            {
                return string.Format("{0:X4}{1:X4}",
                    ((ushort)opcode.Instruction).Swap(),
                    opcode.Argument.Swap());
            }
            else
            {
                return string.Format("{0:X4}{1:X4}",
                    ((ushort)opcode.Instruction),
                    opcode.Argument);
            }
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
                output.WriteLine("# Entrypoint: {0} ; most likely wrong", bf.Procedures[ep].Name);
            }

            output.WriteLine("# Procedures: {0}",
                bf.Procedures.Implode(f => f.Name, ", "));

            output.WriteLine();

            int dataOps = 0;
            for (uint i = 0; i < bf.Code.Length; i++)
            {
                WriteInstruction(output, bf, i, bf.Code[i], dataOps > 0);

                if (dataOps > 0)
                {
                    dataOps--;
                }
                else
                {
                    if (bf.Code[i].Instruction == Instruction.PushInt ||
                        bf.Code[i].Instruction == Instruction.PushFloat)
                    {
                        dataOps++;
                    }
                }
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
