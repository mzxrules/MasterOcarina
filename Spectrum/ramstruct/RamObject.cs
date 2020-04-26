using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Collections.Generic;

namespace Spectrum
{
    class RamObject: IFile
    {
        static int OBJ_FILE_TABLE_ADDR { get { return SpectrumVariables.Object_File_Table; } }
        static Ptr OBJ_ALLOC_TABLE_ADDR { get { return SpectrumVariables.Object_Allocation_Table; } } // near 0x1D9C4C
        public const int LENGTH = 0x44;
        static int OBJECT_FILE_COUNT; // = 0x192; 0x283
        
        static FileAddress[] ObjectFiles;

        public short Object { get; private set; }
        public bool IsLoaded { get; private set; }
        public N64PtrRange Ram { get; }

        public FileAddress VRom
        {
            get { return ObjectFiles[Object]; }
            set { ObjectFiles[Object] = value; }
        }


        internal static void ChangeVersion((RomVersion v, bool g) args)
        {
            var v = args.v;
            if (v.Game == Game.OcarinaOfTime)
                OBJECT_FILE_COUNT = 0x192;
            else if (v.Game == Game.MajorasMask)
                OBJECT_FILE_COUNT = 0x283;
            

            ObjectFiles = new FileAddress[OBJECT_FILE_COUNT];

            Ptr ptr = SPtr.New(OBJ_FILE_TABLE_ADDR);
            
            for (int i = 0; i < OBJECT_FILE_COUNT; i++)
            {
                int off = i * 8;
                ObjectFiles[i] = new FileAddress(ptr.ReadInt32(off), ptr.ReadInt32(off + 4));
            }
        }
        
        public static int GetCount()
        {
            return Zpr.ReadRamByte(OBJ_ALLOC_TABLE_ADDR + 0x08);
        }


        int Size;

        public RamObject(Ptr ptr)
        {
            Object = ptr.ReadInt16(0);
            IsLoaded = Object >= 0;
            Object = Math.Abs(Object);
            N64Ptr addr = ptr.ReadUInt32(4); 
            Size = (ObjectFiles.Length <= Object || Object < 0) ? 0 : ObjectFiles[Object].Size;
            Ram = new N64PtrRange(addr, addr + Size);
        }

        internal static List<RamObject> GetObjects()
        {
            List<RamObject> ovlObjects = new List<RamObject>();

            if (SpectrumVariables.GlobalContext == 0)
                return ovlObjects;

            int objCount = GetCount();

            Ptr ptr = OBJ_ALLOC_TABLE_ADDR.RelOff(0xC);

            for (int i = 0; i < objCount; i++)
            {
                RamObject working = new RamObject(ptr);
                ovlObjects.Add(working);

                ptr = ptr.RelOff(LENGTH);
            }
            return ovlObjects;
        }

        public override string ToString()
        {
            var addr = ObjectFiles[Object];
            return $"OB {Object:X4}    {IsLoaded} {addr.Start:X8}:{addr.End:X8}";
        }
    }
}
