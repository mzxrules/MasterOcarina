using System;
using System.IO;
using mzxrules.Helper;

namespace mzxrules.OcaLib
{
    /// <summary>
    /// Provides a means of accessing files in a Zelda 64 Rom
    /// </summary>
    public class OFileTable : VFileTable
    {
        public OFileTable(string romLoc, RomVersion version)
        {
            Tables = new TableInfo(version);
            using (FileStream fs = new FileStream(romLoc, FileMode.Open, FileAccess.Read))
            {
                dmadata = new DmaData(fs, version);
            }

            RomLocation = romLoc;
            Version = version;
        }

        #region GetFile
        /// <summary>
        /// Returns a message_data_static file (the game's text dialog) for a specific language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public RomFile GetMessageFile(Rom.Language language)
        {
            switch (language)
            {
                case Rom.Language.Japanese: return GetFile(ORom.FileList.jpn_message_data_static);
                case Rom.Language.English: return GetFile(ORom.FileList.nes_message_data_static);
                case Rom.Language.German: return GetFile(ORom.FileList.ger_message_data_static);
                case Rom.Language.French: return GetFile(ORom.FileList.fra_message_data_static);
                default: throw new NotImplementedException();
            }
        }

        public override RomFile GetSceneFile(int i)
        {
            if (!(Version == ORom.Build.DBGMQ
                && i < 110
                || i < 101))
                return null;

            var sceneFile = GetSceneVirtualAddress(i);
            if (sceneFile.Start == 0)
                return null;
            return GetFile(sceneFile);
        }
        #endregion

        #region FetchAddresses

        public FileAddress GetTitleCardVirtualAddress(int scene)
        {
            return GetFileByTable(Tables.TitleCards, scene);
        }

        public FileAddress GetHyruleFieldSkyboxVirtualAddress(int id)
        {
            return GetFileByTable(Tables.HyruleSkybox, id);
        }
        #endregion
    }
}