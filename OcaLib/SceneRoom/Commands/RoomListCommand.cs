using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace mzxrules.OcaLib.SceneRoom.Commands
{
    public class RoomListCommand : SceneCommand, IDataCommand
    {
        public SegmentAddress SegmentAddress { get; set; }
        public int RoomListAddress
        {
            get { return SegmentAddress.Offset; }
            set { SegmentAddress = new SegmentAddress(SegmentAddress, value); }
        }
        public int Rooms { get; set; }
        public List<FileAddress> RoomAddresses { get; set; }

        public override void SetCommand(SceneWord command)
        {
            base.SetCommand(command);
            Rooms = Command.Data1;
            SegmentAddress = Command.Data2;

            if (command[4] != (byte)ORom.Bank.scene)
                throw new Exception();
        }

        public void Initialize(BinaryReader br)
        {
            RoomAddresses = new List<FileAddress>();
            br.BaseStream.Position = RoomListAddress;

            for (int i = 0; i < Rooms; i++)
            {
                var fileAddress = new FileAddress(br.ReadBigUInt32(), br.ReadBigUInt32());
                RoomAddresses.Add(fileAddress);
            }
        }

        public override string Read()
        {
            string result;

            result = ToString();

            foreach (FileAddress address in RoomAddresses)
            {
                result += $"{Environment.NewLine}{address.Start:X8} {address.End:X8}";
            }
            return result;
        }

        public override string ToString()
        {
            return $"There are {RoomAddresses.Count} room(s). List starts at {RoomListAddress:X8}";
        }
    }
}
