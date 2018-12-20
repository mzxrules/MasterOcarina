using System;
using System.Runtime.Serialization;

namespace mzxrules.Helper
{
    [Serializable]
    public struct FileAddress
    {
        //[NonSerialized]
        public long Start { get; private set; } 

        //[NonSerialized]
        public long End { get; private set; }

        //[NonSerialized]
        public long Size { get { return End - Start; } }
        public FileAddress(byte[] data)
            : this()
        {
            int[] list = new int[2];

            Endian.ReverseBytes(ref data, sizeof(int));
            Buffer.BlockCopy(data, 0, list, 0, 8);
            Start = list[0];
            End = list[1];
        }
        public FileAddress(long start, long end)
            : this()
        {
            Start = start;
            End = end;
        }

        public FileAddress((long start, long end) addr)
        {
            Start = addr.start;
            End = addr.end;
        }

        public static implicit operator FileAddress((long start, long end) addr)
        {
            return new FileAddress(addr);
        }
        public static implicit operator FileAddress((uint start, uint end) addr)
        {
            return new FileAddress(addr);
        }

        public static bool operator ==(FileAddress v1, FileAddress v2)
        {
            if (object.ReferenceEquals(v1, v2))
                return true;

            if ((object)v1 == null || (object)v2 == null)
                return false;

            return v1.Start == v2.Start && v1.End == v2.End;
        }
        public static bool operator !=(FileAddress v1, FileAddress v2)
        {
            return !(v1 == v2);
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Start:X8}:{End:X8}";
        }
    }
}
