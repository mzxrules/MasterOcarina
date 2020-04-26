using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mzxrules.Helper;

namespace Spectrum
{
    class RoomCtx
    {
        public class Room
        {
            public sbyte Num;
            byte unk_01;
            byte unk_02;
            byte unk_03;
            sbyte Echo;
            byte ShowInvisActors;
            public N64Ptr mesh;
            public N64Ptr segment;
            int unk_10;

            public Room(Ptr p)
            {
                Num = p.ReadSByte(0x00);
                unk_01 = p.ReadByte(0x01);
                unk_02 = p.ReadByte(0x02);
                unk_03 = p.ReadByte(0x03);
                Echo = p.ReadSByte(0x04);
                ShowInvisActors = p.ReadByte(0x05);
                mesh = p.ReadInt32(0x08);
                segment = p.ReadInt32(0x0C);
                unk_10 = p.ReadInt32(0x10);
            }
            public override string ToString()
            {
                return $"ROOM {Num}: {unk_01:X2} {unk_02:X2} {unk_03:X2} MESH: {mesh} SEGMENT: {segment} unk_10: {unk_10:X8}";
            }
        }

        public Room CurRoom;
        public Room PrevRoom;

        public RoomCtx(Ptr p)
        {
            CurRoom = new Room(p);
            PrevRoom = new Room(p.RelOff(0x14));
        }
    }
}
