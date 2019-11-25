using mzxrules.Helper;
using System.IO;
using s16 = System.Int16;
using u16 = System.UInt16;
using s32 = System.Int32;
using u32 = System.UInt32;
using u8 = System.Byte;

namespace mzxrules.OcaLib.Actor
{
    public class ActorInit
    {
        public s16 number;
        public u8 type;
        public u8 room;
        public s32 flags; //unsigned long
        public s16 object_number;
        public u32 instance_size; //unsigned long
        public N64Ptr init_func; //unsigned 
        public N64Ptr dest_func; //unsigned long
        public N64Ptr update_func; //unsigned long
        public N64Ptr draw_func; //unsigned long

        public ActorInit() { }
        public ActorInit(BinaryReader br)
        {
            number = br.ReadBigInt16();
            type = br.ReadByte();
            room = br.ReadByte();
            flags = br.ReadBigInt32();
            object_number = br.ReadBigInt16();
            br.BaseStream.Position += 2;
            instance_size = br.ReadBigUInt32();
            init_func = br.ReadBigUInt32();
            dest_func = br.ReadBigUInt32();
            update_func = br.ReadBigUInt32();
            draw_func = br.ReadBigUInt32();
        }
        public override string ToString()
        {
            return $"{number:X4},{type:X2},{room:X2}," +
                $"{flags:X8},{object_number:X4},{instance_size:X8}," +
                $"{init_func},{dest_func},{update_func},{draw_func}";
        }
    }

}
