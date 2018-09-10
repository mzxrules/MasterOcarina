using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Collections.Generic;

namespace Spectrum
{
    class OvlActor : ActorOverlayRecord, IFile, IVRamItem, IActorItem
    {
        //Length = 0x20
        static int TOTAL_ACTORS;// 0x1D7, 0x2B2
        static int ACTOR_LL_LENGTH = 12;
        public static Ptr ACTOR_LL_TABLE_ADDR { get { return SpectrumVariables.Actor_Category_Table; } }// = 0x1CA0D0;
        public static int OVL_TABLE_ADDR { get { return SpectrumVariables.Actor_Ovl_Table; } }// = 0x0E8530;

        static ushort[] InstanceSize;
        public bool IsFileLoaded { get { return Ram.Start != 0; } }

        static RomVersion version;

        internal static void ChangeVersion(RomVersion b)
        {
            TOTAL_ACTORS = (b.Game == Game.OcarinaOfTime) ? 0x1D7 : 0x2B2;
            InstanceSize = new ushort[TOTAL_ACTORS];

            version = b;
        }

        public static ushort GetInstanceSize(int actor)
        {
            return InstanceSize[actor];
        }

        public OvlActor(int index, byte[] data)
            : base(index, data)
        {
            if (InstanceSize[Actor] == 0)
                SetInstanceSize();
        }

        public static List<OvlActor> GetActorFiles()
        {
            List<OvlActor> actorFiles = new List<OvlActor>();
            OvlActor working;
            byte[] actorTbl;
            byte[] actorFileData = new byte[LENGTH];

            actorTbl = Zpr.ReadRam(OVL_TABLE_ADDR, TOTAL_ACTORS * LENGTH);

            for (int i = 0; i < TOTAL_ACTORS; i++)
            {
                Array.Copy(actorTbl, i * LENGTH, actorFileData, 0, LENGTH);
                working = new OvlActor(i, actorFileData);
                if (working.IsFileLoaded)
                    actorFiles.Add(working);
            }
            return actorFiles;
        }

        public static List<ActorInstance> GetActorInstances(int category)
        {
            return GetActorsInCategory(category);
        }

        public static List<ActorInstance> GetActorInstances()
        {
            List<ActorInstance> result = new List<ActorInstance>();

            for (int i = 0; i < ACTOR_LL_LENGTH; i++)
            {
                result.AddRange(GetActorsInCategory(i));
            }
            return result;
        }

        private static List<ActorInstance> GetActorsInCategory(int cat)
        {
            List<ActorInstance> result = new List<ActorInstance>();
            int actors;
            N64Ptr actorPtr;

            if (SpectrumVariables.GlobalContext == 0)
                return new List<ActorInstance>();

            if (version.Game == Game.OcarinaOfTime)
            {
                actors = Zpr.ReadRamInt32(ACTOR_LL_TABLE_ADDR + (cat * 8));
                actorPtr = Zpr.ReadRamInt32(ACTOR_LL_TABLE_ADDR + (cat * 8) + 4);
            }
            else
            {
                actors = Zpr.ReadRamInt32(ACTOR_LL_TABLE_ADDR + (cat * 0xC));
                actorPtr = Zpr.ReadRamInt32(ACTOR_LL_TABLE_ADDR + (cat * 0xC) + 4);
            }

            if (actors > 256)
                return result;
            
            if (actors > 0)
            {
                for (int j = 0; j < actors; j++)
                {
                    ActorInstance ai = new ActorInstance(version, actorPtr);//, Zpr.ReadRam((int)actorPtr , 0x13C));
                    result.Add(ai);
                    actorPtr = ai.NextActor;
                }
            }
            return result;
        }

        private void SetInstanceSize()
        {
            N64Ptr readAddr;
            uint offset;
            

            if ((VRamActorInfo & 0xFFFFFF) < 0x800000)
            {
                //Read address 0E in the ovl file
                readAddr = VRamActorInfo + 0x0E;
            }
            else if (!IsFileLoaded)
            {
                return;
            }
            else
            {
                offset = (uint)(VRamActorInfo - VRam.Start) + 0x0E;
                readAddr = Ram.Start + offset;
            }

            InstanceSize[Actor] = (ushort)Zpr.ReadRamInt16(readAddr);
        }

        public override string ToString()
        {
            int dataOffset = (int)(VRamActorInfo-VRam.Start);
            int initRam = (int)(Ram.Start + dataOffset);
            int initRom = (int)(VRom.Start + dataOffset);
            return $"AF {Actor:X4}:  {AllocationType:X4} {NumSpawned:X2} FILE: " +
                $"{VRom.Start:X8}:{VRom.End:X8} INIT {initRam:X8}:{initRom:X8}";
        }
    }
}
