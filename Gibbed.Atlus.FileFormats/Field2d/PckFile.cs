using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats.Field2d
{
    /* *_pck.dat under field2d
     * I assume "pck" is "pick" or "picker".
     */
    public class PckFile
    {
        public List<Entry> Entries;

        public void Deserialize(Stream input)
        {
            uint length = input.ReadValueU32();
            if (input.Position + length > input.Length)
            {
                throw new InvalidOperationException();
            }

            uint count = input.ReadValueU32();
            var memory = input.ReadToMemoryStream(length);
            this.Entries = new List<Entry>();
            for (uint i = 0; i < count; i++)
            {
                var entry = new Entry();
                entry.Deserialize(memory);
                this.Entries.Add(entry);
            }
        }

        public class Entry
        {
            public short Id;
            public ushort Unknown2;
            public ushort Unknown4;
            public ushort Unknown6;
            public int Unknown8;
            public uint VisibilityFlag;

            public void Deserialize(Stream input)
            {
                this.Id = input.ReadValueS16();
                this.Unknown2 = input.ReadValueU16();
                this.Unknown4 = input.ReadValueU16();
                this.Unknown6 = input.ReadValueU16();
                this.Unknown8 = input.ReadValueS32();
                this.VisibilityFlag = input.ReadValueU32();
            }
        }
    }
}
