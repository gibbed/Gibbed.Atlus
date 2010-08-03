using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats
{
    public class SprFile
    {
        public uint Version;
        public List<Frame> Frames;
        public List<TxdFile> Sources;

        public void Deserialize(Stream input)
        {
            var position = input.Position;
            var header = input.ReadStructure<Header>();

            if (header.Version != 1)
            {
                throw new FormatException();
            }

            if (header.Unknown04 != 0)
            {
                throw new FormatException();
            }

            if (header.Magic != 0x30525053)
            {
                throw new FormatException();
            }

            if (header.Unknown0C != 32)
            {
                throw new FormatException();
            }

            if (position + header.TotalSize > input.Length)
            {
                throw new InvalidOperationException();
            }

            input.Seek(position, SeekOrigin.Begin);
            var data = input.ReadToMemoryStream(header.TotalSize);

            data.Seek(header.FrameIndexOffset, SeekOrigin.Begin);
            uint[] frameOffsets = new uint[header.FrameCount];
            for (ushort i = 0; i < header.FrameCount; i++)
            {
                if (data.ReadValueU32() != 0)
                {
                    throw new FormatException();
                }

                frameOffsets[i] = data.ReadValueU32();
            }

            data.Seek(header.SourceIndexOffset, SeekOrigin.Begin);
            uint[] sourceOffsets = new uint[header.SourceCount];
            for (ushort i = 0; i < header.SourceCount; i++)
            {
                if (data.ReadValueU32() != 0)
                {
                    throw new FormatException();
                }

                sourceOffsets[i] = data.ReadValueU32();
            }

            this.Frames = new List<Frame>();
            foreach (var frameOffset in frameOffsets)
            {
                data.Seek(frameOffset, SeekOrigin.Begin);
                var frame = new Frame();
                frame.Deserialize(data);
                this.Frames.Add(frame);
            }

            /*
            this.Sources = new List<TxdFile>();
            foreach (var sourceOffset in sourceOffsets)
            {
                data.Seek(sourceOffset, SeekOrigin.Begin);
                var source = new TxdFile();
                source.Deserialize(data);
                this.Sources.Add(source);
            }
            */
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Header
        {
            public uint Version;
            public uint Unknown04;
            public uint Magic;
            public uint Unknown0C; // probably header size
            public uint TotalSize;
            public ushort SourceCount;
            public ushort FrameCount;
            public uint SourceIndexOffset;
            public uint FrameIndexOffset;
        }

        public class Frame
        {
            public uint Unknown00;
            public string Comment;
            public int TextureIndex;
            public uint Unknown18;
            public byte[] Unknown1C;
            public uint Unknown2C;
            public byte[] Unknown30;
            public uint Unknown3C;
            public uint Unknown40;
            public uint Unknown44;
            public uint Unknown48;
            public byte[] Unknown4C;
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public byte[] Unknown64;
            public short Unknown74;
            public short Unknown76;
            public byte[] Unknown78;

            public void Deserialize(Stream input)
            {
                this.Unknown00 = input.ReadValueU32();
                this.Comment = input.ReadStringSJIS(16, true);
                this.TextureIndex = input.ReadValueS32();
                this.Unknown18 = input.ReadValueU32();
                this.Unknown1C = new byte[16];
                input.Read(this.Unknown1C, 0, this.Unknown1C.Length);
                this.Unknown2C = input.ReadValueU32();
                this.Unknown30 = new byte[12];
                input.Read(this.Unknown30, 0, this.Unknown30.Length);
                this.Unknown3C = input.ReadValueU32();
                this.Unknown40 = input.ReadValueU32();
                this.Unknown44 = input.ReadValueU32();
                this.Unknown48 = input.ReadValueU32();
                this.Unknown4C = new byte[8];
                input.Read(this.Unknown4C, 0, this.Unknown4C.Length);
                this.Left = input.ReadValueS32();
                this.Top = input.ReadValueS32();
                this.Right = input.ReadValueS32();
                this.Bottom = input.ReadValueS32();
                this.Unknown64 = new byte[16];
                input.Read(this.Unknown64, 0, this.Unknown64.Length);
                this.Unknown74 = input.ReadValueS16();
                this.Unknown76 = input.ReadValueS16();
                this.Unknown78 = new byte[8];
                input.Read(this.Unknown78, 0, this.Unknown78.Length);

                foreach (var u1C in this.Unknown1C)
                {
                    if (u1C != 0)
                    {
                        throw new FormatException();
                    }
                }

                foreach (var u4C in this.Unknown4C)
                {
                    if (u4C != 0)
                    {
                        throw new FormatException();
                    }
                }

                foreach (var u64 in this.Unknown64)
                {
                    if (u64 != 128 && u64 != 255)
                    {
                        //throw new FormatException();
                    }
                }

                foreach (var u78 in this.Unknown78)
                {
                    if (u78 != 0)
                    {
                        throw new FormatException();
                    }
                }
            }

            public override string ToString()
            {
                if (string.IsNullOrEmpty(this.Comment) == true)
                {
                    return base.ToString();
                }

                return "render: " + this.Comment;
            }
        }
    }
}
