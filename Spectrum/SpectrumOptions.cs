using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Spectrum
{
    [DataContract]
    class SpectrumOptions
    {
        static RomVersion DEFAULT_VERSION = ORom.Build.N0;
        
        [DataMember]
        public Dictionary<string, Emulator> Emulators { get; set; }
        
        [DataMember]
        public RomVersion Version { get; set; }
        
        [DataMember]
        public bool ShowLinkedList = false;
        [DataMember]
        public bool ShowObjects = true;
        [DataMember]
        public bool ShowSize = false;
        [DataMember]
        public bool ShowActors = true;
        [DataMember]
        public bool ShowAllSegments = false;
        [DataMember]
        public bool ShowParticles = true;


        [DataMember]
        public bool ShowThreadingStructs = false;

        [DataMember]
        public bool EnableDataLogging = false;
        
        public List<int> HiddenActors;

        public SpectrumOptions()
        {
            Initialize();
        }
        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
            Initialize();
        }
        private void Initialize()
        {
            if (Version == null)
                Version = DEFAULT_VERSION;

            if (Emulators == null)
                Emulators = new Dictionary<string, Emulator>();

            HiddenActors = new List<int>();
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ViewVariableAttribute : Attribute
    {
        public string Description;

        public ViewVariableAttribute(string description = null)
        {
            Description = description;
        }
    }
    
    static class SpectrumVariables
    {
        [ViewVariable]
        public static int Player_Pause_Ovl_Table; // = 0x0FE480;
        [ViewVariable]
        public static int Actor_Ovl_Table;// = 0x0E8530;

        //Global Context
        [ViewVariable("Global Context")]
        public static Ptr GlobalContext;
        [ViewVariable]
        public static Ptr Actor_Category_Table;// = 0x1CA0D0;
        [ViewVariable]
        public static Ptr Scene_Heap_Ptr;

        [ViewVariable]
        public static int Object_File_Table;
        [ViewVariable]
        public static Ptr Object_Allocation_Table;// = 0x1D9C44;

        [ViewVariable]
        public static Ptr Room_Allocation_Table;

        [ViewVariable("Graphics Context")]
        public static Ptr Gfx;

        [ViewVariable]
        public static Ptr Main_Heap_Ptr;
        [ViewVariable("Save Context")]
        public static Ptr SaveContext;
        [ViewVariable]
        public static int ParticleEffect_Ovl_Table;
        [ViewVariable]
        public static int Segment_Table;

        [ViewVariable ("code")]
        public static int Code_Addr;
        public static int Code_VRom;

        [ViewVariable("dmadata")]
        public static Ptr Dmadata_Addr;

        [ViewVariable]
        public static Ptr Debug_Heap_Ptr;

        public static Ptr SceneTable;
        public static Ptr EntranceTable;

        public static Ptr Queue_Thread_Ptr;
        public static Ptr Stack_List_Ptr;

        public static void GetVariables()
        {
            List<(int addr, string desc)> Values = new List<(int, string)>();
            foreach (System.Reflection.FieldInfo field in typeof(SpectrumVariables).GetFields())
            {
                ViewVariableAttribute attribute = (ViewVariableAttribute)Attribute.GetCustomAttribute(field, typeof(ViewVariableAttribute));
                if (attribute != null)
                {
                    var fieldVar = field.GetValue(null);

                    string description = attribute.Description;

                    if (description == null)
                        description = field.Name.Replace('_', ' ');
                    
                    if (fieldVar is int fieldInt)
                    {
                        Values.Add((fieldInt, description));
                    }
                    else if (fieldVar is Ptr fieldSPtr)
                    {
                        description = $"{description,-24:X8} {fieldSPtr.GetChain()}";
                        Values.Add((fieldSPtr, description));
                    }
                }
            }
            Console.Clear();
            foreach (var item in Values.OrderBy(x => x.addr))
            {
                Console.WriteLine($"{item.addr:X8} - {item.desc}");
            }
        }

        static SpectrumVariables()
        {
            GlobalContext = SPtr.New(0);
            SaveContext =  SPtr.New(0);
        }

        internal static void ChangeVersion(RomVersion version, bool setGctx = true)
        {
            RomFileToken fileToken;

            //dma data
            fileToken = (version == Game.OcarinaOfTime) ?
                (RomFileToken)ORom.FileList.dmadata : (RomFileToken)MRom.FileList.dmadata;
            Addresser.TryGetRam(fileToken, version, out int temp);
            Dmadata_Addr = SPtr.New(temp);

            //code
            fileToken = (version == Game.OcarinaOfTime) ?
                (RomFileToken)ORom.FileList.code : (RomFileToken)MRom.FileList.code;
            Addresser.TryGetRam(fileToken, version, out Code_Addr);
            Addresser.TryGetRom(fileToken, version, (uint)Code_Addr, out Code_VRom);

            //Global Context
            if (setGctx)
            {
                Addresser.TryGetRam("RAM_GLOBAL_CONTEXT", version, out temp);
                if (version == ORom.Build.IQUEC || version == ORom.Build.IQUET)
                {
                    GlobalContext = SPtr.New(temp);
                }
                else
                {
                    GlobalContext = SPtr.New(temp).Deref();
                }
            }

            SetGfxContext(version);

            //Heap
            Addresser.TryGetRam("RAM_ARENA_MAIN", version, out temp);
            Main_Heap_Ptr = SPtr.New(temp).Deref(); 

            Addresser.TryGetRam("RAM_ARENA_SCENES", version, out temp);
            Scene_Heap_Ptr = SPtr.New(temp).Deref();
            
            Addresser.TryGetRam("RAM_ARENA_DEBUG", version, out temp);
            if (temp == 0)
                Debug_Heap_Ptr = SPtr.New(0);
            else
                Debug_Heap_Ptr = SPtr.New(temp).Deref();


            Addresser.TryGetOffset("ACTOR_CAT_LL_Start", version, out temp);
            Actor_Category_Table = GlobalContext.RelOff(temp);

            //Overlay Tables
            Addresser.TryGetRam("ActorTable_Start", ORom.FileList.code, version, out Actor_Ovl_Table);
            Addresser.TryGetRam("PlayerPauseOverlayTable_Start", ORom.FileList.code, version, out Player_Pause_Ovl_Table);
            Addresser.TryGetRam("ParticleTable_Start", ORom.FileList.code, version, out ParticleEffect_Ovl_Table);
            Addresser.TryGetRam("ObjectTable_Start", ORom.FileList.code, version, out Object_File_Table);


            Addresser.TryGetOffset("OBJ_ALLOC_TABLE", version, out temp);
            Object_Allocation_Table = GlobalContext.RelOff(temp);

            Addresser.TryGetOffset("ROOM_ALLOC_ADDR", version, out temp);
            Room_Allocation_Table = GlobalContext.RelOff(temp);


            Addresser.TryGetRam("SRAM_START", version, out temp);
            SaveContext = SPtr.New(temp);

            Addresser.TryGetRam("RAM_SEGMENT_TABLE", version, out temp);
            Segment_Table = temp;

            if (Addresser.TryGetRam("SceneTable_Start", version, out temp))
                SceneTable = SPtr.New(temp);
            else
                SceneTable = null;

            if (Addresser.TryGetRam("EntranceIndexTable_Start", version, out temp))
                EntranceTable = SPtr.New(temp);
            else
                EntranceTable = null;

            Addresser.TryGetRam("QUEUE_THREAD", version, out temp);
            Queue_Thread_Ptr = SPtr.New(temp);

            Addresser.TryGetRam("STACK_LIST", version, out temp);
            Stack_List_Ptr = SPtr.New(temp);
        }

        public static void SetGfxContext(RomVersion version)
        {
            if (!Addresser.TryGetRam("GFX_START", version, out int temp))
            {
                if (GlobalContext != 0)
                    Gfx = GlobalContext.Deref();
                else
                    Gfx = SPtr.New(0);
            }
            else
            {
                Gfx = SPtr.New(temp);
            }
        }
        public static void SetGfxContext(int addr)
        {
            Gfx = SPtr.New(addr);
        }
        
    }
}
