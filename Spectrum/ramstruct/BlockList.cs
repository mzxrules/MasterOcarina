﻿using mzxrules.Helper;
using System.Collections.Generic;

namespace Spectrum
{
    internal class BlockNode : IRamItem
    {
        public N64PtrRange Ram { get; protected set; }
        public bool IsFree { get; protected set; }

        public uint Size;
        public N64Ptr Prev, Next;

        public static int LENGTH;

        public IRamItem RamItem;

        public BlockNode(N64Ptr addr)
        {
            Ptr ptr = SPtr.New(addr);
            IsFree = (ptr.ReadInt32(0) & 1) == 1;
            Size = ptr.ReadUInt32(4);
            Next = ptr.ReadInt32(8);
            Prev = ptr.ReadInt32(0xC);
            Ram = new N64PtrRange(addr, addr + LENGTH);
        }

        public static List<BlockNode> GetBlockList(N64Ptr address)
        {
            List<BlockNode> list = new List<BlockNode>();
            BlockNode working;

            if (address == 0)
                return list;

            N64Ptr addr = (int)(address | 0x80000000);
            
            do
            {
                working = new BlockNode(addr);
                list.Add(working);
                addr = working.Next;
            }
            while (working.Next != 0);
            return list;
        }

        public static bool operator ==(BlockNode v1, BlockNode v2)
        {
            if (ReferenceEquals(v1, v2))
                return true;

            if (v1 is null || v2 is null)
                return false;

            if (v1.Size == v2.Size
                && v1.IsFree == v2.IsFree)
            {
                if (v1.RamItem == null)
                    return true;
                else if (v1.RamItem == v2.RamItem)
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
            return $"LINK      {((IsFree) ? "F" : " ")} SIZE {Size:X6} NEXT {Next:X8} PREV {Prev:X8}";
        }
    }
}
