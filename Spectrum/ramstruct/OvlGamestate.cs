using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Collections.Generic;



namespace Spectrum
{
    class OvlGamestate : GameStateRecord, IFile, IVRamItem
    {
        public string Name { get; set; }

        public OvlGamestate(int index, byte[] data, RomVersion version)
            : base(index, data)
        {

        }

        public override string ToString()
        {
            return Name;
        }
    }
}
