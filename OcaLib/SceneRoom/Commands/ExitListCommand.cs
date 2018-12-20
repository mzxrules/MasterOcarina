using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mzxrules.Helper;

namespace mzxrules.OcaLib.SceneRoom.Commands
{
    class ExitListCommand : SceneCommand, IDataCommand
    {
        public SegmentAddress SegmentAddress { get; set; }
        public int ExitListAddress
        {
            get { return SegmentAddress.Offset; }
            set { SegmentAddress = new SegmentAddress(SegmentAddress, value); }
        }
        public long EndOffset { get; set; }
        public List<ushort> ExitList = new List<ushort>();
        public override void SetCommand(SceneWord command)
        {
            base.SetCommand(command);
            SegmentAddress = Command.Data2;
            if (SegmentAddress.Segment != (byte)ORom.Bank.scene)
                throw new Exception();
        }

        public void Initialize(System.IO.BinaryReader br)
        {
            long count;
            if (EndOffset != 0)
            {
                count = (EndOffset - ExitListAddress) / 2;
                if (count > 0x20)
                    throw new ArgumentOutOfRangeException();
                br.BaseStream.Position = ExitListAddress;
                for (int i = 0; i < count; i++)
                {
                    Endian.Convert(out ushort index, br.ReadBytes(2));
                    ExitList.Add(index);
                }
            }
        }
        public override string Read()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{ToString()}: ");
            foreach (ushort index in ExitList)
            {
                sb.Append($" {index:X4}");
            }
            return sb.ToString();
        }
        public override string ToString()
        {
            return $"Exit List starts at {ExitListAddress:X8}";
        }

        public string CSV()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"{ExitListAddress:X8},");
            foreach (ushort index in ExitList)
            {
                sb.Append($"{index:X4},");
            }
            return sb.ToString();
        }
    }
}
