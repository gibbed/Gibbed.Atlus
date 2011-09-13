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

using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Atlus.FileFormats.Battle
{
    public class PersonaTableFile
    {
        public List<PersonaStatistics> Statistics;

        private MemoryStream ReadAlignedBlock(Stream input)
        {
            uint tableLength = input.ReadValueU32();
            uint alignedLength = tableLength.Align(16);

            var memory = input.ReadToMemoryStream(tableLength);

            if (alignedLength != tableLength)
            {
                input.Seek(alignedLength - tableLength, SeekOrigin.Current);
            }

            return memory;
        }

        public void Deserialize(Stream input)
        {
            this.DeserializePersonaStats(this.ReadAlignedBlock(input));
        }

        private void DeserializePersonaStats(MemoryStream input)
        {
            this.Statistics = new List<PersonaStatistics>();
            uint count = (uint)input.Length / 14;
            for (uint i = 0; i < count; i++)
            {
                var stats = new PersonaStatistics();
                stats.Deserialize(input);
                this.Statistics.Add(stats);
            }
        }
    }
}
