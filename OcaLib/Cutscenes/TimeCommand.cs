using System.Collections.Generic;
using System.IO;
using mzxrules.Helper;

namespace mzxrules.OcaLib.Cutscenes
{
    internal class TimeCommand : CutsceneCommand
    {
        const int LENGTH = 8;
        private int commandId;
        public List<TimeEntry> Entries = new List<TimeEntry>();

        public TimeCommand(int commandId, BinaryReader br)
        {
            this.commandId = commandId;

            int entries = br.ReadBigInt32();
            for (int i = 0; i< entries; i++)
            {
                Entries.Add(new TimeEntry(this, br));
            }
        }

        protected override int GetLength()
        {
            return Entries.Count * TimeEntry.LENGTH + LENGTH;
        }

        public class TimeEntry : IFrameData
        {
            public static int LENGTH = 0xC;
            public CutsceneCommand RootCommand { get; set; }

            /* 0x00 */
            short unk;
            /* 0x02 */ public short StartFrame { get; set; } //ushort
            /* 0x04 */ public short EndFrame { get; set; } //ushort
            /* 0x06 */ byte hour;
            /* 0x07 */ byte min;
            /* 0x08 */ int unk3;

            public TimeEntry(CutsceneCommand root, BinaryReader br)
            {
                RootCommand = root;

                unk = br.ReadBigInt16();
                StartFrame = br.ReadBigInt16();
                EndFrame = br.ReadBigInt16();
                hour = br.ReadByte();
                min = br.ReadByte();
                unk3 = br.ReadBigInt32();
            }
            public override string ToString()
            {
                return $"{unk:X4}, start frame {StartFrame}, end frame {EndFrame} Set Time: {hour:D2}:{min:D2}, {unk3:X8}";
            }
        }
    }
}