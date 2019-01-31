using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mzxrules.OcaLib
{
    class MapMarkDataOverlayRecord : OverlayRecord
    {
        public N64Ptr DungeonMarkData;

        public MapMarkDataOverlayRecord(BinaryReader br)
        {
            Initialize(br);
        }

        /// <summary>
        /// Initializes the actor overlay record. Assumes data is stored in Big Endian form
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        public MapMarkDataOverlayRecord(byte[] data)
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(data)))
                Initialize(br);
        }

        private void Initialize(BinaryReader br)
        {
            RamStart = br.ReadBigUInt32();
            VRom = new FileAddress(br.ReadBigUInt32(), br.ReadBigUInt32());
            VRam = new N64PtrRange(br.ReadBigUInt32(), br.ReadBigUInt32());

            DungeonMarkData = br.ReadBigInt32();
        }
    }
}
