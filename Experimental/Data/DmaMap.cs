using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using mzxrules.OcaLib;
using mzxrules.OcaLib.SceneRoom;
using mzxrules.Helper;

namespace Experimental.Data
{
    static partial class Get
    {
        class FOut
        {
            public FileAddress Source;
            public int SourceIndex;
            public FileAddress Target;
            public int TargetIndex;

            public FOut(FileRecord rec)
            {
                Source = rec.VRom;
                SourceIndex = 0; //rec.Index;
                Target = new FileAddress();
                TargetIndex = -1;
            }
        }

        public static void DmaMap(IExperimentFace face, List<string> filePath)
        {
            Rom sourceRom = new MRom(filePath[0], MRom.Build.J0);
            Rom targetRom = new MRom(filePath[1], MRom.Build.U0);
            
            VFileTable source; //rom containing the complete file list
            VFileTable target; //rom we're using to link to file names
            Dictionary<long, FileRecord> unmatched = new(); //address pool
            Dictionary<long, FOut> fOut = new();

            source = sourceRom.Files; 
            target = targetRom.Files; 

            //Load file table data
            foreach (FileRecord t in target)
            {
                if (t.VRom.End != 0)
                    fOut.Add(t.VRom.Start, new FOut(t));
            }
            foreach (FileRecord s in source)
            {
                if (s.VRom.End != 0)
                    unmatched.Add(s.VRom.Start, s);
            }

            //match scene files
            MatchGeneric(TableInfo.Type.Scenes, source, target, fOut, unmatched);
            
            //match room files
            MatchRooms(source, target, fOut, unmatched);

            MatchGeneric(TableInfo.Type.GameOvls, source, target, fOut, unmatched);
            MatchGeneric(TableInfo.Type.TitleCards, source, target, fOut, unmatched);
            MatchGeneric(TableInfo.Type.Actors, source, target, fOut, unmatched);
            MatchGeneric(TableInfo.Type.Objects, source, target, fOut, unmatched);
            MatchGeneric(TableInfo.Type.HyruleSkybox, source, target, fOut, unmatched);
            MatchGeneric(TableInfo.Type.Particles, source, target, fOut, unmatched);
            MatchGeneric(TableInfo.Type.Transitions, source, target, fOut, unmatched);


            StringBuilder sw = new StringBuilder();
            {
                foreach (FOut item in fOut.Values)
                {
                    FileAddress vS = item.Source;
                    FileAddress vT = item.Target;
                    sw.AppendLine($"{item.SourceIndex}\t{vS.Start}\t{vS.End}\t{item.TargetIndex}\t{vT.Start}\t{vT.End}");
                }
            }
            face.OutputText(sw.ToString());
        }

        //Generic Algorithm

        delegate FileAddress GetVirtualAddress(int i);

        private static void MatchGeneric(TableInfo.Type type, VFileTable sourceFileList, VFileTable targetFileList, 
            Dictionary<long, FOut> fOut, Dictionary<long, FileRecord> unmatched)
        {
            var sourceT = sourceFileList.Tables.GetTable(type);
            var targetT = targetFileList.Tables.GetTable(type);

            int iter = Math.Min(sourceT.Records, targetT.Records);
            for (int i = 0; i < iter; i++)
            {
                FileAddress source = sourceFileList.GetFileByTable(type, i);
                FileAddress target = targetFileList.GetFileByTable(type, i);
                TestMatch(source, target, fOut, unmatched);
            }
        }

        private static void TestMatch(FileAddress source, FileAddress target,  Dictionary<long, FOut> fOut, Dictionary<long, FileRecord> unmatched)
        {
            if (target.Start != 0 && source.Start != 0)
            {
                throw new NotImplementedException("FileRecord no longer contains index property");

                FOut rec = fOut[target.Start];
                if (unmatched.TryGetValue(source.Start, out FileRecord match))
                {
                    unmatched.Remove(source.Start);
                    //rec.TargetIndex = match.Index;
                    //rec.Target = source;
                }
            }
        }


        private static void MatchRooms(VFileTable sourceFileList, VFileTable targetFileList,
            Dictionary<long, FOut> fOut, Dictionary<long, FileRecord> unmatched)
        {
            var sourceT = sourceFileList.Tables.GetTable(TableInfo.Type.Scenes);
            var targetT = targetFileList.Tables.GetTable(TableInfo.Type.Scenes);

            int scenes = Math.Min(sourceT.Records, targetT.Records);


            for (int i = 0; i < scenes; i++)
            {
                var sR = sourceFileList.GetSceneFile(i);
                var tR = targetFileList.GetSceneFile(i);
                if (sR == null)
                    continue;
                if (tR == null)
                    continue;

                Scene s = SceneRoomReader.InitializeScene(sR, i);
                Scene t = SceneRoomReader.InitializeScene(tR, i);

                List<FileAddress> sourceRooms = s.Header.GetRoomAddresses();
                List<FileAddress> targetRooms = t.Header.GetRoomAddresses();

                int rooms = Math.Min(sourceRooms.Count, targetRooms.Count);

                for (int j = 0; j < rooms; j++)
                {
                    TestMatch(sourceRooms[j], targetRooms[j], fOut, unmatched);
                }
            }
        }
    }
}
