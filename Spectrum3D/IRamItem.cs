using mzxrules.Helper;

namespace Spectrum3D
{
    interface IRamItem
    {
        FileAddress Ram { get; }
    }
    interface IVRamItem
    {
        FileAddress VRam { get; }
    }
}
