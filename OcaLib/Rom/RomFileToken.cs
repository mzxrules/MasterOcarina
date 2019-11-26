namespace mzxrules.OcaLib
{
    public class RomFileToken
    {
        public Game Game { get; private set; }
        ORom.FileList OFile { get; set; }
        MRom.FileList MFile { get; set; }

        public RomFileToken(ORom.FileList file)
        {
            Game = Game.OcarinaOfTime;
            OFile = file;
            MFile = MRom.FileList.invalid;
        }

        public RomFileToken(MRom.FileList file)
        {
            Game = Game.MajorasMask;
            OFile = ORom.FileList.invalid;
            MFile = file;
        }

        public static implicit operator RomFileToken(ORom.FileList file)
        {
            return new RomFileToken(file);
        }
        public static implicit operator RomFileToken(MRom.FileList file)
        {
            return new RomFileToken(file);
        }
        public static implicit operator ORom.FileList(RomFileToken token)
        {
            return token.OFile;
        }
        public static implicit operator MRom.FileList(RomFileToken token)
        {
            return token.MFile;
        }
        public static implicit operator Game(RomFileToken token)
        {
            return token.Game;
        }

        public static RomFileToken Select(RomVersion version, ORom.FileList ootFile, MRom.FileList mmFile)
        {
            return (version == Game.OcarinaOfTime) ? (RomFileToken)ootFile : (RomFileToken)mmFile;
        }

        public override string ToString()
        {
            switch (Game)
            {
                case Game.OcarinaOfTime: return OFile.ToString();
                case Game.MajorasMask: return MFile.ToString();
                default: return base.ToString();
            }
        }
    }
}
