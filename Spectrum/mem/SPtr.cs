using mzxrules.Helper;

namespace Spectrum
{
    /// <summary>
    /// Spectrum Pointer. Used to create a dynamic reference to an item in memory.
    /// </summary>
    static class SPtr
    {
        static readonly MemoryModel mem = new()
        {
            ReadInt64 = Zpr.ReadRamInt64,
            ReadInt32 = Zpr.ReadRamInt32,
            ReadFloat = Zpr.ReadRamFloat,
            ReadInt16 = Zpr.ReadRamInt16,
            ReadByte = Zpr.ReadRamByte,
            ReadBytes = Zpr.ReadRam,

            WriteInt32 = Zpr.WriteRam32,
            WriteFloat32 = Zpr.WriteRam32,
            WriteInt16 = Zpr.WriteRam16,
            WriteByte = Zpr.WriteRam8
        };

        public static Ptr New(N64Ptr addr)
        {
            return Ptr.New(mem, addr);
        }
    }
}
