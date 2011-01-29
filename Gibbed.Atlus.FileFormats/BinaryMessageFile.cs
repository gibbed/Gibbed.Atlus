using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats
{
    /* Binary Messages (Database?)
     * 
     * Formats
     *   source: *.msg (converted to binary form with 'msgconv')
     *   variant 1:   *.bmd
     *   variant 2:   *.bm2
     *   variant 3:   *.bmd
     * 
     * Games (variant 1)
     *   Persona 3 FES
     *   Persona 4
     *   Persona 3 Portable
     *   
     * Games (variant 2)
     *   Trauma Center
     *   Trauma Team
     * 
     * Games (variant 3)
     *   Catherine
     */
    public class BinaryMessageFile
    {
        public uint Version;
        public List<Message> Messages;
        public List<string> Strings;
        public byte[] Extra;

        public void Deserialize(Stream input)
        {
            var header = input.ReadStructure<FileHeader>();

            if (header.Version != 7)
            {
                throw new FormatException("version");
            }

            if (header.EndOfFile != input.Length)
            {
                throw new FormatException();
            }

            if (header.Magic != 0x3147534D)
            {
                throw new FormatException();
            }

            if (header.Unknown0C != 0)
            {
                throw new FormatException();
            }

            if (header.EndOfData + header.ExtraSize != input.Length)
            {
                throw new FormatException();
            }

            var messageInfos = new FileMessageInfo[header.MessageCount];
            for (int i = 0; i < messageInfos.Length; i++)
            {
                messageInfos[i] = input.ReadStructure<FileMessageInfo>();
            }

            var stringTableInfo = input.ReadStructure<FileBlockInfo>();
            var unknownInfo = input.ReadStructure<FileBlockInfo>();

            if (unknownInfo.Offset != 0 ||
                unknownInfo.Count != 0)
            {
                throw new FormatException();
            }

            this.Messages = new List<Message>();
            foreach (var messageInfo in messageInfos)
            {
                input.Seek(0x20 + messageInfo.Offset, SeekOrigin.Begin);

                switch (messageInfo.Type)
                {
                    case 0:
                    {
                        string name = input.ReadStringASCII(24, true);
                        short pageCount = input.ReadValueS16();
                        short unknown = input.ReadValueS16();
                        
                        uint[] pageOffsets = new uint[pageCount];
                        for (int i = 0; i < pageOffsets.Length; i++)
                        {
                            pageOffsets[i] = input.ReadValueU32();
                        }

                        uint size = input.ReadValueU32();

                        for (int i = 0; i < pageOffsets.Length; i++)
                        {
                            pageOffsets[i] -= (uint)input.Position;
                        }

                        var memory = input.ReadToMemoryStream(size);

                        var dialogue = new Dialogue();
                        dialogue.Name = name;
                        dialogue.Unknown = unknown;
                        dialogue.PageOffsets.AddRange(pageOffsets);
                        dialogue.Data = memory;

                        this.Messages.Add(dialogue);
                        
                        break;
                    }

                    case 1:
                    {
                        string name = input.ReadStringASCII(24, true);
                        short unknown = input.ReadValueS16();
                        short choiceCount = input.ReadValueS16();
                        uint unknown2 = input.ReadValueU32();

                        if (unknown != 0)
                        {
                            throw new FormatException();
                        }

                        uint[] choiceOffsets = new uint[choiceCount];
                        for (int i = 0; i < choiceOffsets.Length; i++)
                        {
                            choiceOffsets[i] = input.ReadValueU32();
                        }

                        uint size = input.ReadValueU32();

                        for (int i = 0; i < choiceOffsets.Length; i++)
                        {
                            choiceOffsets[i] -= (uint)input.Position;
                        }

                        var memory = input.ReadToMemoryStream(size);

                        var dialogue = new Choices();
                        dialogue.Name = name;
                        dialogue.ChoiceOffsets.AddRange(choiceOffsets);
                        dialogue.Data = memory;

                        this.Messages.Add(dialogue);

                        break;
                    }

                    default:
                    {
                        throw new FormatException();
                    }
                }
            }

            input.Seek(0x20 + stringTableInfo.Offset, SeekOrigin.Begin);
            uint[] stringOffsets = new uint[stringTableInfo.Count];
            for (int i = 0; i < stringOffsets.Length; i++)
            {
                stringOffsets[i] = input.ReadValueU32();
            }

            this.Strings = new List<string>(stringOffsets.Length);
            for (int i = 0; i < stringOffsets.Length; i++)
            {
                input.Seek(0x20 + stringOffsets[i], SeekOrigin.Begin);
                
                /* this really should read the string as an
                 * array of bytes but for now, since it's not really 
                 * ASCII, but I'm lazy and it's not necessary to
                 * retain it at the moment */
                //this.Strings.Add(input.ReadStringASCIIZ());
                this.Strings.Add(input.ReadStringGameZ());
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileHeader
        {
            public uint Version;
            public uint EndOfFile;
            public uint Magic;
            public uint Unknown0C;
            public uint EndOfData;
            public uint ExtraSize;
            public uint MessageCount;
            public uint Unknown1C;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileMessageInfo
        {
            public uint Type;
            public uint Offset;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileBlockInfo
        {
            public uint Offset;
            public uint Count;
        }

        public abstract class Message
        {
            public string Name;
        }

        public class Dialogue : Message
        {
            public short Unknown;
            public List<uint> PageOffsets
                = new List<uint>();
            public Stream Data;
        }

        public class Choices : Message
        {
            public List<uint> ChoiceOffsets
                = new List<uint>();
            public Stream Data;
        }
    }
}
