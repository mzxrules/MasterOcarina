using System;
using System.Runtime.Serialization;

namespace mzxrules.Helper
{
    [Serializable]
    public struct FileAddress
    {
        //[NonSerialized]
        public int Start { get; private set; } 

        //[NonSerialized]
        public int End { get; private set; }

        //[NonSerialized]
        public int Size { get { return End - Start; } }
        public FileAddress(byte[] data)
            : this()
        {
            int[] list = new int[2];

            Endian.ReverseBytes(ref data, sizeof(int));
            Buffer.BlockCopy(data, 0, list, 0, 8);
            Start = list[0];
            End = list[1];
        }
        public FileAddress(int start, int end)
            : this()
        {
            Start = start;
            End = end;
        }

        public FileAddress((int start, int end) addr)
            : this()
        {
            Start = addr.start;
            End = addr.end;
        }

        public FileAddress(long start, long end)
        {
            Start = (int)start;
            End = (int)end;
        }

        public void Deconstruct(out int start, out int end)
        {
            start = Start;
            end = End;
        }

        public static implicit operator FileAddress((int start, int end) addr)
        {
            return new FileAddress(addr);
        }

        public static bool operator ==(FileAddress v1, FileAddress v2)
        {
            if (ReferenceEquals(v1, v2))
                return true;

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
