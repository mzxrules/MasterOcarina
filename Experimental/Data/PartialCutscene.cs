//using OcaBase;
using mzxrules.OcaLib;
using mzxrules.OcaLib.Cutscenes;
using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Experimental.Data
{
    static class PartialCutscene
    {
        const int N0_EndAddr = 0x384980;
        static string CUTSCENE_OFFSET_DATA_LOC = "Data/PartialCutscenes.txt";
        //structure to grab the incoming cutscene pointer data
        class InCutsceneData
        {
            public delegate bool Get_Scene(int key, out _Scene a);
            public static Get_Scene GetSceneDelegate;
            public int scene;
            public int setup;
            public int offset;
            public int RamStart { get { return GetRamAddress(); } }

            private int GetRamAddress()
            {
                if (GetSceneDelegate != null &&
                    GetSceneDelegate(scene, out _Scene s))
                {
                    return s.RamStart + offset;
                }
                return 0;
            }
            public InCutsceneData(int scene, int setup, int offset)
            {
                this.scene = scene;
                this.setup = setup;
                this.offset = offset;
            }
        }

        class ResultData
        {
            public InCutsceneData inData { get; set; }
            public _Scene scene { get; set; }
            public ResultType result { get; set; }
            public int cutsceneRomAddress = 0;
            public enum ResultType
            {
                Exception,
                Error_CutsceneParserLimitReached,
                Limit_UnallocatedSpace,
                Success_InvalidCutscene,
                Success_ValidCutscene
            }

            public ResultData(_Scene s, InCutsceneData inData)
            {
                this.scene = s;
                this.inData = inData;
            }
        }

        //Structure for storing a subset of scene data
        class _Scene
        {
            public int Id { get; set; }
            public FileAddress Address { get; set; }
            public int RamStart { get { return N0_EndAddr - Address.Size; } }

            public bool TryConvertToRom(int ramAddr, out int romAddr)
            {
                romAddr = 0;

                if (ramAddr < RamStart)
                    return false;

                romAddr = ramAddr - RamStart + Address.Start;
                return true;
            }
        }

        public static void GetCutsceneExitAsm(IExperimentFace face, List<string> file)
        {
            List<InCutsceneData> cutsceneData;
            Dictionary<int, _Scene> sceneInfo;
            StringBuilder sb = new();
            ORom rom;
            string romLoc = file[0];

            rom = new ORom(romLoc, ORom.Build.N0);

            //get list of cutscene bank offsets
            cutsceneData = GetCutsceneOffsetData();

            //get stats on the scenes within the input list
            sceneInfo = GetSceneInfo(rom);

            InCutsceneData.GetSceneDelegate = sceneInfo.TryGetValue;

            foreach (InCutsceneData item in cutsceneData)
            {
                Cutscene cs;
                DestinationCommand exit;
                RomFile sceneFile;
                sceneFile = rom.Files.GetSceneFile(item.scene);
                sceneFile.Stream.Position += item.offset & 0xFFFFFF;
                cs = new Cutscene(sceneFile);

                exit = cs.Commands.OfType<DestinationCommand>().SingleOrDefault();

                if (exit != null)
                {
                    sb.AppendFormat("{0:D3}\t{1:D2}\t{2:X2}", item.scene, item.setup, exit.Action);
                    sb.AppendLine();
                }
            }
            face.OutputText(sb.ToString());
        }

        public static string GetPartialCutscene()
        {
            List<InCutsceneData> cutsceneData;
            Dictionary<int, _Scene> sceneInfo;
            StringBuilder sb = new();
            ORom rom;
            string romLoc = string.Empty;

            if (!VerboseOcarina.RomLocation.TryGetRomLocation(ORom.Build.N0, ref romLoc))
                throw new NotImplementedException();

            rom = new ORom(romLoc, ORom.Build.N0);

            //get list of cutscene bank offsets
            cutsceneData = GetCutsceneOffsetData();

            //get stats on the scenes within the input list
            sceneInfo = GetSceneInfo(rom);

            InCutsceneData.GetSceneDelegate = sceneInfo.TryGetValue;

            for (int sceneId = 0; sceneId < 101; sceneId++)
            {
                RomFile file;
                sceneInfo.TryGetValue(sceneId, out _Scene scene);
                file = rom.Files.GetSceneFile(sceneId);

                foreach (InCutsceneData inData in cutsceneData)
                {
                    if (inData.scene == sceneId)
                        continue;

                    var odat = GetData(scene, file, inData);

                    sb.AppendFormat("{0},{1},{2:X6},{3:X6},{4:X8},{5},{6}",
                        odat.inData.scene, odat.inData.setup, odat.inData.offset,
                        odat.inData.RamStart,
                        odat.cutsceneRomAddress,
                        odat.scene.Id,
                        odat.result.ToString());
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }


        private static ResultData GetData(_Scene scene, RomFile file, InCutsceneData inData)
        {
            ResultData outdata = new(scene, inData);

            if (!scene.TryConvertToRom(inData.RamStart, out int romAddr))
            {
                outdata.result = ResultData.ResultType.Limit_UnallocatedSpace;
                return outdata;
            }
            outdata.cutsceneRomAddress = romAddr;
            //else in range
            file.Stream.Position = file.Record.GetRelativeAddress(romAddr);

            try
            {
                Cutscene cs = new(file);

                if (cs.CommandCount < 0 || cs.Frames < 0)
                    outdata.result = ResultData.ResultType.Success_InvalidCutscene;

                else if (cs.CommandCapReached)
                    outdata.result = ResultData.ResultType.Error_CutsceneParserLimitReached;

                else
                    outdata.result = ResultData.ResultType.Success_ValidCutscene;

            }
            catch
            {
                outdata.result = ResultData.ResultType.Exception;
            }
            return outdata;
        }

        private static List<InCutsceneData> GetCutsceneOffsetData()
        {
            List<string> cutsceneStr = new();
            List<InCutsceneData> cutsceneData = new();

            using (TextReader reader = System.IO.File.OpenText(CUTSCENE_OFFSET_DATA_LOC))
            {
                while (reader.Peek() > -1)
                {
                    cutsceneStr.Add(reader.ReadLine());
                }
            }

            //parse input data
            foreach (string s in cutsceneStr)
            {
                var data = s.Split(new char[] { '\t', ',' });
                var inData =
                    new InCutsceneData(
                        int.Parse(data[0]),
                        int.Parse(data[1]),
                        int.Parse(data[2], System.Globalization.NumberStyles.HexNumber) & 0xFFFFFF);

                cutsceneData.Add(inData);
            }
            return cutsceneData;
        }

        private static Dictionary<int, _Scene> GetSceneInfo(ORom rom)
        {
            Dictionary<int, _Scene> result = new();
            for (int sceneId = 0; sceneId < 101; sceneId++)
            {
                _Scene s = new()
                {
                    Id = sceneId,
                    Address = rom.Files.GetSceneVirtualAddress(sceneId)
                };
                result.Add(sceneId, s);
            }
            return result;
        }

        //private static Dictionary<int, _Scene> GetSceneInfo(IEnumerable<int> scenes)
        //{
        //    Dictionary<int, _Scene> result = new Dictionary<int, _Scene>();
        //    OcarinaDataContext db = new OcarinaDataContext();

        //    var scenelist = from scene in db.Scenes
        //                    where scenes.Contains(scene.ID)
        //                    select scene;

        //    foreach (OcaBase.Scene s in scenelist)
        //    {
        //        _Scene outScene = new _Scene();
        //        outScene.Id = s.ID;
        //        outScene.Address = new FileAddress(
        //            s.File.Address_N0.StartAddr,
        //            s.File.Address_N0.EndAddr);

        //        result.Add(outScene.Id, outScene);
        //    }
        //    return result;
        //}

        public static IEnumerable<string> ReadFrom(string file)
        {
            string line;
            using (var reader = File.OpenText(file))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }

        }
        
        [DataContract]
        class SceneData
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public List<CutsceneData> Cutscenes { get; set; }

            public SceneData() { }
            public SceneData(int id)
            {
                Id = id;
                Cutscenes = new List<CutsceneData>();
            }
        }

        [DataContract]
        class CutsceneData
        {
            [DataMember]
            public int FileOffset { get; set; }

            [DataMember]
            public int Setup { get; set; }

            [DataMember]
            public string Description { get; set; }

            public CutsceneData() { }
            public CutsceneData(int off, int setup, string desc)
            {
                FileOffset = off;
                Setup = setup;
                Description = desc;
            }
        }

        public static void JsonManip()
        {

            var csvData =
            from row in ReadFrom(@"C:\Users\mzxrules\Google Drive\Documents\Visual Studio 2013\Projects\Projects\MasterOcarina\Experimental\Data\PartialCutscenes.txt")

            let columns = row.Split('\t')
            select new
            {
                Scene = int.Parse(columns[0]),
                Setup = int.Parse(columns[1]),
                Offset = int.Parse(columns[2], NumberStyles.HexNumber, new CultureInfo("en-US")) & 0xFFFFFF
            };

            Dictionary<int, SceneData> stuff = new();


            foreach(var item in csvData)
            {
                if (!stuff.ContainsKey(item.Scene))
                {
                    stuff.Add(item.Scene, new(item.Scene));
                }

                stuff[item.Scene].Cutscenes.Add(new CutsceneData(item.Offset, item.Setup, "?"));
            }

            //DataContractJsonSerializer dmaSerializer =
            //    new DataContractJsonSerializer(typeof(List<DmaData_Wiki>));


            DataContractJsonSerializer dmaNewSerializer =
                new(typeof(List<SceneData>));

            //List<DmaData_Wiki> dmaOld;
            //List<DmaData> dma;

            //using (FileStream sr = new FileStream("../../data/DBGMQ/DMADATA.json", FileMode.Open))
            //{
            //    dmaOld = (List<DmaData_Wiki>)dmaSerializer.ReadObject(sr);
            //}

            //dma = new List<DmaData>();

            //foreach (var i in dmaOld)
            //{
            //    DmaData d = new DmaData();
            //    d.Filename = i.Filename;
            //    d.VRomStart = int.Parse(i.VRomStart.ToString(), System.Globalization.NumberStyles.HexNumber);
            //    d.VRomEnd = int.Parse(i.VRomEnd.ToString(), System.Globalization.NumberStyles.HexNumber);
            //    dma.Add(d);
            //}

            using FileStream sw = new("sceneCs.json", FileMode.OpenOrCreate);
            dmaNewSerializer.WriteObject(sw, stuff.Values.ToList());
            //filesNewSerializer.WriteObject(sw, newItems);
        }

    }
}
