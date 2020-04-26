using mzxrules.Helper;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spectrum
{
    class PatternRecord
    {
        public const int SIZE = 0x10;
        const int PATTERN_MAGIC = 0x7074726E;
        public int Length;
        public N64Ptr Address;
        public bool IsPrimaryPattern;

        public uint[] Pattern;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteBig(PATTERN_MAGIC);
            bw.WriteBig(IsPrimaryPattern ? -1 : 0);
            bw.WriteBig(Length);
            bw.WriteBig(Address);

            foreach (var item in Pattern)
            {
                bw.WriteBig(item);
            }
            bw.BaseStream.PadToNextLine();
        }

        public static PatternRecord Deserialize(BinaryReader br)
        {
            br.ReadBigInt32();
            bool primary = br.ReadBigInt32() < 0;
            int length = br.ReadBigInt32();
            int addr = br.ReadBigInt32();

            var data = new uint[length / 4];

            for (int cur = 0; cur < length; cur += 4)
            {
                data[cur / 4] = br.ReadBigUInt32();
            }
            br.BaseStream.Position = Align.To16(br.BaseStream.Position);
            return new PatternRecord()
            {
                IsPrimaryPattern = primary,
                Length = length,
                Address = addr,
                Pattern = data
            };
        }
    }
    class SearchSignature
    {
        //const int ootN0SearchStringOff = 0x0FB4C0; //-0x20 of scene table
        //static uint[] ootN0SearchString =
        //{
        //        0x57094183, 0x57094183, 0x57094183, 0x57094183,
        //        0x5C084183, 0x5C084183, 0x5C084183, 0x5C084183,
        //        0x02499000, 0x024A6A10, 0x01994000, 0x01995B00,
        //        0x01130200, 0x01F12000, 0x01F27140, 0x01998000,
        //        0x01999B00, 0x01140300, 0x0273E000, 0x027537C0,
        //        0x01996000, 0x01997B00, 0x01150400, 0x023CF000,
        //        0x023E4F90, 0x0198A000, 0x0198BB00, 0x02160500,
        //        0x022D8000, 0x022F2970, 0x0198E000, 0x0198FB00,
        //        0x02120600, 0x025B8000, 0x025CDCF0, 0x01990000,
        //        0x01991B00, 0x01170700, 0x02ADE000, 0x02AF7B40,
        //        0x01992000, 0x01993B00, 0x01190800, 0x027A7000,
        //        0x027BF3C0, 0x0198C000, 0x0198DB00, 0x02180900,
        //        0x032C6000, 0x032D2560, 0x019F4000, 0x019F5B00,
        //        0x02180A00, 0x02BEB000, 0x02BFC610, 0x0199C000,
        //        0x0199DB00, 0x00250000, 0x02EE3000, 0x02EF37B0
        //};

        //public uint[] Pattern;
        //public N64Ptr Address;
        //public (N64Ptr, uint)[] Verification;

        private List<PatternRecord> _patterns = new List<PatternRecord>();
        public PatternRecord PrimaryPattern => _patterns.Single(x => x.IsPrimaryPattern == true);
        public List<PatternRecord> SecondaryPatterns => _patterns.Where(x => x.IsPrimaryPattern == false).ToList();



        //public static void Serialize(SearchSignature signature, string path)
        //{
        //    List<PatternRecord> patterns = new List<PatternRecord>();
        //    foreach (var (ptr, value) in signature.Verification)
        //    {
        //        var record = new PatternRecord()
        //        {
        //            length = 4,
        //            addr = ptr,
        //            primary = false,
        //            data = new uint[] { value }
        //        };
        //        patterns.Add(record);
        //    }
        //    patterns.Add(new PatternRecord()
        //    {
        //        length = signature.Pattern.Length * 4,
        //        addr = signature.Address,
        //        primary = true,
        //        data = signature.Pattern
        //    });

        //    MemoryStream ms = new MemoryStream();
        //    using (BinaryWriter bw = new BinaryWriter(ms))
        //    {
        //        foreach (var pattern in patterns)
        //        {
        //            pattern.Serialize(bw);
        //        }

        //    }

        //    File.WriteAllBytes(path, ms.ToArray());
        //}

        public static SearchSignature Deserialize(string path)
        {
            List<PatternRecord> patterns = new List<PatternRecord>();
            using (BinaryReader br = new BinaryReader(File.OpenRead(path)))
            {
                while (true)
                {
                    var pattern = PatternRecord.Deserialize(br);
                    patterns.Add(pattern);
                    if (pattern.IsPrimaryPattern)
                    {
                        break;
                    }
                }
            }
            return new SearchSignature()
            {
                _patterns = patterns
            };
        }
    }
}
