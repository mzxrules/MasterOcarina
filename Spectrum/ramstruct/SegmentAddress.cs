using mzxrules.Helper;
using System.Collections.Generic;
using System.Linq;

namespace Spectrum
{
    class SegmentAddress : IRamItem
    {
        static int SegmentAddressTable { get { return SpectrumVariables.Segment_Table; } }// = 0x120C38;
        public N64PtrRange Ram { get; }

        private int segment;

        public SegmentAddress(int id, N64PtrRange addr)
        {
            Ram = addr;
            segment = id;
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

            Ptr ptr = SPtr.New(SegmentAddressTable);
            for (int i = 0; i < 0x10; i++)
            {
                addrs.Add(new SegmentAddress(i, new N64PtrRange(ptr.ReadInt32(i * 0x4), 0)));
            }

            if (showAllSegments)
                return addrs.Where(x => x.Ram.Start != 0).ToList();
            else
                return addrs.Where(x => new int[] { 0x03 }.Contains(x.segment)).ToList();
        }

    }
}
