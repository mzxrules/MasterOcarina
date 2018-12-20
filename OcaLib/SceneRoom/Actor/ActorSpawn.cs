using System;
using mzxrules.Helper;
using System.IO;
//OCA LIB **********************************
namespace mzxrules.OcaLib.Actor
{
    public class ActorSpawn : IActor
    {
        public static int SIZE = 0x10;
        public ushort Actor { get; set; } = 0xFFFF;
        public ushort Variable { get; set; }
        public Vector3<short> Coords { get; set; } = new Vector3<short>();
        public Vector3<bool> NoRotation { get; set; } = new Vector3<bool>();
        public Vector3<ushort> Rotation { get; set; } = new Vector3<ushort>();
        protected int[] objectDependencies;
        public ActorSpawn(short[] record)
        {
            Actor = (ushort)record[0];
            Coords = new Vector3<short>(record[1], record[2], record[3]);
            Rotation = new Vector3<ushort>(
                (ushort)record[4],
                (ushort)record[5],
                (ushort)record[6]);
            Variable = (ushort)record[7];

            objectDependencies = null;
        }
        protected ActorSpawn()
        {
        }
        public virtual void Serialize(BinaryWriter bw)
        {
            bw.WriteBig(Actor);
            bw.WriteBig(Coords);
            bw.WriteBig(Rotation);
            bw.WriteBig(Variable);
        }
        public virtual Vector3<short> GetCoords()
        {
            return Coords;
        }
        public virtual string Print()
        {
            string actorName;
            string variables;

            actorName = GetActorName();
            variables = GetVariable();
            return string.Format("{0:X4}:{1:X4} {2}{3}{4} {5}",
                Actor,
                Variable,
                (actorName.Length > 0) ? actorName + ", " : "",
                (variables.Length > 0) ? variables + ", " : "",
                PrintCoord(),
                PrintRotation());
        }
        public virtual string PrintCommaDelimited()
        {
            string actorName;
            string variables;

            actorName = GetActorName();
            variables = GetVariable();
            return string.Format("{0:X4},{1:X4},{2},{3},{4},{5},{6},{7:X4},{8:X4},{9:X4}",
                Actor,
                Variable,
                actorName.Replace(',', ';'),
                variables.Replace(',', ';'),
                Coords.x, Coords.y, Coords.z,
                Rotation.x, Rotation.y, Rotation.z);
        }
        protected virtual string GetVariable()
        {
            return "";
        }
        protected virtual string GetActorName()
        {
            return "???";
        }
        protected string PrintWithoutActor()
        {
            return $"{Variable:X4}, {PrintCoord()} {PrintRotation()}";
        }
        public virtual string PrintCoord()
        {
            return $"({Coords.x}, {Coords.y}, {Coords.z})";
        }
        public virtual string PrintRotation()
        {
            return $"({Rotation.x:X4}, {Rotation.y:X4}, {Rotation.z:X4})";
        }
        public string PrintCoordAndRotation()
        {
            return PrintCoord() + " " + PrintRotation();
        }
        protected static float Degrees(ushort v)
        {
            return ((float)v / ushort.MaxValue) * 360.0f;
        }
    }
}
