using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mzxrules.Helper;
using mzxrules.OcaLib;

namespace Spectrum
{
    class ActorInstance : IRamItem, IActorItem
    {
        /*
        struct z64_actor_t
        {
            u16                     number;
            u8                      type;
            u8                      status;
            u8                      __pad0000[4];
            union z64_xyz_t         coords_1;
            struct z64_rot_t        rotation_1;
            u8                      __pad0001[2];
            u16                     variable;
            u8                      __pad0002[6];
            union z64_xyz_t         coords_2;
            struct z64_rot_t        rotation_2;
            u8                      __pad0003[2];
            union z64_xyz_t         coords_3;
            struct z64_rot_t        rotation_3;
            u8                      __pad0004[6];
            union z64_xyz_t         scale;
            union z64_xyz_t         acceleration;
            u8                      __pad0005[184];
            struct z64_actor_t *    previous;
            struct z64_actor_t *    next;
            ZAFunc                  f_init;
            ZAFunc                  f_routine1;
            ZAFunc                  f_routine2;
            ZAFunc                  f_routine3;
            ZAFunc                  f_code_entry;
            u8                      __pad0006[84];
            ZAFunc                  f_next;
        };
         
         */

        public FileAddress Ram
        {
            get { return _RamAddress; }
        }
        private FileAddress _RamAddress;

        public int Address;
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

            _RamAddress = new FileAddress(Address, Address + instanceSize);


            Variable = ptr.ReadUInt16(0x1C); //BitConverter.ToUInt16(instance, Zpr.End16(0x1C));

            Position = new Vector3<float>(
                ptr.ReadFloat(0x24), //BitConverter.ToSingle(instance, 0x24),
                ptr.ReadFloat(0x28), //BitConverter.ToSingle(instance, 0x28),
                ptr.ReadFloat(0x2C)); //BitConverter.ToSingle(instance, 0x2C));


            if (version.Game == Game.OcarinaOfTime)
                off = 0;
            else
                off = 8;

            Rotation = new Vector3<Int16>(
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