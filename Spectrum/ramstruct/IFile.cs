using mzxrules.Helper;

namespace Spectrum
{
    internal interface IFile : IRamItem
    {
        FileAddress VRom { get; }
    }
}