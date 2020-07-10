using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mzxrules.OcaLib
{
    public class TransitionOverlayRecord : OverlayRecord
    {
        /* 0x00 */ //N64Ptr RamStart;
        /* 0x04 */ //FileAddress VRam;
        /* 0x0C */ //FileAddress VRom;
        /* 0x14 */ public N64Ptr unk;
        /* 0x18 */ public int AllocateSize;

        public int Index;

        public const int LENGTH = 0x1C;


        public TransitionOverlayRecord(int index, BinaryReader br)
        {
            Initialize(index, br);
        }
        public TransitionOverlayRecord(TransitionOverlayRecord a)
        {
            Index = a.Index;
            RamStart = a.RamStart;
            VRam = a.VRam;
            VRom = a.VRom;
            unk = a.unk;
            AllocateSize = a.AllocateSize;
        }

        private void Initialize(int index, BinaryReader br)
        {
            Index = index;
            RamStart = br.ReadBigUInt32();
            VRam = new N64PtrRange(br.ReadBigUInt32(), br.ReadBigUInt32());
            VRom = new FileAddress(br.ReadBigUInt32(), br.ReadBigUInt32());
            unk = br.ReadBigUInt32();
            AllocateSize = br.ReadBigInt32();
        }

        /// <summary>
        /// Initializes the transition overlay record. Assumes data is stored in Big Endian form
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        public TransitionOverlayRecord(int index, byte[] data)
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(data)))
                Initialize(index, br);
        }
    }
}
