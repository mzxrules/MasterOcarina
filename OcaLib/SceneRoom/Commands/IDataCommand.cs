using mzxrules.Helper;
namespace mzxrules.OcaLib.SceneRoom.Commands
{
    public interface IDataCommand
    {
        SegmentAddress SegmentAddress { get; set; }
        void Initialize(System.IO.BinaryReader br);
    }
}
