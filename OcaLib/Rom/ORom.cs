using mzxrules.OcaLib.Text;

namespace mzxrules.OcaLib
{
    /// <summary>
    /// Encapsulates an n64 Ocarina of Time binary file
    /// </summary>
    public partial class ORom(string fileLocation, ORom.Build version) : Rom(fileLocation, version)
    {
        public new OFileTable Files { get { return (OFileTable) base.Files; } }
        public GameText Text
        {
            get
            {
                _Text ??= new GameText(this);
                return _Text;
            }
        }
        private GameText _Text;
    }
}
