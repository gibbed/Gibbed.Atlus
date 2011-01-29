using System;
using System.Runtime.InteropServices;
using Gibbed.Helpers;

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
