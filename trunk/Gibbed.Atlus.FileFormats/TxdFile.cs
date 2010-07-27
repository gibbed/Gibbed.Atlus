using System;
using System.IO;
using System.Runtime.InteropServices;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats
{
    public class TxdFile
    {
        public void Deserialize(Stream input)
        {
            var header = input.ReadStructure<Header>();

            if (header.Version != 2)
            {
                throw new FormatException();
            }

            if (header.Magic != 0x30584D54) // TMX0
            {
                throw new FormatException();
            }

            if (header.Size != input.Length)
            {
                //throw new FormatException();
            }

            if (header.Mips != 1)
            {
                throw new FormatException();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Header
        {
            public uint Version;
            public uint Size;
            public uint Magic;
            public uint Unknown0C;
            public ushort Mips;
            public ushort Width;
            public ushort Height;
            public uint Flags;
            public ushort Unknown1A;
            public uint Unknown1C;
        }
    }
}
