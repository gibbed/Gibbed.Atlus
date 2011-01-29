using System;
using System.Runtime.InteropServices;
using Gibbed.Helpers;

namespace Gibbed.Atlus.FileFormats.BinaryScript
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FileBlockInfo
    {
        public FileBlockType Type;
        public uint ElementSize;
        public uint ElementCount;
        public uint Offset;

        public void Swap()
        {
            this.Type = (FileBlockType)(((uint)this.Type).Swap());
            this.ElementSize = this.ElementSize.Swap();
            this.ElementCount = this.ElementCount.Swap();
            this.Offset = this.Offset.Swap();
        }
    }
}
