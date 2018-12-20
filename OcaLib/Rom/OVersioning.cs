using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mzxrules.OcaLib
{
    public partial class ORom
    {
        public class BuildInformation
        {
            public static BuildInformation[] builds;

            public Build Version { get { return _Version; } }
            public string Name { get { return _Name; } }
            public Localization Localization { get { return _Localization; } }
            public ulong CRC { get { return _CRC; } }

            Build _Version;
            string _Name;
            Localization _Localization;
            ulong _CRC;

            BuildInformation() { }

            static BuildInformation()
            {
                builds = new BuildInformation[] {
                    new BuildInformation { _CRC = 0x0000000000000000, _Version = Build.UNKNOWN, _Name = "Unknown", _Localization = Localization.UNKNOWN},

                    new BuildInformation { _CRC = 0xEC7011B77616D72B, _Version = Build.N0, _Name = "NTSC 1.0", _Localization = Localization.NTSC },
                    new BuildInformation { _CRC = 0xD43DA81F021E1E19, _Version = Build.N1, _Name = "NTSC 1.1", _Localization = Localization.NTSC },
                    new BuildInformation { _CRC = 0x693BA2AEB7F14E9F, _Version = Build.N2, _Name = "NTSC 1.2", _Localization = Localization.NTSC },

                    new BuildInformation { _CRC = 0xB044B569373C1985, _Version = Build.P0, _Name = "PAL 1.0", _Localization = Localization.PAL },
                    new BuildInformation { _CRC = 0xB2055FBD0BAB4E0C, _Version = Build.P1, _Name = "PAL 1.1", _Localization = Localization.PAL },

                    new BuildInformation { _CRC = 0xF611F4BAC584135C, _Version = Build.GCJ, _Name = "NTSC-J GCN", _Localization = Localization.NTSC },
                    new BuildInformation { _CRC = 0xF3DD35BA4152E075, _Version = Build.GCU, _Name = "NTSC-U GCN", _Localization = Localization.NTSC },
                    new BuildInformation { _CRC = 0x09465AC3F8CB501B, _Version = Build.GCP, _Name = "PAL GCN", _Localization = Localization.PAL },
            
                    new BuildInformation { _CRC = 0xF43B45BA2F0E9B6F, _Version = Build.MQJ, _Name = "JPN Master Quest", _Localization = Localization.NTSC},
                    new BuildInformation { _CRC = 0xF034001AAE47ED06, _Version = Build.MQU, _Name = "USA Master Quest", _Localization = Localization.NTSC},
                    new BuildInformation { _CRC = 0x1D4136F3AF63EEA9, _Version = Build.MQP, _Name = "PAL Master Quest", _Localization = Localization.PAL },
                    new BuildInformation { _CRC = 0x0000000000000000, _Version = Build.DBGMQ, _Name = "Debug Master Quest", _Localization = Localization.PAL },
                    new BuildInformation { _CRC = 0x0000000000000000, _Version = Build.IQUEC, _Name = "Chinese iQue", _Localization = Localization.CHI },
                    new BuildInformation { _CRC = 0x0000000000000000, _Version = Build.IQUET, _Name = "Taiwanese iQue", _Localization = Localization.CHI }
                };
            }

            public static BuildInformation Get(Build v)
            {
                return builds.SingleOrDefault(x => x._Version == v);
            }
            public static Localization GetLocalization(Build v)
            {
                BuildInformation stats = builds.SingleOrDefault(x => x._Version == v);
                return (stats != null) ? stats._Localization : Localization.UNKNOWN;
            }

            internal static ulong GetCrc(Build v)
            {
                BuildInformation stats = builds.SingleOrDefault(x => x._Version == v);
                return (stats != null) ? stats._CRC : 0;
            }
        }


        internal enum Bank
        {
            unknown0 = 0,
            unknown1 = 1,
            scene = 2,
            map = 3
        }

        //private static Language[] SupportedLanguages = new Language[] {
        //    Language.Japanese,
        //    Language.English,
        //    Language.German,
        //    Language.French, };

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
            GCU, //NTSC-U GCN
            GCP, //PAL GCN

            MQJ, //NTSC-J GCN MQ
            MQU, //NTSC-U GCN MQ
            MQP, //PAL GCN MQ

            DBGMQ, //PAL MQ Debug
            IQUEC, //Chinese Ocarina of Time
            IQUET, //Taiwanese Ocarina of Time
        }

        private static Build[] SupportedBuilds = new Build[] {

            Build.N0, //NTSC 1.0
            Build.N1, //NTSC 1.1
            Build.N2, //NTSC 1.2

            Build.P0, //PAL 1.0 
            Build.P1, //PAL 1.1

            Build.GCJ,
            Build.GCP,
            Build.GCP,

            Build.MQJ,
            Build.MQU,
            Build.MQP,

            Build.DBGMQ,
        };

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
            yield return Build.GCP;
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
    }
}
