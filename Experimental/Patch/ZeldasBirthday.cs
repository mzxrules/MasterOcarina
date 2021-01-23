using System.Linq;
using mzxrules.OcaLib;
using mzxrules.OcaLib.SceneRoom;
using System.IO;
using mzxrules.OcaLib.SceneRoom.Commands;
using System.Collections.Generic;

namespace Experimental
{
    class ZeldasBirthday
    {
        public static void Generate(IExperimentFace face, List<string> file)
        {
            ZeldasBirthday zb = new ZeldasBirthday();
            zb.Script(file[0]);
        }

        public void Script (string path)
        {
            ORom rom = new ORom(path, ORom.Build.DBGMQ);
            MemoryStream ms = new MemoryStream(new byte[0x4000000]);

            //clone rom's files by DMA
            foreach (var item in rom.Files)
            {
                var rfile = rom.Files.GetFile(item.VRom);
                CopyToOutputRom(ms, rfile);
            }

            //clone scene and room files

            for (int i = 0; i < rom.Scenes; i++)
            {
                var sceneFile = rom.Files.GetSceneFile(i);
                if (sceneFile == null)
                    continue;
                var sceneFileMeta = SceneRoomReader.InitializeScene(sceneFile, i);
                RemoveActor0001FromFile(ms, sceneFile, sceneFileMeta.Header);
                CopyToOutputRom(ms, sceneFile);

                foreach (var roomAddr in sceneFileMeta.Header.GetRoomAddresses())
                {
                    var roomFile = rom.Files.GetFile(roomAddr);
                    var roomFileMeta = SceneRoomReader.InitializeRoom(roomFile);
                    RemoveActor0001FromFile(ms, roomFile, roomFileMeta.Header);
                    CopyToOutputRom(ms, roomFile);
                }
            }

            //save
            using (Stream sw = new FileStream("ZeldosBirthday.z64", FileMode.Create))
            {
                ms.Position = 0;
                ms.CopyTo(sw);
            }
            //using (BinaryWriter bw = new BinaryWriter(new StreamWriter()))
        }

        private void RemoveActor0001FromFile(MemoryStream ms, RomFile sceneFile, SceneHeader header)
        {
            RemoveActor0001FromHeader(header, sceneFile);
            if (header.HasAlternateHeaders())
                foreach (var item in header.Alternate.Headers)
                {
                    if (item != null) 
                    RemoveActor0001FromHeader(item, sceneFile);
                }
        }

        private static void CopyToOutputRom(MemoryStream ms, RomFile file)
        {
            ms.Position = file.Record.VRom.Start;
            file.Stream.Position = 0;
            file.Stream.CopyTo(ms);
        }

        private void RemoveActor0001FromHeader(SceneHeader header, RomFile sceneFile)
        {
            var item = header[HeaderCommands.TransitionActorList];
            if (item != null)
                RemoveActor0001(sceneFile, item);
            item = header[HeaderCommands.ActorList];
            if (item != null)
                RemoveActor0001(sceneFile, item);
        }

        private static void RemoveActor0001(RomFile sceneFile, SceneCommand item)
        {
            BinaryWriter bw = new BinaryWriter(sceneFile.Stream);
            var addr = (IDataCommand)item;
            var actors = ((IActorList)item).GetActors();
            var reducedList = actors.Where(x => x.Actor != 1).ToList();

            bw.BaseStream.Position = item.OffsetFromFile + 1;
            bw.Write((byte)reducedList.Count);

            bw.BaseStream.Position = addr.SegmentAddress.Offset;

            foreach (var actor in reducedList)
            {
                actor.Serialize(bw);
            }
        }
    }
}
