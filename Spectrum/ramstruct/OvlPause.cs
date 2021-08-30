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
            string name = Item switch
            {
                0 => $"{Item:X2} kaleido_scope",
                1 => $"{Item:X2} player_actor",
                _ => $"UNKNOWN {Item:X2}",
            };
            return $"OVL KS:   {name}";
        }
    }
}
