using mzxrules.Helper;
using System;

namespace Spectrum
{
    class RamDmadata :  IFile
    {
        private static RamDmadata Data;
        public FileAddress Ram { get; set; } 
        public FileAddress VRom { get; set; }

        public RamDmadata()
        {
            Ptr ptr = SpectrumVariables.Dmadata_Addr;
            VRom = new FileAddress(ptr.ReadInt32(0x20), ptr.ReadInt32(0x24));
            Ram = new FileAddress(ptr, ptr + VRom.Size);

            Data = this;
        }

        public static FileAddress GetFileAddress(int addr)
        {
            if (Data.VRom.Size < 0 || Data.VRom.Size > 0x40000)
                throw new InvalidOperationException("dmadata reference is not initialized correctly");

            Ptr ptr = SPtr.New(Data.Ram.Start);
            for (int i = 0; i < Data.VRom.Size; i += 0x10)
            {
                int start = ptr.ReadInt32(i);
                if (addr == start)
                    return new FileAddress(start, ptr.ReadInt32(i + 0x04));
            }
            return new FileAddress();
        }

        public override string ToString()
        {
            return "dmadata";
        }
    }
}
