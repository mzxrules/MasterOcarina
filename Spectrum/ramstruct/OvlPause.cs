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

        public FileAddress Ram
        {
            get { return _RamAddress; }
        }
        int Item;
        FileAddress _RamAddress;
        public FileAddress VRom { get; set; }
        public FileAddress VRam { get; set; }
        uint RamFileName;

        public OvlPause(int i, byte[] data)
        {
            uint ramFileStart;
            Item = i;

            ramFileStart = BitConverter.ToUInt32(data, 0x0);

            VRom = new FileAddress(
                BitConverter.ToUInt32(data, 0x04),
                BitConverter.ToUInt32(data, 0x08));
            VRam = new FileAddress(
                BitConverter.ToUInt32(data, 0x0C),
                BitConverter.ToUInt32(data, 0x10));
            _RamAddress = new FileAddress(ramFileStart, ramFileStart + VRam.Size);

            RamFileName = BitConverter.ToUInt32(data, 0x18);

        }

        internal static List<OvlPause> GetActive()
        {
            List<OvlPause> ovlPause = new List<OvlPause>();
            OvlPause working;
            int ovlPauseCount;
            byte[] objTbl;
            byte[] ovlObjData = new byte[LENGTH];

            ovlPauseCount = COUNT;

            objTbl = Zpr.ReadRam(TABLE_ADDRESS, ovlPauseCount * LENGTH);
            objTbl.Reverse32();
            for (int i = 0; i < ovlPauseCount; i++)
            {
                Array.Copy(objTbl, i * LENGTH, ovlObjData, 0, LENGTH);
                working = new OvlPause(i, ovlObjData);
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
