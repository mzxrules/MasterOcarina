using mzxrules.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mzxrules.OcaLib
{
    public class MBlock : IEnumerable
    {
        public FileAddress Address;
        public bool IsBounded { get { return StartSet && EndSet; } }
        bool StartSet = false;
        bool EndSet = false;
        public object Asset;
        LinkedListNode<MBlock> Parent = null;
        LinkedListNode<MBlock> Self = null;
        LinkedList<MBlock> Children = new LinkedList<MBlock>();
        public bool ContainsChildren { get { return Children.Count != 0; } }

        public MBlock(FileAddress addr)
        {
            Address = addr;
            StartSet = true;
            EndSet = true;
            Self = new LinkedListNode<MBlock>(this);
        }

        public MBlock(FileAddress addr, object asset) : this(addr)
        {
            Asset = asset;
        }

        private MBlock(object asset, FileAddress addr, bool startSet, bool endSet)
        {
            Asset = asset;
            Address = addr;
            StartSet = startSet;
            EndSet = endSet;
        }

        private MBlock(object asset, uint start, bool startSet, uint end, bool endSet)
        {

        }

        public MBlock InsertAsset(object asset, uint? start, uint? end)
        {
            if (!IsBounded)
                throw new NotSupportedException();

            if (start == null && end == null)
                throw new ArgumentOutOfRangeException();

            if (start != null && end != null)
                return InsertCompleteBlock(asset, (uint)start, (uint)end);
            else if (end == null)
                return InsertStartBlock(asset, (uint)start);

            return InsertEndBlock(asset, (uint)end);

        }

        private MBlock InsertEndBlock(object asset, uint p)
        {
            throw new NotImplementedException();
        }

        private MBlock InsertStartBlock(object asset, uint p)
        {
            throw new NotImplementedException();
        }

        private MBlock InsertCompleteBlock(object asset, uint start, uint end)
        {
            MBlock result;
            MBlock query;
            FileAddress blockAddr = new FileAddress(start, end);

            //create new block
            result = new MBlock(new FileAddress(start, end), asset);

            //check if block falls within the bounds of an existing block
            query = Children.SingleOrDefault(x => x.Address.Start <= start && end <= x.Address.End);

            //If block doesn't fall within the bounds of another block
            if (query == null)
            {
                result.Parent = Self;
                //if no nodes, add to list
                if (Children.Count == 0)
                {
                    result.Self = Children.AddFirst(result);
                    return result;
                }
                else // find a block to link to
                {
                    LinkedListNode<MBlock> curr;
                    LinkedListNode<MBlock> prev = null;
                    curr = Children.First;

                    while (curr != null)
                    {
                        if (start < curr.Value.Address.Start)
                        {
                            result.Self = Children.AddBefore(curr, result);
                            return result;
                        }
                        prev = curr;
                        curr = curr.Next;
                    }
                    result.Self = Children.AddAfter(prev, result);
                    return result;
                }

            }
            else //block is within another block's bounds
            {
                //if same reference
                if (query.Address == blockAddr)
                {
                    return query;
                }
                else if (query.IsBounded)
                {
                    return query.InsertCompleteBlock(asset, start, end);
                }
                else if (query.EndSet == false)
                {
                    query.Address = new FileAddress(query.Address.Start, start);
                    result.Parent = Self;
                    var node = Children.Find(query);
                    result.Self = Children.AddAfter(node, result);
                    return result;
                }
                else
                    throw new NotImplementedException();
            }
        }



        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        class Enumerator : IEnumerator
        {
            MBlock List;
            bool IsBeforeList;
            //bool StepInThisPass;
            LinkedListNode<MBlock> levelNode;
            MBlock current;
            Enumerator sub;

            public Enumerator(MBlock list)
            {
                List = list;
                Reset();
            }

            public object Current
            {
                get { return current; }
            }

            public bool MoveNext()
            {
                if (IsBeforeList)
                {
                    levelNode = List.Children.First;
                    current = (levelNode != null) ? levelNode.Value : null;
                    IsBeforeList = false;
                    //StepInThisPass = true;
                    return (levelNode != null);
                }
                else if (levelNode == null)
                    return false;

                if (sub == null)
                    sub = new Enumerator(levelNode.Value);
                if (sub.MoveNext())
                {
                    current = (MBlock)sub.Current;
                    return true;
                }
                sub = null;

                levelNode = levelNode.Next;
                current = (levelNode != null) ? levelNode.Value : null;
                return (levelNode != null);
            }

            public void Reset()
            {
                IsBeforeList = true;
                //StepInThisPass = false;
            }
        }
    }
}