using System;
using System.Collections.Generic;
using System.Linq;

namespace mzxrules.OcaLib
{
    public partial class ORom
    {
        public class BuildInformation
        {
            public static readonly BuildInformation[] builds = new BuildInformation[]
            {
                new BuildInformation { CRC = 0x0000000000000000, Version = Build.UNKNOWN, Name = "Unknown", Localization = Localization.UNKNOWN},

                new BuildInformation { CRC = 0xEC7011B77616D72B, Version = Build.N0, Name = "NTSC 1.0", Localization = Localization.NTSC },
                new BuildInformation { CRC = 0xD43DA81F021E1E19, Version = Build.N1, Name = "NTSC 1.1", Localization = Localization.NTSC },
                new BuildInformation { CRC = 0x693BA2AEB7F14E9F, Version = Build.N2, Name = "NTSC 1.2", Localization = Localization.NTSC },

                new BuildInformation { CRC = 0xB044B569373C1985, Version = Build.P0, Name = "PAL 1.0", Localization = Localization.PAL },
                new BuildInformation { CRC = 0xB2055FBD0BAB4E0C, Version = Build.P1, Name = "PAL 1.1", Localization = Localization.PAL },

                new BuildInformation { CRC = 0xF611F4BAC584135C, Version = Build.GCJ, Name = "NTSC-J GCN", Localization = Localization.NTSC },
                new BuildInformation { CRC = 0xF7F52DB82195E636, Version = Build.GCJC,Name = "NTSC-J GCN (Zelda Collection)", Localization = Localization.NTSC },
                new BuildInformation { CRC = 0xF3DD35BA4152E075, Version = Build.GCU, Name = "NTSC-U GCN", Localization = Localization.NTSC },
                new BuildInformation { CRC = 0x09465AC3F8CB501B, Version = Build.GCP, Name = "PAL GCN", Localization = Localization.PAL },

                new BuildInformation { CRC = 0xF43B45BA2F0E9B6F, Version = Build.MQJ, Name = "JPN Master Quest", Localization = Localization.NTSC},
                new BuildInformation { CRC = 0xF034001AAE47ED06, Version = Build.MQU, Name = "USA Master Quest", Localization = Localization.NTSC},
                new BuildInformation { CRC = 0x1D4136F3AF63EEA9, Version = Build.MQP, Name = "PAL Master Quest", Localization = Localization.PAL },
                new BuildInformation { CRC = 0x0000000000000000, Version = Build.DBGMQ, Name = "Debug Master Quest", Localization = Localization.PAL },
                new BuildInformation { CRC = 0x0000000000000000, Version = Build.IQUEC, Name = "Chinese iQue", Localization = Localization.CHI },
                new BuildInformation { CRC = 0x0000000000000000, Version = Build.IQUET, Name = "Traditional Chinese iQue", Localization = Localization.CHI }
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

        internal enum Bank
        {
            unknown0 = 0,
            unknown1 = 1,
            scene = 2,
            map = 3
        }

        public enum Build
        {
            UNKNOWN,
            CUSTOM,
            N0, //NTSC 1.0
            N1, //NTSC 1.1
            N2, //NTSC 1.2
            
            P0, //PAL 1.0 
            P1, //PAL 1.1

            GCJ, //NTSC-J GCN
            GCJC,//NTSC-J GCN, Zelda Collection
            GCU, //NTSC-U GCN
            GCP, //PAL GCN

            MQJ, //NTSC-J GCN MQ
            MQU, //NTSC-U GCN MQ
            MQP, //PAL GCN MQ

            DBGMQ, //PAL MQ Debug
            IQUEC, //Chinese Ocarina of Time
            IQUET, //Taiwanese Ocarina of Time
        }

        public enum Localization
        {
            UNKNOWN,
            NTSC,
            PAL,
            CHI
        }

        public static IEnumerable<RomVersion> GetSupportedBuilds()
        {
            yield return Build.N0; //NTSC 1.0
            yield return Build.N1; //NTSC 1.1
            yield return Build.N2; //NTSC 1.2

            yield return Build.P0; //PAL 1.0 
            yield return Build.P1; //PAL 1.1

            yield return Build.GCJ;
            yield return Build.GCU;
            yield return Build.GCP;

            yield return Build.MQJ;
            yield return Build.MQU;
            yield return Build.MQP;

            yield return Build.DBGMQ;
            yield return Build.IQUEC;
            yield return Build.IQUET;
        }

        public static Localization GetLocalization(Build v)
        {
            return BuildInformation.GetLocalization(v);

        }

        public static ulong GetCrc(Build v)
        {
            return BuildInformation.GetCrc(v);
        }


        public static IEnumerable<Language> GetSupportedLanguages(Build b)
        {
            switch (GetLocalization(b))
            {
                case Localization.NTSC: return new Language[] { Language.Japanese, Language.English };
                case Localization.PAL: return new Language[] { Language.English, Language.German, Language.French };
                case Localization.CHI: return new Language[] { Language.Chinese };
                default: return null;
            }
        }
        
        public static bool IsBuildNintendo(Build build)
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
            Console.WriteLine("Ocarina of Time: use GameId 'oot'");
            Console.WriteLine("Version:");
            foreach (var item in GetSupportedBuilds())
            {
                var info = BuildInformation.Get(item);
                Console.WriteLine($" {info.Version + ":",-5} {info.Name}");
            }
        }
    }
}
