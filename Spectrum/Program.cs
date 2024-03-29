﻿using mzxrules.Helper;
using mzxrules.OcaLib;
using mzxrules.OcaLib.PathUtil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Spectrum
{
    struct FrameHalt { public int update; public int deconstructor; public FrameHalt(int u, int d) { update = u; deconstructor = d; } }
    partial class Program
    {
        const string TITLE = "Spectrum - Time never really passes in Hyrule... does it?";
        static SpectrumOptions Options = new();
        static Rom curRom;
        public delegate void SetVersionEventHandler((SpectrumOptions options, bool setGctx) args);
        public static event SetVersionEventHandler ChangeVersion;
        static List<BlockNode> LastActorLL = new();
        static readonly ExpressTest.ExpressionEvaluator Evaluator = new((x) => Zpr.ReadRamInt32((int)x) & 0xFFFFFFFF);
        static Dictionary<string, (SpectrumCommand attr, SpectrumCommandSignature[] args, CommandDelegate method)> CommandDictionary;

        static CollisionAutoDoc CollisionActorDoc = new();
        static ReferenceLogger<DisplayListRecord> DisplayListLogger = new();

        public static Ptr SaveContext { get { return SpectrumVariables.SaveContext; } }
        public static Ptr GlobalContext { get { return SpectrumVariables.GlobalContext; } }
        public static Ptr Gfx { get { return SpectrumVariables.Gfx; } }

        public static FrameHalt FrameHaltVars = new(0, 0);
        const int EXECUTE_PTR = unchecked((int)0x80000490);

        static void Main(string[] args)
        {
            EncodingExtension.RegisterCodePagesEncodingProvider();
            Console.OutputEncoding = Encoding.Unicode;
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

            Console.WriteLine($"Created by mzxrules 2014-2022, compiled {Timestamp}");
            
            CommandDictionary = BuildCommands();
            SpawnData.Load();
            LoadSettings();
            PathUtil.Initialize();

            if (!Directory.Exists("dump"))
            {
                Directory.CreateDirectory("dump");
            }

            ChangeVersion += args => { Console.WriteLine($"Configuring game {args.options.Version.GameNiceName} version {args.options.Version}"); };
            ChangeVersion += SpectrumVariables.ChangeVersion;
            ChangeVersion += MemoryMapper.ChangeVersion;
            ChangeVersion += UpdateSetVersion;
            ChangeVersion += args => { Console.WriteLine("Game setup complete."); };

            MountEmulator("");
        }

        private static void ProcessCommand(CommandRequest request)
        {
            if (!CommandDictionary.TryGetValue(request.CommandName, out var value))
            {
                return;
            }

            if (!value.attr.IsSupported(Options.Version))
            {
                Console.WriteLine("Command not supported for selected game or version");
                return;
            }

            foreach (var signature in value.args)
            {
                Arguments args = new(request.Arguments, signature.Sig);
                if (args.Valid)
                {
                    value.method(args);
                    return;
                }
            }
        }

        public delegate void CommandDelegate(Arguments args);

        static Dictionary<string, (SpectrumCommand attr, SpectrumCommandSignature[] args, CommandDelegate method)> BuildCommands()
        {
            var methods = typeof(Program).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => Attribute.IsDefined(m, typeof(SpectrumCommand))).ToList();

            var Commands = new Dictionary<string, (SpectrumCommand, SpectrumCommandSignature[], CommandDelegate)>();

            foreach (var method in methods)
            {
                var commandAttr = (SpectrumCommand)method.GetCustomAttribute(typeof(SpectrumCommand));
                var argsAttr = (SpectrumCommandSignature[])method.GetCustomAttributes(typeof(SpectrumCommandSignature));
                if (argsAttr.Length == 0)
                {
                    argsAttr = new SpectrumCommandSignature[]
                    {
                        new SpectrumCommandSignature()
                        {
                            Sig = Array.Empty<Tokens>()
                        }
                    };
                }
                var command = (CommandDelegate)method.CreateDelegate(typeof(CommandDelegate));
#if DEBUG
                if (commandAttr.Cat == SpectrumCommand.Category.Proto)
                    continue;
#endif
                Commands.Add(commandAttr.Name, (commandAttr, argsAttr, command));
            }
            return Commands;
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

        private static double CalculateDistance3D(Vector3<float> position1, Vector3<float> position2)
        {
            Vector3<float> delta = new(position2.x - position1.x, position2.y - position1.y, position2.z - position1.z);
            double result = Math.Sqrt((double)delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
            return result;
        }

        private static int GetRamSize()
        {
            Ptr ptr = SPtr.New(0x318);

            int value = ptr.ReadInt32(0);

            if (value == 0x40_0000)
                return value;

            if (value == 0x80_0000)
                return value;

            return 0x40_0000;
        }

        private static void WriteRam(string expression, object value)
        {
            if (!TryEvaluate(expression, out long address))
                return;

            N64Ptr addr = address;
            WriteRam(addr, value);
        }

        private static void WriteRam(N64Ptr addr, object value)
        {
            if (addr.Offset >= GetRamSize())
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
            using StreamWriter sw = new("dump/collision.txt");
            foreach (var item in CollisionActorDoc.Meshes.Values)
            {
                sw.WriteLine(item.ToString());
            }
        }

        private static void DumpReferenceLogger<T>(ReferenceLogger<T> logger, string path)
        {
            using StreamWriter sw = new(path);
            foreach (var item in logger.LoggedReferences)
            {
                sw.WriteLine(item.ToString());
            }
        }


        private static IFile GetIFile(IEnumerable<IFile> ramMap, N64Ptr address)
        {
            var addr = address.Offset;
            return ramMap.Where(x =>
                x.Ram.Start.Offset <= addr
                && x.Ram.End.Offset > addr).SingleOrDefault();
        }

        private static IFile GetIFile(N64Ptr address)
        {
            return MemoryMapper.GetRamMap(Options, true).Where(x =>
                x.Ram.Start.Offset  <= address.Offset
                && x.Ram.End.Offset  > address.Offset).OfType<IFile>().SingleOrDefault();
        }

        private static GfxDList[] GetFrameDlists(Ptr Gfx)
        {
            if (Options.Version == Game.OcarinaOfTime)
            {
                GfxDList Work_Disp = new("WORK_DISP", Gfx.RelOff(0x1B4));
                GfxDList Overlay_Disp = new("OVERLAY_DISP", Gfx.RelOff(0x2A8));
                GfxDList Poly_Opa_Disp = new("POLY_OPA_DISP", Gfx.RelOff(0x2B8));
                GfxDList Poly_Xlu_Disp = new("POLY_XLU_DISP", Gfx.RelOff(0x2C8));
                return new GfxDList[] { Work_Disp, Poly_Opa_Disp, Poly_Xlu_Disp, Overlay_Disp };
            }
            else if(Options.Version == Game.MajorasMask)
            {
                GfxDList Work_Disp = new("WORK_DISP", Gfx.RelOff(0x1A4));
                GfxDList Overlay_Disp = new("OVERLAY_DISP", Gfx.RelOff(0x298));
                GfxDList Poly_Opa_Disp = new("POLY_OPA_DISP", Gfx.RelOff(0x2A8));
                GfxDList Poly_Xlu_Disp = new("POLY_XLU_DISP", Gfx.RelOff(0x2B8));
                return new GfxDList[] { Work_Disp, Poly_Opa_Disp, Poly_Xlu_Disp, Overlay_Disp };
            }
            return null;
        }

        private static void UpdateSetVersion((SpectrumOptions options, bool g) args)
        {
            var v = args.options.Version;
            string gameStr = v.GameNiceName;
            string buildStr = "?";
            
            if (v.Game == Game.OcarinaOfTime)
            {
                buildStr = ORom.BuildInformation.Get(v).Name;
            }
            else if (v.Game == Game.MajorasMask)
            {
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
            using StreamWriter sw = new(savePath);
            using MemoryStream ms = new();
            DataContractJsonSerializer serializer = new(jsonType);

            serializer.WriteObject(ms, graph);

            StreamReader sr = new(ms);
            sr.BaseStream.Position = 0;
            sw.Write(FormatJson(sr.ReadToEnd()));
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
                using (FileStream fs = new(path, FileMode.Open))
                {
                    DataContractJsonSerializer serializer = new(typeof(T));
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
            var zoneoutOff = (MRom.Build)Options.Version switch
            {
                MRom.Build.J0 => 0x3F68,
                MRom.Build.J1 => 0x3F68,
                _ => 0x3CB4,
            };
            Ptr zoneoutRecord = SaveContext.RelOff(zoneoutOff);
            Ptr linkAddr;
            if (SpectrumVariables.Actor_Category_Table == 0)
                return;

            linkAddr = SpectrumVariables.Actor_Category_Table.RelOff(0xC * 2 + 4).Deref();

            if (linkAddr == 0)
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

        private static Vector3<float> GetActorCoordinates(Ptr actorInstance)
        {
            if (SpectrumVariables.Actor_Category_Table == 0)
                return new(float.MaxValue, float.MaxValue, float.MaxValue);
            return actorInstance.ReadVec3f(0x24);
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
            List<IRamItem> delta = new();
            BlockNode prev;
            List<IRamItem> actors = new();

            var map = ActorMemoryMapper.FetchFilesAndInstances();

            actors.AddRange(map.Files);
            actors.AddRange(map.Instances);


            List<BlockNode> actorLL = BlockNode.GetBlockList(SpectrumVariables.Scene_Heap_Ptr);
            currentActorLL = GetActorLinkedList(actorLL, actors);
            foreach (BlockNode item in currentActorLL)
            {
                prev = LastActorLL.SingleOrDefault(x => x.Ram == item.Ram);
                if (prev != item)
                    delta.Add(item);
            }
            Console.Clear();
            foreach (BlockNode a in delta.OrderBy(x => x.Ram.Start.Offset))
            {
                Console.WriteLine(a.ToString());
                if (a.RamItem != null)
                    Console.WriteLine(a.RamItem.ToString());
            }

            Console.WriteLine("PRE: " + MemoryMapper.GetMemoryUsage(LastActorLL));
            Console.WriteLine("CUR: " + MemoryMapper.GetMemoryUsage(currentActorLL));
            LastActorLL = currentActorLL;
        }

        private static void PrintAddresses(IEnumerable<IRamItem> Items)
        {
            Console.Clear();
            foreach (IRamItem item in Items)
                Console.WriteLine(string.Format("{0:X6}:{1:X6} {2}",
                    item.Ram.Start.Offset,
                    (!Options.ShowSize) ? item.Ram.End.Offset : item.Ram.Size.Offset,
                    item.ToString()));
        }

        private static List<BlockNode> GetActorLinkedList(List<BlockNode> nodes, List<IRamItem> ramItems)
        {
            foreach (BlockNode item in nodes)
            {
                item.RamItem = ramItems.FirstOrDefault(x => x.Ram.Start == item.Ram.End);
            }
            return nodes;
        }
    }
}
