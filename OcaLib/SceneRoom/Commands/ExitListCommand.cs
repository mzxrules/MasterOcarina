using System;
using System.Collections.Generic;
using System.Text;
using mzxrules.Helper;

namespace mzxrules.OcaLib.SceneRoom.Commands
{
    class ExitListCommand : SceneCommand, IDataCommand
    {
        public SegmentAddress SegmentAddress { get; set; }
        public long EndOffset { get; set; }
        public List<ushort> ExitList = [];
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
                count = (EndOffset - SegmentAddress.Offset) / 2;
                if (count > 0x20)
                    throw new InvalidOperationException("Exit list is too long");
                br.BaseStream.Position = SegmentAddress.Offset;
                for (int i = 0; i < count; i++)
                {
                    Endian.Convert(out ushort index, br.ReadBytes(2));
                    ExitList.Add(index);
                }
            }
        }
        public override string Read()
        {
            StringBuilder sb = new();
            sb.Append($"{ToString()}: ");
            foreach (ushort index in ExitList)
            {
                sb.Append($" {index:X4}");
            }
            return sb.ToString();
        }
        public override string ToString()
        {
            return $"Exit List starts at {SegmentAddress}";
        }

        public string CSV()
        {
            StringBuilder sb = new();

            sb.Append($"{SegmentAddress},");
            foreach (ushort index in ExitList)
            {
                sb.Append($"{index:X4},");
            }
            return sb.ToString();
        }
    }
}
