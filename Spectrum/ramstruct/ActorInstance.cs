using mzxrules.Helper;
using mzxrules.OcaLib;

namespace Spectrum
{
    class ActorInstance : IRamItem, IActorItem
    {
        public FileAddress Ram { get; }

        public N64Ptr Address;
        public int Actor
        {
            get { return ActorId; }
        }

        public ushort ActorId;
        protected bool forcedActorId = false;
        public byte Type;
        public byte Room;
        public int PrevActor;
        public int NextActor;
        public ushort Variable;

        public byte ProcessInstance;
        public Vector3<float> Position = new Vector3<float>();
        public Vector3<short> Rotation;

        public ActorInstance(RomVersion version, N64Ptr address)
        {
            int instanceSize;
            int off = 0;

            Ptr ptr = SPtr.New(address);


            ActorId = ptr.ReadUInt16(0); 
            Type = ptr.ReadByte(2); 
            Room = ptr.ReadByte(3); 
            Address = address;

            try
            {
                if (version.Game == Game.OcarinaOfTime)
                {
                    if (ActorId == 0 && Type == 4)
                    {
                        ActorId = 0x008F;
                        forcedActorId = true;
                    }
                }
                // & 0xFFF is hack to correct instances with no proper actor id
                instanceSize = OvlActor.GetInstanceSize(ActorId & 0xFFF); 
            }
            catch
            {
                instanceSize = 0;
            }

            Ram = new FileAddress(Address, Address + instanceSize);


            Variable = ptr.ReadUInt16(0x1C); 

            Position = new Vector3<float>(
                ptr.ReadFloat(0x24),
                ptr.ReadFloat(0x28),
                ptr.ReadFloat(0x2C)); 


            if (version.Game == Game.OcarinaOfTime)
                off = 0;
            else
                off = 8;

            Rotation = new Vector3<short>(
                ptr.ReadInt16(0xB4 + off),
                ptr.ReadInt16(0xB6 + off),
                ptr.ReadInt16(0xB8 + off)); 


            PrevActor = ptr.ReadInt32(0x120 + off); 
            NextActor = ptr.ReadInt32(0x124 + off); 

            ProcessInstance = ptr.ReadByte(0x115); 
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