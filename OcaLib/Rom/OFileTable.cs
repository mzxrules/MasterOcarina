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
            using (FileStream fs = new(romLoc, FileMode.Open, FileAccess.Read))
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
            return language switch
            {
                Rom.Language.Japanese => GetFile(ORom.FileList.jpn_message_data_static),
                Rom.Language.English => GetFile(ORom.FileList.nes_message_data_static),
                Rom.Language.German => GetFile(ORom.FileList.ger_message_data_static),
                Rom.Language.French => GetFile(ORom.FileList.fra_message_data_static),
                Rom.Language.Spanish => throw new NotImplementedException(),
                Rom.Language.Chinese => throw new NotImplementedException(),
                _ => throw new InvalidOperationException(),
            };
        }

        public override RomFile GetSceneFile(int i)
        {
            if (!(0 <= i && i < Tables.Scenes.Records))
            {
                return null;
            }

            var sceneFile = GetSceneVirtualAddress(i);
            if (sceneFile.Start == 0)
            {
                return null;
            }

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