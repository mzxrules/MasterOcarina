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
            return obj is RomVersion && this == (RomVersion)obj;
        }

        public override int GetHashCode()
        {
            return ((int)OVer << 16) + (int)MVer;
        }

        public bool IsCustomBuild()
        {
            if (Game == Game.OcarinaOfTime)
                return OVer == ORom.Build.CUSTOM;
            else if (Game == Game.MajorasMask)
                return MVer == MRom.Build.CUSTOM;
            return false;
        }

        public string GetGroup()
        {
            if (MVer == MRom.Build.J0
                || MVer == MRom.Build.J1)
                return "J";
            return null;
        }

        public string GetGameAbbr()
        {
            switch (Game)
            {
                case Game.OcarinaOfTime: return "oot";
                case Game.MajorasMask: return "mm" ;
                default: return "invalid";
            }
        }

        public override string ToString()
        {
            switch (Game)
            {
                case Game.OcarinaOfTime: return OVer.ToString();
                case Game.MajorasMask: return MVer.ToString();
                default: return base.ToString();
            }
        }

        public Type GetInternalType()
        {
            if (Game == Game.OcarinaOfTime)
                return OVer.GetType();
            else if (Game == Game.MajorasMask)
                return MVer.GetType();
            else return GetType();
        }

        public static bool TryGet(string game, string version, out RomVersion value)
        {
            value = new RomVersion(game, version);
            return value.Game != Game.Undefined;
        }
    }
}
