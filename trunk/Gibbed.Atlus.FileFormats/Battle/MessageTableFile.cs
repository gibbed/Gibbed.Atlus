using System.Collections.Generic;
using System.IO;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats.Battle
{
    public class MessageTableFile
    {
        public List<string> ArcanaNames;
        public List<string> SkillNames;
        public List<string> UnitNames;
        public List<string> PersonaNames;

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
            this.ArcanaNames = this.DeserializeTable(
                this.ReadAlignedBlock(input), 21);
            this.SkillNames = this.DeserializeTable(
                this.ReadAlignedBlock(input), 19);
            this.UnitNames = this.DeserializeTable(
                this.ReadAlignedBlock(input), 19);
            this.UnitNames = this.DeserializeTable(
                this.ReadAlignedBlock(input), 17);

            // There is a BmdFile at the end of MSG.TBL, who knows why??
        }

        private List<string> DeserializeTable(MemoryStream input, uint stringLength)
        {
            var strings = new List<string>();
            uint stringCount = (uint)input.Length / stringLength;
            for (uint i = 0; i < stringCount; i++)
            {
                strings.Add(input.ReadStringASCII(stringLength, true));
            }

            return strings;
        }
    }
}
