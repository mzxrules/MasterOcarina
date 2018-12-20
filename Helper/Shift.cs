namespace mzxrules.Helper
{
    public static class Shift
    {
        const ulong LEFT = 0x8000000000000000;
        public static bool AsBool(ulong value, ulong mask)
        {
            return (value & mask) > 0;
        }
        public static byte AsByte(ulong value, ulong mask)
        {
            return (byte)((value & mask) >> GetRight(mask));
        }
        public static sbyte AsSByte(ulong value, ulong mask)
        {
            return (sbyte)((value & mask) >> GetRight(mask));
        }
        public static ushort AsUInt16(ulong value, ulong mask)
        {
            return (ushort)((value & mask) >> GetRight(mask));
        }
        public static int GetLeft(ushort mask)
        {
            ulong v = mask;
            v <<= 48;
            return GetLeft(v);
        }

        /// <summary>
        /// Gets the leftmost bit of the given mask, where the leftmost bit is index 0
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static int GetLeft(uint mask)
        {
            ulong v = mask;
            v <<= 32;
            return GetLeft(v);
        }

        /// <summary>
        /// Gets the leftmost bit of the given mask, where the leftmost bit is index 0
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static int GetLeft(ulong mask)
        {
            int shift;

            if (mask == 0)
                return 0;

            for (shift = 0; (mask & LEFT) == 0; mask <<= 1)
                shift++;

            return shift;
        }
        /// <summary>
        /// Gets the rightmost bit index of the given mask
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static int GetRight(ulong mask)
        {
            int shift;

            //a mask of 0 isn't valid, but could come up by accident
            if (mask == 0)
                return 0;

            //Check the right bit
            //If the right bit is 0 shift the mask over one and increment the shift count
            //If the right bit is 1, the right side of the mask is found, we know how much to shift by
            for (shift = 0; (mask & 1) == 0; mask >>= 1)
            {
                shift++;
            }

            return shift;
        }
    }
}