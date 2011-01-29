using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.Atlus.FileFormats.BinaryScript
{
    internal enum FileBlockType : uint
    {
        Procedures = 0,
        Labels = 1,
        Opcodes = 2,
        Messages = 3,
        Unknown4 = 4,
    }
}
