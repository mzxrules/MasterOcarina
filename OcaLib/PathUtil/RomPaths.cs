using System.IO;
using System.Linq;

namespace mzxrules.OcaLib.PathUtil
{
    public static class PathUtil
    {
        static Roms settings;
        const string XML_NAME = "rompath.xml";

        public static void Initialize()
        {
            if (!Roms.LoadFromFile(XML_NAME, out Roms s))
            {
                s = new Roms();
            }
            settings = s;
        }

        public static bool TryGetRomLocation(RomVersion input, out string path)
        {
            var key = input.UniqueKey;
            XPath rPath = settings.Rom.SingleOrDefault(x => x.key == key);

            if (rPath == null)
            {
                path = "";
                return false;
            }

            path = rPath.Value;
            return File.Exists(path);
        }

        public static bool TryGetRomLocation(string key, out string path, out RomVersion outver)
        {
            outver = new RomVersion(key);
            return TryGetRomLocation(outver, out path);
        }

        public static void SetRomLocation(RomVersion version, string path)
        {
            var key = version.UniqueKey;
            XPath romPath = settings.Rom.SingleOrDefault(x => x.key == key);

            if (romPath == null)
            {
                romPath = new XPath
                {
                    key = key
                };
                settings.Rom.Add(romPath);
            }
            romPath.Value = path;
            settings.SaveToFile(XML_NAME);
        }
    }
}
