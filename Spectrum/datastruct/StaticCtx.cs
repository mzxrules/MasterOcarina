using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spectrum
{
    class StaticCtx
    {
        const string regChar = " SOPQMYDUIZCNKXcsiWAVHGmnBdkb";

        static Regex regNameRegex = new Regex(@"^([SOPQMYDUIZCNKXcsiWAVHGmnBdkb]{0,1}REG)\(([0-9]{1,2})\)$");

        public static string GetRegFromOffset(int off)
        {
            off -= 0x14;
            off /= 2;

            int regGroup = off / 0x60;
            int regIndex = off % 0x60;

            if (off < 0 || regGroup < 0 || regGroup >= 29)
            {
                return "ERROR: Offset out of range";
            }

            return $"{regChar[regGroup]}REG({regIndex})".TrimStart();
        }

        public static bool TryGetOffsetFromReg(string input, out int offset)
        {
            offset = -1;
            Match match = regNameRegex.Match(input);
            if (!match.Success)
            {
                return false;
            }

            string name = match.Groups[1].Value;
            string indexStr = match.Groups[2].Value;
            char chr = name.Length == 3 ? ' ' : name[0];

            if (!int.TryParse(indexStr, out int regIndex))
            {
                return false;
            }
            int regGroup = regChar.IndexOf(chr);
            offset = (regGroup * 0x60 + regIndex) * 2 + 0x14;

            return true;
        }
    }
}
