using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mzxrules.XActor.Gui
{
    static class Helper
    {
        public static UInt16 ShiftMask(UInt16 value)
        {
            int shift = 0;
            if (value == 0)
                return 0;

            while (shift < 16)
            {
                if ((value & (1 << shift)) != 0)
                    break;
                shift++;
            }
            return (UInt16)(value >> shift);
        }

        public static bool TryParseHex(string s, out UInt16 o)
        {
            return UInt16.TryParse(s, System.Globalization.NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out o);
        }

        public static bool TryParseHex(string s, out Int32 o)
        {
            return Int32.TryParse(s, System.Globalization.NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out o);
        }

        public static string TrimComment(string p)
        {
            StringBuilder sb = new StringBuilder();

            string[] commentLines;
            bool emptyLine = false;
            bool firstLine = true;

            if (p == null)
                return "";

            commentLines = p.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None);

            if (commentLines.Length == 0)
                return "";


            for (int i = 0; i < commentLines.Length; i++)
            {
                string s = commentLines[i].Trim();

                if (s.Length == 0)
                {
                    emptyLine = true;
                    continue;
                }
                if (emptyLine && !firstLine)
                {
                    sb.AppendLine();
                }
                emptyLine = false;
                firstLine = false;
                sb.AppendLine(s);
            }
            return sb.ToString().TrimEnd();
        }
    }
}