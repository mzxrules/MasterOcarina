using mzxrules.Helper;

namespace Spectrum
{
    class CodeFile : IFile
    {
        public FileAddress Ram { get; set; }
        public FileAddress VRom { get; set; }

        public CodeFile()
        {
            var ramStart = SpectrumVariables.Code_Addr;
            VRom = RamDmadata.GetFileAddress(SpectrumVariables.Code_VRom);
            Ram = new FileAddress(ramStart, ramStart + VRom.Size);
        }

        public override string ToString()
        {
            return "code";
        }

    }
}
