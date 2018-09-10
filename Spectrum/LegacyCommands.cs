using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    partial class Program
    {
        private static void PrintRam(string[] args)
        {
            if (args.Length < 2)
                return;

            if (!TryEvaluate(args.Skip(1).ToArray(), out long address))
                return;

            address &= -4;
            Console.Clear();
            for (int i = 0; i < 0x10; i++)
            {
                int lineAddr = (int)address + i * 0x10;
                Console.WriteLine(string.Format("{0:X8}: {1:X8} {2:X8} {3:X8} {4:X8}",
                    lineAddr,
                    Zpr.ReadRamInt32(lineAddr),
                    Zpr.ReadRamInt32(lineAddr + 0x04),
                    Zpr.ReadRamInt32(lineAddr + 0x08),
                    Zpr.ReadRamInt32(lineAddr + 0x0C)));
            }
        }

        private static void PrintRamF(string[] args)
        {
            if (args.Length < 2)
                return;

            if (!TryEvaluate(args.Skip(1).ToArray(), out long address))
                return;

            address &= -4;
            Console.Clear();
            for (int i = 0; i < 0x10; i++)
            {
                int lineAddr = (int)address + i * 0x10;
                string[] v = new string[4];

                for (int j = 0; j < 4; j++)
                {
                    var dat = Zpr.ReadRamInt32(lineAddr + (j * 4));
                    var val = BitConverter.ToSingle(BitConverter.GetBytes(dat), 0);

                    var exponent = Math.Floor(Math.Log10(Math.Abs(val)));
                    if (exponent >= 0 && exponent < 8)
                        v[j] = val.ToString("F3");
                    else
                        v[j] = val.ToString("E5");
                }

                Console.WriteLine(string.Format("{0:X8}: {1,13} {2,13} {3,13} {4,13}",
                    lineAddr, v[0], v[1], v[2], v[3]));
            }
        }

        private static void GetAddresses(string[] args)
        {
            if (args.Length < 2)
                return;
            if (!TryEvaluate(args.Skip(1).ToArray(), out long address))
                return;

            address &= 0xFFFFFF;

            IEnumerable<IRamItem> items;
            IEnumerable<IVRamItem> vItems;
            if (address >= 0x800000)
            {
                var vAddr = address;
                vItems = from x in GetRamMap(true).OfType<IVRamItem>()
                         where (x.VRam.Start & 0xFFFFFF) <= vAddr
                         && (x.VRam.End & 0xFFFFFF) > vAddr
                         select x;
                items = vItems.Cast<IRamItem>();
                var temp = vItems.ToList();
                if (temp.Count == 0)
                {
                    Console.WriteLine($"{address:X8}: No Conversion");
                }
                else if (temp.Count == 1)
                {
                    address = (address - (temp[0].VRam.Start & 0xFFFFFF)) + ((IRamItem)temp[0]).Ram.Start;
                    Console.WriteLine($"{vAddr:X8} -> {address:X8}:");
                }
                else
                {
                    Console.WriteLine("Conversion error");
                }
            }
            else
            {
                Console.WriteLine($"{address:X8}:");
                items = from x in GetRamMap(true)
                        where (x.Ram.Start & 0xFFFFFF) <= address
                        && (x.Ram.End & 0xFFFFFF) > address
                        select x;
            }

            var debug = items.ToArray();
            foreach (var item in debug)
            {
                Console.WriteLine(item.ToString());
                address -= (int)(item.Ram.Start & 0xFFFFFF);
                Console.Write("Start {0:X8} Off: {1:X6} ", (int)item.Ram.Start, address);

                if (item is IFile iFile)
                {
                    if (iFile.VRom.Size > address)
                        Console.Write("VRom: {0:X8} ", (int)(iFile.VRom.Start + address));
                    else
                        Console.Write(".bss: {0:X8} ", (int)(address - iFile.VRom.Size));
                }

                if (item is IVRamItem)
                    Console.Write("VRam: {0:X8} ", (int)(((IVRamItem)item).VRam.Start + address));

                Console.WriteLine();
            }
        }

        private static void SpawnAtEntranceIndex(string[] args)
        {
            if (args.Length != 2)
                return;

            if (!short.TryParse(args[1], NumberStyles.HexNumber, new CultureInfo("en-US"), out short index))
                return;

            //if (Options.Version == Game.MajorasMask)
            //{
            //    if (args[1].Length <= 2)
            //    {
            //        index &= 0xFE;
            //        index <<= 8;
            //    }
            //}
            SetEntranceIndexSpawn(index);
        }

        private static void SpawnAnywhere(string[] args)
        {
            if (args.Length != 6)
                return;

            if (!int.TryParse(args[1], out int s)
                || !byte.TryParse(args[2], out byte r)
                || !short.TryParse(args[3], out short x)
                || !short.TryParse(args[4], out short y)
                || !short.TryParse(args[5], out short z))
                return;

            if (!TryGetEntranceIndex(s, 0, out EntranceIndex index))
                return;

            Spawn spawn = new Spawn(-1, -1, -1, r, x, y, z, 0);

            if (Options.Version.Game == Game.OcarinaOfTime)
                SetZoneoutSpawn_Ocarina(index.id, ref spawn);
            else if (Options.Version.Game == Game.MajorasMask
                && Options.Version == MRom.Build.J0)
                SetZoneoutSpawn_Mask(index.id, ref spawn);
        }

        private static void SpawnInRoom(string[] args)
        {
            if (args.Length != 5)
                return;

            //if (!byte.TryParse(args[1], out byte r)
            //    || !short.TryParse(args[2], out short x)
            //    || !short.TryParse(args[3], out short y)
            //    || !short.TryParse(args[4], out short z))
            //    return;
            if (!byte.TryParse(args[1], out byte r)
                || !TryGetCoordinateArgs(args, 2, out Vector3<float> a))
                return;

            Spawn spawn = new Spawn(-1, -1, -1, r, (short)a.x, (short)a.y, (short)a.z, 0);

            if (Options.Version.Game == Game.OcarinaOfTime)
                SetZoneoutSpawn_Ocarina(null, ref spawn);
            else if (Options.Version.Game == Game.MajorasMask
                && Options.Version == MRom.Build.J0)
                SetZoneoutSpawn_Mask(null, ref spawn);
        }

        private static void EventFlag(string[] args, bool setFlag)
        {
            if (args.Length != 3)
                return;

            if (Options.Version != Game.OcarinaOfTime)
                return;

            if (!short.TryParse(args[1], NumberStyles.HexNumber, new CultureInfo("en-US"), out short offset)
                || !byte.TryParse(args[2], out byte bit))
                return;

            if (!(offset >= 0xED4 && offset < 0xF34))
                return;

            int val = SaveContext.ReadByte(offset);

            if (setFlag)
            {
                int shift = 1 << bit;
                val |= shift;
            }
            else
            {
                int shift = 1 << bit;
                shift ^= 0xFF;
                val &= shift;
            }
            SaveContext.Write((int)offset, (byte)val);
        }
        
        private static void GetTime(string[] args)
        {
            if (args.Length != 1)
                return;

            ushort time = SaveContext.ReadUInt16(0xC);

            float f_time = ((float)time * 24) / 0x10000;
            int hour = (int)(f_time);
            int min = (int)(f_time * 60) % 60;
            Console.WriteLine($"Time: {hour:D2}:{min:D2}");
        }
    }
}
