using Gen;
using mzxrules.Helper;
using mzxrules.OcaLib;
using System.Collections.Generic;
using System.IO;

namespace Experimental
{
    static class DungeonRush2
    {
        public static void GenerateUncompressedRom(string filename)
        {
            List<int> keepscenes = KeepScenes();

            SceneImporter.ImportToUncompressedRom(filename, ORom.Build.N0, "dr2", keepscenes);
            CRC.Write(filename);
        }

        private static void DumpRooms(string filename)
        {
            List<int> keepscenes = KeepScenes();
            SceneImporter.DumpScenesAndRooms(new ORom(filename,  ORom.Build.MQP), "mqp", keepscenes);
        }

        private static List<int> KeepScenes()
        {
            List<int> keepscenes = new List<int>();

            for (int i = 0; i <= 26; i++)
            {
                if (i != 16)
                    keepscenes.Add(i);
            }

            keepscenes.AddRange(new int[] { 46, 59, 71, 79, 80, 81, 82, 95 });

            return keepscenes;
        }


        private static void TitleLogoHack()
        {
            BinaryReader br;
            byte[] ia4;
            using (FileStream fs = new FileStream("logo.rgba", FileMode.Open))
            {
                br = new BinaryReader(fs);

                ia4 = ImageHelper.ConvertRGBAToIA4(br);
            }
            using (FileStream fs = new FileStream("logo.ia4", FileMode.CreateNew))
            {
                fs.Write(ia4, 0, ia4.Length);
            }

            byte[] i4x9;
            using (FileStream fs = new FileStream("logomask.i4", FileMode.Open))
            {
                br = new BinaryReader(fs);

                i4x9 = ImageHelper.ConvertTitleMask(br);
            }

            using (FileStream fs = new FileStream("tiledlogomask.i4", FileMode.CreateNew))
            {
                fs.Write(i4x9, 0, i4x9.Length);
            }
        }
    }
}
