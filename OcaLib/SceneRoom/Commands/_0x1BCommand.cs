using System;
using System.Collections.Generic;
using System.IO;
using mzxrules.Helper;
using mzxrules.OcaLib.SceneRoom.Commands;

namespace mzxrules.OcaLib.SceneRoom
{
    internal class _0x1BCommand : SceneCommand, IDataCommand
    {
        int Entries;
        public SegmentAddress SegmentAddress { get; set; }

        List<Record> records = new List<Record>();

        public override void SetCommand(SceneWord command)
        {
            base.SetCommand(command);
            Entries = Command.Data1;
            SegmentAddress = command.Data2;
        }

        public override string Read()
        {
            string result;

            result = ToString();
            foreach (Record r in records)
            {
                result += Environment.NewLine + r.ToString();
            }
            return result;
        }

        public override string ToString()
        {
            return $"There are {Entries} entries. List starts at {SegmentAddress:X8}";
        }

        public void Initialize(BinaryReader br)
        {
            if (SegmentAddress.Segment != (byte)ORom.Bank.scene)
                throw new Exception();
            br.BaseStream.Position = SegmentAddress.Offset;
            for (int i = 0; i < Entries; i++)
            {
                records.Add(new Record(br));
            }
        }


        class Record
        {
            /* 0x00 */ ushort unk_0x00;
            /* 0x02 */ short length;
            /* 0x04 */ short cameraId;
            /* 0x06 */ short cutsceneId;
            /* 0x08 */ short next; //?
            /* 0x0A */ short unk; //byte
            /* 0x0C */ ushort unk_0x0C; 
            /* 0x0E */ byte cameraAfter;
            /* 0x0F */ byte blackBars;

            public Record(BinaryReader br)
            {
                unk_0x00 = br.ReadBigUInt16();
                length = br.ReadBigInt16();
                cameraId = br.ReadBigInt16();
                cutsceneId = br.ReadBigInt16();
                next = br.ReadBigInt16();
                unk = br.ReadBigInt16();
                unk_0x0C = br.ReadBigUInt16();
                cameraAfter = br.ReadByte();
                blackBars = br.ReadByte();
            }

            public override string ToString()
            {
                return $"{unk_0x00:X4}, Length {length}, cameraId {cameraId:X4}, " +
                    $"cutscene {cutsceneId}, next {next:X4}, unk sfx {unk:X4}, unk_0x0C {unk_0x0C:X4}, " +
                    $"cameraAfter {cameraAfter:X2}, black bars {blackBars}";
            }
        }
    }
}