using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats.Field2d
{
    public class DatFile
    {
        public List<Entry> Entries;

        public void Deserialize(Stream input)
        {
            uint count = input.ReadValueU32();
            if (input.Position + (count * 12) > input.Length)
            {
                throw new InvalidOperationException();
            }

            this.Entries = new List<Entry>();
            for (uint i = 0; i < count; i++)
            {
                var entry = new Entry();
                entry.Deserialize(input);
                this.Entries.Add(entry);
            }
        }

        public class Entry
        {
            public uint Id;
            public ushort X;
            public ushort Y;
            public ushort Unknown8;
            public ushort UnknownA;

            public void Deserialize(Stream input)
            {
                this.Id = input.ReadValueU32();

                if (this.Id > 0xFFFF)
                {
                    throw new Exception("hurf");
                }

                this.X = input.ReadValueU16();
                this.Y = input.ReadValueU16();
                this.Unknown8 = input.ReadValueU16();
                this.UnknownA = input.ReadValueU16();
            }
        }
    }
}
