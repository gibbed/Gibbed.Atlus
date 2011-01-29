using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Gibbed.Atlus.FileFormats.Script;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats.BinaryScript
{
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
}
