using System;
using System.Collections.Generic;
using System.Linq;
using Spectrum3D.memory;

namespace Spectrum3D
{
    class Program
    {

        static ExpressTest.ExpressionEvaluator Evaluator = new((x) => Zpr.ReadRamInt32((int)x) & 0xFFFFFFFF);
        static void Main(string[] args)
        {
            bool flip = true;
            bool dump = false;
            //091CFD80
            Mount();
            string line = "";
            while (line != "q")
            {
                line = Console.ReadLine();
                switch (line)
                {
                    case "flip": flip = !flip; break;
                    case "dump": dump = !dump; break;
                    case "link": LinkList(0x08080000); break;
                    case "link2": LinkList(0x080A6E40); break;
                    case "link3": LinkList(0x08000064); break;
                    case "link4": LinkList(0x08002AD0); break;
                    case "linkc": LinkListCircular(0x9EA0000); break;
                    case "mount": Mount(); break;
                    default:
                        {
                            if (!TryEvaluate(line.Trim(), out long addr))
                                continue;

                            //if (!int.TryParse(line.Trim(), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out addr))
                            //    continue;
                            if (!dump)
                                ReadRam((int)addr, flip);
                            else
                            {
                                Console.Clear();
                                var arr = Zpr.ReadRam((int)addr, 0x100);
                                for (int i = 0; i < 0x100; i++)
                                    Console.Write($"{arr[i]:X2}");
                            }
                        }
                        break;
                }
            }
        }

        private static void Mount()
        {
            Emulator e = new("citra-qt", "citra", "`citra-qt.exe`");
            Zpr.TryMountEmulator(new List<Emulator>() { e });
        }


        private static bool TryEvaluate(string[] args, out long value)
        {
            return TryEvaluate(string.Join("", args), out value);
        }

        private static bool TryEvaluate(string s, out long value)
        {
            bool result = true;
            value = 0;
            try
            {
                value = Evaluator.Evaluate(s);
            }
            catch
            {
                result = false;
            }
            return result;
        }
        //9EA0000
        class Test
        {
            public short Id;
            public short Unk1;
            public int Unk2;
            public int Address;
            public int Next;
            public int Prev;
            public Test(int addr)
            {
                Address = addr;
                Id = Zpr.ReadRamInt16(addr);
                Unk1 = Zpr.ReadRamInt16(addr + 2);
                Unk2 = Zpr.ReadRamInt32(addr + 4);
                Prev = Zpr.ReadRamInt32(addr + 8);
                Next = Zpr.ReadRamInt32(addr + 0xC);
            }
            public override string ToString()
            {
                return $"{Address:X8}: NEXT {Next:X8} PREV {Prev:X8} {Id:X4} {Unk1:X4} {Unk2:X8}";
            }
        }

        private static void LinkListCircular(int addr)
        {
            Test node = new(addr);
            Console.Clear();
            List<Test> nodes = new() { node };

            Test cur = node;
            while (cur.Next != node.Address && cur.Next != 0)
            {
                cur = new(cur.Next);
                nodes.Add(cur);
            }

            foreach (var item in nodes.OrderBy(x => x.Address))
            {
                Console.WriteLine($"{Zpr.GetEmulatedAddress(item.Address):X16}:{item}");
            }
        }

        private static void LinkList(int addr )
        {
            BlockNode node = new(addr, Zpr.ReadRam(addr, 0x10));
            Console.Clear();
            List<BlockNode> nodes = new() { node };
            var cur = node;
            while (cur.Prev != 0)
            {
                cur = new(cur.Prev, Zpr.ReadRam(cur.Prev, 0x10));
                nodes.Add(cur);
            }

            cur = node;
            while (cur.Next != 0)
            {
                cur = new BlockNode(cur.Next, Zpr.ReadRam(cur.Next, 0x10));
                nodes.Add(cur);
            }

            foreach (var item in nodes.OrderBy(x => x.Ram.Start))
            {
                Console.WriteLine($"{item.Ram.Start:X8}:{item}");
            }

        }

        private static void ReadRam(int addr, bool flip)
        {
            Console.Clear();
            long emuAddr = 0;
            long emuAddrOld = 0;
            if (flip)
            {
                for (int i = 0; i < 0x100; i += 0x10)
                {
                    emuAddr = Zpr.GetEmulatedAddress(addr);
                    if (emuAddrOld != emuAddr)
                    {
                        Console.WriteLine($"{emuAddr:X16}");
                        emuAddrOld = emuAddr;
                    }
                    int addrLocal = addr + i;
                    Console.WriteLine($"{addrLocal:X8} {Zpr.ReadRamInt32(addrLocal + 0):X8} {Zpr.ReadRamInt32(addrLocal + 4):X8} {Zpr.ReadRamInt32(addrLocal + 0x8):X8} {Zpr.ReadRamInt32(addrLocal + 0xC):X8}");
                }
            }
            else
            {
                byte[] arr = Zpr.ReadRam(addr, 0x100);

                emuAddr = Zpr.GetEmulatedAddress(addr);
                Console.WriteLine($"{emuAddr:X16}");

                for (int i = 0; i < 0x100; i += 0x10)
                {
                    Console.Write($"{(addr + i):X8}");
                    for (int j = 0; j < 0x10; j += 4)
                    {
                        Console.Write($" {arr[i + j]:X2}{arr[i + j + 1]:X2}{arr[i + j + 2]:X2}{arr[i + j + 3]:X2}");
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}