using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mzxrules.OcaLib
{
    public class RomFileToken
    {
        public Game Game { get; private set; }
        ORom.FileList oFile { get;  set; }
        MRom.FileList mFile { get;  set; }

        public RomFileToken(ORom.FileList file)
        {
            Game = Game.OcarinaOfTime;
            oFile = file;
            mFile = MRom.FileList.invalid;
        }

        public RomFileToken(MRom.FileList file)
        {
            Game = Game.MajorasMask;
            oFile = ORom.FileList.invalid;
            mFile = file;
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
            return token.oFile;
        }
        public static implicit operator MRom.FileList(RomFileToken token)
        {
            return token.mFile;
        }
        public static implicit operator Game(RomFileToken token)
        {
            return token.Game;
        }
        public override string ToString()
        {
            switch (Game)
            {
                case Game.OcarinaOfTime: return this.oFile.ToString();
                case Game.MajorasMask: return this.mFile.ToString();
                default: return base.ToString();
            }
        }
    }
}
