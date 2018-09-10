using mzxrules.Helper;

namespace Spectrum
{
    /// <summary>
    /// Spectrum Pointer. Used to create a dynamic reference to an item in memory.
    /// </summary>
    static class SPtr
    {

        static MemoryModel mem = new MemoryModel()
        {
            ReadInt64 = Zpr.ReadRamInt64,
            ReadInt32 = Zpr.ReadRamInt32,
            ReadFloat = Zpr.ReadRamFloat,
            ReadInt16 = Zpr.ReadRamInt16,
            ReadByte = Zpr.ReadRamByte,

            WriteInt32 = Zpr.WriteRam32,
            WriteFloat32 = Zpr.WriteRam32,
            WriteInt16 = Zpr.WriteRam16,
            WriteByte = Zpr.WriteRam8
        };

        public static Ptr New(N64Ptr addr)
        {
            return Ptr.New(mem, addr);
        }

        //public static implicit operator SPtr(Ptr p)
        //{
        //    return p.GetAddr();
        //}


        ///// <summary>
        ///// Parent reference
        ///// </summary>
        //private SPtr Parent;
        ///// <summary>
        ///// Offset from the parent reference, or address 0 if the parent is null
        ///// </summary>
        //private int Offset;

        ///// <summary>
        ///// Creates a new dynamic pointer object, pointed at the given address
        ///// </summary>
        ///// <param name="addr"></param>
        ///// <returns></returns>
        //public static SPtr New(int addr)
        //{
        //    return new SPtr(null, addr);
        //}

        ///// <summary>
        ///// Defines a pointer that dereferences this pointer's value to reach it's address
        ///// </summary>
        //public SPtr Deref()
        //{
        //    return new SPtr(this, 0);
        //}
        //public SPtr Deref(int offset)
        //{
        //    return new SPtr(this.RelOff(offset), 0);
        //}

        ///// <summary>
        ///// Defines a pointer who's value will change depending on the input pointer's value
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="o"></param>
        //private SPtr(SPtr p, int o)
        //{
        //    Parent = p;
        //    Offset = o;
        //}


        ///// <summary>
        ///// Creates a new SPtr with the same parent reference, but with different offset.
        ///// The offset passed in is relative to this SPtr's offset
        ///// </summary>
        ///// <param name="offset">Relative to this SPtr's offset</param>
        ///// <returns></returns>
        //public SPtr RelOff(int offset)
        //{
        //    return new SPtr(Parent, Offset + offset);
        //}

        //public static implicit operator int(SPtr p)
        //{
        //    return p.GetAddr();
        //}

        //public static implicit operator N64Ptr(SPtr p)
        //{
        //    return new N64Ptr(p.GetAddr());
        //}

        ///// <summary>
        ///// Returns base address for this pointer (i.e. what the parent points to)
        ///// </summary>
        ///// <returns></returns>
        //private int GetBase()
        //{
        //    int o = 0;
        //    if (Parent != null)
        //    {
        //        o = Parent.GetAddr(); 
        //        o = Zpr.ReadRamInt32(o);
        //    }
        //    return o;
        //}

        ///// <summary>
        ///// Returns an address by resolving the pointer's parent references
        ///// </summary>
        ///// <returns></returns>
        //private int GetAddr()
        //{
        //    int o = 0;
        //    if (Parent != null)
        //    {
        //        o = Parent.GetAddr(); 
        //        o = Zpr.ReadRamInt32(o); 
        //    }

        //    return o + Offset;
        //}

        ///// <summary>
        ///// Writes a series of primitives to the SPtr address
        ///// </summary>
        ///// <param name="p">offset1, value1, offset2, value2...</param>
        //public void Write(params object[] p)
        //{
        //    if (p.Length % 2 != 0)
        //        return;

        //    for (int i = 0; i < (p.Length / 2); i++)
        //    {
        //        int offset = (int)p[i * 2];
        //        object item = p[i * 2 + 1];

        //        if (item is int)
        //        {
        //            Zpr.WriteRam32(this + offset, (int)item);
        //        }
        //        else if (item is uint)
        //        {
        //            Zpr.WriteRam32(this + offset, (int)((uint)item));
        //        }
        //        else if (item is float)
        //        {
        //            Zpr.WriteRam32(this + offset, (float)item);
        //        }
        //        else if (item is short)
        //        {
        //            Zpr.WriteRam16(this + offset, (short)item);
        //        }
        //        else if (item is ushort)
        //        {
        //            Zpr.WriteRam16(this + offset, (short)((ushort)item));
        //        }
        //        else if (item is byte)
        //        {
        //            Zpr.WriteRam8(this + offset, (byte)item);
        //        }
        //        else
        //        {
        //            System.Console.WriteLine($"Invalid SPtr Write Type: {item.GetType()}");
        //        }
        //    }
        //}
        //public override string ToString()
        //{
        //    return $"{(int)this:X8}";
        //}

        //public string GetChain()
        //{
        //    if (Parent != null)
        //        if (Offset == 0)
        //        { return $"{Parent.GetChain()}->{this}"; }
        //        else
        //        { return $"{Parent.GetChain()}->{GetBase():X8}+{Offset:X8}"; }
        //    else
        //        return ToString();
        //}
        //public long ReadInt64(int offset)
        //{
        //    return Zpr.ReadRamInt64(this + offset);
        //}
        //public ulong ReadUInt64(int offset)
        //{
        //    return (ulong)Zpr.ReadRamInt64(this + offset);
        //}
        //public int ReadInt32(int offset)
        //{
        //    return Zpr.ReadRamInt32(this + offset);
        //}
        //public uint ReadUInt32(int offset)
        //{
        //    return (uint)Zpr.ReadRamInt32(this + offset);
        //}
        //public float ReadFloat(int offset)
        //{
        //    return Zpr.ReadRamFloat(this + offset);
        //}
        //public short ReadInt16(int offset)
        //{
        //    return Zpr.ReadRamInt16(this + offset);
        //}
        //public ushort ReadUInt16(int offset)
        //{
        //    return (ushort)Zpr.ReadRamInt16(this + offset);
        //}
        //public byte ReadByte(int offset)
        //{
        //    return Zpr.ReadRamByte(this + offset);
        //}
    }
}
