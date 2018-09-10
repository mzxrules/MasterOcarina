using mzxrules.Helper;
using System;

namespace Spectrum
{
    class RamDmadata :  IFile
    {
        public static RamDmadata Data;
        public FileAddress Ram { get; set; } 
        public FileAddress VRom { get; set; }

        public RamDmadata()
        {
            var start = SpectrumVariables.Dma_Data_Addr;
            VRom = new FileAddress(Zpr.ReadRamInt32(start + 0x20), Zpr.ReadRamInt32(start + 0x24));
            Ram = new FileAddress(start, start + VRom.Size);

            Data = this;
        }

        public FileAddress GetFileAddress(int addr)
        {
            if (VRom.Size < 0 || VRom.Size > 0x400000)
                throw new InvalidOperationException("DMA data is not initialized correctly");
            for (int i = 0; i < VRom.Size; i += 0x10)
            {
                int start = Zpr.ReadRamInt32((int)Ram.Start + i);
                if (addr == start)
                    return new FileAddress(Zpr.ReadRamInt32((int)Ram.Start + i), Zpr.ReadRamInt32((int)Ram.Start + i + 4));
            }
            return new FileAddress();
        }

        public override string ToString()
        {
            return "dmadata";
        }
    }
}
