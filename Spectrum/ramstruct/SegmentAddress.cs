using mzxrules.Helper;
using System.Collections.Generic;
using System.Linq;

namespace Spectrum
{
    class SegmentAddress : IRamItem
    {
        static int SegmentAddressTable { get { return SpectrumVariables.Segment_Table; } }// = 0x120C38;
        public FileAddress Ram
        {
            get { return _RamAddress; }
        }
        private FileAddress _RamAddress;
        private int segment;

        public SegmentAddress(int bank, FileAddress f)
        {
            _RamAddress = f;
            this.segment = bank;
        }
        public override string ToString()
        {
            switch (segment)
            {
                case 02: return "02 SCENE";
                case 03: return "03 ROOM";
                default: return $"{segment:D2} SEGMENT";

            }
        }

        public static List<SegmentAddress> GetSegmentAddressMap(bool showAllSegments)
        {
            List<SegmentAddress> addrs = new List<SegmentAddress>();
            if (SegmentAddressTable == 0)
                return addrs;

            for (int i = 0; i < 0x10; i++)
            {
                if (i != 2)
                    addrs.Add(new SegmentAddress(i, new FileAddress(Zpr.ReadRamInt32(SegmentAddressTable + (i * 0x4)), 0)));
                else
                    addrs.Add(new SegmentAddress(02, new FileAddress(Zpr.ReadRamInt32(SegmentAddressTable + (2 * 0x4)), 0x384980)));
            }

            if (showAllSegments)
                return addrs.Where(x => x.Ram.Start != 0).ToList();
            else
                return addrs.Where(x => new int[] { 0x02, 0x03 }.Contains(x.segment)).ToList();
        }

    }
}
