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
            return TryGetRomLocation(input, out path, out RomVersion v);
        }

        public static bool TryGetRomLocation(RomVersion input, out string path, out RomVersion outver)
        {
            var key = GetVersionKey(input);
            return TryGetRomLocation(key, out path, out outver);
        }

        public static bool TryGetRomLocation(string key, out string path, out RomVersion outver)
        {
            XPath rPath;
            path = "";
            outver = new RomVersion();

            rPath = settings.Rom.SingleOrDefault(x => x.key == key);

            if (rPath == null)
                return false;

            path = rPath.Value;
            outver = new RomVersion(rPath.game, rPath.version);
            return File.Exists(path);
        }

        public static void SetRomLocation(RomVersion version, string path)
        {
            var key = GetVersionKey(version);
            SetRomLocation(key, version, path);
        }

        public static void SetRomLocation(string key, RomVersion version, string path)
        {
            XPath romPath;

            romPath = settings.Rom.SingleOrDefault(x => x.key == key);

            if (romPath == null)
            {
                romPath = new XPath
                {
                    key = key,
                    game = version.Game.ToString(),
                    version = version.ToString()
                };
                settings.Rom.Add(romPath);
            }
            romPath.Value = path;
            settings.SaveToFile(XML_NAME);
        }

        private static string GetVersionKey(RomVersion version)
        {
            return $"{version.Game.ToString()}.{version.ToString()}";
        }
    }
}
