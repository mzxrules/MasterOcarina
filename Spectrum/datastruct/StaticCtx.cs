using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    class StaticCtx
    {
        const string regChar = " SOPQMYDUIZCNKXcsiWAVHGmnBdkb";

        public static string GetRegFromOffset(int off)
        {
            off -= 0x14;
            off /= 2;

            int regGroup = off / 0x60;
            int regIndex = off % 0x60;

            if (regGroup < 0 || regGroup >= 29)
            {
                return "ERROR";
            }

            return $"{regChar[regGroup]}REG({regIndex})".TrimStart();
        }
    }
}
