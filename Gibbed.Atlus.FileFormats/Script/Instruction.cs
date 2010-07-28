using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.Atlus.FileFormats.Script
{
    public enum Instruction : ushort
    {
        BeginProcedure = 7,
        CallNative = 8,
        Return = 9,
        CallProcedure = 11,
        Jump = 13,
        Add = 14,
        JumpZero = 28,
        PushWord = 29,
    }
}
