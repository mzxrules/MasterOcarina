using mzxrules.Helper;
using System;
using System.Collections.Generic;

namespace Spectrum
{
    class OvlPause : IFile, IVRamItem
    {
        public static int TABLE_ADDRESS { get { return SpectrumVariables.Player_Pause_Ovl_Table; } }
        public const int LENGTH = 4 * 7;
        public const int COUNT = 2;

        public N64PtrRange Ram { get; }
        int Item;

        public FileAddress VRom { get; set; }
        public N64PtrRange VRam { get; set; }
        uint RamFileName;

        public OvlPause(int i, Ptr ptr)
        {
            uint ramFileStart;
            Item = i;

            ramFileStart = ptr.ReadUInt32(0x00);

            VRom = new FileAddress(
                ptr.ReadUInt32(0x04),
                ptr.ReadUInt32(0x08));
            VRam = new N64PtrRange(
                ptr.ReadUInt32(0x0C),
                ptr.ReadUInt32(0x10));
            Ram = new N64PtrRange(ramFileStart, ramFileStart + VRam.Size);

            RamFileName = ptr.ReadUInt32(0x18);

        }

        internal static List<OvlPause> GetActive()
        {
            List<OvlPause> ovlPause = new List<OvlPause>();
            Ptr ovlPausePtr = SPtr.New(TABLE_ADDRESS);
            
            for (int i = 0; i < COUNT; i++)
            {
                Ptr ptr = ovlPausePtr.RelOff(LENGTH * i);
                OvlPause working = new OvlPause(i, ptr);
                if (working.Ram.Start != 0)
                    ovlPause.Add(working);
            }
            return ovlPause;
        }

        public override string ToString()
        {
            return $"OVL PAU:  {Item:X2}";
        }
    }
}
