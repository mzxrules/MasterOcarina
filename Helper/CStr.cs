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

            for (int i = 0; i < maxSeek; i++)
            {
                char c = (char)sw.Read();
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

        public static string Get(byte[] buffer, Encoding encoding, long maxSeek = 0)
        {
            using (var ms = new MemoryStream(buffer))
            {
                return Get(ms, encoding, maxSeek);
            }
        }
        public static string Get(byte[] buffer, string encoding, long maxSeek = 0)
        {
            return Get(buffer, Encoding.GetEncoding(encoding), maxSeek);
        }
    }
}
