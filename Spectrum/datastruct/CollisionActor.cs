using System;
using mzxrules.Helper;

namespace Spectrum
{

    class CollisionActor
    {
        class Moment
        {
            Vector3<float> Scale;
            Vector3<short> Rotation;
            Vector3<float> Coordinates;

            public Moment(Ptr start)
            {
                Scale = new Vector3<float>(start.ReadFloat(0x00), start.ReadFloat(0x04), start.ReadFloat(0x08));
                Rotation = new Vector3<short>(start.ReadInt16(0x0C), start.ReadInt16(0x0A), start.ReadInt16(0x10));
                Coordinates = new Vector3<float>(start.ReadFloat(0x14), start.ReadFloat(0x18), start.ReadFloat(0x1C));
            }

            public override string ToString()
            {
                return string.Format("({0:F2}, {1:F2}, {2:F2}) {3:X4} {4:X4} {5:X4} ({6:F1}, {7:F1}, {8:F1})",
                    Scale.x, Scale.y, Scale.z,
                    Rotation.x, Rotation.y, Rotation.z,
                    Coordinates.x, Coordinates.y, Coordinates.z);

            }
        }

        public N64Ptr Address;
        public N64Ptr ActorInstance;
        public short ActorId;
        public N64Ptr MeshPtr;
        Moment First;  //0x14
        Moment Second; //0x34

        public CollisionActor(Ptr pointer)
        {
            Address = (int)pointer;
            ActorInstance = pointer.ReadInt32(0);
            ActorId = pointer.Deref().ReadInt16(0);
            MeshPtr = pointer.ReadInt32(4);
            First = new Moment(pointer.RelOff(0x14));
            Second = new Moment(pointer.RelOff(0x34));
        }

        public override string ToString()
        {
            return $"{Address.Base():X6}: AI {ActorId:X4} - {(int)ActorInstance:X8}   MESH {(int)MeshPtr:X8} {Environment.NewLine}"
                + $"         {First}{Environment.NewLine}"
                + $"         {Second}";
        }
    }
}
