using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    class HelpFunc
    {
        public static string GetArguments(IEnumerable<string> test)
        {
            return (test.Count() > 0) ? "<" + string.Join("> <", test) + ">" : "";
        }
        public static IEnumerable<string> ConvertTokensToString(Tokens[] signature)
        {
            return from a in signature
                   select ConvertTokenToString(a);
        }
        public static string ConvertTokenToString(Tokens t)
        {
            switch (t)
            {
                case Tokens.COORDS_FLOAT: return "x,y,z";
                default: return t.ToString().ToLowerInvariant();
            }
        }
    }
}
