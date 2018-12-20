using mzxrules.Helper;
using System;
using System.IO;

namespace mzxrules.OcaLib.Actor
{
    public class TransitionActor : ActorSpawn
    {
        byte SwitchToFrontRoom;
        byte SwitchToFrontCamera;
        byte SwitchToBackRoom;
        byte SwitchToBackCamera;
        public TransitionActor(byte[] record)
        {
            SwitchToFrontRoom = record[0];
            SwitchToFrontCamera = record[1];
            SwitchToBackRoom = record[2];
            SwitchToBackCamera = record[3];

            Endian.Convert(out ushort actor, record, 4);
            Actor = actor;
            
            Endian.Convert(out Vector3<short> coords, record, 6);
            Coords = coords;
            
            Endian.Convert(out ushort ry, record, 12);
            Rotation = new Vector3<ushort>(0, ry, 0);

            Endian.Convert(out ushort variable, record, 14);
            Variable = variable;
        }
        protected TransitionActor()
        {
        }
        public override void Serialize(BinaryWriter bw)
        {
            bw.Write(SwitchToFrontRoom);
            bw.Write(SwitchToFrontCamera);
            bw.Write(SwitchToBackRoom);
            bw.Write(SwitchToBackCamera);
            bw.WriteBig(Actor);
            bw.WriteBig(Coords);
            bw.WriteBig(Rotation.y);
            bw.WriteBig(Variable);
        }

        public override string Print()
        {
            string varString;

            varString = GetVariable();
            return
                string.Format("{0}, {1:X4}:{2:X4}, {3}, {4}{5}",
                PrintTransition(),
                Actor,
                Variable,
                GetActorName(),
                (varString.Length > 0) ? varString + ", " : "",
                PrintCoordAndRotation());
        }
        protected override string GetActorName()
        {
            return "Unknown TransitionActor";
        }
        protected string PrintTransition()
        {
            return string.Format("{0:D2} {1:X2} -> {2:D2} {3:X2}",
                SwitchToBackRoom,
                SwitchToBackCamera,
                SwitchToFrontRoom,
                SwitchToFrontCamera);
        }
    }
}
