using System;

using static mzxrules.OcaLib.AddressToken;

namespace mzxrules.OcaLib
{
    public class TableInfo
    {
        public enum Type
        {
            GameOvls,
            PlayerPause,
            Actors,
            Particles,
            Objects,
            Scenes,
            TitleCards,
            HyruleSkybox,
            Transitions,
            MapMarkData
        }
        public class Table
        {
            /// <summary>
            /// Key used to look up info from Addresses.xml
            /// </summary>
            public AddressToken Id { get; set; }

            /// <summary>
            /// Length of each record in bytes
            /// </summary>
            public int Length { get; set; }

            /// <summary>
            /// Number of records within the table
            /// </summary>
            public int Records { get; set; }

            /// <summary>
            /// Start offset of dlf data or something
            /// </summary>
            public int StartOff { get; set; }
        }

        public Table GameOvls = new Table();
        public Table PlayerPause = new Table(); 
        public Table Actors = new Table();
        public Table Particles = new Table();
        public Table Objects = new Table();
        public Table Scenes = new Table();
        public Table TitleCards = new Table();
        public Table HyruleSkybox = new Table();
        public Table Transitions = new Table();
        public Table MapMarkData = new Table();

        public TableInfo()
        {

        }

        public TableInfo(RomVersion version)
        {
            if (version == Game.OcarinaOfTime)
            {
                int scenes = (version == ORom.Build.DBGMQ) ? 110 : 101;
                GameOvls = new Table    { Id = GameContextTable_Start,        Length = 0x30, StartOff = 4, Records = 6 };
                PlayerPause = new Table { Id = PlayerPauseOverlayTable_Start, Length = 0x1C, StartOff = 4, Records = 2 };
                Actors = new Table      { Id = ActorTable_Start,              Length = 0x20, StartOff = 0, Records = 0x1D7 };
                Particles = new Table   { Id = ParticleTable_Start,           Length = 0x1C, StartOff = 0, Records = 0x25 };
                Objects = new Table     { Id = ObjectTable_Start,             Length = 0x08, StartOff = 0, Records = 0x192 };
                Scenes = new Table      { Id = SceneTable_Start,              Length = 0x14, StartOff = 0, Records = scenes };
                TitleCards = new Table  { Id = SceneTable_Start,              Length = 0x14, StartOff = 8, Records = scenes };
                HyruleSkybox = new Table{ Id = HyruleSkyboxTable_Start,       Length = 0x08, StartOff = 0, Records = 18 };
                MapMarkData = new Table { Id = MapMarkDataTable_Start,        Length = 0x18, StartOff = 4, Records = 1 };
            }
            else if (version == Game.MajorasMask)
            {
                GameOvls = new Table    { Id = GameContextTable_Start,        Length = 0x30, StartOff = 0, Records = 7 };
                PlayerPause = new Table { Id = PlayerPauseOverlayTable_Start, Length = 0x1C, StartOff = 4, Records = 2 };
                Actors = new Table      { Id = ActorTable_Start,              Length = 0x20, StartOff = 0, Records = 0x2B2 };
                Particles = new Table   { Id = ParticleTable_Start,           Length = 0x1C, StartOff = 0, Records = 0x027 };
                Objects = new Table     { Id = ObjectTable_Start,             Length = 0x08, StartOff = 0, Records = 0x283 };
                Scenes = new Table      { Id = SceneTable_Start,              Length = 0x10, StartOff = 0, Records = 0x071 };
                Transitions = new Table { Id = TransitionTable_Start,         Length = 0x1C, StartOff = 0xC, Records = 7};
            }
        }

        public Table GetTable(Type type)
        {
            switch (type)
            {
                case Type.Actors: return Actors;
                case Type.GameOvls: return GameOvls;
                case Type.HyruleSkybox: return HyruleSkybox;
                case Type.Objects: return Objects;
                case Type.Particles: return Particles;
                case Type.PlayerPause: return PlayerPause;
                case Type.Scenes: return Scenes;
                case Type.TitleCards: return TitleCards;
                case Type.Transitions: return Transitions;
                case Type.MapMarkData: return MapMarkData;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
