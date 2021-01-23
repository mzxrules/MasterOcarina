using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using mzxrules.OcaLib;

namespace Experimental.Data
{
    static partial class Get
    {
        public static void DmaFile(ORom r)
        {
            using (StreamWriter fs = new StreamWriter($"{r.Version}-dmadata.txt"))
            {
                for (int i = 0; i < r.Files.Count; i++)
                {
                    FileRecord f = r.Files[i];
                    fs.WriteLine(string.Format("{0},{1},{2:X8},{3:X8},{2},{3},{4},{5}",
                        i,
                        r.Version.ToString(),
                        f.VRom.Start,
                        f.VRom.End,
                        f.Rom.Start,
                        f.Rom.End));
                }
            }
        }
    }
}
