using mzxrules.Helper;
using mzxrules.OcaLib;

namespace Spectrum
{
    public class ActorInstance : IRamItem, IActorItem
    {
        public N64PtrRange Ram { get; }

        public N64Ptr Address;
        public int Actor
        {
            get { return ActorId; }
        }

        public ushort ActorId;
        public bool forcedActorId = false;
        public byte Type;
        public byte Room;
        public int PrevActor;
        public int NextActor;
        public N64Ptr Update;
        public ushort Variable;

        public byte ProcessInstance;
        public Vector3<float> Position = new Vector3<float>();
        public Vector3<short> Rotation;

        public ActorInstance(RomVersion version, N64Ptr address, ActorMemoryMapper map)
        {
            Ptr ptr = SPtr.New(address);


            ActorId = ptr.ReadUInt16(0); 
            Type = ptr.ReadByte(2); 
            Room = ptr.ReadByte(3); 
            Address = address;

            Variable = ptr.ReadUInt16(0x1C); 

            Position = new Vector3<float>(
                ptr.ReadFloat(0x24),
                ptr.ReadFloat(0x28),
                ptr.ReadFloat(0x2C));


            int off = version.Game == Game.OcarinaOfTime ? 0 : 8;

            Rotation = new Vector3<short>(
                ptr.ReadInt16(0xB4 + off),
                ptr.ReadInt16(0xB6 + off),
                ptr.ReadInt16(0xB8 + off));

            PrevActor = ptr.ReadInt32(0x120 + off); 
            NextActor = ptr.ReadInt32(0x124 + off);
            Update = ptr.ReadInt32(0x130 + off);
            ProcessInstance = ptr.ReadByte(0x115);


            map.GetActorIdAndSize(this, out ushort actorId, out uint instanceSize);
            ActorId = actorId;

            Ram = new N64PtrRange(Address, Address + instanceSize);
        }
        public override string ToString()
        {
            string coord = $"({Position.x,7:F1} {Position.y,7:F1} {Position.z,7:F1})";
            string actorId = (forcedActorId) ? $"i{ActorId:X3}" : $"{ActorId:X4}";

            return $"AI {actorId}:  {Type:X1} {Room:X2} {ProcessInstance:X1} {Variable:X4}"
                + $" {coord,-12} {Rotation.x:X4} {Rotation.y:X4} {Rotation.z:X4}";
        }

    }
}