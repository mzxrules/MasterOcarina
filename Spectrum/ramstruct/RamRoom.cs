using mzxrules.Helper;
using System;

namespace Spectrum
{
    class RamRoom : IFile
    {
        static int Room_Alloc_Table { get { return SpectrumVariables.Room_Allocation_Table; } }
        public FileAddress Ram { get { return _RamAddress; } }
        public FileAddress VRom { get; set; }
        FileAddress _RamAddress;

        public RamRoom(byte[] data)
        {
            _RamAddress = new FileAddress(
                BitConverter.ToInt32(data, 0),
                BitConverter.ToInt32(data, 4));

            int RoomFile = BitConverter.ToInt32(data, 0x10);
            VRom = RamDmadata.Data.GetFileAddress(RoomFile);
        }

        public static RamRoom GetRoomInfo()
        {
            RamRoom result;
            byte[] data;

            data = Zpr.ReadRam(Room_Alloc_Table, 0x1C);
            data.Reverse32();
            result = new RamRoom(data);

            return result;
        }

        public override string ToString()
        {
            return $"ROOM {VRom.Start:X8}";
        }
    }
}
