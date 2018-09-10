using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugRomStats
{
    public static class ActorFactory
    {   
        public static ActorRecord NewActor(byte[] record)
        {
            ushort actor;

            actor = (ushort)((record[0] << 8) + record[1]);
            switch (actor)
            {
                case 0x000A:
                    return new ChestActor(record);
                default:
                    return new ActorRecord(record);
            }
        }
    }
    #region ActorRecord
    public class ActorRecord
    {
        public static int LENGTH = 0x10;
        public ushort Actor;
        public ushort Variable;
        public Vector3<short> coords = new Vector3<short>();
        public Vector3<ushort> rotation = new Vector3<ushort>();
        public ActorRecord(byte[] record)
        {
            Actor = (ushort)((record[0] << 8) + record[1]);
            coords.x = (short)((record[2] << 8) + record[3]);
            coords.y = (short)((record[4] << 8) + record[5]);
            coords.z = (short)((record[6] << 8) + record[7]);
            rotation.x = (ushort)((record[8] << 8) + record[9]);
            rotation.y = (ushort)((record[10] << 8) + record[11]);
            rotation.z = (ushort)((record[12] << 8) + record[13]);
            Variable = (ushort)((record[14] << 8) + record[15]);
        }
        public virtual string Print()
        {
            return String.Format("{0}, {3}, {1} {2} ",
                  Actor.ToString("X4"), 
                  PrintCoord(),
                  PrintRotation(),
                  Variable.ToString("X4"));
        }
        public string PrintCoord()
        {
            return string.Format("({0}, {1}, {2})",
                coords.x, coords.y, coords.z);
        }
        public string PrintRotation()
        {
            return string.Format("({0}, {1}, {2})",
                  Degrees(rotation.x).ToString("F0"),
                  Degrees(rotation.y).ToString("F0"),
                  Degrees(rotation.z).ToString("F0"));
        }
        public string PrintCoordAndRotation()
        {
            return PrintCoord() + " " + PrintRotation();
        }
        protected static float Degrees(ushort rx)
        {
            return ((float)rx / (float)ushort.MaxValue) * 360.0f;
        }
    }
    #endregion

    //Believe this is the Link actor.
    public class PositionRecord : ActorRecord
    {
        public PositionRecord(byte[] record)
            : base(record)
        {
        }
        public override string Print()
        {
            return PrintCoordAndRotation();
        }
    }
}
