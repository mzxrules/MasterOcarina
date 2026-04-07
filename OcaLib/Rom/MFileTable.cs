using System;
using System.IO;

namespace mzxrules.OcaLib
{
    public class MFileTable : VFileTable
    {
        public MFileTable(string romLocation, RomVersion version)
        {
            Tables = new TableInfo(version);
            using (FileStream fs = new(romLocation, FileMode.Open, FileAccess.Read))
            {
                dmadata = new DmaData(fs, version);
            }
            RomLocation = romLocation;
            Version = version;
        }

        #region GetFile

        public RomFile GetMessageFile(Rom.Language language)
        {
            return language switch
            {
                Rom.Language.Japanese => GetFile(MRom.FileList.jpn_message_data_static),
                Rom.Language.English => GetFile(MRom.FileList.nes_message_data_static),
                Rom.Language.German => GetFile(MRom.FileList.ger_message_data_static),
                Rom.Language.French => GetFile(MRom.FileList.fra_message_data_static),
                Rom.Language.Spanish => GetFile(MRom.FileList.spa_message_data_static),
                Rom.Language.Chinese => throw new InvalidOperationException(),
                _ => throw new NotImplementedException(),
            };
        }

        public override RomFile GetSceneFile(int i)
        {
            byte? sceneIndex = (byte)i; //GetInternalSceneIndex(i);

            if (sceneIndex == null)
                return null;

            var sceneFile = GetSceneVirtualAddress((sbyte)sceneIndex);
            if (sceneFile.Start == 0)
                return null;
            return GetFile(sceneFile);
        }

        /// <summary>
        /// Converts the scene index value stored in an entrance index into the internal scene number
        /// </summary>
        /// <param name="entranceSceneIndex"></param>
        /// <returns></returns>
        public byte? GetInternalSceneIndex(int entranceSceneIndex)
        {
            var entranceTableBase = Addresser.GetRom(MRom.FileList.code, Version, AddressToken.EntranceIndexTable_Start);
            var entranceTableAddr = entranceTableBase + (sizeof(int) * 3) * entranceSceneIndex + 4;

            //Capture pointer
            if (!Addresser.TryGetRom(MRom.FileList.code, Version, (uint)ReadInt32(entranceTableAddr), out int entranceAddr)
                || !Addresser.TryGetRom(MRom.FileList.code, Version, (uint)ReadInt32(entranceAddr), out entranceAddr))
            {
                return null;
            }
            uint EntranceRecord = (uint)ReadInt32(entranceAddr);

            sbyte sceneIndex = (sbyte)(EntranceRecord >> 24);
            sceneIndex = Math.Abs(sceneIndex);

            return (byte?)sceneIndex;
        }

        #endregion

    }
}
