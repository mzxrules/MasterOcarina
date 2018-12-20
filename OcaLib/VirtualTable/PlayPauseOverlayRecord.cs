using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mzxrules.OcaLib
{
    public class PlayPauseOverlayRecord : OverlayRecord
    {
        public const int LENGTH = 0x1C;
        
        int Item;
        uint RamFileName;

        public PlayPauseOverlayRecord(int index, BinaryReader br)
        {
            Item = index;

            RamStart = br.ReadBigUInt32();
            VRom = new FileAddress(br.ReadBigUInt32(), br.ReadBigUInt32());
            VRam = new FileAddress(br.ReadBigUInt32(), br.ReadBigUInt32());
            br.ReadBigUInt32();
            RamFileName = br.ReadBigUInt32();
        }

        //Spectrum interface
        //public PlayPauseOverlayRecord(int index, byte[] data)
        //{

        //}
    }
}
