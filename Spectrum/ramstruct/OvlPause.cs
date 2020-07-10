using mzxrules.Helper;
using System;
using System.Collections.Generic;
using mzxrules.OcaLib;

namespace Spectrum
{
    class OvlPause : PlayPauseOverlayRecord, IFile, IVRamItem
    {

        public OvlPause(int i, byte[] data) : base(i, data)
        {

        }

        public OvlPause(PlayPauseOverlayRecord record) : base(record)
        {

        }

        public override string ToString()
        {
            string name;
            switch (Item)
            {
                case 0: name = $"{Item:X2} kaleido_scope"; break;
                case 1: name = $"{Item:X2} player_actor"; break;
                default: name = $"UNKNOWN {Item:X2}"; break;
            }
            return $"OVL KS:   {name}";
        }
    }
}
