using System;
using System.Collections.Generic;
using System.IO;
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
        public static string GetExclusions(BinaryReader read, int dmadata)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{dmadata:X8}");

            read.Seek(dmadata);

            var records = GetDmadataRecords(read);
            for(int i = 0; i < records.Count; i++)
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

        public static int Compress(BinaryReader read, byte[] write, CompressTask task, int cur = 0)
        {
            read.Seek(task.Dmadata);
            var dmadata = GetDmadataRecords(read);

            Span<byte> w = write;

            for (int i = 0; i < dmadata.Count; i++)
            {
                if (i % 50 == 0)
                    Console.WriteLine($"File {i}");
                var dmarec = dmadata[i];
                FileAddress vrom = dmarec.VRom;
                FileAddress rom = dmarec.Rom;
                if (rom.Start < 0)
                    continue;

                read.Seek(rom.Start);

                Span<byte> r = read.ReadBytes((int)vrom.Size);

                //if file should be compressed
                if (!task.Exclusions.Contains(i))
                {
                    int size = Yaz.Encode(r.ToArray(), (int)vrom.Size, out byte[] comp);
                    if (size > 0)
                    {
                        Span<byte> c = new Span<byte>(comp, 0, size);
                        c.CopyTo(w.Slice(cur, size));
                        dmarec.Rom = new FileAddress(cur, cur + size);
                        cur += size;
                        continue;
                    }
                }

                r.CopyTo(w.Slice(cur, (int)vrom.Size));
                dmarec.Rom = new FileAddress(cur, 0);
                cur += (int)vrom.Size;
            }

            read.Seek(task.Dmadata);

            Span<int> dmabinary = new int[dmadata.Count * 0x4];
            for (int i = 0; i < dmadata.Count * 4; i += 4)
            {
                DmadataRecord rec = dmadata[i / 4];
                dmabinary[i + 0] = Endian.ConvertInt32((int)rec.VRom.Start);
                dmabinary[i + 1] = Endian.ConvertInt32((int)rec.VRom.End);
                dmabinary[i + 2] = Endian.ConvertInt32((int)rec.Rom.Start);
                dmabinary[i + 3] = Endian.ConvertInt32((int)rec.Rom.End);
            }
            MemoryMarshal.Cast<int, byte>(dmabinary)
                .CopyTo(w.Slice(task.Dmadata, dmadata.Count * 0x10));

            return cur;
        }

        static List<DmadataRecord> GetDmadataRecords(BinaryReader br)
        {
            List<DmadataRecord> result = new List<DmadataRecord>();
            DmadataRecord record;
            do
            {
                record = new DmadataRecord()
                {
                    VRom = new FileAddress(br.ReadBigInt32(), br.ReadBigInt32()),
                    Rom = new FileAddress(br.ReadBigInt32(), br.ReadBigInt32())
                };
                result.Add(record);
            }
            while (record.VRom.End != 0);
            return result;
        }

        static void WriteDmadataRecords(BinaryWriter bw, List<DmadataRecord> record)
        {
            foreach (var item in record)
            {
                bw.WriteBig((int)item.VRom.Start);
                bw.WriteBig((int)item.VRom.End);
                bw.WriteBig((int)item.Rom.Start);
                bw.WriteBig((int)item.Rom.End);
            }
        }

        const int STATIC_SEGMENT = 0x400_0000 - 0x10000;
        public static void CompressDual(BinaryReader br, BinaryWriter bw, CompressTask g0, CompressTask g1)
        {
            g0.Dmadata = STATIC_SEGMENT + 0x40;
            g1.Dmadata = g0.Dmadata + 0x6200;

            byte[] compressed = new byte[0x400_0000];
            Console.WriteLine("Compressing G0");
            int cur = Compress(br, compressed, g0);
            Console.WriteLine($"Compressing G1, cur = {cur:X8}");
            Compress(br, compressed, g1, cur);
            Console.WriteLine($"Compression Complete, cur = {cur:X8}");
            br.Seek(STATIC_SEGMENT);
            Span<byte> header = br.ReadBytes(0x40);
            Span<byte> comp = compressed;
            header.CopyTo(comp.Slice(STATIC_SEGMENT, 0x40));
            bw.Write(compressed);
        }
    }
}
