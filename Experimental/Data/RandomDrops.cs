using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experimental.Data
{
    partial class Get
    {
        class RandomDropTable
        {
            class DropRecord
            {
                public byte Id;
                public string Name;
                public byte NumberSpawned = 0;
                public string Key
                {
                    get
                    {
                        if (Id == 0xFF)
                            return String.Format("{0:X2}{1:X2}", Id, 1);
                        return String.Format("{0:X2}{1:X2}", Id, NumberSpawned);
                    }
                }
                public string QualifiedName
                {
                    get
                    {
                        if (Id == 0xFF)
                            return Name;
                        else if (NumberSpawned == 1)
                            return Name;
                        else return string.Format("{0} x{1}", Name, NumberSpawned);
                    }
                }

                public DropRecord() { }

                public DropRecord(byte id, string name, byte spawns)
                {
                    Id = id;
                    Name = name;
                    NumberSpawned = spawns;
                }
            }

            DropRecord[] DroppedItemTable = new DropRecord[]{
                new DropRecord { Id = 0x00, Name = "Green Rupee" },
                new DropRecord { Id = 0x01, Name = "Blue Rupee" },
                new DropRecord { Id = 0x02, Name = "Red Rupee" },
                new DropRecord { Id = 0x03, Name = "Heart" },
                new DropRecord { Id = 0x04, Name = "Bomb" },
                new DropRecord { Id = 0x05, Name = "Arrow (1) " },
                new DropRecord { Id = 0x06, Name = "Heart Piece" },
                new DropRecord { Id = 0x07, Name = "Heart Container (Alpha!)" },
                new DropRecord { Id = 0x08, Name = "Seeds or Arrows (5)" },
                new DropRecord { Id = 0x09, Name = "Seeds (5) or Arrows (10)" },
                new DropRecord { Id = 0x0A, Name = "Seeds (5) or Arrows (30)" },
                new DropRecord { Id = 0x0B, Name = "Bomb (5)" },
                new DropRecord { Id = 0x0C, Name = "Deku Nut" },
                new DropRecord { Id = 0x0D, Name = "Deku Stick" },
                new DropRecord { Id = 0x0E, Name = "Magic Jar (Large)" },
                new DropRecord { Id = 0x0F, Name = "Magic Jar (Small)" },
                new DropRecord { Id = 0x10, Name = "Seeds or Arrows (5)" },
                new DropRecord { Id = 0x11, Name = "Small Key" },
                new DropRecord { Id = 0x12, Name = "Flexible Drop" },
                new DropRecord { Id = 0x13, Name = "Giant orange rupee" },
                new DropRecord { Id = 0x14, Name = "Large purple rupee" },
                new DropRecord { Id = 0x15, Name = "Deku Shield" },
                new DropRecord { Id = 0x16, Name = "Hylian Shield" },
                new DropRecord { Id = 0x17, Name = "Zora Tunic" },
                new DropRecord { Id = 0x18, Name = "Goron Tunic" },
                new DropRecord { Id = 0x19, Name = "Bomb (5)" },
                new DropRecord { Id = 0xFF, Name = "Nothing" }};


            List<DropRecord> DroppedItems = new List<DropRecord>();
            public RandomDropTable(BinaryReader br)
            {
                byte[] DropItem;
                byte[] DropOccurance;
                DropItem = br.ReadBytes(0x10);

                long returnPos = br.BaseStream.Position;
                br.BaseStream.Position += ((15 - 1) * 0x10);

                DropOccurance = br.ReadBytes(0x10);
                br.BaseStream.Position = returnPos;

                for (int i = 0; i < 0x10; i++)
                {
                    var id = DropItem[i];
                    DroppedItems.Add(new DropRecord(id, DroppedItemTable.Single(x=> x.Id == id).Name, DropOccurance[i]));
                }
            }
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                //for (int i = 0; i < 16; i++)
                {
                //    sb.AppendFormat("{0:X2},{1},", DroppedItems[i].Id, DroppedItems[i].NumberSpawned);
                }
                sb.Append(GetRngResult());
                return sb.ToString();
            }

            private string GetRngResult()
            {
                string result = "";

                var query = from r in DroppedItems
                            group r by r.QualifiedName into g
                            orderby g.Key
                            select new { Count = g.Count(), Value = g.Key };

                //var query = from r in DropItem
                //            group r by r into g
                //            orderby g.Key
                //            select new { Count = g.Count(), Value = g.Key };
                            

                foreach (var item in query)
                {
                    result += string.Format("{0} ({1}/16)\t", item.Value, item.Count);
                }
                return result;
            }
        }
        public static void RandomDrops(IExperimentFace face, List<string> file)
        {
            ORom rom = new ORom(file[0], ORom.Build.N0);
            StringBuilder sb = new StringBuilder();

            var codeFile = rom.Files.GetFile(new RomFileToken(ORom.FileList.code));
            long dropTableAddr = codeFile.Record.GetRelativeAddress(0xB5D764);

            BinaryReader br = new BinaryReader(codeFile);
            br.BaseStream.Position = dropTableAddr;

            for (int i = 0; i < 15; i++)
            {
                sb.AppendLine(new RandomDropTable(br).ToString());
            }

            face.OutputText(sb.ToString());
        }
    }
}
