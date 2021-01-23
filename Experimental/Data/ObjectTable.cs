using mzxrules.OcaLib;
using System;
using System.IO;
using System.Linq;
using System.Text;
using mzxrules.Helper;
using OcaBase;

namespace Experimental.Data
{
    static partial class Get
    {
        public static string GetObjectTable(ORom r)
        {
            return "Get.GetObjectTable not implemented";
            //RomFile codeFile;
            //BinaryReader codeFileReader;
            
            //int objectTableStart;
            //StringBuilder sb = new StringBuilder();
            //OcarinaDataContext db = new OcarinaDataContext();

            //codeFile = r.Files.GetFile(ORom.FileList.code);

            //codeFileReader = new BinaryReader(codeFile);
            //objectTableStart = Addresser.GetRom(ORom.FileList.code, r.Version, AddressToken.ObjectTable_Start);
            //codeFileReader.BaseStream.Position = codeFile.Record.GetRelativeAddress(objectTableStart);

            //for (int i = 0; i < 0x200; i++)
            //{
            //    sb.AppendLine($"{i:X4}\t{codeFileReader.ReadBigUInt32():X8}\t{codeFileReader.ReadBigUInt32():X8}");
            //}
            //db.Dispose();
            //codeFileReader.Close();
            //return sb.ToString();
        }
    }
}
