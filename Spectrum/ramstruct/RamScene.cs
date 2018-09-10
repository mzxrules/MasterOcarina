using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mzxrules.Helper;

namespace Spectrum
{
    class RamScene : IFile
    {
        public FileAddress VRom { get; set; }

        public FileAddress Ram { get; set; }

        public static RamScene GetSceneInfo(Ptr globalCtx)
        {
            RamScene result = new RamScene();
            var sceneRecord = globalCtx.Deref(0x1242C);
            result.VRom = new FileAddress(sceneRecord.ReadInt32(0), sceneRecord.ReadInt32(4));
            N64Ptr ramStart = globalCtx.ReadInt32(0xB0);
            N64Ptr ramEnd = ramStart + result.VRom.Size;
            result.Ram = new FileAddress(ramStart, ramEnd);

            return result;
        }

        public override string ToString()
        {
            return string.Format("SCENE {0:X8}", VRom.Start);
        }
    }
}
