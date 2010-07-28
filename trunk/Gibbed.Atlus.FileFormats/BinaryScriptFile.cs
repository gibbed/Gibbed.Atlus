using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats
{
    public class BinaryScriptFile
    {
        public bool LittleEndian;

        public uint Unknown04;
        public uint Entrypoint;

        public List<CodeReference> Procedures;
        public List<CodeReference> Labels;
        public Op[] Code;
        public BinaryMessageFile Data;
        public byte[] Junk;

        public void Deserialize(Stream input)
        {
            this.LittleEndian = true;

            var header = input.ReadStructure<FileHeader>();

            // guess...
            if (header.BlockCount > 255)
            {
                this.LittleEndian = false;

                header.Unknown00 = header.Unknown00.Swap();
                header.Unknown04 = header.Unknown04.Swap();
                header.Unknown0C = header.Unknown0C.Swap();
                header.BlockCount = header.BlockCount.Swap();
                header.Entrypoint = header.Entrypoint.Swap();
                header.Unknown18 = header.Unknown18.Swap();
                header.Unknown1C = header.Unknown1C.Swap();
            }

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

                if (this.LittleEndian == false)
                {
                    blockInfos[i].Type = blockInfos[i].Type.Swap();
                    blockInfos[i].ElementSize = blockInfos[i].ElementSize.Swap();
                    blockInfos[i].ElementCount = blockInfos[i].ElementCount.Swap();
                    blockInfos[i].Offset = blockInfos[i].Offset.Swap();
                }
            }

            foreach (var blockInfo in blockInfos)
            {
                input.Seek(blockInfo.Offset, SeekOrigin.Begin);

                switch (blockInfo.Type)
                {
                    // Procedures
                    case 0:
                    {
                        if (blockInfo.ElementSize != 32)
                        {
                            throw new FormatException("procedure info size mismatch");
                        }

                        this.Procedures = new List<CodeReference>();
                        for (uint i = 0; i < blockInfo.ElementCount; i++)
                        {
                            var procedure = new CodeReference();
                            procedure.Deserialize(input, this.LittleEndian);
                            this.Procedures.Add(procedure);
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
                            label.Deserialize(input, this.LittleEndian);
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
                            ushort instruction = input.ReadValueU16(this.LittleEndian);
                            ushort extra = input.ReadValueU16(this.LittleEndian);

                            this.Code[i] = new Op()
                            {
                                Instruction = (Instruction)instruction,
                                Extra = extra,
                            };
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

                            //this.Data = new BmdFile();
                            //this.Data.Deserialize(memory);
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

            public void Deserialize(Stream input, bool littleEndian)
            {
                this.Name = input.ReadStringASCII(24, true);
                this.Offset = input.ReadValueU32(littleEndian);
                this.Unknown1C = input.ReadValueU32(littleEndian);

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
            public string ToCode(bool littleEndian)
            {
                if (littleEndian == true)
                {
                    return string.Format("{0:X4}{1:X4}",
                        (ushort)this.Instruction,
                        this.Extra);
                }
                else
                {
                    return string.Format("{0:X4}{1:X4}",
                        ((ushort)this.Instruction).Swap(),
                        this.Extra.Swap());
                }
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
            BeginProcedure = 7,
            CallNative = 8,
            Return = 9,
            CallProcedure = 11,
            Jump = 13,
            Add = 14,
            JumpZero = 28,
            PushWord = 29,
        }
    }
}
