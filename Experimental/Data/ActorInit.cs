//using System.IO;
//using mzxrules.Helper;

//namespace Experimental.Data
//{
//    static partial class Get_Obsolete
//    {
//        class ActorInit
//        {
//            public short number { get; private set; }
//            public byte type { get; private set; }
//            public sbyte room { get; private set; }

//            public int flags { get; private set; }
//            public short object_number { get; private set; }

//            public int instance_size { get; private set; }

//            public N64Ptr init_func;
//            public N64Ptr dest_func;
//            public N64Ptr update_func;
//            public N64Ptr draw_func;

//            public ActorInit() { }

//            public ActorInit(BinaryReader br)
//            {
//                /* 0x00 */
//                number = br.ReadBigInt16();
//                type = br.ReadByte();
//                room = br.ReadSByte();

//                /* 0x04 */
//                flags = br.ReadBigInt32();


//                object_number = br.ReadInt16();
//                br.ReadInt16();

//                instance_size = br.ReadBigInt32();

//                init_func = br.ReadBigInt32();
//                dest_func = br.ReadBigInt32();
//                update_func = br.ReadBigInt32();
//                draw_func = br.ReadBigInt32();
//            }
//        }

//    }
//}
