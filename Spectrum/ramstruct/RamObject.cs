using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Collections.Generic;

namespace Spectrum
{
    class RamObject: IFile
    {
        public const int LENGTH = 0x44;

        public short Object { get; private set; }
        public bool IsLoaded { get; private set; }
        public N64PtrRange Ram { get; }

        public FileAddress VRom { get; set; }
        public RamObject(Ptr ptr)
        {
            Object = ptr.ReadInt16(0);
            IsLoaded = Object >= 0;
            Object = Math.Abs(Object);
            N64Ptr addr = ptr.ReadUInt32(4);
            VRom = (MemoryMapper.ObjectFiles.Length <= Object || Object < 0) ? new FileAddress(0, 0) : MemoryMapper.ObjectFiles[Object];
            Ram = new N64PtrRange(addr, addr + VRom.Size);
        }

        public override string ToString()
        {
            return $"OB {Object:X4}    {IsLoaded} {VRom.Start:X8}:{VRom.End:X8}";
        }
    }
}
