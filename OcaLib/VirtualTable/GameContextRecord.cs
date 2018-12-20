using mzxrules.Helper;
using System.IO;

namespace mzxrules.OcaLib
{
    public class GameStateRecord : OverlayRecord
    {
        public const int LENGTH = 0x30;
        /* 0x00 */ //N64Ptr RamStart;
        /* 0x04 */ //FileRecord VRom; //if applicable
        /* 0x0C */ //FileRecord VRam;   //if applicable
        /* 0x14 */ uint unknown2;
        /* 0x18 */ N64Ptr InitFunc; 
        /* 0x1C */ N64Ptr DestFunc; 

        /* 0x20-0x2C */ //unknown

        /* 0x2C */ int AllocateSize; //Think it's size of initialized instance of the overlay

        static string[] Files = new string[]
        {
            "N/A",
            "ovl_select",
            "ovl_title",
            "N/A",
            "ovl_opening",
            "ovl_file_choose"
        };
        public GameStateRecord()
        { }

        public GameStateRecord (int index, BinaryReader br)
        {
            Initialize(index, br);
        }

        public GameStateRecord(int index, byte[] data)
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(data)))
                Initialize(index, br);
        }

        private void Initialize(int index, BinaryReader br)
        {
            RamStart = br.ReadBigUInt32();
            VRom = new FileAddress(br.ReadBigUInt32(), br.ReadBigUInt32());
            VRam = new FileAddress(br.ReadBigUInt32(), br.ReadBigUInt32());
            unknown2 = br.ReadBigUInt32();
            InitFunc = br.ReadBigUInt32();
            DestFunc = br.ReadBigUInt32();
            br.BaseStream.Position += 0xC;
            AllocateSize = br.ReadBigInt32();
        }
    }
}
