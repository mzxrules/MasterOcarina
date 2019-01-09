using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mzxrules.OcaLib.SceneRoom.Commands
{
    public class AlternateHeadersCommand : SceneCommand, IDataCommand
    {
        Game Game { get; set; }
        public int DataEndOffset { get; set; }
        public List<SceneHeader> Headers = new List<SceneHeader>();
        public List<SegmentAddress> Offsets = new List<SegmentAddress>();
        public SegmentAddress SegmentAddress { get; set; }

        public AlternateHeadersCommand(Game game)
        {
            Game = game;
        }

        public override void SetCommand(SceneWord command)
        {
            base.SetCommand(command);
            SegmentAddress = Command.Data2;
            if (command[4] != (byte)ORom.Bank.scene 
                && command[4] != (byte)ORom.Bank.map)
                throw new Exception();
        }

        public void Initialize(BinaryReader br)
        {
            int headerCount;
            List<SegmentAddress> headerOffsets = new List<SegmentAddress>();

            //Get the number of headers
            headerCount = (DataEndOffset - SegmentAddress.Offset) / 4;

            //If there are an impossibly high number of headers, only parse the maximum of 20
            if (headerCount >= 0x14)
                headerCount = 0x14;

            //Read the header offset list
            br.BaseStream.Position = SegmentAddress.Offset;
            for (int i = 0; i < headerCount; i++)
            {
                headerOffsets.Add(br.ReadBigUInt32());
            }
            
            //For every header
            foreach (var segoff in headerOffsets)//int i = 0; i < headerCount; i++)
            {
                //Capture the Bank Number and offset to the headers
                var segment = segoff.Segment;
                var off = segoff.Offset;

                if (segment != 02 && segment != 03 && segoff != 0)
                    break;

                Offsets.Add(segoff);

                if (off != 0)
                {
                    Headers.Add(new SceneHeader(Game));
                }
                else
                {
                    Headers.Add(null);
                }
            }
        }

        public void SpiritHack(int cs0, int cs1)
        {
            for (int i = 0; i < 3; i++)
            {
                Headers.Add(null);
                Offsets.Add(0);
            }
            Headers.Add(new SceneHeader(Game.OcarinaOfTime));
            Offsets.Add(cs0);
            Headers.Add(new SceneHeader(Game.OcarinaOfTime));
            Offsets.Add(cs1);
        }

        public override string Read()
        {
            return ToString();
        }
        
        public override string ToString()
        {
            return $"Alternate headers start at {SegmentAddress.Offset:X8}.";
        }
    }
}
