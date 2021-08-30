using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using mzxrules.OcaLib.SymbolMapParser;
using mzxrules.OcaLib.Addr2;

namespace Spectrum
{
    [DataContract]
    class MapfileOptions
    {
        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public RomVersion Version { get; set; }

        [DataMember]
        public bool UseMap { get; set; }

        public SymbolMap SymbolMap { get; set; }

        public bool CanUseMap(RomVersion version)
        {
            return UseMap && Version == version && SymbolMap != null;
        }
    }

    [DataContract]
    class SpectrumOptions
    {
        [DataMember]
        public Dictionary<string, Emulator> Emulators { get; set; }

        [DataMember]
        public RomVersion Version { get; set; }

        [DataMember]
        public MapfileOptions MapfileOptions { get; set; }
        
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
            if (Emulators == null)
                Emulators = new Dictionary<string, Emulator>();

            if (MapfileOptions == null)
                MapfileOptions = new MapfileOptions();

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
        static SpectrumOptions Options;

        [ViewVariable]
        public static int Gamestate_Table;
        [ViewVariable]
        public static int Player_Pause_Ovl_Table; // = 0x0FE480;
        [ViewVariable]
        public static int Actor_Ovl_Table;// = 0x0E8530;
        [ViewVariable]
        public static int Transition_Ovl_Table;

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
        public static Ptr Room_Context; 

        [ViewVariable]
        public static Ptr Room_Allocation_Table;

        [ViewVariable]
        public static Ptr Room_List_Ptr;

        [ViewVariable("Graphics Context")]
        public static Ptr Gfx;

        [ViewVariable]
        public static Ptr Main_Heap_Ptr;
        [ViewVariable("Save Context")]
        public static Ptr SaveContext;
        [ViewVariable]
        public static int ParticleEffect_Ovl_Table; // = 0x0E8530;
        [ViewVariable]
        public static Ptr Segment_Table;

        [ViewVariable ("code")]
        public static Ptr Code_Addr;
        public static Ptr Code_VRom;

        [ViewVariable("dmadata")]
        public static Ptr Dmadata_Addr;
        public static Ptr Dmadata_VRom;

        [ViewVariable]
        public static Ptr Debug_Heap_Ptr;

        public static Ptr SceneTable;
        public static Ptr EntranceTable;

        public static Ptr Queue_Thread_Ptr;
        public static Ptr Stack_List_Ptr;

        public static void GetVariables()
        {
            List<(int addr, string desc)> Values = new();
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
            foreach (var (addr, desc) in Values.OrderBy(x => x.addr))
            {
                Console.WriteLine($"{addr:X8} - {desc}");
            }
        }

        static SpectrumVariables()
        {
            GlobalContext = SPtr.New(0);
            SaveContext =  SPtr.New(0);
        }

        static Ptr BindPtr(AddressToken token, RomVersion version, Func<int, Ptr> func)
        {
            if (Options.MapfileOptions.CanUseMap(version))
            {
                if (Addresser.TryGetSymbolRef(token, version, out MapBinding binding) && binding != null
                    && Options.MapfileOptions.SymbolMap.TryGetSymbolAddress(binding, out N64Ptr ptr))
                {
                    return func(ptr);
                }
                Console.WriteLine($"{token} symbol not found");
                return SPtr.New(0);    
            }    
            else if (Addresser.TryGetRam(token, version, out int temp))
                return func(temp);
            else
                return SPtr.New(0);
        }

        static Ptr BindOff(AddressToken token, RomVersion version, Func<int, Ptr> func)
        {
            Addresser.TryGetOffset(token, version, out int temp);
            return func(temp);
        }

        static void BindSegment(RomVersion version, ORom.FileList fileO, MRom.FileList fileM, ref Ptr ram, ref Ptr rom)
        {
            RomFileToken token = RomFileToken.Select(version, fileO, fileM);
            if (Options.MapfileOptions.CanUseMap(version))
            {
                Segment seg = Options.MapfileOptions.SymbolMap.GetSegment(token.ToString());
                if (seg != null)
                {
                    ram = SPtr.New(seg.Address);
                    rom = SPtr.New(seg.LoadAddress);
                }
                else
                {
                    Console.WriteLine($"Segment {token} not found.");
                }
            }
            else
            {
                Addresser.TryGetRam(token, version, out var t1);
                ram = SPtr.New(t1);
                Addresser.TryGetRom(token, version, t1, out var t2);
                rom = SPtr.New(t2);
            }
        }

        internal static void ChangeVersion((SpectrumOptions options, bool setGctx) args)
        {
            var (options, setGctx) = args;
            Options = options;
            RomVersion version = options.Version;

            //dma data
            BindSegment(version, ORom.FileList.dmadata, MRom.FileList.dmadata, ref Dmadata_Addr, ref Dmadata_VRom);

            //code
            BindSegment(version, ORom.FileList.code, MRom.FileList.code, ref Code_Addr, ref Code_VRom);

            //Heap
            Main_Heap_Ptr = BindPtr(AddressToken.RAM_ARENA_MAIN, version, x => SPtr.New(x).Deref());
            Scene_Heap_Ptr = BindPtr(AddressToken.RAM_ARENA_SCENES, version, x => SPtr.New(x).Deref());
            Debug_Heap_Ptr = BindPtr(AddressToken.RAM_ARENA_DEBUG, version, x => SPtr.New(x).Deref());

            //Overlay Tables
            Gamestate_Table = BindPtr(AddressToken.GameContextTable_Start, version, x => SPtr.New(x));
            Actor_Ovl_Table = BindPtr(AddressToken.ActorTable_Start, version, x => SPtr.New(x));
            Player_Pause_Ovl_Table = BindPtr(AddressToken.PlayerPauseOverlayTable_Start, version, x => SPtr.New(x));
            ParticleEffect_Ovl_Table = BindPtr(AddressToken.ParticleTable_Start, version, x => SPtr.New(x));
            Transition_Ovl_Table = BindPtr(AddressToken.TransitionTable_Start, version, x => SPtr.New(x));
            Object_File_Table = BindPtr(AddressToken.ObjectTable_Start, version, x => SPtr.New(x));

            SaveContext = BindPtr(AddressToken.SRAM_START, version, x => SPtr.New(x));
            Segment_Table = BindPtr(AddressToken.RAM_SEGMENT_TABLE, version, x => SPtr.New(x));
            SceneTable = BindPtr(AddressToken.SceneTable_Start, version, x => SPtr.New(x));
            EntranceTable = BindPtr(AddressToken.EntranceIndexTable_Start, version, x => SPtr.New(x));
            Queue_Thread_Ptr = BindPtr(AddressToken.QUEUE_THREAD, version, x => SPtr.New(x));
            Stack_List_Ptr = BindPtr(AddressToken.STACK_LIST, version, x => SPtr.New(x));

            //Global Context
            if (setGctx)
            {
                GlobalContext = BindPtr(AddressToken.RAM_GLOBAL_CONTEXT, version, x =>
                {
                    if (version == ORom.Build.IQUEC || version == ORom.Build.IQUET)
                    {
                        return SPtr.New(x);
                    }
                    return SPtr.New(x).Deref();
                });
            }

            Actor_Category_Table = BindOff(AddressToken.ACTOR_CAT_LL_Start, version, x => GlobalContext.RelOff(x));
            Object_Allocation_Table = BindOff(AddressToken.OBJ_ALLOC_TABLE, version, x => GlobalContext.RelOff(x));
            Room_Context = BindOff(AddressToken.ROOM_CONTEXT, version, x => GlobalContext.RelOff(x));
            Room_Allocation_Table = BindOff(AddressToken.ROOM_ALLOC_ADDR, version, x => GlobalContext.RelOff(x));
            Room_List_Ptr = BindOff(AddressToken.ROOM_LIST_PTR, version, x => GlobalContext.Deref(x));

            Gfx = GlobalContext != 0 ? GlobalContext.Deref() : SPtr.New(0);
        }

        public static void SetGfxContext(int addr)
        {
            Gfx = SPtr.New(addr);
        }
        
    }
}
