﻿/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
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
