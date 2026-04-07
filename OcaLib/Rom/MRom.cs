using mzxrules.OcaLib.Text;

namespace mzxrules.OcaLib
{
    public partial class MRom(string fileLocation, MRom.Build version) : Rom(fileLocation, version)
    {
        public new MFileTable Files { get { return (MFileTable) base.Files; } }
        public GameText Text { get; set; }
    }
}
