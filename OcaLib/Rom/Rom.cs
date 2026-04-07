using System;
using System.Linq;

namespace mzxrules.OcaLib
{
    public class Rom
    {
        public string FileLocation { get; set; }
        public VFileTable Files { get; set; }
        public RomVersion Version => Files.Version;
        public bool IsCompressed { get; set; }


        public int Scenes { get { return Files.Tables.Scenes.Records; } }
        public int Objects { get { return Files.Tables.Objects.Records; } }
        public int Particles { get { return Files.Tables.Particles.Records; } }
        public int Actors { get { return Files.Tables.Actors.Records; } }

        public enum Language
        {
            Japanese,
            English,
            French,
            German,
            Spanish,
            Chinese
        }

        protected Rom(string fileLocation, RomVersion version)
        {
            Files = version.Game switch
            {
                Game.OcarinaOfTime => new OFileTable(fileLocation, version),
                Game.MajorasMask => new MFileTable(fileLocation, version),
                _ => throw new InvalidOperationException("Invalid Rom.Game"),
            };

            FileRecord fileRecord = Files.FirstOrDefault(static x => x.IsCompressed);

            IsCompressed = !fileRecord.IsNull;
        }

        public static Rom New(string fileLocation, RomVersion version)
        {
            return version.Game switch
            {
                Game.OcarinaOfTime => new ORom(fileLocation, version),
                Game.MajorasMask => new MRom(fileLocation, version),
                _ => null
            };
        }
    }
}