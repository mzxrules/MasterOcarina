using System.Collections.Generic;
using System.Linq;

namespace mzxrules.OcaLib
{
    public partial class MRom
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

                    new BuildInformation { _CRC = 0x0000000000000000, _Version = Build.J0, _Name = "NTSC-J 1.0", _Localization = Localization.NTSCJ},
                    new BuildInformation { _CRC = 0x0000000000000000, _Version = Build.J1, _Name = "NTSC-J 1.1", _Localization = Localization.NTSCJ},

                    new BuildInformation { _CRC = 0x0000000000000000, _Version = Build.U0, _Name = "NTSC-U 1.0", _Localization = Localization.NTSCU},

                    new BuildInformation { _CRC = 0x0000000000000000, _Version = Build.P0, _Name = "PAL 1.0", _Localization = Localization.PAL},
                    new BuildInformation { _CRC = 0x0000000000000000, _Version = Build.P1, _Name = "PAL 1.1", _Localization = Localization.PAL},

                    new BuildInformation { _CRC = 0x0000000000000000, _Version = Build.GCJ, _Name = "GCJ 1.0", _Localization = Localization.NTSCJ},
                    new BuildInformation { _CRC = 0x0000000000000000, _Version = Build.GCU, _Name = "GCU 1.0", _Localization = Localization.NTSCU},
                    new BuildInformation { _CRC = 0x0000000000000000, _Version = Build.GCP, _Name = "GCP 1.0", _Localization = Localization.PAL},

                    new BuildInformation { _CRC = 0x0000000000000000, _Version = Build.DBG, _Name = "PAL DBG", _Localization = Localization.PAL},
                    
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

        private static Language[] SupportedLanguages = new Language[] { 
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

        private static List<RomVersion> SupportedBuilds = new List<RomVersion> {
             Build.J0,
             Build.J1,
             Build.U0,
             Build.P0,
             Build.P1,
             Build.GCJ,
             Build.GCU,
             Build.GCP,
             Build.DBG,
        };

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

            //switch (v)
            //{
            //    case Build.N0: return Localization.NTSC; //NTSC 1.0
            //    case Build.N1: return Localization.NTSC; //NTSC 1.1
            //    case Build.N2: return Localization.NTSC; //NTSC 1.2

            //    case Build.P0: return Localization.PAL; //PAL 1.0 
            //    case Build.P1: return Localization.PAL; //PAL 1.1

            //    case Build.GCNJ: return Localization.NTSC;
            //    case Build.GCNP: return Localization.PAL;

            //    case Build.MQP: return Localization.PAL;
            //    case Build.MQJ: return Localization.NTSC;
            //    case Build.DBGMQ: return Localization.PAL;

            //    //non-official builds
            //    //case Build.DUNGRUSH: return Localization.NTSC;
            //    //case Build.DUNGRUSH2: return Localization.NTSC;
            //    default: return Localization.UNKNOWN;
            //}
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
    }
}
