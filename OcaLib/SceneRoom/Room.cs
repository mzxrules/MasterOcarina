using mzxrules.Helper;

namespace mzxrules.OcaLib.SceneRoom
{
    public class Room : ISceneRoomHeader
    {
        public Room(Game game, FileAddress a)
        {
            Header = new SceneHeader(game);
            VirtualAddress = a;
        }
    }
}
