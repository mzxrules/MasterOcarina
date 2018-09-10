using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yaz0enc
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
                return;
            var file = args[0];
            var fileOut = $"{file}.yaz0";

            if (!FileExists(file))
                return;

            using (FileStream fw = new FileStream(fileOut, FileMode.Create))
            {
                using (FileStream input = new FileStream(file, FileMode.Open))
                {
                    byte[] data = new byte[input.Length];
                    input.Read(data, 0, data.Length);
                    mzxrules.Helper.Yaz.Encode(data, data.Length, fw);
                }
            }
        }
        private static bool FileExists(string file)
        {
            if (File.Exists(file))
            {
                return true;
            }
            Console.WriteLine("File does not exist.");
            return false;
        }
    }
}
