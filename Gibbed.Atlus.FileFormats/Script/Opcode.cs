using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats.Script
{
    public class Opcode
    {
        public Instruction Instruction;
        public ushort Argument;
        public string ToCode(bool littleEndian)
        {
            if (littleEndian == true)
            {
                return string.Format("{0:X4}{1:X4}",
                    (ushort)this.Instruction,
                    this.Argument);
            }
            else
            {
                return string.Format("{0:X4}{1:X4}",
                    ((ushort)this.Instruction).Swap(),
                    this.Argument.Swap());
            }
        }

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
