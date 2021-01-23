using mzxrules.OcaLib;
using mzxrules.OcaLib.Actor;
using System;
using System.IO;
using System.Linq;
using System.Text;
using mzxrules.Helper;
using OcaBase;
using System.Collections.Generic;

namespace Experimental.Data
{
    static partial class Get
    {
        public static string ActorTable(Rom r, bool dumpActorInit = true)
        {
            StringBuilder sb = new StringBuilder();

            RomFileToken codeFileToken = RomFileToken.Select(r.Version, ORom.FileList.code, MRom.FileList.code);

            RomFile codeFile = r.Files.GetFile(codeFileToken);
            BinaryReader codeFileReader = new BinaryReader(codeFile);

            Addresser.TryGetRam(codeFileToken, r.Version, out N64Ptr codePtr);

            List<ActorOverlayRecord> records = new List<ActorOverlayRecord>();

            for (int i = 0; i < r.Files.Tables.Actors.Records; i++)
            {
                records.Add(r.Files.GetActorOverlayRecord(i));   
            }
            string header = $"Id,VROM Start,VROM End,VROM Size,VRAM Start,VRAM End,VRAM Size,Actor Init,Alloc";
            if (dumpActorInit)
            {
                header += 
                    ",number,type,room,flags,object_number,instance_size," +
                    "init_func,dest_func,update_func,draw_func";
            }
            sb.AppendLine(header);
            foreach (var item in records)
            {
                string entry = $"{item.Actor:X4}," +
                    $"{item.VRom.Start:X8},{item.VRom.End:X8},{item.VRom.Size:X8}," +
                    $"{item.VRam.Start:X8},{item.VRam.End:X8},{item.VRam.Size:X8}," +
                    $"{item.VRamActorInit}," +
                    $"{item.AllocationType}";

                sb.Append(entry);
                if (dumpActorInit)
                {
                    ActorInit stats;
                    if (item.VRamActorInit.IsNull())
                    {
                        stats = new ActorInit();
                    }
                    else
                    {
                        BinaryReader reader;
                        N64Ptr basePtr;
                        if (item.VRamActorInit < 0x8080_0000)
                        {
                            basePtr = codePtr;
                            reader = codeFileReader;
                        }
                        else
                        {
                            basePtr = item.VRam.Start;
                            var fileRecord = r.Files.GetFileByTable(TableInfo.Type.Actors, item.Actor);
                            reader = new BinaryReader(r.Files.GetFile(fileRecord));
                        }
                        reader.BaseStream.Position = item.VRamActorInit - basePtr;
                        stats = new ActorInit(reader);
                    }
                    sb.Append($",{stats}");
                }
                sb.AppendLine();
            }
            codeFileReader.Dispose();
            return sb.ToString();
        }

    }
}
