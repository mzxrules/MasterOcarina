using mzxrules.OcaLib;
using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experimental
{
    static class DRMQ_PauseChest
    {
        public static void Generate(IExperimentFace face, List<string> file)
        {
            byte[] buffer;
            MemoryStream ms;

            const int RecordLength = 0x1EC;
            const int RecordCount = 0x4158 / 0x1EC;

            ORom rom = new ORom(file[0], ORom.Build.MQU);
            var addr = rom.Files.GetPlayPauseAddress(0);
            var kaleido_scope = rom.Files.GetFile(addr);
            var kalBr = new BinaryReader(kaleido_scope);
            BinaryWriter newBw; 

            using (FileStream fs = new FileStream(file[1], FileMode.Open))
            {
                buffer = new byte[fs.Length];
                ms = new MemoryStream(buffer);
                fs.CopyTo(ms);
            }
            newBw = new BinaryWriter(ms);

            ms.Position = 0x016C20;
            kaleido_scope.Stream.Position = kaleido_scope.Record.GetRelativeAddress(0xBB49E0);

            for (int i = 0; i < RecordCount; i++)
            {
                for (int j = 0; j < RecordLength; j += 4)
                {
                    var word = kalBr.ReadBigInt32();
                    if (j == 0x8)
                    {
                        newBw.BaseStream.Position += 4;
                        continue;
                    }
                    else
                        newBw.WriteBig(word);
                }
            }
            using (FileStream fs = new FileStream("new_ovl_kaleido_scope", FileMode.Create))
            {
                newBw.Seek(0, SeekOrigin.Begin);
                newBw.BaseStream.CopyTo(fs);
            }
        }
    }
}
