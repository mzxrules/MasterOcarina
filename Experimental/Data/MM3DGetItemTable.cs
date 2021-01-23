using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experimental.Data
{
    static partial class Get
    {
        public static void MM3DGetItemTable(IExperimentFace face, List<string> file)
        {
            const int Get_Item_Table = 0x68F220;
            const int readStartIndex = -255;
            const int baseIndex = 1;

            List<MM3d.GetItemRecord> GetItems = new List<MM3d.GetItemRecord>();
            

            using (BinaryReader br = new BinaryReader(new FileStream(file[0], FileMode.Open, FileAccess.Read)))
            {
                br.BaseStream.Position = MM3d.RAM.GetFileRelAddr(Get_Item_Table) + (8 * (readStartIndex - baseIndex));

                for (int i = readStartIndex; i < 255; i++)
                {
                    GetItems.Add(new MM3d.GetItemRecord((short)i, br));
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (MM3d.GetItemRecord record in GetItems)
                sb.AppendLine(record.ToString());

            face.OutputText(sb.ToString());
        }
    }
}
