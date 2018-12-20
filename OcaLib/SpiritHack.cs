using mzxrules.OcaLib.SceneRoom;
using mzxrules.OcaLib.SceneRoom.Commands;
using System.IO;


namespace mzxrules.OcaLib.SceneRoom
{
    public partial class SceneHeader
    {
        //HACK
        public void SpiritHackSetAlternateHeaders(long cs0, long cs1)
        {
            AlternateHeadersCommand cmd = new AlternateHeadersCommand(Game.OcarinaOfTime);
            cmd.SetCommand(new byte[] { 0x18, 0, 0, 0, /**/ 2, 0, 0, 0 });
            cmds.Add(cmd);

            Alternate.SpiritHack(cs0, cs1);
        }
    }
}

namespace mzxrules.OcaLib
{
    public static class SpiritHack
    {
        #region Cutscene 0 Data
        static int[] Cutscene0 = new int[] {
            0x250,
            0x150,
            0x1F0,
            0x110,
            0x1E0,
            0x3A0,
            0x170,
            0x170,
            0x260,
            0x160,
            0x1C0,
            0x070,
            0x0C0,
            0x140,
            0x100,
            0x1E0,
            0x160,
            0x120,
            0x190,
            0x150,
            0x1C0,
            0x0C0,
            0x1C0,
            0x210,
            0x0E0,
            0x310,
            0x230,
            0x170,
            0x0C0
        };
        #endregion

        #region Cutscene 1 Data
        static int[] Cutscene1 = new int[] {
            0x2A0,
            0x1D0,
            0x260,
            0x170,
            0x250,
            0x460,
            0x1C0,
            0x1D0,
            0x2C0,
            0x190,
            0x270,
            0x0A0,
            0x0F0,
            0x170,
            0x130,
            0x240,
            0x190,
            0x150,
            0x1C0,
            0x180,
            0x270,
            0x0F0,
            0x1F0,
            0x2A0,
            0x110,
            0x340,
            0x2B0,
            0x1A0,
            0x0F0
        };
        #endregion

        static int[] SpiritSceneSetups= new int[] { 0x17220, 0x17710 };

        public static int GetBetaSceneSetupOffset(int cutsceneNo)
        {
            return SpiritSceneSetups[cutsceneNo];
        }

        public static int GetBetaRoomSetupOffset(int cutsceneNo, int roomNo)
        {
            return (cutsceneNo == 0) ? Cutscene0[roomNo] : (cutsceneNo == 1) ? Cutscene1[roomNo] : -1;
        }

        public static void LoadSpiritSceneHeader(BinaryReader br, Scene item)
        {
            LoadSpiritSceneMapHeader(br, item, SpiritSceneSetups[0], SpiritSceneSetups[1]);
        }

        public static void LoadSpiritRoomHeader(BinaryReader br, Room item, int roomNo)
        {
            LoadSpiritSceneMapHeader(br, item, Cutscene0[roomNo], Cutscene1[roomNo]);
        }

        private static void LoadSpiritSceneMapHeader(BinaryReader br, ISceneRoomHeader item, int cs0, int cs1)
        {
            SceneHeader header;
            header = item.Header;

            //Load the root header
            header.Load(br, 0);
            header.SpiritHackSetAlternateHeaders(cs0, cs1);
            header.InitializeAssets(br);

            for (int i = 0; i < header.Alternate.HeaderList.Count; i++)
            {
                if (header.Alternate.HeaderList[i] != null)
                {
                    header.Alternate.HeaderList[i].Load(br, header.Alternate.HeaderOffsetsList[i]);
                    header.Alternate.HeaderList[i].InitializeAssets(br);
                }
            }
        }
    }
}
