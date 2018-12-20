using mzxrules.Helper;
using System;

namespace mzxrules.OcaLib.SceneRoom.Commands
{
    class EnvironmentSettingsCommand : SceneCommand, IDataCommand
    {
        public SegmentAddress SegmentAddress { get; set; }

        public override void SetCommand(SceneWord command)
        {
            base.SetCommand(command);
            SegmentAddress = Command.Data2;
            if (SegmentAddress.Segment != (byte)ORom.Bank.scene)
                throw new Exception();
        }
        public void Initialize(System.IO.BinaryReader br)
        {
        }
        public override string ToString()
        {
            return $"There are {Command[1]} environment setting(s). List starts at {SegmentAddress.Offset:X8}.";
        }
        public override string Read()
        {
            return ToString();
        }
    }
}
