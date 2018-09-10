using mzxrules.Helper;

namespace Spectrum
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
