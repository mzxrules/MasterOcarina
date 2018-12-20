using mzxrules.Helper;
using System.Collections.Generic;
using System.IO;

namespace mzxrules.OcaLib
{
    public class ActorOverlayRecord : OverlayRecord
    {
        #region Structure
        /* 00: [Vrom] File Start, End
         * 08: [Vram] File Start, End
         * 10: [Ram]  File location
         * 14: [Vram] Actor Info?
         * 18: [Ram]  File name location
         * 1C: [?]    Unknown
         */

        //Main data structure
        //public FileAddress VRom;
        //public FileAddress VRam;
        //N64Ptr RamStart;
        public N64Ptr VRamActorInfo;
        public N64Ptr RamFileName;
        public ushort AllocationType;
        public sbyte NumSpawned;
        #endregion

        #region Extended Fields

        public const int LENGTH = 0x20;


        public int Actor { get; protected set; }

        #endregion

        protected ActorOverlayRecord() { }

        public ActorOverlayRecord(int index, BinaryReader br)
        {
            Initialize(index, br);
        }

        /// <summary>
        /// Initializes the actor overlay record. Assumes data is stored in Big Endian form
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        public ActorOverlayRecord(int index, byte[] data)
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(data)))
                Initialize(index, br);
        }

        private void Initialize(int index, BinaryReader br)
        {
            Actor = index;
            VRom = new FileAddress(br.ReadBigUInt32(), br.ReadBigUInt32());
            VRam = new FileAddress(br.ReadBigUInt32(), br.ReadBigUInt32());

            RamStart = br.ReadBigUInt32();
            VRamActorInfo = br.ReadBigUInt32();
            RamFileName = br.ReadBigUInt32();
            AllocationType = br.ReadBigUInt16();
            NumSpawned = br.ReadSByte();
        }

        private void Initialize(int index, Ptr recordPtr)
        {
            Actor = index;
            VRom = new FileAddress(recordPtr.ReadUInt32(0x00), recordPtr.ReadUInt32(0x04));
            VRam = new FileAddress(recordPtr.ReadUInt32(0x08), recordPtr.ReadUInt32(0x0C));

            RamStart = recordPtr.ReadUInt32(0x10);
            VRamActorInfo = recordPtr.ReadUInt32(0x14);
            RamFileName = recordPtr.ReadUInt32(0x18);
            AllocationType = recordPtr.ReadUInt16(0x1C);
            NumSpawned = (sbyte)recordPtr.ReadByte(0x1E);
        }
    }
}