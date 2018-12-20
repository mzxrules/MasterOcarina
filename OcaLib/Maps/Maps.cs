using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using mzxrules.Helper;

namespace mzxrules.OcaLib.Maps
{
    [DataContract]
    public class DungeonFloor
    {
        [DataMember]
        List<DungeonFloorIcon> Icons = new List<DungeonFloorIcon>(3);

        public DungeonFloor(BinaryReader br)
        {
            for (int i = 0; i < 3; i++)
            {
                Icons.Add(new DungeonFloorIcon(br));
            }
        }
    }

    [DataContract]
    public class DungeonMinimap
    {
        [DataMember]
        List<DungeonMinimapIcon> Icons = new List<DungeonMinimapIcon>(3);

        public DungeonMinimap(BinaryReader br)
        {
            for (int i = 0; i < 3; i++)
            {
                Icons.Add(new DungeonMinimapIcon(br));
            }
        }
    }

    [DataContract]
    public class DungeonFloorIcon
    {
        [DataMember(Order = 1)]
        /* 0x00 */
        public short Icon;

        //[DataMember(Order = 2)]
        ///* 0x04 */ public int unk_0x04;

        [DataMember(Order = 2)]
        /* 0x10 */
        public int Count;

        [DataMember(Order = 3)]
        /* 0x14 */
        public List<IconPoint> IconPoints = new List<IconPoint>();

        public DungeonFloorIcon(BinaryReader br)
        {
            var seek = br.BaseStream.Position + 0xA4;
            /* 0x00 */ Icon = br.ReadBigInt16();
            br.BaseStream.Position += 0x2 + 0xC;
            Count = br.ReadBigInt32();
            for (int i = 0; i < Count; i++)
            {
                IconPoints.Add(new IconPoint(br));
            }
            br.BaseStream.Position = seek;
        }
    }

    [DataContract]
    public class DungeonMinimapIcon
    {
        [DataMember(Order = 1)]
        public sbyte Icon;

        [DataMember(Order = 2)]
        public byte Count;

        [DataMember(Order = 3)]
        public List<IconPoint_Minimap> IconPoints = new List<IconPoint_Minimap>();

        public DungeonMinimapIcon(BinaryReader br)
        {
            var seek = br.BaseStream.Position + 0x26;
            Icon = br.ReadSByte();
            Count = br.ReadByte();
            for (int i = 0; i < Count; i++)
            {
                IconPoints.Add(new IconPoint_Minimap(br));
            }
            br.BaseStream.Position = seek;
        }
    }

    [DataContract]
    public class IconPoint
    {
        [DataMember(Order = 1)]
        public short Flag;

        [DataMember(Order = 2)]
        public float x;
        [DataMember(Order = 3)]
        public float y;

        public IconPoint(BinaryReader br)
        {
            Flag = br.ReadBigInt16();
            br.BaseStream.Position += 2;
            x = br.ReadBigFloat();
            y = br.ReadBigFloat();
        }
    }

    [DataContract]
    public class IconPoint_Minimap
    {
        [DataMember(Order = 1)]
        public sbyte Flag;

        [DataMember(Order = 2)]
        public byte x;
        [DataMember(Order = 3)]
        public byte y;

        public IconPoint_Minimap(BinaryReader br)
        {
            Flag = br.ReadSByte();
            x = br.ReadByte();
            y = br.ReadByte();
        }
    }
}
