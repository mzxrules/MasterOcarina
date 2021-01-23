using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OcaBase;
using System.IO;
using mzxrules.Helper;

namespace Experimental.Data
{
    static partial class Get
    {
        public static string EffectTable(ORom n0, ORom dbg)
        {
            return "Get.EffectTable not implemented";
            //OcarinaDataContext db = new OcarinaDataContext();
            //FileAddress dbgAddr;
            //FileAddress n0Addr;
            //StringBuilder sb = new StringBuilder();

            ////BinaryReader n0Code = new BinaryReader(n0.Files.GetFile(Rom.FileList.code));
            ////BinaryReader dbgCode = new BinaryReader(dbg.Files.GetFile(Rom.FileList.code));

            //for (int i = 0; i <= 36; i++)
            //{
            //    dbgAddr = dbg.Files.GetParticleEffectAddress(i);
            //    n0Addr = n0.Files.GetParticleEffectAddress(i);

            //    if (dbgAddr.Start == 0 && n0Addr.Start == 0)
            //        continue;

            //    var n0File = db.Files.Single(x => x.Address_N0.StartAddr == n0Addr.Start);
            //    var dbgFile = db.Files.Single(x => x.Address_DBG.StartAddr == dbgAddr.Start);


            //    sb.AppendLine(string.Format("{2} {3:X8}={0:X8} : {4} {1:X8}", dbgAddr.Start, n0Addr.Start,
            //        dbgFile.FileId,
            //        dbgFile.DBG_Address,
            //        n0File.FileId));
            //}

            ////n0Code.BaseStream.Position = Addresser.GetAddress(Rom.Build.N0, Rom.FileList.code, "ParticleTable_Start")
            ////    - Addresser.GetAddress(Rom.Build.N0, Rom.FileList.code, "__Start");
            ////dbgCode.BaseStream.Position = Addresser.GetAddress(Rom.Build.DBGMQ, Rom.FileList.code, "ParticleTable_Start")
            ////    - Addresser.GetAddress(Rom.Build.DBGMQ, Rom.FileList.code, "__Start");



            //return sb.ToString();
        }
    }
}
