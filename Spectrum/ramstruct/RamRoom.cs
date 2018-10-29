using mzxrules.Helper;
using System;

namespace Spectrum
{
    class RamRoom : IFile
    {
        static int Room_Alloc_Table { get { return SpectrumVariables.Room_Allocation_Table; } }
        public FileAddress Ram { get; }
        public FileAddress VRom { get; set; }

        public RamRoom(Ptr ptr)
        {
            Ram = new FileAddress(
                ptr.ReadInt32(0x00),
                ptr.ReadInt32(0x04));

            int RoomFile = ptr.ReadInt32(0x10);
            VRom = RamDmadata.GetFileAddress(RoomFile);
        }

        public static RamRoom GetRoomInfo()
        {
            Ptr ptr = SPtr.New(Room_Alloc_Table);
            return new RamRoom(ptr);
        }

        public override string ToString()
        {
            return $"ROOM {VRom.Start:X8}";
        }
    }
}
