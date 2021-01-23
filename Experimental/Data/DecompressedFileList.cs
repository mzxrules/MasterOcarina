using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experimental.Data
{
    static partial class Get
    {
        public static string DecompressedFileList(ORom romFile)
        {
            OFileTable ft;
            StringBuilder sb = new StringBuilder();

            ft = new OFileTable(romFile.FileLocation, romFile.Version);

            sb.AppendLine(romFile.Version.ToString());

            for (int i = 0; i < ft.Count; i++)
            {
                FileRecord record = ft[i];
                if (record.IsCompressed == false)
                {
                    sb.AppendLine($"{i:D4} {record.VRom.Start:X8}");
                }
            }
            return sb.ToString();
        }
    }
}
