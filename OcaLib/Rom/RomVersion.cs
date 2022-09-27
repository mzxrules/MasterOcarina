using System;
using System.Runtime.Serialization;

namespace mzxrules.OcaLib
{
    /// <summary>
    /// Adapter class designed to let you pass in equivalent enumerations when requested
    /// </summary>
    [DataContract]
    public struct RomVersion
    {
        [DataMember]
        public Game Game { get; private set; }
        [DataMember]
        ORom.Build OVer { get; set; }
        [DataMember]
        MRom.Build MVer { get; set; }

        private RomVersion(ORom.Build build)
        {
            Game = Game.OcarinaOfTime;
            OVer = build;
            MVer = MRom.Build.UNKNOWN;
        }

        private RomVersion(MRom.Build build)
        {
            Game = Game.MajorasMask;
            OVer = ORom.Build.UNKNOWN;
            MVer = build;
        }

        public RomVersion(Game game, string build)
        {
            if (game == Game.OcarinaOfTime)
            {
                MVer = MRom.Build.UNKNOWN;
                if (Enum.TryParse(build, true, out ORom.Build oVer))
                {
                    Game = Game.OcarinaOfTime;
                    OVer = oVer;
                }
                else
                {
                    Game = Game.Undefined;
                    OVer = ORom.Build.UNKNOWN;
                }
            }
            else if (game == Game.MajorasMask)
            {
                OVer = ORom.Build.UNKNOWN;
                if (Enum.TryParse(build, true, out MRom.Build mVer))
                {
                    Game = Game.MajorasMask;
                    MVer = mVer;
                }
                else
                {
                    Game = Game.Undefined;
                    MVer = MRom.Build.UNKNOWN;
                }
            }
            else
            {
                Game = Game.Undefined;
                OVer = ORom.Build.UNKNOWN;
                MVer = MRom.Build.UNKNOWN;
            }
        }

        public RomVersion(string game, string build) : this(ResolveGame(game), build) { }

        public RomVersion(string key)
        {
            MVer = MRom.Build.UNKNOWN;
            OVer = ORom.Build.UNKNOWN;
            Game = Game.Undefined;

            if (!key.Contains('.'))
            {
                return;
            }

            var game_ver = key.Split(new char[] { '.' }, 1);
            if (Enum.TryParse(game_ver[0], out Game game))
            {
                Game = game;
            }

            switch (Game)
            {
                case Game.OcarinaOfTime:
                    if (Enum.TryParse(game_ver[1], out ORom.Build oV))
                    {
                        OVer = oV;
                    }
                    return;
                case Game.MajorasMask:
                    if (Enum.TryParse(game_ver[1], out MRom.Build mV))
                    {
                        MVer = mV;
                    }
                    return;
            }
        }

        private static Game ResolveGame(string game)
        {
            if (game.ToLowerInvariant() == "oot"
                   || game == Game.OcarinaOfTime.ToString())
                return Game.OcarinaOfTime;
            else if (game.ToLowerInvariant() == "mm"
                || game == Game.MajorasMask.ToString())
                return Game.MajorasMask;

            return Game.Undefined;
        }

        public static implicit operator RomVersion(ORom.Build v)
        {
            return new RomVersion(v);
        }

        public static implicit operator RomVersion(MRom.Build v)
        {
            return new RomVersion(v);
        }

        public static implicit operator ORom.Build(RomVersion v)
        {
            return v.OVer;
        }

        public static implicit operator MRom.Build(RomVersion v)
        {
            return v.MVer;
        }

        public static implicit operator Game(RomVersion v)
        {
            return v.Game;
        }

        public static bool operator== (RomVersion a, RomVersion b)
        {
            return a.Game == b.Game && a.OVer == b.OVer && a.MVer == b.MVer;
        }

        public static bool operator!= (RomVersion a, RomVersion b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is RomVersion version && this == version;
        }

        public override int GetHashCode()
        {
            return ((int)OVer << 16) + (int)MVer;
        }

        public bool IsCustomBuild => Game switch
        {
            Game.OcarinaOfTime => OVer == ORom.Build.CUSTOM,
            Game.MajorasMask => MVer == MRom.Build.CUSTOM,
            _ => false,
        };

        public string GetGroup() => MVer switch
        {
            MRom.Build.J0 or MRom.Build.J1 => "J",
            _ => null
        };

        public string GameNiceName => Game switch
        {
            Game.OcarinaOfTime => "Ocarina of Time",
            Game.MajorasMask => "Majora's Mask",
            _ => "Invalid",
        };

        public string GameAbbr => Game switch
        {
            Game.OcarinaOfTime => "oot",
            Game.MajorasMask => "mm",
            _ => "invalid",
        };

        public string VerAbbr => Game switch
        {
            Game.OcarinaOfTime => OVer.ToString().ToLowerInvariant(),
            Game.MajorasMask => MVer.ToString().ToLowerInvariant(),
            _ => "n/a",
        };

        public string UniqueKey => $"{Game}.{ToString()}";

        public string ShortUniqueKey => $"{GameAbbr}_{VerAbbr}";

        public override string ToString() => Game switch
        {
            Game.OcarinaOfTime => OVer.ToString(),
            Game.MajorasMask => MVer.ToString(),
            _ => base.ToString(),
        };

        public Type GetInternalType() => Game switch
        {
            Game.OcarinaOfTime => OVer.GetType(),
            Game.MajorasMask => MVer.GetType(),
            _ => GetType()
        };

        public static bool TryGet(string game, string version, out RomVersion value)
        {
            value = new RomVersion(game, version);
            return value.Game != Game.Undefined;
        }
    }
}
