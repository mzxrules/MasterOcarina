using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Collections.Generic;

namespace Spectrum
{
    public class OvlParticle : ParticleOverlayRecord, IRamItem, IVRamItem, IFile, IActorItem
    {
        public bool IsFileLoaded { get { return Ram.Start != 0; } }

        public int Actor
        {
            get
            {
                return Id;
            }
        }

        public OvlParticle(int index, byte[] data)
            : base(index, data)
        {

        }

        public OvlParticle(ParticleOverlayRecord record) : base(record)
        {
        }

        public override string ToString()
        {
            return $"PF {Id:X4}:  {1:X4} {0:X2} DATA {RamStart:X8}:{VRom.Start:X8}";
        }
    }
}
