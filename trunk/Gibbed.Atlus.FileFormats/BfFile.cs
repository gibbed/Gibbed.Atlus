using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats
{
    public class BfFile
    {
        public uint Unknown04;
        public uint Entrypoint;

        public List<CodeReference> Functions;
        public List<CodeReference> Labels;
        public Op[] Code;
        public BmdFile Data;
        public byte[] Junk;

        public void Deserialize(Stream input)
        {
            var header = input.ReadStructure<FileHeader>();

            if (header.Unknown00 != 0 ||
                header.Magic != 0x30574C46 || // FLW0
                header.Unknown0C != 0 ||
                header.Unknown18 != 0 ||
                header.Unknown1C != 0)
            {
                throw new FormatException();
            }

            this.Unknown04 = header.Unknown04;
            this.Entrypoint = header.Entrypoint;

            var blockInfos = new FileBlockInfo[header.BlockCount];
            for (int i = 0; i < blockInfos.Length; i++)
            {
                blockInfos[i] = input.ReadStructure<FileBlockInfo>();
            }

            foreach (var blockInfo in blockInfos)
            {
                input.Seek(blockInfo.Offset, SeekOrigin.Begin);

                switch (blockInfo.Type)
                {
                    // Functions
                    case 0:
                    {
                        if (blockInfo.ElementSize != 32)
                        {
                            throw new FormatException("function info size mismatch");
                        }

                        this.Functions = new List<CodeReference>();
                        for (uint i = 0; i < blockInfo.ElementCount; i++)
                        {
                            var func = new CodeReference();
                            func.Deserialize(input);
                            this.Functions.Add(func);
                        }

                        break;
                    }

                    // Labels
                    case 1:
                    {
                        if (blockInfo.ElementSize != 32)
                        {
                            throw new FormatException("sub info size mismatch");
                        }

                        this.Labels = new List<CodeReference>();
                        for (uint i = 0; i < blockInfo.ElementCount; i++)
                        {
                            var label = new CodeReference();
                            label.Deserialize(input);
                            this.Labels.Add(label);
                        }

                        break;
                    }

                    // Opcodes
                    case 2:
                    {
                        if (blockInfo.ElementSize != 4)
                        {
                            throw new FormatException("code size mismatch");
                        }

                        this.Code = new Op[blockInfo.ElementCount];
                        for (int i = 0; i < this.Code.Length; i++)
                        {
                            this.Code[i] = (Op)input.ReadValueU32();
                        }

                        break;
                    }

                    // Message Data
                    case 3:
                    {
                        if (blockInfo.ElementSize != 1)
                        {
                            throw new FormatException("data size mismatch");
                        }

                        if (blockInfo.ElementCount > 0)
                        {
                            var memory = input.ReadToMemoryStream(blockInfo.ElementCount);

                            this.Data = new BmdFile();
                            this.Data.Deserialize(memory);
                        }

                        break;
                    }

                    // ???
                    case 4:
                    {
                        if (blockInfo.ElementSize != 1)
                        {
                            throw new FormatException("junk size mismatch");
                        }

                        this.Junk = new byte[blockInfo.ElementCount];
                        input.Read(this.Junk, 0, this.Junk.Length);

                        break;
                    }

                    default:
                    {
                        throw new FormatException();
                    }
                }
            }
        }

        public class CodeReference
        {
            public string Name;
            public uint Offset;
            public uint Unknown1C;

            public void Deserialize(Stream input)
            {
                this.Name = input.ReadStringASCII(24, true);
                this.Offset = input.ReadValueU32();
                this.Unknown1C = input.ReadValueU32();

                if (this.Unknown1C != 0)
                {
                    throw new FormatException();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileHeader
        {
            public uint Unknown00;
            public uint Unknown04;
            public uint Magic;
            public uint Unknown0C;
            public uint BlockCount;
            public uint Entrypoint;
            public uint Unknown18;
            public uint Unknown1C;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileBlockInfo
        {
            public uint Type;
            public uint ElementSize;
            public uint ElementCount;
            public uint Offset;
        }

        public class Op
        {
            public Instruction Instruction;
            public ushort Extra;

            public static implicit operator uint(Op op)
            {
                return (uint)(op.Extra << 16) | (ushort)op.Instruction;
            }

            public static implicit operator Op(uint value)
            {
                return new Op()
                {
                    Instruction = (Instruction)(value & 0xFFFF),
                    Extra = (ushort)((value & 0xFFFF0000) >> 16),
                };
            }

            public override string ToString()
            {
                if (this.Extra == 0)
                {
                    return this.Instruction.ToString();
                }

                return string.Format("{0} ({1})",
                    this.Instruction,
                    this.Extra);
            }
        }

        public enum Instruction : ushort
        {
            EnterFunction = 7,
            CallNative = 8,
            Return = 9,
            CallFunction = 11,
            Jump = 13,
            Add = 14,
            JumpZero = 28,
            PushWord = 29,
        }
    }
}
