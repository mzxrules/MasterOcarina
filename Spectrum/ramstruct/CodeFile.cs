using mzxrules.Helper;

namespace Spectrum
{
    class CodeFile : IFile
    {
        public N64PtrRange Ram { get; set; }
        public FileAddress VRom { get; set; }

        public CodeFile()
        {
            var ramStart = SpectrumVariables.Code_Addr;
            VRom = RamDmadata.GetFileAddress(SpectrumVariables.Code_VRom);
            Ram = new N64PtrRange(ramStart, ramStart + VRom.Size);
        }

        public override string ToString()
        {
            return "code";
        }

    }
}
