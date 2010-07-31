namespace Gibbed.Atlus.FileFormats.Script
{
    public class Opcode
    {
        public Instruction Instruction;
        public ushort Argument;

        public override string ToString()
        {
            if (this.Argument == 0)
            {
                return this.Instruction.ToString();
            }

            return string.Format("{0} ({1})",
                this.Instruction,
                this.Argument);
        }
    }
}
