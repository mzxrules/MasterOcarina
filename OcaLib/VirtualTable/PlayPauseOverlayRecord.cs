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
        
        public int Item;
        public int Unk_0x14;
        public N64Ptr RamFileName;

        public PlayPauseOverlayRecord(PlayPauseOverlayRecord a)
        {
            Item = a.Item;

            RamStart = a.RamStart;
            VRom = a.VRom;
            VRam = a.VRam;
            Unk_0x14 = a.Unk_0x14;
            RamFileName = a.RamFileName;
        }

        public PlayPauseOverlayRecord(int index, BinaryReader br)
        {
            Initialize(index, br);
        }

        public PlayPauseOverlayRecord(int index, byte[] data)
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(data)))
                Initialize(index, br);
        }

        private void Initialize(int index, BinaryReader br)
        {
            Item = index;

            RamStart = br.ReadBigUInt32();
            VRom = new FileAddress(br.ReadBigUInt32(), br.ReadBigUInt32());
            VRam = new N64PtrRange(br.ReadBigUInt32(), br.ReadBigUInt32());
            Unk_0x14 = br.ReadBigInt32();
            RamFileName = br.ReadBigUInt32();
        }
    }
}
