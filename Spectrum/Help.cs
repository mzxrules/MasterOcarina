using System.Collections.Generic;
using System.Linq;

namespace Spectrum
{
    class HelpFunc
    {
        public static string GetArguments(IEnumerable<string> test)
        {
            return test.Any() ? "<" + string.Join("> <", test) + ">" : "";
        }
        public static IEnumerable<string> ConvertTokensToString(Tokens[] signature)
        {
            return from a in signature
                   select ConvertTokenToString(a);
        }
        public static string ConvertTokenToString(Tokens t)
        {
            return t switch
            {
                Tokens.COORDS_FLOAT => "x,y,z",
                _ => t.ToString().ToLowerInvariant(),
            };
        }
    }
}
