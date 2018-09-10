using System.IO;
using System.Linq;
using VerboseOcarina.Settings;

namespace Z64Tables
{
    public static class RomLocation
    {
        static Options settings;

        static RomLocation()
        {
            Options s;
            if (!Options.LoadFromFile("Settings.xml", out s))
            {
                s = new Options();
            }
            settings = s;
        }

        public static bool TryGetRomLocation(string version, out string location)
        {
            RomDirectory romDir;
            location = string.Empty;

            romDir = settings.Roms.SingleOrDefault
                (x => x.game == "OcarinaOfTime" && x.version == version);

            if (romDir == null)
                return false;

            location = romDir.Value;
            return File.Exists(location);
        }

        public static void SetRomLocation(string version, string location)
        {
            RomDirectory romDir;

            romDir = settings.Roms.SingleOrDefault
                (x => x.game == "OcarinaOfTime" && x.version == version.ToString());

            if (romDir == null)
            {
                romDir = new RomDirectory();
                romDir.game = "OcarinaOfTime";
                romDir.version = version.ToString();
                settings.Roms.Add(romDir);
            }
            romDir.Value = location;
            settings.SaveToFile("Settings.xml");
        }
    }
}
