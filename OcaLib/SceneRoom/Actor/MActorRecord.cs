using mzxrules.Helper;
using System;

namespace mzxrules.OcaLib.Actor
{
    public class MActorRecord : ActorSpawn
    {
        protected ushort DayFlags;
        protected byte Scene_0x1B;
        
        public MActorRecord(short[] record)
        {
            //Endian.Convert(out ushort actor, record, 0);
            //Actor = actor;
            Actor = (ushort)record[0];

            NoRotation = new Vector3<bool>
                (
                    (Actor & 0x8000) > 0,
                    (Actor & 0x4000) > 0,
                    (Actor & 0x2000) > 0
                );
            Actor &= 0xFFF;

            Coords = new Vector3<short>(record[1], record[2], record[3]);


            DayFlags = (ushort)((record[4] & 7) << 7 + (record[6] & 0x7F)); 

            ushort rx = Shift.AsUInt16((ushort)record[4], 0xFF80); 
            ushort ry = Shift.AsUInt16((ushort)record[5], 0xFF80); 
            ushort rz = Shift.AsUInt16((ushort)record[6], 0xFF80);
            Rotation = new Vector3<ushort>(rx, ry, rz);

            Scene_0x1B = Shift.AsByte((ushort)record[5], 0x7F);
            Variable = (ushort)record[7];
        }
        protected MActorRecord()
        {
        }
        new public virtual Vector3<short> GetCoords()
        {
            return Coords;
        }
        public override string Print()
        {
            string actorName;
            string variables;

            actorName = GetActorName();
            variables = GetVariable();
            return String.Format("{0:X3}:{1:X4} {2}{3}{4} {5} {6} Days: {7} 1B?: {8:X4}",
                Actor,
                Variable,
                (actorName.Length > 0) ? actorName + ", " : "",
                (variables.Length > 0) ? variables + ", " : "",
                PrintCoord(),
                PrintRotation(),
                PrintRotationVars(),
                PrintDayFlags(),
                Scene_0x1B);
        }


        private string PrintDayFlags()
        {
            return string.Format("({0} {1}{2}{3} {4})",
                (DayFlags >> 8) & 3,
                (DayFlags >> 6) & 3,
                (DayFlags >> 4) & 3,
                (DayFlags >> 2) & 3,
                (DayFlags) & 3);
        }
        override public string PrintCommaDelimited()
        {
            string actorName;
            string variables;

            actorName = GetActorName();
            variables = GetVariable();
            return String.Format("{0:X3},{1:X4},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14:X2}",
                Actor,
                Variable,
                actorName.Replace(',', ';'),
                variables.Replace(',', ';'),
                Coords.x, Coords.y, Coords.z,
                (!NoRotation.x) ? Rotation.x.ToString() : "",
                (!NoRotation.y) ? Rotation.y.ToString() : "",
                (!NoRotation.z) ? Rotation.z.ToString() : "",
                (NoRotation.x) ? Rotation.x.ToString("X2") : "",
                (NoRotation.y) ? Rotation.y.ToString("X2") : "",
                (NoRotation.z) ? Rotation.z.ToString("X2") : "",
                PrintDayFlags(),
                Scene_0x1B);
        }
        public override string PrintCoord()
        {
            return string.Format("({0}, {1}, {2})",
                Coords.x, Coords.y, Coords.z);
        }
        public override string PrintRotation()
        {
            return string.Format("({0}, {1}, {2})",
                (!NoRotation.x) ? Rotation.x.ToString() : "N/A",
                (!NoRotation.y) ? Rotation.y.ToString() : "N/A",
                (!NoRotation.z) ? Rotation.z.ToString() : "N/A");
        }

        private object PrintRotationVars()
        {
            return string.Format("({0}, {1}, {2})",
                (NoRotation.x) ? Rotation.x.ToString() : "N/A",
                (NoRotation.y) ? Rotation.y.ToString() : "N/A",
                (NoRotation.z) ? Rotation.z.ToString() : "N/A");
        }
    }
}
