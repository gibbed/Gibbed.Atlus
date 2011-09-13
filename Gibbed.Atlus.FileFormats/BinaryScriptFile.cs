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
using Gibbed.Atlus.FileFormats.Script;
using Gibbed.IO;

namespace Gibbed.Atlus.FileFormats
{
    /* Binary Script File
     * 
     * Formats
     *   source: *.scr, *.h (compiled to binary form with 'scr2bin')
     *   ver0:   *.bf
     * 
     * Games
     *   Persona 3 FES (ver0)
     *   Persona 4 (ver0)
     *   Persona 3 Portable (ver0)
     *   Trauma Center (ver0)
     *   Trauma Team (ver0)
     *   Strange Journey (ver0)
     */
    public class BinaryScriptFile
    {
        public bool LittleEndian;

        public uint Unknown04Offset;
        public uint Entrypoint;

        public List<BinaryScript.CodeReference> Procedures;
        public List<BinaryScript.CodeReference> Labels;
        public Opcode[] Code;
        public BinaryMessageFile Data;
        public byte[] Junk;

        public void Deserialize(Stream input)
        {
            this.LittleEndian = true;

            var header = input.ReadStructure<BinaryScript.FileHeader>();

            // guess...
            if (header.BlockCount > 255)
            {
                this.LittleEndian = false;
                header.Swap();
            }

            if (header.Unknown00 != 0 ||
                header.Magic != 0x30574C46 || // FLW0
                header.Unknown0C != 0 ||
                header.Unknown18 != 0 ||
                header.Unknown1C != 0)
            {
                throw new FormatException();
            }

            this.Unknown04Offset = header.Unknown04Offset;
            this.Entrypoint = header.Entrypoint;

            var blockInfos = new BinaryScript.FileBlockInfo[header.BlockCount];
            for (int i = 0; i < blockInfos.Length; i++)
            {
                blockInfos[i] = input.ReadStructure<BinaryScript.FileBlockInfo>();

                if (this.LittleEndian == false)
                {
                    blockInfos[i].Swap();
                }
            }

            foreach (var blockInfo in blockInfos)
            {
                input.Seek(blockInfo.Offset, SeekOrigin.Begin);

                switch (blockInfo.Type)
                {
                    case BinaryScript.FileBlockType.Procedures:
                    {
                        if (blockInfo.ElementSize != 32)
                        {
                            throw new FormatException("procedure info size mismatch");
                        }

                        this.Procedures = new List<BinaryScript.CodeReference>();
                        for (uint i = 0; i < blockInfo.ElementCount; i++)
                        {
                            var procedure = new BinaryScript.CodeReference();
                            procedure.Deserialize(input, this.LittleEndian);
                            this.Procedures.Add(procedure);
                        }

                        break;
                    }

                    case BinaryScript.FileBlockType.Labels:
                    {
                        if (blockInfo.ElementSize != 32)
                        {
                            throw new FormatException("sub info size mismatch");
                        }

                        this.Labels = new List<BinaryScript.CodeReference>();
                        for (uint i = 0; i < blockInfo.ElementCount; i++)
                        {
                            var label = new BinaryScript.CodeReference();
                            label.Deserialize(input, this.LittleEndian);
                            this.Labels.Add(label);
                        }

                        break;
                    }

                    case BinaryScript.FileBlockType.Opcodes:
                    {
                        if (blockInfo.ElementSize != 4)
                        {
                            throw new FormatException("code size mismatch");
                        }

                        this.Code = new Opcode[blockInfo.ElementCount];
                        for (int i = 0; i < this.Code.Length; i++)
                        {
                            ushort instruction = input.ReadValueU16(this.LittleEndian);
                            ushort arg = input.ReadValueU16(this.LittleEndian);

                            this.Code[i] = new Opcode()
                            {
                                Instruction = (Instruction)instruction,
                                Argument = arg,
                            };
                        }

                        break;
                    }

                    case BinaryScript.FileBlockType.Messages:
                    {
                        if (blockInfo.ElementSize != 1)
                        {
                            throw new FormatException("data size mismatch");
                        }

                        if (blockInfo.ElementCount > 0)
                        {
                            var memory = input.ReadToMemoryStream(blockInfo.ElementCount);

                            this.Data = new BinaryMessageFile();
                            this.Data.Deserialize(memory);
                        }

                        break;
                    }

                    case BinaryScript.FileBlockType.Unknown4:
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
                        throw new FormatException("unknown block type");
                    }
                }
            }
        }
    }
}
