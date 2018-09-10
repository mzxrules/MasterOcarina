using mzxrules.Helper;

namespace Spectrum
{
    class GfxDList
    {
        public string Name;
        public int Size;
        public Ptr RecordPtr;
        public N64Ptr StartPtr;
        public N64Ptr AppendStartPtr;
        public N64Ptr AppendEndPtr;

        public GfxDList(string name, Ptr addr)
        {
            Name = name;
            RecordPtr = addr;
            Size = addr.ReadInt32(0x0);
            StartPtr = RecordPtr.RelOff(0x4).Deref();
            AppendStartPtr = RecordPtr.RelOff(0x8).Deref();
            AppendEndPtr = RecordPtr.RelOff(0xC).Deref();
        }
    }
}
