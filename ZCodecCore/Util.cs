using System;
using System.Collections.Generic;
using System.IO;
using System.Buffers.Binary;
using mzxrules.Helper;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ZCodecCore
{
    class DmadataRecord
    {
        public FileAddress VRom;
        public FileAddress Rom;
    }
    public class CompressTask
    {
        public int Dmadata;
        public List<int> Exclusions = new List<int>();

        public CompressTask()
        {

        }

        public CompressTask(StreamReader reader)
        {
            string line;

            line = reader.ReadLine().Trim();
            Dmadata = int.Parse(line, System.Globalization.NumberStyles.HexNumber);

            while (reader.Peek() >= 0)
            {
                line = reader.ReadLine().Trim();
                if (line.Length == 0)
                    continue;
                Exclusions.Add(int.Parse(line));
            }
        }
    }
    public class Util
    {
        public static string GetExclusions(ReadOnlySpan<byte> read, int dmadata)
        {
            DmadataRecord rec = GetDmadataRec(read, dmadata);
            var records = GetDmadataRecords(read.Slice(rec.Rom.Start, rec.VRom.Size));

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{dmadata:X8}");

            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];

                if (record.VRom.End != 0
                    && record.Rom.End == 0)
                {
                    sb.AppendLine($"{i}");
                }
            }
            return sb.ToString();
        }

        public static int Compress(ReadOnlySpan<byte> read, Span<byte> w, CompressTask task, int cur = 0)
        {
            DmadataRecord dmadataRec = GetDmadataRec(read, task.Dmadata);
            var dmadata = GetDmadataRecords(read, task.Dmadata);

            for (int i = 0; i < dmadata.Count; i++)
            {
                if (i % 50 == 0)
                    Console.WriteLine($"File {i}");
                var dmarec = dmadata[i];
                FileAddress vrom = dmarec.VRom;
                FileAddress rom = dmarec.Rom;
                if (rom.Start < 0)
                    continue;

                ReadOnlySpan<byte> file = read.Slice(rom.Start, vrom.Size);

                //if file should be compressed
                if (!task.Exclusions.Contains(i))
                {
                    int size = Yaz.Encode(file.ToArray(), vrom.Size, out byte[] comp);
                    if (size > 0)
                    {
                        Span<byte> c = new Span<byte>(comp, 0, size);
                        c.CopyTo(w.Slice(cur, size));
                        dmarec.Rom = new FileAddress(cur, cur + size);
                        cur += size;
                        continue;
                    }
                }

                file.CopyTo(w.Slice(cur, vrom.Size));
                dmarec.Rom = new FileAddress(cur, 0);
                cur += vrom.Size;
            }
            WriteDmadataRecords(w.Slice(dmadataRec.Rom.Start, dmadataRec.VRom.Size), dmadata);

            return cur;
        }

        private static DmadataRecord GetDmadataRec(ReadOnlySpan<byte> read, int dmadata)
        {
            int start = EndianX.ConvertInt32(read, dmadata + 0x20);
            int end = EndianX.ConvertInt32(read, dmadata + 0x24);

            return new DmadataRecord()
            {
                VRom = new FileAddress(start, end),
                Rom = new FileAddress(dmadata, 0)
            };
        }

        static List<DmadataRecord> GetDmadataRecords(ReadOnlySpan<byte> read, int dmadata)
        {
            var dmadataRec = GetDmadataRec(read, dmadata);
            var slice = read.Slice(dmadataRec.Rom.Start, dmadataRec.VRom.Size);
            return GetDmadataRecords(slice);
        }

        static List<DmadataRecord> GetDmadataRecords(ReadOnlySpan<byte> read)
        {
            List<DmadataRecord> result = new List<DmadataRecord>();
            DmadataRecord record;

            for (int i = 0; i < read.Length; i+= 0x10)
            { 
                record = new DmadataRecord()
                {
                    VRom = new FileAddress(EndianX.ConvertInt32(read, i + 0x0), EndianX.ConvertInt32(read, i + 0x4)),
                    Rom = new FileAddress(EndianX.ConvertInt32(read, i + 0x8), EndianX.ConvertInt32(read, i + 0xC))
                };
                if (record.VRom.End == 0)
                {
                    break;
                }
                result.Add(record);
            }
            return result;
        }

        static void WriteDmadataRecords(Span<byte> write, List<DmadataRecord> record)
        {
            int i = 0;
            foreach (var item in record)
            {
                BinaryPrimitives.WriteInt32BigEndian(write.Slice(i + 0x0), item.VRom.Start);
                BinaryPrimitives.WriteInt32BigEndian(write.Slice(i + 0x4), item.VRom.End);
                BinaryPrimitives.WriteInt32BigEndian(write.Slice(i + 0x8), item.Rom.Start);
                BinaryPrimitives.WriteInt32BigEndian(write.Slice(i + 0xC), item.Rom.End);
                i += 0x10;
            }
        }
    }
}
