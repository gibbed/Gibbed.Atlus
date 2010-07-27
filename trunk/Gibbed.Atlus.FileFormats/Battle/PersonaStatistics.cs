using System.IO;
using Gibbed.Helpers;

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
