using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace mzxrules.OcaLib.SceneRoom.Commands
{
    class CutsceneCommand(Game game) : SceneCommand, IDataCommand
    {
        struct Record
        {
            public SegmentAddress Cutscene;
            public short EntranceIndex;
            public byte Spawn;
            public byte EventFlag;

            public Record(BinaryReader br)
            {
                Cutscene = br.ReadBigInt32();
                EntranceIndex = br.ReadBigInt16();
                Spawn = br.ReadByte();
                EventFlag = br.ReadByte();
            }

            public override string ToString()
            {
                return $"Cutscene Offset {Cutscene}, {EntranceIndex:X4} {Spawn} {EventFlag:X2}";
            }
        }

        public SegmentAddress SegmentAddress { get; set; }

        public int Cutscenes;

        private readonly List<Record> records = [];

        public override void SetCommand(SceneWord command)
        {
            base.SetCommand(command);
            Cutscenes = Command.Data1;
            SegmentAddress = Command.Data2;
        }

        public override string Read()
        {
            return ToString();
        }
        public override string ToString()
        {
            if (game == Game.MajorasMask)
            {
                string result = $"There are {Cutscenes} cutscenes. List starts at {SegmentAddress}";
                foreach (var item in records)
                {
                    result += $"{Environment.NewLine}{item}";
                }
                return result;
            }
            else
            {
                return $"Cutscene starts at {SegmentAddress}";
            }
        }

        public void Initialize(BinaryReader br)
        {
            if (game == Game.MajorasMask)
            {
                if (SegmentAddress.Segment != (byte)ORom.Bank.scene)
                {
                    throw new Exception();
                }

                br.BaseStream.Position = SegmentAddress.Offset;
                for (int i = 0; i < Cutscenes; i++)
                {
                    records.Add(new Record(br));
                }
            }
        }
    }
}
