using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Collections.Generic;

namespace Spectrum
{
    class OvlParticle : ParticleOverlayRecord, IRamItem, IVRamItem, IFile, IActorItem
    {
        static int TOTAL_PARTICLE_EFFECTS; //
        static int LENGTH = 0x1C;
        public static int OVL_TABLE_ADDR { get { return SpectrumVariables.ParticleEffect_Ovl_Table; } }// = 0x0E8530;
        public bool IsFileLoaded { get { return Ram.Start != 0; } }

        public int Actor
        {
            get
            {
                return Id;
            }
        }

        internal static void ChangeVersion(RomVersion v, bool g)
        {
            if (v.Game == Game.OcarinaOfTime)
                TOTAL_PARTICLE_EFFECTS = 0x19;
            else
                TOTAL_PARTICLE_EFFECTS = 0x27;
            //InstanceSize = new ushort[TOTAL_PARTICLE_EFFECTS];
        }

        public OvlParticle(int index, byte[] data)
            : base(index, data)
        {

        }

        public static List<OvlParticle> GetFiles()
        {
            List<OvlParticle> particleFiles = new List<OvlParticle>();
            OvlParticle working;
            byte[] particleOvlTable;
            byte[] particleFileData = new byte[LENGTH];

            particleOvlTable = Zpr.ReadRam(OVL_TABLE_ADDR, TOTAL_PARTICLE_EFFECTS * LENGTH);
            particleOvlTable.Reverse32();
            for (int i = 0; i < TOTAL_PARTICLE_EFFECTS; i++)
            {
                Array.Copy(particleOvlTable, i * LENGTH, particleFileData, 0, LENGTH);
                Endian.Reverse32(particleFileData);
                working = new OvlParticle(i, particleFileData);
                if (working.IsFileLoaded)
                    particleFiles.Add(working);
            }
            return particleFiles;
        }

        public override string ToString()
        {
            return $"PF {Id:X4}:  {1:X4} {0:X2} DATA {RamStart:X8}:{VRom.Start:X8}";
        }
    }
}
