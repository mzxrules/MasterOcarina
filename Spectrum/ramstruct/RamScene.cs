using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mzxrules.Helper;
using mzxrules.OcaLib;

namespace Spectrum
{
    class RamScene : IFile
    {
        public FileAddress VRom { get; set; }

        public FileAddress Ram { get; set; }

        public int Id { get; set; }

        public static RamScene GetSceneInfo(RomVersion ver, Ptr globalCtx, Ptr sceneTable)
        {
            short sceneId = globalCtx.ReadInt16(0xA4);
            N64Ptr ramStart = globalCtx.ReadInt32(0xB0);
            int recordSize = (ver.Game == Game.OcarinaOfTime) ? 0x14 : 0x10;
            Ptr sceneRecord = sceneTable.RelOff(recordSize * sceneId);
            FileAddress vrom = new FileAddress(sceneRecord.ReadInt32(0), sceneRecord.ReadInt32(4));
            N64Ptr ramEnd = ramStart + vrom.Size;

            RamScene result = new RamScene
            {
                VRom = vrom,
                Ram = new FileAddress(ramStart, ramEnd),
                Id = sceneId
            };

            return result;
        }

        public override string ToString()
        {
            return $"SCENE {Id:X2} {VRom.Start:X8}";
        }
    }
}
