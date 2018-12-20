using mzxrules.Helper;
using System;
using System.Collections.Generic;

namespace mzxrules.OcaLib
{
    public class OverlayRecord
    {
        public FileAddress VRom { get; set; }
        public FileAddress VRam { get; set; }
        public N64Ptr RamStart { get; protected set; }

        //Spectrum dependency, used by ActorOverlayRecord
        public FileAddress Ram
        {
            get
            {
                return (RamStart == 0) ? 
                    new FileAddress() : new FileAddress(RamStart, RamStart + VRam.Size);
            }
        }
    }
}
