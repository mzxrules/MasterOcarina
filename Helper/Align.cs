namespace mzxrules.Helper
{
    public static class Align
    {
        public static uint To(uint value, uint align)
        {
            uint i = value % align;

            if (i == 0)
                return value;

            return value + align - i;
        }
        public static int To16(int value)
        {
            return (int)To((uint)value, 16);
        }
        public static int To8(int value)
        {
            return (int)To((uint)value, 8);
        }
    }
}
