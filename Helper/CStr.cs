using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mzxrules.Helper
{
    public static class CStr
    {
        public static string Get(Stream stream, Encoding enc, long maxSeek = 0)
        {
            string result = "";
            StreamReader sw = new StreamReader(stream, enc);

            if (maxSeek == 0)
                maxSeek = int.MaxValue;
            maxSeek = Math.Min(maxSeek, stream.Length);

            char c = (char)sw.Read();
            int i = 0;
            while (c != '\0' && i < maxSeek)
            {
                result += c;
                c = (char)sw.Read();
                i++;
            }

            return result;
        }

        public static string Get(Stream stream, long maxSeek = 0)
        {
            return Get(stream, Encoding.UTF8, maxSeek);
        }

        public static string Get(Stream stream, string encoding, long maxSeek = 0)
        {
            return Get(stream, Encoding.GetEncoding(encoding), maxSeek);
        }
    }
}
