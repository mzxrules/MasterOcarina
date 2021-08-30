using System;
using System.IO;
using System.Text;

namespace mzxrules.Helper
{
    public static class CStr
    {
        public static string Get(Stream stream, Encoding enc, long maxSeek = long.MaxValue)
        {
            string result = "";
            StreamReader sw = new(stream, enc);
            
            maxSeek = Math.Min(maxSeek, stream.Length - stream.Position);
            for (int i = 0; i < maxSeek; i++)
            {
                char c = (char)sw.Read();
                if (c == '\0')
                    break;

                result += c;
            }

            return result;
        }

        public static string Get(Stream stream, long maxSeek = long.MaxValue)
        {
            return Get(stream, Encoding.UTF8, maxSeek);
        }

        public static string Get(byte[] buffer, Encoding encoding, long maxSeek = long.MaxValue)
        {
            using var ms = new MemoryStream(buffer);
            return Get(ms, encoding, maxSeek);
        }
        public static string Get(byte[] buffer, string encoding, long maxSeek = long.MaxValue)
        {
            return Get(buffer, Encoding.GetEncoding(encoding), maxSeek);
        }
    }
}
