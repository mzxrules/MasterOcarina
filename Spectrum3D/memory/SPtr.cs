using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum3D.memory
{    /// <summary>
     /// Spectrum Pointer. Used to create a dynamic reference to an item in memory.
     /// </summary>
    class SPtr
    {
        /// <summary>
        /// Parent reference
        /// </summary>
        private SPtr Parent;
        /// <summary>
        /// Offset from the parent reference, or address 0 if the parent is null
        /// </summary>
        private int Offset;

        public static SPtr New(int o)
        {
            return new SPtr(null, o);
        }

        /// <summary>
        /// Defines a pointer that dereferences this pointer's value to reach it's address
        /// </summary>
        public SPtr Deref()
        {
            return new SPtr(this, 0);
        }
        public SPtr Deref(int o)
        {
            return new SPtr(this, o);
        }

        /// <summary>
        /// Defines a pointer who's value will change depending on the input pointer's value
        /// </summary>
        /// <param name="p"></param>
        /// <param name="o"></param>
        private SPtr(SPtr p, int o)
        {
            Parent = p;
            Offset = o;
        }
        /// <summary>
        /// Defines a pointer that does not have a dynamic reference
        /// </summary>
        /// <param name="o">Sets </param>


        /// <summary>
        /// Creates a new SPtr with the same parent reference, but with different offset.
        /// The offset passed in is relative to this SPtr's offset
        /// </summary>
        /// <param name="offset">Relative to this SPtr's offset</param>
        /// <returns></returns>
        public SPtr RelOff(int offset)
        {
            return new SPtr(Parent, Offset + offset);
        }

        public static implicit operator int(SPtr p)
        {
            return p.GetAddr();
        }

        /// <summary>
        /// Returns an address by resolving the pointer's parent references
        /// </summary>
        /// <returns></returns>
        private int GetAddr()
        {
            int o = 0;
            if (Parent != null)
            {
                o = Parent.GetAddr() & 0xFFFFFF;
                o = Zpr.ReadRamInt32(o) & 0xFFFFFF;
            }

            return o + Offset;
        }

        public void Write(params object[] p)
        {
            if (p.Length % 2 != 0)
                return;

            for (int i = 0; i < (p.Length / 2); i++)
            {
                int offset = (int)p[i * 2];
                object item = p[i * 2 + 1];

                if (item is int)
                {
                    Zpr.WriteRam32(this + offset, (int)item);
                }
                else if (item is uint)
                {
                    Zpr.WriteRam32(this + offset, (int)((uint)item));
                }
                else if (item is float)
                {
                    Zpr.WriteRam32(this + offset, (float)item);
                }
                else if (item is short)
                {
                    Zpr.WriteRam16(this + offset, (short)item);
                }
                else if (item is ushort)
                {
                    Zpr.WriteRam16(this + offset, (short)((ushort)item));
                }
                else if (item is byte)
                {
                    Zpr.WriteRam8(this + offset, (byte)item);
                }
            }
        }
        public override string ToString()
        {
            return ((int)this).ToString("X8");
        }

        public string GetChain()
        {
            if (Parent != null)
                return string.Format("{0}->{1}", Parent.GetChain(), ToString());
            else
                return ToString();
        }
        public int ReadInt32(int offset)
        {
            return Zpr.ReadRamInt32(this + offset);
        }
        public uint ReadUInt32(int offset)
        {
            return (uint)Zpr.ReadRamInt32(this + offset);
        }
        public float ReadFloat(int offset)
        {
            return System.BitConverter.ToSingle(Zpr.ReadRam(this + offset, 4), 0);
        }
        public short ReadInt16(int offset)
        {
            return Zpr.ReadRamInt16(this + offset);
        }
        public ushort ReadUInt16(int offset)
        {
            return (ushort)Zpr.ReadRamInt16(this + offset);
        }
        public int ReadByte(int offset)
        {
            return Zpr.ReadRamByte(this + offset);
        }
    }
}
