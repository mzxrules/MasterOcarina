using mzxrules.Helper;
using Spectrum3D.memory;
using System;
using System.Collections.Generic;

namespace Spectrum3D
{
    class BlockNode : IRamItem
    {
        public FileAddress Ram { get; protected set; }
        public bool IsFree { get; protected set; }

        protected int[] Data;

        public int Size;
        public int Prev, Next;

        public static int LENGTH = 0x10;

        public BlockNode(int Addr, byte[] data)
        {
            Data = new int[LENGTH / 4];


            for (int i = 0; i < LENGTH / 4; i++)
            {
                Data[i] = BitConverter.ToInt32(data, i * sizeof(uint));
            }

            IsFree = BitConverter.ToInt16(data, 2) == 1;
            Size = Data[1];
            Next = Data[2];
            Prev = Data[3];
            Ram = new FileAddress(Addr, Addr + LENGTH);
        }

        public static List<BlockNode> GetBlockList(int address)
        {
            List<BlockNode> list = new();
            BlockNode working;

            if (address == 0)
                return list;

            int addr = address ;
            
            do
            {
                working = new BlockNode(addr, Zpr.ReadMem(new IntPtr(addr), LENGTH));
                list.Add(working);
                addr = working.Next;
            }
            while (working.Next != 0);
            return list;
        }

        public static bool operator ==(BlockNode v1, BlockNode v2)
        {
            if (ReferenceEquals(v1, v2))
            {
                return true;
            }

            if (v1 is null || v2 is null)
            {
                return false;
            }

            if (v1.Size == v2.Size
                && v1.IsFree == v2.IsFree)
            {
                return true;
            }
            return false;
        }

        public static bool operator !=(BlockNode v1, BlockNode v2)
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
            string result = string.Format("LINK      {3} SIZE {0:X6} NEXT {1:X8} PREV {2:X8}",
                Size,
                Next,
                Prev,
                (IsFree)? "F": " ");
            
            return result;
        }
    }
}
