using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mzxrules.OcaLib;
using mzxrules.Helper;

namespace Spectrum
{
    public class ActorMemoryMapper
    {
        static RomVersion Version;

        //actors
        public static int TOTAL_ACTORS;// 0x1D7, 0x2B2
        static int ACTOR_CATEGORY_TABLE_LENGTH = 12;
        static uint[] ActorInstanceSize;

        public List<OvlActor> Files = new();
        public List<ActorInstance> Instances = new();


        internal static void ChangeVersion(RomVersion version)
        {
            //Actors
            Version = version;
            TOTAL_ACTORS = (Version.Game == Game.OcarinaOfTime) ? 0x1D7 : 0x2B2;
            ActorInstanceSize = new uint[TOTAL_ACTORS];
        }

        public static ActorMemoryMapper FetchFiles()
        {
            ActorMemoryMapper map = new();
            map.GetFiles();
            map.GetInstances();
            return map;
        }

        public static ActorMemoryMapper FetchInstances()
        {
            return FetchFiles();
        }

        public static ActorMemoryMapper FetchFilesAndInstances()
        {
            return FetchFiles();
        }

        public uint GetActorInstanceSize(int actor)
        {
            return ActorInstanceSize[actor];
        }

        public void SetActorInstanceSize(OvlActor actor)
        {
            N64Ptr readAddr;
            uint offset;

            if (ActorInstanceSize[actor.Actor] <= 0)
            {
                if (actor.VRamActorInit.Offset < 0x800000)
                {
                    //Read address 0C in the ovl file
                    readAddr = actor.VRamActorInit + 0x0C;
                }
                else if (actor.IsFileLoaded)
                {
                    offset = (uint)(actor.VRamActorInit - actor.VRam.Start) + 0x0C;
                    readAddr = actor.Ram.Start + offset;
                }
                else
                    return;

                ActorInstanceSize[actor.Actor] = (uint)Zpr.ReadRamInt32(readAddr);
            }
        }

        internal void GetActorIdAndSize(ActorInstance actor, out ushort actorId, out uint instanceSize)
        {
            actorId = actor.ActorId;
            instanceSize = 0;

            if (actorId == 0 && actor.Type != 2)
            {
                if (Version.Game == Game.OcarinaOfTime)
                {
                    if (actorId == 0x28)
                    {

                        //ActorId = 0x008F;
                        actor.forcedActorId = true;
                        InferActorIdAndSize(SPtr.New(actor.Address), out actorId, out instanceSize);
                        return;
                    }
                }
            }
            
            if (actorId >= 0 && actorId < TOTAL_ACTORS)
            {
                // & 0xFFF is hack to correct instances with no proper actor id
                instanceSize = GetActorInstanceSize(actorId & 0xFFF);
            }
        }

        internal void InferActorIdAndSize(Ptr actor, out ushort actorId, out uint size)
        {
            actorId = 0;
            BlockNode node = new(actor - BlockNode.LENGTH);
            size = node.Size;

            N64Ptr update = actor.Deref(0x130);

            var file = Files.FirstOrDefault(x => x.Ram.IsInRange(update));
            if (file != null)
            {
                actorId = (ushort)file.Actor;
                size = GetActorInstanceSize(actorId);
            }
        }

        private void GetFiles()
        {
            Files = new List<OvlActor>();
            int length = ActorOverlayRecord.LENGTH;
            byte[] tableData;
            byte[] recordData = new byte[length];

            tableData = Zpr.ReadRam(SpectrumVariables.Actor_Ovl_Table, TOTAL_ACTORS * length);

            for (int i = 0; i < TOTAL_ACTORS; i++)
            {
                Array.Copy(tableData, i * length, recordData, 0, length);
                OvlActor working = new(i, recordData);
                SetActorInstanceSize(working);
                if (working.IsFileLoaded)
                    Files.Add(working);
            }
        }

        private void GetInstances()
        {
            Instances = new List<ActorInstance>();

            for (int i = 0; i < ACTOR_CATEGORY_TABLE_LENGTH; i++)
            {
                Instances.AddRange(GetActorsInCategory(i));
            }
        }

        private List<ActorInstance> GetActorsInCategory(int cat)
        {
            List<ActorInstance> result = new();
            int actors;
            N64Ptr actorPtr;

            if (SpectrumVariables.GlobalContext == 0)
                return new List<ActorInstance>();

            int recordSize = (Version.Game == Game.OcarinaOfTime) ? 0x08 : 0x0C;

            Ptr p = SpectrumVariables.Actor_Category_Table;
            actors = p.ReadInt32(cat * recordSize);
            actorPtr = p.ReadInt32(cat * recordSize + 4);

            if (actors > 0 && actors <= 255)
            {
                for (int j = 0; j < actors; j++)
                {
                    ActorInstance ai = new(Version, actorPtr, this);
                    result.Add(ai);
                    actorPtr = ai.NextActor;
                }
            }
            return result;
        }
    }

    public static class MemoryMapper
    {
        static RomVersion version;

        static int OBJECT_FILE_COUNT; // = 0x192; 0x283
        public static FileAddress[] ObjectFiles;

        static int TOTAL_PARTICLE_EFFECTS; //

        public const int PLAY_PAUSE_RECORDS = 2;


        internal static void ChangeVersion((SpectrumOptions options, bool gctx) args)
        {
            var (options, gctx) = args;
            version = options.Version;
            ActorMemoryMapper.ChangeVersion(version);

            SetBlockNodeLength(version);

            //Particles

            if (version.Game == Game.OcarinaOfTime)
                TOTAL_PARTICLE_EFFECTS = 0x19;
            else
                TOTAL_PARTICLE_EFFECTS = 0x27;

            //Objects

            if (version.Game == Game.OcarinaOfTime)
                OBJECT_FILE_COUNT = 0x192;
            else if (version.Game == Game.MajorasMask)
                OBJECT_FILE_COUNT = 0x283;

            ObjectFiles = new FileAddress[OBJECT_FILE_COUNT];

            Ptr ptr = SPtr.New(SpectrumVariables.Object_File_Table);

            for (int i = 0; i < OBJECT_FILE_COUNT; i++)
            {
                int off = i * 8;
                ObjectFiles[i] = new FileAddress(ptr.ReadInt32(off), ptr.ReadInt32(off + 4));
            }
        }


        private static void SetBlockNodeLength(RomVersion v)
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

        internal static string GetMemoryUsage(List<BlockNode> list)
        {
            uint total = 0;
            uint available = 0;
            uint largestBlock = 0;
            foreach (BlockNode item in list)
            {
                total += item.Size + (uint)BlockNode.LENGTH;
                if (item.IsFree)
                {
                    available += item.Size;
                    if (largestBlock < item.Size)
                        largestBlock = item.Size;
                }
            }

            return $"Actor Mem: {available:X6}/{total:X6} Largest Block {largestBlock:X6}";
        }


        #region Actor Funcs



        #endregion

        #region Object Funcs


        internal static List<RamObject> GetObjectFiles()
        {
            List<RamObject> ovlObjects = new();

            if (SpectrumVariables.GlobalContext == 0)
                return ovlObjects;

            Ptr allocTable = SpectrumVariables.Object_Allocation_Table;

            int objCount = allocTable.ReadByte(0x08);

            Ptr ptr = allocTable.RelOff(0xC);

            for (int i = 0; i < objCount; i++)
            {
                RamObject working = new(ptr);
                ovlObjects.Add(working);

                ptr = ptr.RelOff(RamObject.LENGTH);
            }
            return ovlObjects;
        }

        #endregion

        public static List<ParticleOverlayRecord> GetParticleOverlayRecords()
        {
            List<ParticleOverlayRecord> records = new();
            int length = ParticleOverlayRecord.LENGTH;
            byte[] tableData;
            byte[] recordData = new byte[length];

            tableData = Zpr.ReadRam(SpectrumVariables.ParticleEffect_Ovl_Table, TOTAL_PARTICLE_EFFECTS * length);
            tableData.Reverse32();

            for (int i = 0; i < TOTAL_PARTICLE_EFFECTS; i++)
            {
                Array.Copy(tableData, i * length, recordData, 0, length);
                Endian.Reverse32(recordData);

                ParticleOverlayRecord item = new(i, recordData);
                records.Add(item);
            }
            return records;
        }

        public static List<OvlParticle> GetParticleFiles()
        {
            List<OvlParticle> particleFiles = new();

            foreach (var record in GetParticleOverlayRecords())
            {
                OvlParticle working = new(record);
                if (working.IsFileLoaded)
                    particleFiles.Add(working);
            }
            return particleFiles;
        }


        public static List<PlayPauseOverlayRecord> GetPlayPauseOverlayRecords()
        {
            List<PlayPauseOverlayRecord> records = new();
            int length = PlayPauseOverlayRecord.LENGTH;
            byte[] tableData = Zpr.ReadRam(SpectrumVariables.Player_Pause_Ovl_Table, PLAY_PAUSE_RECORDS * length);
            tableData.Reverse32();
            byte[] recordData = new byte[length];

            for (int i = 0; i < PLAY_PAUSE_RECORDS; i++)
            {
                Array.Copy(tableData, i * length, recordData, 0, length);
                Endian.Reverse32(recordData);

                PlayPauseOverlayRecord item = new(i, recordData);
                records.Add(item);
            }
            return records;
        }


        internal static List<OvlPause> GetActivePlayPause()
        {
            List<OvlPause> ovlPause = new();

            foreach (var item in GetPlayPauseOverlayRecords())
            {
                OvlPause working = new(item);
                if (!working.Ram.Start.IsNull())
                {
                    ovlPause.Add(working);
                }
            }
            return ovlPause;
        }

        internal static OverlayRecord GetOverlayRecord()
        {
            return null;
        }

        internal static bool TryGetStaticContext(out IRamItem staticCtx)
        {
            List<BlockNode> mainHeap = BlockNode.GetBlockList(SpectrumVariables.Main_Heap_Ptr);
            staticCtx = null;
            if (mainHeap.Count > 0)
            {
                var item = mainHeap[0];
                staticCtx = new SimpleRamItem()
                {
                    Ram = new N64PtrRange(item.Ram.End, item.Ram.End + item.Size),
                    Description = "STATIC CONTEXT"
                };
                return true;
            }
            return false;
        }

        enum GameState
        {
            NONE,
            NA,
            Select,
            Title,
            Game_Play,
            Opening,
            File_Choose
        }

        internal static List<IRamItem> GetRamMap(SpectrumOptions Options, bool fetchAll = false)
        {
            // Get core files
            List<IRamItem> ramItems = new List<IRamItem>
            {
                new RamDmadata(),
                new CodeFile()
            };

            //Map out the heap nodes

            //Heap Blocklists
            List<BlockNode> mainHeap = BlockNode.GetBlockList(SpectrumVariables.Main_Heap_Ptr);
            List<BlockNode> gameHeap = BlockNode.GetBlockList(SpectrumVariables.Scene_Heap_Ptr);
            List<BlockNode> debugHeap = BlockNode.GetBlockList(SpectrumVariables.Debug_Heap_Ptr);

            //create a SimpleRamItem for the STATIC CONTEXT node
            if (mainHeap.Count > 0)
            {
                var item = mainHeap[0];
                SimpleRamItem ramItem = new()
                {
                    Ram = new N64PtrRange(item.Ram.End, item.Ram.End + item.Size),
                    Description = "STATIC CONTEXT"
                };
                item.RamItem = item;
                ramItems.Add(ramItem);
            }

            //create a SimpleRamItem for the GAME STATE node
            var globalCtx = mainHeap.SingleOrDefault(x => x.Ram.End == SpectrumVariables.GlobalContext);
            if (globalCtx != null)
            {
                SimpleRamItem ramItem = new()
                {
                    Ram = new N64PtrRange(globalCtx.Ram.End, globalCtx.Ram.End + globalCtx.Size),
                    Description = "GAME STATE"
                };
                globalCtx.RamItem = ramItem;
                ramItems.Add(ramItem);
            }

            //map code file data

            if (fetchAll || Options.ShowThreadingStructs)
                ramItems.AddRange(ThreadStack.GetIRamItems());

            if (Options.Version == Game.OcarinaOfTime)
            {
                ramItems.AddRange(SegmentAddress.GetSegmentAddressMap(Options.ShowAllSegments));
            }

            OvlGamestate curOvlGamestate = null;
            GameState gameState = GameState.NONE;
            const int GAMESTATE_TOTAL_RECORDS = 6;
            const int GAMESTATE_RECORD_LENGTH = 0x30;
            //infer current gamestate
            if (Options.Version == Game.OcarinaOfTime)
            {
                OvlGamestate[] table = new OvlGamestate[6];
                GameState[] gamestateIds = { 
                    GameState.NA, GameState.Select, GameState.Title, GameState.Game_Play, GameState.Opening, GameState.File_Choose };
                string[] gamestateNames =
                {
                    "N/A",
                    "ovl_select",
                    "ovl_title",
                    "game_play",
                    "ovl_opening",
                    "ovl_file_choose"
                };
                byte[] tableData;
                byte[] recordData = new byte[GAMESTATE_RECORD_LENGTH];

                tableData = Zpr.ReadRam(SpectrumVariables.Gamestate_Table, GAMESTATE_TOTAL_RECORDS * GAMESTATE_RECORD_LENGTH);

                for (int i = 0; i < GAMESTATE_TOTAL_RECORDS; i++)
                {
                    Array.Copy(tableData, i * GAMESTATE_RECORD_LENGTH, recordData, 0, GAMESTATE_RECORD_LENGTH);
                    table[i] = new OvlGamestate(i, recordData, Options.Version);
                    table[i].Name = gamestateNames[i];
                }
                for (int i = 0; i < GAMESTATE_TOTAL_RECORDS; i++)
                {
                    if (!table[i].RamStart.IsNull())
                    {
                        curOvlGamestate = table[i];
                        gameState = gamestateIds[i];
                        break;
                    }
                }
                if (gameState == GameState.NONE)
                {
                    for (int i = 0; i < GAMESTATE_TOTAL_RECORDS; i++)
                    {
                        if ((table[i].AllocateSize + 0xF & -0x10) == globalCtx.Size)
                        {
                            gameState = gamestateIds[i];
                            break;
                        }    
                    }
                }
            }
            else
            {
                gameState = GameState.Game_Play;
            }

            switch(gameState)
            {
                case GameState.Game_Play:
                    MapGameplayState(Options, fetchAll, ramItems);
                    break;
            }
            if (curOvlGamestate != null)
            {
                ramItems.Add(curOvlGamestate);
            }

            //create a SimpleRamItem for each non-named node
            foreach (var heap in new List<List<BlockNode>>() { mainHeap, gameHeap, debugHeap })
            {
                foreach (var item in heap.Where(x => !x.IsFree && x.RamItem == null))
                {
                    //if the node is corrupted, don't create an item for it
                    if (item.Size >= 0x100000)
                        continue;

                    item.RamItem = ramItems.FirstOrDefault(x => x.Ram.Start == item.Ram.End);
                    if (item.RamItem == null)
                    {
                        ramItems.Add(new SimpleRamItem()
                        {
                            Ram = new N64PtrRange(item.Ram.End, item.Ram.End + item.Size),
                            Description = "UNKNOWN"
                        });
                    }
                }
            }

            ramItems.AddRange(mainHeap);
            if (fetchAll || Options.ShowLinkedList)
                ramItems.AddRange(gameHeap);

            ramItems.AddRange(debugHeap);
            return ramItems;
        }

        private static void MapGameplayState(SpectrumOptions Options, bool fetchAll, List<IRamItem> ramItems)
        {
            // Add files that are also mapped in ROM
            ActorMemoryMapper actorMap = null;
            if (fetchAll || Options.ShowActors)
            {
                actorMap = ActorMemoryMapper.FetchFilesAndInstances();
            }

            if (fetchAll || Options.ShowObjects)
                ramItems.AddRange(GetObjectFiles());

            if (fetchAll || Options.ShowParticles)
                ramItems.AddRange(GetParticleFiles());

            ramItems.AddRange(GetActivePlayPause());
            ramItems.Add(RamScene.GetSceneInfo(Options.Version, SpectrumVariables.GlobalContext, SpectrumVariables.SceneTable));
            ramItems.AddRange(RamRoom.GetRoomInfo());


            if (fetchAll || Options.ShowActors)
            {
                ramItems.AddRange(actorMap.Files.Where(x => !Options.HiddenActors.Contains(x.Actor)));
            }

            // Files mapped in ram
            if (fetchAll || Options.ShowActors)
            {
                ramItems.AddRange(actorMap.Instances.Where(x => !Options.HiddenActors.Contains(x.Actor)));
            }

            CollisionCtx ctx = new (Program.GetColCtxPtr(), Options.Version);
            ramItems.AddRange(ctx.GetRamMap());
        }
    }
}
