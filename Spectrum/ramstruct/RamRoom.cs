using mzxrules.Helper;
using System;
using System.Collections.Generic;

namespace Spectrum
{
    class RamRoom // : IFile
    {
        //static int Room_Alloc_Table { get { return SpectrumVariables.Room_Allocation_Table; } }
        //public N64PtrRange Ram { get; }
        //public FileAddress VRom { get; set; }

        //public RamRoom(Ptr ptr)
        //{
        //    Ram = new N64PtrRange(
        //        ptr.ReadInt32(0x00),
        //        ptr.ReadInt32(0x04));

        //    int RoomFile = ptr.ReadInt32(0x10);
        //    VRom = RamDmadata.GetFileAddress(RoomFile);
        //}

        //public static RamRoom GetRoomInfo()
        //{
        //    Ptr ptr = SPtr.New(Room_Alloc_Table);
        //    return new RamRoom(ptr);
        //}

        public static List<SimpleFile> GetRoomInfo()
        {
            RoomCtx roomCtx = new RoomCtx(SpectrumVariables.Room_Context);
            var roomList = SpectrumVariables.Room_List_Ptr;

            List<SimpleFile> files = new List<SimpleFile>();
           
            foreach (var item in new RoomCtx.Room[] { roomCtx.CurRoom, roomCtx.PrevRoom })
            {
                if (item.Num == -1)
                    continue;

                if (roomList == 0)
                    continue;

                FileAddress vrom = RamDmadata.GetFileAddress(roomList.RelOff(item.Num * 8).ReadInt32(0));
                N64PtrRange ram = new N64PtrRange(item.segment, item.segment + vrom.Size);

                SimpleFile file = new SimpleFile()
                {
                    VRom = vrom,
                    Ram = ram,
                    Description = $"ROOM {item.Num}: {vrom}"
                };
                files.Add(file);
            }
            return files;
        }
    }
}
