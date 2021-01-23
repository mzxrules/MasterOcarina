using Experimental.Data;
using OcaBase;
using mzxrules.OcaLib;
using mzxrules.OcaLib.SceneRoom;
using mzxrules.OcaLib.PathUtil;
using mzxrules.Helper;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;
//using mzxrules.ZActor;

namespace Experimental
{
    interface IExperimentFace
    {
        bool GetFileDialogs(out List<string> files, params string[] args);
        bool GetFileDialogs(out List<string> files, params RomVersion[] args);
        void OutputText(string text);
    }
    public partial class TestForm : Form, IExperimentFace
    {
        GameText g;
        public TestForm()
        {
            InitializeComponent();
        }

        public void OutputText(string text)
        {
            outRichTextBox.Text = text;
        }

        public bool GetFileDialogs(out List<string> files, params RomVersion[] roms)
        {
            files = new List<string>();

            foreach (var rom in roms)
            {
                if (PathUtil.TryGetRomLocation(rom, out string fileLoc))
                {
                    files.Add(fileLoc);
                }
                else if (GetFileDialogs(out List<string> _f, new string[] { rom.ToString() }))
                {
                    string f = _f[0];
                    PathUtil.SetRomLocation(rom, f);
                    files.Add(f);
                }
                else
                    return false;

            }
            return true;
        }

        public bool GetFileDialogs(out List<string> files, params string[] args)
        {
            files = new List<string>();
            int count = (args == null) ? 0 : args.Length;

            if (count <= 0)
                return false;

            string dialogText = openFileDialog.Title;
            
            for (int i = 0; i < count; i++)
            {
                openFileDialog.Title = args[i];
                DialogResult dialogResult = openFileDialog.ShowDialog();
                
                if (dialogResult != DialogResult.OK)
                {
                    openFileDialog.Title = dialogText;
                    return false;
                }

                files.Add(openFileDialog.FileName);
            }
            return true;
        }


        private void experimentButton_Click(object sender, EventArgs e)
        {
            object item = testComboBox.SelectedItem;

            if (item == null)
                return;

            FuncSelector function = (FuncSelector)item;
            function.Function();
        }

        private void ExperimentalForm_Load(object sender, EventArgs e)
        {
            LoadFunctions();
            PathUtil.Initialize();
        }

        private void LoadFunctions()
        {
            FuncSelector.ExperimentFace = this;
            testComboBox.DataSource = new FuncSelector[] {
                //Operation Name, Function, Open File Dialog Array
                //Function is either 0 params, or accepts IExperimentalFace and List<string>
                //
                new FuncSelector("Get Actor Table", DumpActorTable),
                new FuncSelector("Get Object Table", DumpObjectTable),
                new FuncSelector("Get Ovl Effect Addresses", DumpOvlEffect),
                new FuncSelector("Text Dump (Compare)", DumpText),
                new FuncSelector("Text Dump (Rom)", DumpTextSingle),
                new FuncSelector("MBlock type test", MBlockTest),
                new FuncSelector("Get Chest Contents (ovl_actor_player?)", GetChestContents),
                new FuncSelector("Petrie's Mod Generator", GeneratePetriesModFiles),
                new FuncSelector("Generate Dungeon Rush Files", GenerateDungeonRushFiles),
                new FuncSelector("Actor Tagging Test", ActorTaggingTest),
                new FuncSelector("DMA Map Dump", Get.DmaMap, MRom.Build.J0, MRom.Build.U0),
                new FuncSelector("MM Scene Test", GetMMSceneData),
                new FuncSelector("MM Decompress", DecompressMM),
                new FuncSelector("OoT WW Scene Test", GetPartialCutscene),
                new FuncSelector("MM Scene Convertion (U10)", GetInternalSceneMap),
                new FuncSelector("MM Entrance Table Rip (U10)", Get.MMEntranceTable, "Select a MM U1.0 File"),
                new FuncSelector("MM 3D Entrance Table Rip", Get.MM3DEntranceTable, "Select RAM dump"),
                new FuncSelector("MM 3D Get Item Table Rip", Get.MM3DGetItemTable, "Select RAM dump"),
                new FuncSelector("OoT Base Entrance Index List", GetBaseIndex),
                new FuncSelector("OoT WW Result", GetWWResult),
                new FuncSelector("OoT ULTIMA WW Calculator", GetWWResultFromData, "Select N0 File"),
                new FuncSelector("Texture Explorer Data", TextureExplorer.Script.Test, ORom.Build.DBGMQ, ORom.Build.N0),
                new FuncSelector("OoT Dump Drop Rng Tables", Get.RandomDrops, "Select NTSC 1.0"),
                new FuncSelector("Compare Two Files", CompareFiles, "File 1", "File 2"),
                new FuncSelector("Cutscene Serialization Test", Get.CutsceneSerializationTest, "NTSC 1.0 file"),
                new FuncSelector("Cutscene Exit Asm Ids (N0)", PartialCutscene.GetCutsceneExitAsm, "NTSC 1.0 file"),
                new FuncSelector("Json Generation", PartialCutscene.JsonManip),
                new FuncSelector("Compare Frame Dumps", CompareFrameDumps, "File 1", "File 2"),
                new FuncSelector("Rip Roms from ISO", RipRomsFromIso, "File 1"),
                new FuncSelector("GCN compare", DmaDataCompare, "File 1", "File 2"),
                new FuncSelector("Gen DR MQ ovl_kaliedo_scope", DRMQ_PauseChest.Generate, "MQ", "Ovl_kaleido_scope"),
                new FuncSelector("Zelda's Birthday Patch", ZeldasBirthday.Generate, "Zelda's Birthday Rom" ),
                new FuncSelector("Yaz Compression Test", Get.TestYazCompression, "Select NTSC 1.0"),
                new FuncSelector("Yaz Compression Quick Test", Get.TestYazSimple, ORom.Build.N0),
                new FuncSelector("Yaz Decompression Test", Get.TestYazDecompress, ORom.Build.N0),
                new FuncSelector("Ass Skull", Get.AssSkulls, "Select NTSC 1.0"),
                new FuncSelector("Compare MQ", Get.MQRandoCompareHeaders,  ORom.Build.N0, ORom.Build.MQU),
                new FuncSelector("Compare MQ Collision" , Get.CompareCollision, ORom.Build.N0, ORom.Build.MQU),
                new FuncSelector("Dump MQ Json", Get.OutputMQJson, ORom.Build.N0, ORom.Build.MQU),
                new FuncSelector("Add Col to MQ Json", Get.MQJsonImportAndPatch, ORom.Build.N0, ORom.Build.MQU),
                new FuncSelector("Add Map Data to MQ Json", Get.ImportMapData, ORom.Build.N0, ORom.Build.MQU),
                new FuncSelector("DeflateStream Compression Test", DeflateStreamCompress),
            };
        }

        private void DeflateStreamCompress()
        {
            MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes("A"));
            using (var test = System.IO.File.Create("DeflateStream.z64"))
            {
                using (DeflateStream compressionStream = new DeflateStream(test, CompressionMode.Compress))
                {
                    ms.CopyTo(compressionStream);
                }
            }
            ms.Close();
        }

        private void DmaDataCompare(IExperimentFace face, List<string> file)
        {
            ORom rom1 = new ORom(file[0], ORom.Build.GCU);
            ORom rom2 = new ORom(file[1], ORom.Build.GCU);
            //ORom ntsc = new ORom(file[3], ORom.Build.N0);
            StringBuilder sb = new StringBuilder();

            var f1 = rom1.Files.ToList();
            var f2 = rom2.Files.ToList();
            
            for(int i = 0; i < f1.Count; i++)
            {
                var f1a = f1[i].VRom;
                var f2a = f2[i].VRom;
                if (f1a.Size != f2a.Size)
                    sb.AppendFormat("DMA Diff: {0:X8} {1:X8}\n", f1a.Start, f2a.Start);


                var s1 = rom1.Files.GetFile(f1[i].VRom);
                var s2 = rom2.Files.GetFile(f2[i].VRom);

                if (s1.Stream.IsDifferentTo(s2))
                {
                    sb.AppendFormat("File Read Diff: {0:X8} {1:X8}\n", f1a.Start, f2a.Start);
                    foreach (var item in s1.Stream.GetDelta(s2))
                    {
                        if (item[1] + 0x10 != item[2])
                            sb.AppendFormat("{0:X8}: {1:X8} {2:X8}\n", item[0], item[1], item[2]);
                    }
                }
                
            }

            face.OutputText(sb.ToString());
        }

        private void RipRomsFromIso(IExperimentFace face, List<string> file)
        {
            byte[] buffer = new byte[0x1000];
            using (var fs = new FileStream(file[0], FileMode.Open, FileAccess.Read))
            {
                using (var rom1 = new FileStream("Mask (GCJ).z64", FileMode.Create, FileAccess.Write))
                {
                    //fs.Position = 0x52FBC1E0;
                    //fs.Position = 0xC4E1FC0;
                    fs.Position = 0x54FC1FC0;
                    for (int i = 0; i < 0x2000000; i++)
                    {
                        rom1.WriteByte((byte)fs.ReadByte());
                    }
                }
            }

            //using (var fs = new FileStream(file[0], FileMode.Open, FileAccess.Read))
            //{
            //    using (var rom2 = new FileStream("Ocarina (GCJ) (c).z64", FileMode.Create, FileAccess.Write))
            //    {
            //        //fs.Position = 0x550569D8;
            //        //fs.Position = 0x3B9D1FC0;
            //        fs.Position = 0x27BD8000;
            //        for (int i = 0; i < 0x2000000; i++)
            //        {
            //            rom2.WriteByte((byte)fs.ReadByte());
            //        }
            //    }
            //}
        }
        private void CompareFrameDumps(IExperimentFace face, List<string> file)
        {
            long pos;
            int f0h, f0l, f1h, f1l;
            StringBuilder sb = new StringBuilder();
            using (BinaryReader f0 = new BinaryReader(new FileStream(file[0], FileMode.Open, FileAccess.Read)))
            {
                using (BinaryReader f1 = new BinaryReader(new FileStream(file[1], FileMode.Open, FileAccess.Read)))
                {
                    while (f0.BaseStream.Position < f0.BaseStream.Length)
                    {
                        pos = f0.BaseStream.Position;
                        f0h = f0.ReadBigInt32();
                        f0l = f0.ReadBigInt32();
                        f1h = f1.ReadBigInt32();
                        f1l = f1.ReadBigInt32();

                        if (f0h != f1h
                            || f0l != f1l)
                        {
                            sb.AppendFormat("{0:X6}: {1:X8} {2:X8} | {3:X8} {4:X8}",
                                pos, f0h, f0l, f1h, f1l);
                            sb.AppendLine();
                        }
                    }
                }
            }
            face.OutputText(sb.ToString());
        }

        private void CompareFiles(IExperimentFace face, List<string> file)
        {
            using (FileStream f0 = new FileStream(file[0], FileMode.Open, FileAccess.Read))
            {
                using (FileStream f1 = new FileStream(file[1], FileMode.Open, FileAccess.Read))
                {
                    face.OutputText(f0.IsDifferentTo(f1) ? "different" : "same");
                }
            }
        }

        private void GetWWResultFromData(IExperimentFace face, List<string> files)
        {
            UltimaWrongWarp.Calculate(face, files);
        }


        private void GetInternalSceneMap()
        {
            List<string> file;
            MRom rom;
            StringBuilder sb = new StringBuilder();

            file = SingleDialog();
            if (file == null)
                return;

            rom = new MRom(file[0], MRom.Build.U0);

            for (int i = 0; i < 0x6E; i++)
            {
                sb.AppendLine(string.Format("{0},{1}", i,
                rom.Files.GetInternalSceneIndex(i)));
            }
            outRichTextBox.Text = sb.ToString();
        }

        private void GetPartialCutscene()
        {
            outRichTextBox.Text = PartialCutscene.GetPartialCutscene();
        }

        private void DecompressMM()
        {
            List<string> file = SingleDialog();

            if (file == null)
                return;
            using (FileStream fs = new FileStream("mmdec.z64", FileMode.CreateNew)) 
            {
                Gen.RomBuilder.DecompressRom(new MRom(file[0], MRom.Build.U0), fs);
            }
        }

        private void GetMMSceneData()
        {
            List<string> file;
            if (!int.TryParse(inputTextBox.Text, out int sceneIndex))
            {
                return;
            }

            file = SingleDialog();
            if (file == null)
                return;

            Rom r = new MRom(file[0], MRom.Build.U0);

            mzxrules.OcaLib.SceneRoom.Scene scene = SceneRoomReader.InitializeScene(r.Files.GetSceneFile(sceneIndex), sceneIndex);
            outRichTextBox.Text = SceneRoomReader.ReadScene(scene);
        }


        private void DumpActorTable()
        {
            List<RomVersion> versions = new List<RomVersion>();
            versions.AddRange(ORom.GetSupportedBuilds());
            versions.AddRange(MRom.GetSupportedBuilds());

            StringBuilder sb = new StringBuilder();
            foreach (var version in versions)
            {
                string outFile = $"actor_dlf_{version.ShortUniqueKey}.txt";
                if (PathUtil.TryGetRomLocation(version, out string file))
                {
                    var txt = Get.ActorTable(Rom.New(file, version));
                    System.IO.File.WriteAllText(outFile, txt);
                    sb.AppendLine($"Created {outFile}");
                }
                else
                {
                    sb.AppendLine($"{outFile} failed; no rom found");
                }
            }
            outRichTextBox.Text = sb.ToString();
        }

        private void DumpObjectTable()
        {
            List<string> file = SingleDialog();
            if (file == null)
                return;

            outRichTextBox.Text = Get.GetObjectTable(new ORom(file[0], ORom.Build.N0));

        }

        private void DumpOvlEffect()
        {
            List<string> files = DoubleDialog();

            if (files == null || files.Count != 2)
                return;

            outRichTextBox.Text = Get.EffectTable(
                new ORom(files[0], ORom.Build.N0),
                new ORom(files[1], ORom.Build.DBGMQ));
        }

        private void inputButton_Click(object sender, EventArgs e)
        {
            Rom.Language language = Rom.Language.Japanese;
            if (ushort.TryParse(inputTextBox.Text,
                System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture, out ushort id)
                && g != null)
            {
                outRichTextBox.Clear();
                outRichTextBox.AppendText(g.GetMessageOffset(id, language));
                outRichTextBox.AppendText(Environment.NewLine);
                outRichTextBox.AppendText(g.GetMessage(id, language));
                outRichTextBox.AppendText(Environment.NewLine);
            }
        }

        void MBlockTest()
        {
            MBlock container;

            container = new MBlock(new FileAddress(0, 20));

            container.InsertAsset(null, 3, 8);
            container.InsertAsset(null, 10, 15);
            container.InsertAsset(null, 10, 12);
            container.InsertAsset(null, 0, 1);

            StringBuilder sb = new StringBuilder();
            foreach (MBlock b in container)
            {
                sb.AppendLine(string.Format("{0:X2}:{1:X2}",
                    b.Address.Start,
                    b.Address.End));
            }
            outRichTextBox.Text = sb.ToString();
        }

        private void DumpTextSingle()
        {
            List<string> s = SingleDialog();

            ORom r = new ORom(s[0], ORom.Build.N0);

            outRichTextBox.Text = Get.TextDump(r, ORom.Language.English);
        }

        private void DumpText()
        {
            Get.TextCompareDump();
        }

        private void TextTestClass(List<string> file)
        {
            ORom r = new ORom(file[0], ORom.Build.N0);
            g = new GameText(r);
            //outRichTextBox.AppendText(g.GetMessage(0x1/*01A*/, Rom.Language.English));
            //outRichTextBox.AppendText("                                                 --- End ---");
            //outRichTextBox.AppendText(Environment.NewLine);

            //outRichTextBox.AppendText(g.GetMessage(0x1/*01A*/, Rom.Language.Japanese));
            //outRichTextBox.AppendText(Environment.NewLine);
        }

        private void GetTextBankData(List<string> file)
        {
            Get.TextBankData(new ORom(file[0], ORom.Build.N0));
        }

        private void GetChestContents()
        {
            List<string> file = SingleDialog();
            if (file == null)
                return;

            ORom r = new ORom(file[0], ORom.Build.N2);
            Get.ChestContents(r);
        }

        const int COMPRESS_FILE_TEST = 0xE7DC20;
        private void CompressFile(List<string> file)
        {
            ORom r = new ORom(file[0], ORom.Build.MQP);
            byte[] data;

            using (Stream stream = r.Files.GetFile(COMPRESS_FILE_TEST))
            using (FileStream fs = new FileStream("00E7DC20", FileMode.CreateNew))
            {
                data = new byte[stream.Length];
                stream.Read(data, 0, (int) stream.Length);
                Yaz.Encode(data, data.Length, fs);
            }
        }

        private void GetDmaFile()
        {
            List<string> file = SingleDialog();
            if (file == null)
                return;
            Get.DmaFile(new ORom(file[0], ORom.Build.MQP));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">First file is </param>
        private void GeneratePetriesModFiles()
        {
            List<string> file = DoubleDialog();
            if (file == null)
                return;

            Gen.FileSpitter.GenerateModifiedFiles(
                new mzxrules.OcaLib.ORom(file[0], ORom.Build.MQP),
                new mzxrules.OcaLib.ORom(file[1], ORom.Build.MQP),
                "petrie");
        }

        private void CompareFiles()
        {
            List<string> files = DoubleDialog();
            if (files == null)
                return;

            int addr = 0x7170;
            
            ORom rom1 = new ORom(files[0], ORom.Build.MQP);
            ORom rom2 = new ORom(files[1], ORom.Build.MQP);
            BinaryReader br1 = new BinaryReader(rom1.Files.GetFile(addr));
            BinaryReader br2 = new BinaryReader(rom2.Files.GetFile(addr));

            StringBuilder sb = new StringBuilder();
            while (br1.BaseStream.Position < br1.BaseStream.Length)
            {
                if (br1.ReadInt64() != br2.ReadInt64())
                {
                    sb.AppendLine(string.Format("{0:X4}", br1.BaseStream.Position - 8 + addr));
                }
            }
            br1.Close();
            br2.Close();
            outRichTextBox.Text = sb.ToString();
        }

        private void GenerateDungeonRushFiles()
        {
            List<string> file = SingleDialog();
            if (file == null)
                return;
            DungeonRush2.GenerateUncompressedRom(openFileDialog.FileName);
        }

        void SpiritBetaPatch_func(List<string> file)
        {
            SpiritBetaPatch.Export(file[0], ORom.Build.N0);
            SpiritBetaPatch.Import(file[0], ORom.Build.N0);
        }

        void PrintDecompressedFiles()
        {
            outRichTextBox.Text = Get.DecompressedFileList(new ORom(openFileDialog.FileName, ORom.Build.N2));
        }

        //void XmlActorDictionaryTests()
        //{
        //    XmlActorDictionary.LoadFile(openFileDialog.FileName);
        //    byte[] test = new byte[0x10];
        //    byte[] testInit = test;

        //    //test out our actor def function
        //    XmlActorDictionary.TryGetActorDefinition(0xA, out ActorDefinition def);

        //    string testOut = def.GetDescription(test);

        //    XmlActor testNewRecord = new XmlActor(testInit);

        //    this.outRichTextBox.Text = testNewRecord.Print();
        //}


        private void CutsceneParser()
        {
            #region Cutscene Parser
            mzxrules.OcaLib.Cutscenes.Cutscene a;

            //br =  SceneRoomReader.LocalFileTable.GetFile(0x0318E000);
            //br.BaseStream.Position = 0x1DA40;
            //hyrule field title
            //br = SceneRoomReader.LocalFileTable.GetFile(0x01FB8000);
            //br.BaseStream.Position = 0xA920;

            ORom rom = new ORom("", ORom.Build.N0);

            //navi
            Stream s = rom.Files.GetFile(0x206F000);
            s.Position = 0xA6D0;
            a = new mzxrules.OcaLib.Cutscenes.Cutscene(s);
            outRichTextBox.Clear();
            outRichTextBox.Text = a.PrintByOccurrence();
            #endregion
        }

        private void FileDiffHack()
        {
            DialogResult result;
            string file1, file2;
            //VirtualTable ft1;
            //VirtualTable ft2;
            StringBuilder sb = new StringBuilder();
            List<int> diffList = new List<int>();

            result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK)
                return;
            file1 = openFileDialog.FileName;

            result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK)
                return;
            file2 = openFileDialog.FileName;


            //ft1.Initialize(file1, RomType.V10);
            //ft2.Initialize(file2, RomType.V12);
            //f1 = ft1.GetFile(0xA87000);
            //f2 = ft2.GetFile(0xA87000);

            using (var f1 = new BinaryReader(System.IO.File.OpenRead(file1)))
            {
                using (var f2 = new BinaryReader(System.IO.File.OpenRead(file2)))
                {
                    diffList = FileDiff(f1, f2);
                }
            }
            outRichTextBox.Clear();
            for (int i = 0; i < diffList.Count; i += 3)
            {
                sb.AppendFormat("{0:X8} {1:X8} {2:X8}",
                    diffList[i], Endian.ConvertInt32(diffList[i + 1]), Endian.ConvertInt32(diffList[i + 2]));
                sb.AppendLine();
            }
            outRichTextBox.Text = sb.ToString();
        }

        private List<int> FileDiff(BinaryReader f1, BinaryReader f2)
        {
            List<int> diffList = new List<int>();
            int v1, v2;

            while (f1.BaseStream.Position < f1.BaseStream.Length)
            {
                v1 = f1.ReadInt32();
                v2 = f2.ReadInt32();
                if (v1 != v2)//if ((v1 & 0xFFFF) != (v2 & 0xFFFF))
                {
                    diffList.Add((int)f1.BaseStream.Position - 4);
                    diffList.Add(v1);
                    diffList.Add(v2);
                }
                //if (f1.BaseStream.Position == 0x03C1DC)
                //{
                //    f1.ReadInt32();
                //}
                //if (f1.BaseStream.Position == 0x03C23C)
                //{
                //    f2.BaseStream.Position = 0x03C264;
                //}
                //if (f1.BaseStream.Position == 0x03C2FC)
                //{
                //    f2.ReadInt32();
                //}
            }
            return diffList;
        }

        private List<string> SingleDialog()
        {
            DialogResult r = openFileDialog.ShowDialog();

            if (r == DialogResult.OK)
            {
                //if (func != null)
                //    func(
                return new List<string>(new string[] { openFileDialog.FileName });
            }
            return null;
        }

        private List<string> DoubleDialog()
        {
            DialogResult r1 = openFileDialog.ShowDialog();
            string r1File = openFileDialog.FileName;
            DialogResult r2 = openFileDialog.ShowDialog();
            string r2File = openFileDialog.FileName;

            if (r1 == DialogResult.OK
                && r2 == DialogResult.OK)
            {
                //var s =
                return    new List<string>(new string[] { r1File, r2File });
                //if (func != null)
                //    func(s);
            }
            return null;
        }

    }
}