using mzxrules.Helper;
using mzxrules.OcaLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uCode;

namespace Spectrum
{
    struct FrameHalt { public int update; public int deconstructor; public FrameHalt(int u, int d) { update = u; deconstructor = d; } }
    partial class Program
    {
        const string TITLE = "Spectrum - Time never really passes in Hyrule... does it?";
        static SpectrumOptions Options = new SpectrumOptions();
        public delegate void SetVersionEventHandler(RomVersion v, bool b = true);
        public static event SetVersionEventHandler ChangeVersion;
        static List<BlockNode> LastActorLL = new List<BlockNode>();
        static ExpressTest.ExpressionEvaluator Evaluator = new ExpressTest.ExpressionEvaluator((x) => Zpr.ReadRamInt32((int)x) & 0xFFFFFFFF);
        static Dictionary<string, Command> CommandDictionary = new Dictionary<string, Command>();
        static Dictionary<string, (SpectrumCommand attr, SpectrumCommandSignature[] args, NewCommandDelegate method)> NewCommandDictionary;

        static CollisionAutoDoc CollisionActorDoc = new CollisionAutoDoc();
        static ReferenceLogger<DisplayListRecord> DisplayListLogger = new ReferenceLogger<DisplayListRecord>();

        public static Ptr SaveContext { get { return SpectrumVariables.SaveContext; } }
        public static Ptr GlobalContext { get { return SpectrumVariables.GlobalContext; } }
        public static Ptr Gfx { get { return SpectrumVariables.Gfx; } }

        public static FrameHalt FrameHaltVars = new FrameHalt(0, 0);
        const int EXECUTE_PTR = unchecked((int)0x80000490);

        static void Main(string[] args)
        {
            string readLine;
            Initialize();

            do
            {
                readLine = Console.ReadLine();
                try
                {
                    var arguments = new CommandRequest(readLine);
                    ProcessCommand(arguments);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            while (readLine != "quit");
        }

        private static void Initialize()
        {
            Console.Title = TITLE;

            ChangeVersion += SpectrumVariables.ChangeVersion;
            ChangeVersion += OvlActor.ChangeVersion;
            ChangeVersion += RamObject.ChangeVersion;
            ChangeVersion += SetBlockNodeLength;
            ChangeVersion += UpdateSetVersion;
            ChangeVersion += OvlParticle.ChangeVersion;
            
            NewCommandDictionary = BuildCommands();

            SpawnData.Load();
            LoadSettings();

            MountEmulator("");

            Console.WriteLine($"Created by mzxrules 2014-2018, compiled {Timestamp}");
            Console.WriteLine("Press Enter to perform a memory dump, or type help to see a list of commands");
            Console.WriteLine($"Data logging enabled? {Options.EnableDataLogging}");
        }

        private static void InitializeCommands()
        {
            List<Command> commands = new List<Command>()
            {
                //new Command("help", (a) => Help(), "Shows this help menu"),
                //new Command("=", (a) => Evaluate(a.GetCommandArgumentsString()), "Evaluates an expression"),
                //new Command("emu", (a)=>SetEmulator(a.GetCommandArgumentsString()),"Sets/updates emulator settings"),
                //new Command("delemu", (a)=>DeleteEmulator(a.Legacy()), "Removes emulator settings for an emulator"),
                //new Command("mount", (a)=>MountEmulator(a.Legacy()),"Locates a running emulator to interface with"),
                //new Command("game", (a) => SetGame(a.Legacy()), "Sets game to profile, and optionally lets you specify a version"),
                //new Command("ver", (a)=> TrySetVersion(a.Legacy()), "Sets game version/displays supported game versions"),
                //new Command("", (a)=>Default(), "Creates a memory map"),
                //new Command("var", (a) => SpectrumVariables.GetVariables(), "Shows a list of variables the program uses"),
                //new Command("addr", (a) => PrintEmuAddr(a.Legacy()), "Converts N64 address into an emulator's process address"),
                //new Command("ram", (a)=> PrintRam(a.Legacy()), "Prints RDRAM starting at the given address"),
                //new Command("ramf", (a)=> PrintRamF(a.Legacy()), "Prints RDRAM starting at the given address, view as floating point"),
                //new Command("a", (a)=> GetAddresses(a.Legacy()), "Returns whatever major structure is located at the given address"),
                //new Command("r", (a)=> ConvertRomToRam(a.Legacy()), "Converts rom address to ram, if possible"),
                //new Command("f", (a)=> ToHex(a.Legacy(), typeof(float)), "Converts floating point to hex representation"),
                //new Command("ff", (a)=> FromHex(a.Legacy(), typeof(float)), "Converts hexadecimal to floating point"),
                //new Command("i", (a)=> ToHex(a.Legacy(), typeof(int)), "Converts integer to hex representation"),
                //new Command("ii", (a)=> FromHex(a.Legacy(), typeof(int)), "Converts hex to integer"),
                //new Command("ll", (a)=> { ToggleSettings(ref Options.ShowLinkedList); Default(); }, "Show/Hide link list nodes"),
                //new Command("obj", (a)=> { ToggleSettings(ref Options.ShowObjects); Default(); }, "Show/Hide object files"),
                //new Command("size", (a)=> { ToggleSettings(ref Options.ShowSize); Default(); }, "Show Size or End Address"),
                //new Command("actor", (a)=> { ToggleSettings(ref Options.ShowActors); Default(); }, "Show/Hide actor files/instances"),
                //new Command("thread", (a)=> { ToggleSettings(ref Options.ShowThreadingStructs); Default(); }, "Show/Hide thread structs"),
                //new Command("vthread", (a)=> ViewThread(a.Legacy()), "View the state of a thread on last context swap"),
                //new Command("hidea", (a)=> HideActor(a.Legacy()), "Hide actor file/instance of a given id"),
                //new Command("showalla", (a) => {Options.HiddenActors.Clear(); Default(); }, "Unhide all hidden actors"),
                //new Command("anear",(a)=>FindNearestActor(), "Locates actors nearest to Link in 3D"),
                //new Command("ent", (a)=>SpawnAtEntranceIndex(a.Legacy()), "Spawns Link at a given entrance index"),
                //new Command("sp", (a)=> SpawnToEntrance(a.Legacy()), "Spawns Link in a given scene number"),
                //new Command("sa", (a)=> SpawnAnywhere(a.Legacy()), "Spawn anywhere with a given scene,room,x,y,z coordinate"),
                //new Command("sr", (a)=> SpawnInRoom(a.Legacy()), "Spawns Link in a specified room,x,y,z using the current scene"),
                //new Command("y", (a)=> SetCoordinatesY(a.Legacy()), "Sets y coordinate"),
                //new Command("xyz", (a)=> SetCoordinates(a.Legacy()), "Sets x,y,z coordinates"),
                //new Command("dumpa",(a)=>DumpActor(a.Legacy()), "Dumps all data for an actor instance"),
                //new Command("age", (a)=>SetAge(a.Legacy()), "Sets Link's age"),
                //new Command("ef0", (a)=>EventFlag(a.Legacy(), false), "Sets event flag to 0" ),
                //new Command("ef1", (a)=>EventFlag(a.Legacy(), true), "Sets event flag to 1" ),
                //new Command("time", (a)=>GetTime(a.Legacy()), "Gets world time"),
                //new Command("save", (a)=>SaveSettings(), "Save Settings (program usually does this automatically)"),
                //new Command("txt", (a)=> DisplayText(a.Legacy()), "Interprets data at given address as null terminated string, and displays it"),
                //new Command("col", (a)=>GetActorCollision(), "Gets 'complex mesh' collision data"),
                //new Command("colb", (a)=>GetActorBodyCollision(), "Gets 'simple body' collision data"),
                //new Command("colxyz", (a)=>GetSceneCollisionCoords(a.Legacy()), "converts xyz into scene collision hashtable coords/finds record"),
                //new Command("colsec", (a)=>GetSceneCollsionSection(a.Legacy()), "converts collision sector coords into bounding box for that unit"),
                //new Command("w16",(a)=> WriteRam(a.Legacy(), typeof(short)), "Write 16 bit data"),
                //new Command("w32",(a)=> WriteRam(a.Legacy(), typeof(int)), "Write 32 bit data"),
                //new Command("wf",(a)=> WriteRam(a.Legacy(), typeof(float)), "Write float"),
                //new Command("framepng",(a)=> GetFrameBufferPng(), "Dumps framebuffer to .png"),
                //new Command("view",(a)=>ViewFrameBuffer(), "View framebuffer in console window live"),
                //new Command("gfx", (a)=>DisplayGraphicsContext(), "Displays variables related to the 'Graphics Context'"),
                //new Command("gfxclean", (a)=> CleanFrameDlistBuffers(), "Zero clears 'garbage' data in gbi buffers"),
                //new Command("setgfx", (a)=> SetGraphicsContext(a.Legacy()), "Allows the 'Graphics Context' pointer to be set manually"),
                //new Command("gbi", (a) => PrintGbi(a.Legacy()), "Prints out a sequence of GBI, starting at the specified address"),
                //new Command("dumpgbi", (a) => DumpGbi(a.Legacy()), "Creates a text dump of a GBI task, starting at the specified address"),
                //new Command("dumpgbiimg", (a) => DumpGbiTextures(a.Legacy()), "Creates a text dump of all texture related instructions in a GBI task"), 
                //new Command("tracegbi", (a) => TraceGbi(), "Auto-documents display lists for the current frame"),
                //new Command("dumpframe", (a)=>DumpFrame(a.Legacy()), "Dumps gbi buffer for frame"),
                //new Command("loadframe", (a)=>LoadFrame(a.Legacy()), "Restores gbi buffer for frame from file 'dump/frame.bin'"),
                //new Command("rsp",(a)=>PrintRspAsm(a.Legacy()), "RSP disassembler"),
                //new Command("jtxt", (a)=>DumpJsonToText(a.Legacy()), "dunno"),
                //new Command("status", (a)=>StatusRegister(a.Legacy()),"broken"),
                //new Command("module", (a)=>GetModule(a.Legacy()),"test function"),
                //new Command("mips", (a)=>PrintMipsAsm(a.Legacy()), "MIPS disassembler"),
                new Command("pause",(a)=>PauseGame(), "Pauses game by setting game state's 'update' func pointer to return"),
                new Command("mpause", (a)=>PauseGameModel(), "secret :)"),
                //new Command("dh", (a)=>DlistHalt(a.Legacy()), "secret :)"),
                //new Command("loc", (a)=> PrintLocation(), "Prints Location Info"),
                //new Command("beta_shuffle", (a)=> TestBetaQuestShuffleData(), "Checks for repeats in beta quest shuffle group"),
                //new Command("beta_ent", (a)=>LocateBetaQuestEntrance(a.Legacy()), "Locates Beta Quest entrance"),
            };

            foreach (var item in commands)
            {
                CommandDictionary.Add(item.Id, item);
            }
        }

        private static void ProcessCommand(CommandRequest request)
        {
            if (NewCommandDictionary.TryGetValue(request.CommandName, out var value))
            {
                if (!value.attr.IsSupported(Options.Version))
                {
                    Console.WriteLine("Command not supported for selected game or version");
                    return;
                }

                foreach (var signature in value.args)
                {
                    Arguments args = new Arguments(request.Arguments, signature.Sig);
                    if (args.Valid)
                    {
                        value.method(args);
                        return;
                    }
                }
            }
        }

        private static void ColB_GetGroup(string v1, int v2)
        {
            Ptr ptr = GlobalContext.RelOff(v2);
            int count = ptr.ReadInt32(0);

            Console.WriteLine($"{ptr}: col{v1}, {count:D2} elements");
            if (count > 70)
                return;

            ptr = ptr.RelOff(4);
            for (int i = 0; i < (count * 4); i += 4)
            {
                CollisionBody body = new CollisionBody(ptr.RelOff(i).Deref());
                Console.WriteLine(body.ToString());
            }
            Console.WriteLine();
        }

        private static void Default()
        {
            PrintAddresses(GetRamMap().OrderBy(x => (x.Ram.Start & 0xFFFFFF)));
        }

        
        private static void ModelControl()
        {
            ConsoleKey key = ConsoleKey.Home;

            while (key != ConsoleKey.Q)
            {
                key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.LeftArrow: ModelViewer.ScrollDlist(-1); break;
                    case ConsoleKey.RightArrow: ModelViewer.ScrollDlist(1); break;
                    case ConsoleKey.Enter:
                        {
                            Console.Clear();
                            Console.Write("Enter Command: ");
                            var readLine = Console.ReadLine();
                            var args = new CommandRequest(readLine);
                            ProcessCommand(args);
                        }
                        break;
                }
            }
            Console.Clear();
        }

        public static ModelViewerOoT ModelViewer;

        private static void PauseGameModel()
        {
            if (Options.Version != ORom.Build.N0)
                return;

            Ptr staticContext = SPtr.New(0x11BA00).Deref();

            if (staticContext == 0)
                return;

            Ptr pauseFlag = staticContext.RelOff(0x15D4);

            if (pauseFlag.ReadInt32(0) == 0)
            {
                pauseFlag.Write(0, 1);
                CancellationToken token = new CancellationToken();
                //Action syncToEmu = 
                Task.Run(async () => { while (GlobalContext.ReadInt32(4) != EXECUTE_PTR) { await Task.Delay(200); } Console.WriteLine("Sync Complete"); }
                    , token);
                ModelViewer = new ModelViewerOoT(GetFrameDlists(Gfx));
                ModelControl();

            }

            pauseFlag.Write(0, 0);
            ModelViewer?.RestoreBranches();
            ModelViewer = null;
        }

        private static bool PauseGame()
        {
            if (Options.Version != ORom.Build.N0)
                return false;

            if (GlobalContext == 0)
                return false;

            int update = GlobalContext.ReadInt32(4);
            int deconstruct = GlobalContext.ReadInt32(8);
            if (update == EXECUTE_PTR)
            {
                //unpause
                if (deconstruct != FrameHaltVars.deconstructor)
                    return false;
                GlobalContext.Write(4, FrameHaltVars.update);
                return false;
            }
            else
            {
                //pause
                FrameHaltVars = new FrameHalt(update, deconstruct);
                GlobalContext.Write(4, EXECUTE_PTR);
                return true;
            }

        }



        private static double CalculateDistance3D(Vector3<float> position1, Vector3<float> position2)
        {
            Vector3<float> delta = new Vector3<float>(position2.x - position1.x, position2.y - position1.y, position2.z - position1.z);
            double result = Math.Sqrt((double)delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
            return result;
        }

        private static void WriteRam(string expression, object value)
        {
            if (!TryEvaluate(expression, out long address))
                return;

            N64Ptr addr = address;

            if (addr.Base() >= Constants.GetRamSize(Options.Version))
            {
                return;
            }
            if (value is byte v8)
            {
                Zpr.WriteRam8(addr, v8);
                PrintRam(addr, PrintRam_X8, 1);
            }
            else if (value is short v16)
            {
                if (addr % 2 != 0)
                {
                    Console.WriteLine("Alignment error");
                    return;
                }
                Zpr.WriteRam16(addr, v16);
                PrintRam(addr, PrintRam_X8, 1);
            }
            else if (value is int v32)
            {
                if (addr % 4 != 0)
                {
                    Console.WriteLine("Alignment error");
                    return;
                }
                
                Zpr.WriteRam32(addr, v32);
                PrintRam(addr, PrintRam_X8, 1);
            }
            else if (value is float vf)
            {
                if (addr % 4 != 0)
                {
                    Console.WriteLine("Alignment error");
                    return;
                }
                Zpr.WriteRam32(addr, vf);
                PrintRam(addr, PrintRam_F, 1);
            }
        }

        private static void DumpCollisionData()
        {
            using (StreamWriter sw = new StreamWriter("dump/collision.txt"))
            {
                foreach (var item in CollisionActorDoc.Meshes.Values)
                {
                    sw.WriteLine(item.ToString());
                }
            }
            //using (BinaryReader br = new BinaryReader(new FileStream("dump/frame.bin", FileMode.Open)))
        }

        private static void DumpReferenceLogger<T>(ReferenceLogger<T> logger, string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                foreach (var item in logger.LoggedReferences)
                {
                    sw.WriteLine(item.ToString());
                }
            }
        }


        private static IFile GetIFile(IEnumerable<IFile> ramMap, N64Ptr address)
        {
            var addr = address & 0xFFFFFF;
            return ramMap.Where(x =>
                (x.Ram.Start & 0xFFFFFF) <= addr
                && (x.Ram.End & 0xFFFFFF) > addr).SingleOrDefault();
        }

        private static IFile GetIFile(N64Ptr address)
        {
            return GetRamMap(true).Where(x =>
                (x.Ram.Start & 0xFFFFFF) <= address.Base()
                && (x.Ram.End & 0xFFFFFF) > address.Base()).OfType<IFile>().SingleOrDefault();
        }

        private static GfxDList[] GetFrameDlists(Ptr Gfx)
        {
            if (Options.Version == Game.OcarinaOfTime)
            {
                GfxDList Work_Disp = new GfxDList("WORK_DISP", Gfx.RelOff(0x1B4));
                GfxDList Overlay_Disp = new GfxDList("OVERLAY_DISP", Gfx.RelOff(0x2A8));
                GfxDList Poly_Opa_Disp = new GfxDList("POLY_OPA_DISP", Gfx.RelOff(0x2B8));
                GfxDList Poly_Xlu_Disp = new GfxDList("POLY_XLU_DISP", Gfx.RelOff(0x2C8));
                return new GfxDList[] { Work_Disp, Poly_Opa_Disp, Poly_Xlu_Disp, Overlay_Disp };
            }
            return null;
        }

        private static void UpdateSetVersion(RomVersion v, bool g)
        {
            string gameStr = "?";
            string buildStr = "?";
            Options.Version = v;
            Console.WriteLine($"{v.Game}: version {v} set");

            if (v.Game == Game.OcarinaOfTime)
            {
                gameStr = "Ocarina of Time";
                buildStr = ORom.BuildInformation.Get(v).Name;
            }
            else if (v.Game == Game.MajorasMask)
            {
                gameStr = "Majora's Mask";
                buildStr = MRom.BuildInformation.Get(v).Name;
            }

            Console.Title = $"{TITLE} ({gameStr}: {buildStr})";
            SaveSettings();
        }

        private static void ToggleSettings(ref bool value)
        {
            value = !value;
            SaveSettings();
        }

        private static void SetBlockNodeLength(RomVersion v, bool g)
        {
            if (v.Game == Game.OcarinaOfTime)
            {
                if (v == ORom.Build.GCJ
                    || v == ORom.Build.GCU
                    || v == ORom.Build.GCP
                    || v == ORom.Build.MQJ
                    || v == ORom.Build.MQU
                    || v == ORom.Build.MQP
                    || v == ORom.Build.IQUEC
                    || v == ORom.Build.IQUET)
                {
                    BlockNode.LENGTH = 0x10;
                }
                else
                    BlockNode.LENGTH = 0x30;
            }
            if (v.Game == Game.MajorasMask)
            {
                if (v == MRom.Build.J0
                    || v == MRom.Build.J1
                    || v == MRom.Build.DBG)
                    BlockNode.LENGTH = 0x30;
                else
                    BlockNode.LENGTH = 0x10;
            }
        }

        private static void SaveSettings()
        {
            SaveSettingsFile(typeof(SpectrumOptions), Options, "settings.json");
            if (Options.EnableDataLogging)
            {
                SaveSettingsFile(typeof(CollisionAutoDoc), CollisionActorDoc, "data/collision.json");
                SaveSettingsFile(typeof(ReferenceLogger<DisplayListRecord>), DisplayListLogger, "data/dlist.json");
            }
        }

        private static void SaveSettingsFile(Type jsonType, object graph, string savePath)
        {
            using (StreamWriter sw = new StreamWriter(savePath))
            using (MemoryStream ms = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(jsonType);

                serializer.WriteObject(ms, graph);

                StreamReader sr = new StreamReader(ms);
                sr.BaseStream.Position = 0;
                sw.Write(FormatJson(sr.ReadToEnd()));
            }
        }

        private static void LoadSettings()
        {
            TryLoadSettings(ref CollisionActorDoc, "data/collision.json");
            TryLoadSettings(ref DisplayListLogger, "data/dlist.json");


            if (!TryLoadSettings(ref Options, "settings.json"))
            {
                if (TryLoadSettings(ref Options, "data/settings-default.json"))
                {
                    SaveSettings();
                }
            }
        }

        private static bool TryLoadSettings<T>(ref T data, string path)
        {
            bool result = false;
            if (!File.Exists(path))
                return false;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                    data = (T)serializer.ReadObject(fs);
                }
                result = true;
            }
            catch
            {
            }
            return result;
        }

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        private static int GetExternalSceneId_Mask(byte inScene)
        {
            var table = SpectrumVariables.EntranceTable;
            if (table == null || inScene > 127)
                return 0;

            int[] externIds = new int[128];
            for (int i = 0; i < 0x6E; i ++)
            {
                var off = i * 0x0C;
                if (table.ReadByte(off) == 0)
                    continue;
                var firstRecord = table.RelOff(off + 0x04).Deref().Deref();
                int sceneId = Math.Abs((sbyte)firstRecord.ReadByte(0));
                externIds[sceneId] = i; 
            }
            return externIds[inScene];
        }

        private static void SetEntranceIndexSpawn(short index)
        {
            if (Options.Version == Game.OcarinaOfTime)
            {
                GlobalContext.Write(
                    0x11E1A, index,
                    0x11E15, (byte)0x14,
                    0x11E5E, (byte)0x04);
            }
            else if (Options.Version == Game.MajorasMask)
            {
                if (Options.Version == MRom.Build.J0
                    || Options.Version == MRom.Build.J1)
                {
                    GlobalContext.Write(
                        0x18855, (byte)0x14,
                        0x1885A, index);
                }
                else
                {
                    GlobalContext.Write(
                        0x18875, (byte)0x14,
                        0x1887A, index);
                }
            }
        }



        private static void SetZoneoutSpawn_Mask(short? index, ref Spawn spawn)
        {
            //MM J0 start addr 801F33C4, off 0x3F64
            //MM U0 start addr 801F3324, off 0x3CB4

            int zoneoutOff;

            switch((MRom.Build)Options.Version)
            {
                case MRom.Build.J0: zoneoutOff = 0x3F68; break;
                case MRom.Build.J1: zoneoutOff = 0x3F68; break;
                default: zoneoutOff = 0x3CB4; break;
            }

            //Ptr zonoutType = SaveContext.RelOff(0x3F64);
            Ptr zoneoutRecord = SaveContext.RelOff(zoneoutOff);
            Ptr linkAddr;
            if (SpectrumVariables.Actor_Category_Table == 0)
                return;

            linkAddr = SpectrumVariables.Actor_Category_Table.RelOff(0xC * 2 + 4).Deref();

            if (linkAddr == 0) //0x1DAA30;
                return;

            zoneoutRecord.Write(
                0x00, spawn.x,
                0x04, spawn.y,
                0x08, spawn.z,
                0x0C, spawn.rot,
                0x0E, (short)0x0DFF,
                0x12, spawn.room);

            if (index != null)
                zoneoutRecord.Write(0x18, index ?? 0);

            linkAddr.Write(
                0x0C, -5000.0f,
                0x28, -5000.0f);
        }

        private static bool TryGetCoordinateArgs(string[] args, int index, out Vector3<float> xyz)
        {
            xyz = new Vector3<float>();
            {
                if (int.TryParse(args[index + 0], out int x)
                          && int.TryParse(args[index + 1], out int y)
                          && int.TryParse(args[index + 2], out int z))
                {
                    xyz = new Vector3<float>(x, y, z);
                    return true;
                }
            }
            {
                if (float.TryParse(args[index + 0], out float x)
                    && float.TryParse(args[index + 1], out float y)
                    && float.TryParse(args[index + 2], out float z))
                {
                    xyz = new Vector3<float>((int)x, (int)y, (int)z);
                    return true;
                }
            }
            return false;
        }
        

        private static bool TryGetEntranceIndex(int scene, int ent, out EntranceIndex index)
        {
            index = new EntranceIndex();
            var indexQuery = from a in SpawnData.Indexes
                             where a.scene == scene && (a.ent == ent || a.ent == 0)
                             orderby a.ent descending
                             select a;

            var validIndexes = indexQuery.ToList();

            if (validIndexes.Count == 0)
                return false;

            index = validIndexes[0];
            return true;
        }

        private static void SetZoneoutSpawn_Ocarina(short? index, ref Spawn spawn)
        {
            //SaveContext = 0x11A5D0;
            Ptr zoneoutAddr = SaveContext.RelOff(0x1360);// sramAddr + 0x1360;
            Ptr linkAddr; 
            if (SpectrumVariables.Actor_Category_Table == 0)
                return;

            linkAddr = SpectrumVariables.Actor_Category_Table.RelOff(0x14).Deref();

            if (linkAddr == 0) //0x1DAA30;
                return;

            short health = SaveContext.ReadInt16(0x30);
            
            health += 0x10;
            SaveContext.Write(0x30, health);

            zoneoutAddr.Write(
                0x08, spawn.x,
                0x0C, spawn.y,
                0x10, spawn.z,
                0x14, spawn.rot,
                0x16, (short)0x0DFF,
                0x1A, spawn.room);

            if (index != null)
                zoneoutAddr.Write(0x18, index ?? 0);
            
            linkAddr.Write(
                0x0C, -5000.0f,
                0x28, -5000.0f);
        }

        private static Ptr GetLinkInstance()
        {
            Ptr linkAddr;
            if (Options.Version.Game == Game.OcarinaOfTime)
                linkAddr = SpectrumVariables.Actor_Category_Table.RelOff(0x14).Deref();
            else
                linkAddr = SpectrumVariables.Actor_Category_Table.RelOff(0x1C).Deref();

            if (linkAddr == 0) //0x1DAA30;
                return null;
            else
                return linkAddr;
        }

        private static void SetActorCoordinates(Ptr actorInstance, float? x, float? y, float? z)
        {
            if (SpectrumVariables.Actor_Category_Table == 0)
                return;

            if (x != null)
            {
                actorInstance.Write(
                    0x08, x ?? 0,
                    0x24, x ?? 0);
            }

            if (y != null)
            {
                actorInstance.Write(
                    0x0C, y ?? 0,
                    0x28, y ?? 0);
            }

            if (z != null)
            {
                actorInstance.Write(
                    0x10, z ?? 0,
                    0x2C, z ?? 0);
            }
        }


        private static bool TryEvaluate(string[] args, out long value)
        {
            return TryEvaluate(string.Join("", args), out value);
        }

        private static bool TryEvaluate(string s, out long value)
        {
            bool result = true;
            value = 0;
            try
            {
                value = Evaluator.Evaluate(s);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        private static void PrintActorDelta()
        {
            List<BlockNode> currentActorLL;
            List<IRamItem> delta = new List<IRamItem>();
            BlockNode prev;
            List<IRamItem> actors = new List<IRamItem>();

            actors.AddRange(OvlActor.GetActorFiles());
            actors.AddRange(InfoPoll.GetAllActorInstances());

            currentActorLL = GetActorLinkedList(actors);
            foreach (BlockNode item in currentActorLL)
            {
                prev = LastActorLL.SingleOrDefault(x => x.Ram == item.Ram);
                if (prev != item)
                    delta.Add(item);
            }
            Console.Clear();
            foreach (BlockNode a in delta.OrderBy(x => (x.Ram.Start & 0xFFFFFF)))
            {
                Console.WriteLine(a.ToString());
                if (a.ActorItem != null)
                    Console.WriteLine(a.ActorItem.ToString());
            }

            Console.WriteLine("PRE: " + InfoPoll.GetMemory(LastActorLL));
            Console.WriteLine("CUR: " + InfoPoll.GetMemory(currentActorLL));
            LastActorLL = currentActorLL;
        }

        private static void PrintAddresses(IEnumerable<IRamItem> Items)
        {
            Console.Clear();
            foreach (IRamItem item in Items)
                Console.WriteLine(string.Format("{0:X6}:{1:X6} {2}",
                    item.Ram.Start & 0xFFFFFF,
                    (!Options.ShowSize) ? item.Ram.End & 0xFFFFFF : item.Ram.Size & 0xFFFFFF,
                    item.ToString()));
        }
        
        private static List<IRamItem> GetRamMap(bool fetchAll = false)
        {
            List<IRamItem> ramItems = new List<IRamItem>
            {
                new RamDmadata(),
                new CodeFile()
            };
            if (fetchAll || Options.ShowObjects)
                ramItems.AddRange(RamObject.GetObjects());

            if (fetchAll || Options.ShowParticles)
                ramItems.AddRange(OvlParticle.GetFiles());

            ramItems.AddRange(OvlPause.GetActive());
            if (Options.Version == Game.OcarinaOfTime)
            {
                ramItems.AddRange(SegmentAddress.GetSegmentAddressMap(Options.ShowAllSegments));
            }

            ramItems.Add(RamScene.GetSceneInfo(Options.Version, GlobalContext, SpectrumVariables.SceneTable));

            ramItems.Add(RamRoom.GetRoomInfo());

            //Heap Blocklists
            ramItems.AddRange(BlockNode.GetBlockList(SpectrumVariables.Main_Heap_Ptr));
            if (SpectrumVariables.Debug_Heap_Ptr != 0)
                ramItems.AddRange(BlockNode.GetBlockList(SpectrumVariables.Debug_Heap_Ptr));
            if (fetchAll || Options.ShowLinkedList)
                ramItems.AddRange(GetActorLinkedList(ramItems));


            if (fetchAll || Options.ShowActors)
            {
                ramItems.AddRange(OvlActor.GetActorFiles().Where(x => !Options.HiddenActors.Contains(x.Actor)));
                ramItems.AddRange(InfoPoll.GetAllActorInstances().Where(x => !Options.HiddenActors.Contains(x.Actor)));
            }

            if (Options.Version == ORom.Build.N0
                && (fetchAll || Options.ShowThreadingStructs))
                ramItems.AddRange(ThreadStructs.GetIRamItems());

            return ramItems;
        }
        
        private static List<BlockNode> GetActorLinkedList(List<IRamItem> ramItems)
        {
            List<BlockNode> actorLL;
            actorLL = BlockNode.GetBlockList(SpectrumVariables.Scene_Heap_Ptr);

            foreach (BlockNode item in actorLL)
            {
                item.ActorItem = (IActorItem)ramItems.SingleOrDefault(x => x.Ram.Start == item.Ram.End);
            }
            return actorLL;
        }
    }
}
