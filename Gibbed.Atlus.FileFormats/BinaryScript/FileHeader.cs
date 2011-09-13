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

using System.Runtime.InteropServices;
using Gibbed.IO;

namespace Gibbed.Atlus.FileFormats.BinaryScript
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FileHeader
    {
        public uint Unknown00;
        public uint Unknown04Offset;
        public uint Magic;
        public uint Unknown0C;
        public uint BlockCount;
        public uint Entrypoint;
        public uint Unknown18;
        public uint Unknown1C;

        public void Swap()
        {
            this.Unknown00 = this.Unknown00.Swap();
            this.Unknown04Offset = this.Unknown04Offset.Swap();
            //this.Magic = this.Magic.Swap();
            this.Unknown0C = this.Unknown0C.Swap();
            this.BlockCount = this.BlockCount.Swap();
            this.Entrypoint = this.Entrypoint.Swap();
            this.Unknown18 = this.Unknown18.Swap();
            this.Unknown1C = this.Unknown1C.Swap();
        }
    }
}
