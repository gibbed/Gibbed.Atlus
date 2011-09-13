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

using System.IO;
using Gibbed.IO;

namespace Gibbed.Atlus.FileFormats.Battle
{
    public class PersonaStatistics
    {
        public byte Unknown0;
        public byte Unknown1;
        public byte ArcanaIndex;
        public byte Level;
        public byte St;
        public byte Ma;
        public byte En;
        public byte Ag;
        public byte Lu;
        public byte Unknown9;
        public byte UnknownA;
        public byte UnknownB;
        public byte UnknownC;
        public byte CombineMessageIndex; // Fusion message (I am blah, blah blah...)

        public void Deserialize(Stream input)
        {
            this.Unknown0 = input.ReadValueU8();
            this.Unknown1 = input.ReadValueU8();
            this.ArcanaIndex = input.ReadValueU8();
            this.Level = input.ReadValueU8();
            this.St = input.ReadValueU8();
            this.Ma = input.ReadValueU8();
            this.En = input.ReadValueU8();
            this.Ag = input.ReadValueU8();
            this.Lu = input.ReadValueU8();
            this.Unknown9 = input.ReadValueU8();
            this.UnknownA = input.ReadValueU8();
            this.UnknownB = input.ReadValueU8();
            this.UnknownC = input.ReadValueU8();
            this.CombineMessageIndex = input.ReadValueU8();
        }
    }
}
