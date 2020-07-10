using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Collections.Generic;

namespace Spectrum
{
    public class OvlActor : ActorOverlayRecord, IFile, IVRamItem, IActorItem
    {
        public bool IsFileLoaded { get { return !Ram.Start.IsNull(); } }


        public OvlActor(int index, byte[] data)
            : base(index, data)
        {
        }

        public override string ToString()
        {
            int dataOffset = VRamActorInit - VRam.Start;
            int initRam = Ram.Start + dataOffset;
            int initRom = VRom.Start + dataOffset;
            return $"AF {Actor:X4}:  {AllocationType:X4} {NumSpawned:X2} FILE: " +
                $"{VRom.Start:X8}:{VRom.End:X8} INIT {initRam:X8}:{initRom:X8}";
        }
    }
}
