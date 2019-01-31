using mzxrules.Helper;

namespace Spectrum
{
    interface IRamItem
    {
        N64PtrRange Ram { get; }
    }
    interface IVRamItem
    {
        N64PtrRange VRam { get; }
    }
}
