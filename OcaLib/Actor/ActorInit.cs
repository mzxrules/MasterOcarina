using mzxrules.Helper;
using System.IO;
using u16 = System.UInt16;
using u32 = System.UInt32;
using u8 = System.Byte;

namespace mzxrules.OcaLib.Actor
{
    public class ActorInit
    {
        public u16 number;
        public u8 type;
        public u8 status;
        public u32 unknown_0; //unsigned long
        public u16 object_number;
        //public u16 unknown_1;
        public u32 instance_size; //unsigned long
        public u32 init_func; //unsigned 
        public u32 dest_func; //unsigned long
        public u32 update_func; //unsigned long
        public u32 draw_func; //unsigned long

        public ActorInit() { }
        public ActorInit(BinaryReader br)
        {
            number = br.ReadBigUInt16();
            type = br.ReadByte();
            status = br.ReadByte();
            unknown_0 = br.ReadBigUInt32();
            object_number = br.ReadBigUInt16();
            br.BaseStream.Position += 2;
            instance_size = br.ReadBigUInt32();
            init_func = br.ReadBigUInt32();
            dest_func = br.ReadBigUInt32();
            update_func = br.ReadBigUInt32();
            draw_func = br.ReadBigUInt32();
            
        }
    }

}
