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
using System.IO;
using System.Runtime.InteropServices;
using Gibbed.IO;

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
