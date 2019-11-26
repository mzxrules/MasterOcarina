using System;

using Address = mzxrules.Helper.N64Ptr;

namespace mzxrules.Helper
{
    public class MemoryModel
    {
        public Func<Address, long> ReadInt64;
        public Func<Address, int> ReadInt32;
        public Func<Address, float> ReadFloat;
        public Func<Address, short> ReadInt16;
        public Func<Address, byte> ReadByte;
        public Func<Address, int, byte[]> ReadBytes;

        public Action<Address, int> WriteInt32;
        public Action<Address, float> WriteFloat32;
        public Action<Address, short> WriteInt16;
        public Action<Address, byte> WriteByte;
    }

    public class Ptr
    {
        /// <summary>
        /// Parent reference
        /// </summary>
        protected Ptr Parent;
        /// <summary>
        /// Offset from the parent reference, or address 0 if the parent is null
        /// </summary>
        protected int Offset;

        protected MemoryModel Mem;

        protected Ptr()
        {

        }

        /// <summary>
        /// Creates a new dynamic pointer object, pointed at the given address
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static Ptr New(MemoryModel model, int addr)
        {
            return new Ptr(model, null, addr);
        }

        /// <summary>
        /// Defines a pointer that dereferences this pointer's value to reach it's address
        /// </summary>
        public Ptr Deref()
        {
            return new Ptr(Mem, this, 0);
        }
        public Ptr Deref(int offset)
        {
            return new Ptr(Mem, RelOff(offset), 0);
        }

        /// <summary>
        /// Defines a pointer who's value will change depending on the input pointer's value
        /// </summary>
        /// <param name="p"></param>
        /// <param name="o"></param>
        private Ptr(MemoryModel model, Ptr p, int o)
        {
            Parent = p;
            Offset = o;
            Mem = model;
        }


        /// <summary>
        /// Creates a new SPtr with the same parent reference, but with different offset.
        /// The offset passed in is relative to this SPtr's offset
        /// </summary>
        /// <param name="offset">Relative to this SPtr's offset</param>
        /// <returns></returns>
        public Ptr RelOff(int offset)
        {
            return new Ptr(Mem, Parent, Offset + offset);
        }

        public static implicit operator int(Ptr p)
        {
            return p.GetAddr();
        }

        public static implicit operator N64Ptr(Ptr p)
        {
            return new N64Ptr(p.GetAddr());
        }

        /// <summary>
        /// Returns base address for this pointer (i.e. what the parent points to)
        /// </summary>
        /// <returns></returns>
        private int GetBase()
        {
            int o = 0;
            if (Parent != null)
            {
                o = Parent.GetAddr();
                o = Mem.ReadInt32(o);
            }
            return o;
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
                o = Parent.GetAddr();
                o = Mem.ReadInt32(o);
            }

            return o + Offset;
        }

        /// <summary>
        /// Writes a series of primitives to the SPtr address
        /// </summary>
        /// <param name="p">offset1, value1, offset2, value2...</param>
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
                    Mem.WriteInt32(this + offset, (int)item);
                }
                else if (item is uint)
                {
                    Mem.WriteInt32(this + offset, (int)(uint)item);
                }
                else if (item is float)
                {
                    Mem.WriteFloat32(this + offset, (float)item);
                }
                else if (item is short)
                {
                    Mem.WriteInt16(this + offset, (short)item);
                }
                else if (item is ushort)
                {
                    Mem.WriteInt16(this + offset, (short)(ushort)item);
                }
                else if (item is byte)
                {
                    Mem.WriteByte(this + offset, (byte)item);
                }
                else
                {
                    Console.WriteLine($"Invalid SPtr Write Type: {item.GetType()}");
                }
            }
        }

        public override string ToString()
        {
            return $"{(int)this:X8}";
        }

        public string GetChain()
        {
            if (Parent != null)
                if (Offset == 0)
                { return $"{Parent.GetChain()}->{this}"; }
                else
                { return $"{Parent.GetChain()}->{GetBase():X8}+{Offset:X8}"; }
            else
                return ToString();
        }

        public long ReadInt64(int offset)
        {
            return Mem.ReadInt64(this + offset);
        }
        public ulong ReadUInt64(int offset)
        {
            return (ulong)Mem.ReadInt64(this + offset);
        }
        public int ReadInt32(int offset)
        {
            return Mem.ReadInt32(this + offset);
        }
        public uint ReadUInt32(int offset)
        {
            return (uint)Mem.ReadInt32(this + offset);
        }
        public float ReadFloat(int offset)
        {
            return Mem.ReadFloat(this + offset);
        }
        public short ReadInt16(int offset)
        {
            return Mem.ReadInt16(this + offset);
        }
        public ushort ReadUInt16(int offset)
        {
            return (ushort)Mem.ReadInt16(this + offset);
        }
        public byte ReadByte(int offset)
        {
            return Mem.ReadByte(this + offset);
        }
        public byte[] ReadBytes(int offset, int count)
        {
            return Mem.ReadBytes(this + offset, count);
        }
    }
}
