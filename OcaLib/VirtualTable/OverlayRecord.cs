using mzxrules.Helper;

namespace mzxrules.OcaLib
{
    public class OverlayRecord
    {
        public FileAddress VRom { get; set; }
        public N64PtrRange VRam { get; set; }
        public N64Ptr RamStart { get; protected set; }

        //Spectrum dependency, used by ActorOverlayRecord
        public N64PtrRange Ram
        {
            get
            {
                return (RamStart == 0) ? 
                    new N64PtrRange() : new N64PtrRange(RamStart, RamStart + VRam.Size);
            }
        }
    }
}
