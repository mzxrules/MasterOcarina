using System;
using System.IO;
using System.Text;

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
            maxSeek = Math.Min(maxSeek, stream.Length - stream.Position);

            char c;
            for (int i = 0; i < maxSeek; i++)
            {
                c = (char)sw.Read();
                if (c == '\0')
                    break;

                result += c;
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
