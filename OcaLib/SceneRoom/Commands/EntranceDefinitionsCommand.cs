using mzxrules.Helper;
using System;
using System.Collections.Generic;

namespace mzxrules.OcaLib.SceneRoom.Commands
{
    class EntranceDefinitionsCommand : SceneCommand, IDataCommand
    {
        public SegmentAddress SegmentAddress { get; set; }
        public int EntranceDefinitionsAddress
        {
            get { return SegmentAddress.Offset; }
            set { SegmentAddress = new SegmentAddress(SegmentAddress, value); }
        }
        public List<EntranceDef> EntranceDefinitions = new List<EntranceDef>();
        public int Entrances = 0;

        public override void SetCommand(SceneWord command)
        {
            base.SetCommand(command);
            SegmentAddress = Command.Data2;
            if (SegmentAddress.Segment != (byte)ORom.Bank.scene)
                throw new Exception();
        }

        public void Initialize(System.IO.BinaryReader br)
        {
            int maxEntrances;
            byte position;
            byte room;

            br.BaseStream.Position = EntranceDefinitionsAddress;
            maxEntrances = Entrances;
            for (int i = 0; i < maxEntrances; i++)
            {
                position = br.ReadByte();
                room = br.ReadByte();

                //if the second or more entrances are 0000, no worky
                if (i < 1 || position != 0 || position != 0)
                    EntranceDefinitions.Add(new EntranceDef(position, room));
            }
        }

        public override string Read()
        {
            string result = ToString();
            foreach (var item in EntranceDefinitions)
            {
                result += $"{Environment.NewLine}  {item}";
            }
            return result;
        }

        public override string ToString()
        {
            return $"Entrance Index Definitions starts at {EntranceDefinitionsAddress:X8}";
        }
    }
}