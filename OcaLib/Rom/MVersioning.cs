using System;
using System.Collections.Generic;
using System.Linq;

namespace mzxrules.OcaLib
{
    public partial class MRom
    {
        public class BuildInformation
        {
            public static readonly BuildInformation[] builds = new BuildInformation[]
            {
                new BuildInformation { CRC = 0x0000000000000000, Version = Build.UNKNOWN, Name = "Unknown", Localization = Localization.UNKNOWN},

                new BuildInformation { CRC = 0x0000000000000000, Version = Build.J0, Name = "NTSC-J 1.0", Localization = Localization.NTSCJ},
                new BuildInformation { CRC = 0x0000000000000000, Version = Build.J1, Name = "NTSC-J 1.1", Localization = Localization.NTSCJ},

                new BuildInformation { CRC = 0x0000000000000000, Version = Build.U0, Name = "NTSC-U 1.0", Localization = Localization.NTSCU},

                new BuildInformation { CRC = 0x0000000000000000, Version = Build.P0, Name = "PAL 1.0", Localization = Localization.PAL},
                new BuildInformation { CRC = 0x0000000000000000, Version = Build.P1, Name = "PAL 1.1", Localization = Localization.PAL},

                new BuildInformation { CRC = 0x0000000000000000, Version = Build.GCJ, Name = "GCJ 1.0", Localization = Localization.NTSCJ},
                new BuildInformation { CRC = 0x0000000000000000, Version = Build.GCU, Name = "GCU 1.0", Localization = Localization.NTSCU},
                new BuildInformation { CRC = 0x0000000000000000, Version = Build.GCP, Name = "GCP 1.0", Localization = Localization.PAL},

                new BuildInformation { CRC = 0x0000000000000000, Version = Build.DBG, Name = "PAL DBG", Localization = Localization.PAL},

            };

            public Build Version { get; private set; }
            public string Name { get; private set; }
            public Localization Localization { get; private set; }
            public ulong CRC { get; private set; }

            BuildInformation() { }

            public static BuildInformation Get(Build v)
            {
                return builds.SingleOrDefault(x => x.Version == v);
            }
            public static Localization GetLocalization(Build v)
            {
                BuildInformation stats = builds.SingleOrDefault(x => x.Version == v);
                return (stats != null) ? stats.Localization : Localization.UNKNOWN;
            }

            internal static ulong GetCrc(Build v)
            {
                BuildInformation stats = builds.SingleOrDefault(x => x.Version == v);
                return (stats != null) ? stats.CRC : 0;
            }
        }

        private static readonly Language[] SupportedLanguages = new Language[] {
            Language.Japanese,
            Language.English,
            Language.German,
            Language.French, };

        public enum Build
        {
            UNKNOWN,
            CUSTOM,
            J0,
            J1,
            U0,
            P0,
            P1,
            GCJ,
            GCU,
            GCP,
            DBG,
        }

        public enum Localization
        {
            UNKNOWN,
            NTSCJ,
            NTSCU,
            PAL
        }

        public static IEnumerable<RomVersion> GetSupportedBuilds()
        {
            yield return Build.J0;
            yield return Build.J1;
            yield return Build.U0;
            yield return Build.P0;
            yield return Build.P1;
            yield return Build.GCJ;
            yield return Build.GCU;
            yield return Build.GCP;
            yield return Build.DBG;

        }

        public static Localization GetLocalization(RomVersion v)
        {
            return BuildInformation.GetLocalization(v);
        }

        public static ulong GetCrc(RomVersion v)
        {
            return BuildInformation.GetCrc(v);
        }


        public static IEnumerable<Language> GetSupportedLanguages(RomVersion b)
        {
            return GetSupportedLanguages(GetLocalization(b));
        }

        public static IEnumerable<Language> GetSupportedLanguages(Localization l)
        {
            switch (l)
            {
                case Localization.NTSCJ: return new Language[] { Language.Japanese };
                case Localization.NTSCU: return new Language[] { Language.English };
                case Localization.PAL: return new Language[] { Language.English, Language.German, Language.French, Language.Spanish };
                default: return null;
            }
        }

        public static bool IsBuildNintendo(RomVersion build)
        {
            if (build == Build.UNKNOWN
                || build == Build.CUSTOM)
                return false;
            return true;
        }

        public static IEnumerable<Language> GetAllSupportedLanguages()
        {
            yield return Language.Japanese;
            yield return Language.English;
            yield return Language.German;
            yield return Language.French;
        }

        public IEnumerable<Language> GetSupportedLanguages()
        {
            return GetSupportedLanguages(Version);
        }

        public static void ConsolePrintSupportedVersions()
        {
            Console.WriteLine("Majora's Mask: use GameId 'mm'");
            Console.WriteLine("Version:");
            foreach (var item in MRom.GetSupportedBuilds())
            {
                var info = MRom.BuildInformation.Get(item);
                Console.WriteLine($" {info.Version + ":",-5} {info.Name}");
            }
        }
    }
}
