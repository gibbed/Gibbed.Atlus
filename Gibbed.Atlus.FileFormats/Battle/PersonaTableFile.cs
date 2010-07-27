using System.Collections.Generic;
using System.IO;
using Gibbed.Helpers;

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
