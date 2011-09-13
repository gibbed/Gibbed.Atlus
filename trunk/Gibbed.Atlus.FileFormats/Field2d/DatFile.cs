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
using Gibbed.IO;

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
