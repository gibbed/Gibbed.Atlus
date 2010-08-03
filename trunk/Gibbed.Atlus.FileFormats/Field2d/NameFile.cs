using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats.Field2d
{
    public class NameFile
    {
        public List<string> Entries;

        public void Deserialize(Stream input)
        {
            uint length = input.ReadValueU32();
            if (input.Position + length > input.Length)
            {
                throw new InvalidOperationException();
            }

            uint count = input.ReadValueU32();
            var memory = input.ReadToMemoryStream(length);
            this.Entries = new List<string>();
            for (uint i = 0; i < count; i++)
            {
                this.Entries.Add(memory.ReadStringGame(64, true));
            }
        }
    }
}
