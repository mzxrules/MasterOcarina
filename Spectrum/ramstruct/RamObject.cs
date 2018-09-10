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

        internal static void ChangeVersion(RomVersion b)
        {
            if (b.Game == Game.OcarinaOfTime)
                OBJECT_FILE_COUNT = 0x192;
            else if (b.Game == Game.MajorasMask)
                OBJECT_FILE_COUNT = 0x283;
            

            ObjectFiles = new FileAddress[OBJECT_FILE_COUNT];

            int addr = OBJ_FILE_TABLE_ADDR;
            
            for (int i = 0; i < OBJECT_FILE_COUNT; i++)
            {
                ObjectFiles[i] = new FileAddress(Zpr.ReadRamInt32(addr), Zpr.ReadRamInt32(addr + 4));
                addr += 8;
            }
        }
        
        public static int GetCount()
        {
            return Zpr.ReadRamByte(OBJ_ALLOC_TABLE_ADDR + 0x08);
        }

        public short Object { get; private set; }
        public bool IsLoaded { get; private set; }
        public FileAddress Ram
        {
            get { return _RamAddress; }
        }

        public FileAddress VRom
        {
            get { return ObjectFiles[Object]; }
            set { ObjectFiles[Object] = value; }
        }

        FileAddress _RamAddress;
        int Size;

        public RamObject(Ptr ptr)
        {
            Object = ptr.ReadInt16(0);//BitConverter.ToInt16(data, Zpr.End16(0));
            IsLoaded = Object >= 0;
            Object = Math.Abs(Object);
            N64Ptr addr = ptr.ReadUInt32(4); //BitConverter.ToUInt32(data, 4);
            Size = (ObjectFiles.Length <= Object || Object < 0) ? 0 : (int)ObjectFiles[Object].Size;
            _RamAddress = new FileAddress(addr, addr + Size);
        }

        internal static List<RamObject> GetObjects()
        {
            List<RamObject> ovlObjects = new List<RamObject>();

            if (SpectrumVariables.GlobalContext == 0)
                return ovlObjects;

            int objCount = GetCount();

            Ptr ptr = OBJ_ALLOC_TABLE_ADDR.RelOff(0xC);
            //objTbl = Zpr.ReadRam(OBJ_ALLOC_TABLE_ADDR + 0x0C, ovlObjectCount * LENGTH);
            //objTbl.Reverse32();
            for (int i = 0; i < objCount; i++)
            {
                //Array.Copy(objTbl, i * LENGTH, ovlObjData, 0, LENGTH);
                RamObject working = new RamObject(ptr);
                ptr = ptr.RelOff(LENGTH);
                ovlObjects.Add(working);
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
