using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    class CollisionBody
    {
        public N64Ptr Address;
        public short ActorId;

        /* 0x00 */
        public N64Ptr Instance;
        /* 0x04 */
        public N64Ptr CollidingInstance1;
        /* 0x08 */
        public N64Ptr CollidingInstance2;
        /* 0x0C */
        public N64Ptr CollidingInstance3;
        int flags1, flags2;

        public CollisionBody(Ptr pointer)
        {
            Address = (int)pointer;
            Instance = pointer.ReadInt32(0);
            ActorId = pointer.Deref().ReadInt16(0);
            CollidingInstance1 = pointer.ReadInt32(0x04);
            CollidingInstance2 = pointer.ReadInt32(0x08);
            CollidingInstance3 = pointer.ReadInt32(0x0C);
            flags1 = pointer.ReadInt32(0x10);
            flags2 = pointer.ReadInt32(0x14);
        }
        public override string ToString()
        {
            return $"{Address.Base():X6}: AI {ActorId:X4} OFF: {(Address-Instance&0xFFFFFF):X4}  "
                + $" {(int)Instance:X8} {(int)CollidingInstance1:X8} {(int)CollidingInstance2:X8} {(int)CollidingInstance3:X8}  "
                + $" {flags1:X8} {flags2:X8}";
        }
    }
}
