using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace mzxrules.OcaLib.Cutscenes
{
    public class CameraCommand : CutsceneCommand, IFrameCollection, IFrameData
    {
        public CutsceneCommand RootCommand { get => this; set { throw new InvalidOperationException(); } }
        public Type CommandType => GetCameraCommandType();

        const int LENGTH = 12;
        public short StartFrame { get; set; }
        public short EndFrame { get; set; }

        public IEnumerable<IFrameData> IFrameDataEnum => Entries;

        public ushort Action;
        public ushort zero;

        public List<CameraCommandEntry> Entries = [];

        public CameraCommand(int command, BinaryReader br)
            : base(command, br)
        {
            Load(br);
        }

        public CameraCommand(int command)
        {
            Load(command);
        }

        public CameraCommand(int command, short startFrame, short endFrame)
        {
            Load(command, startFrame, endFrame);
        }

        public void AddEntry(IFrameData d)
        {
            throw new NotImplementedException();
        }

        public void RemoveEntry(IFrameData i)
        {
            CameraCommandEntry entry = (CameraCommandEntry)i;
            Entries.Remove(entry);
        }

        private void Load(int command, short startFrame = 0, short endFrame = 0)
        {
            Command = command;
            Action = 1;
            StartFrame = startFrame;
            EndFrame = endFrame;
            zero = 0;
        }

        private void Load(BinaryReader br)
        {
            CameraCommandEntry entry;
            short startFrame;

            /* 0x04 */ Action = br.ReadBigUInt16();
            /* 0x06 */ StartFrame = br.ReadBigInt16();
            /* 0x08 */ EndFrame = br.ReadBigInt16();
            /* 0x0A */ zero = br.ReadBigUInt16();

            startFrame = StartFrame;

            do
            {
                entry = new CameraCommandEntry(this);
                entry.Load(startFrame, br);
                startFrame += (short)entry.Frames;

                Entries.Add(entry);
            }
            while (!entry.IsLastEntry);
        }

        private Type GetCameraCommandType()
        {
            return Command switch
            {
                01 => Type.Position,
                05 => Type.Position,
                02 => Type.FocusPoint,
                06 => Type.FocusPoint,
                _ => Type.Invalid,
            };
        }

        public override string ToString()
        {
            string commandType;
            string relativity = string.Empty;
            StringBuilder sb = new();

            switch (Command)
            {
                case 01: relativity = "Static"; commandType = "Eye Points"; break;
                case 02: relativity = "Static"; commandType = "LookAt Points"; break;
                case 05: relativity = "Rel Player"; commandType = "Eye Points"; break;
                case 06: relativity = "Rel Player"; commandType = "LookAt Points"; break;
                default: commandType = "Unknown Command"; break;
            }

            sb.AppendLine($"{Command:X4}: Camera {commandType} ({relativity})");
            sb.Append($"  {ParamsToString()}");

            return sb.ToString();
        }

        public string ParamsToString()
        {
            return $"Action: {Action:X4} Start: {StartFrame:X4} End: {EndFrame:X4} {zero:X4}";
        }

        public override string ReadCommand()
        {
            StringBuilder result = new();

            result.AppendLine(ToString());
            foreach (CameraCommandEntry e in Entries)
            {
                result.AppendLine($"    {e}");
            }

            return result.ToString();
        }

        protected override int GetLength()
        {
            return (LENGTH + (CameraCommandEntry.LENGTH * Entries.Count));
        }

        public IEnumerable<IFrameData> GetIFrameDataEnumerator()
        {
            yield return this;
            foreach (IFrameData e in Entries)
                yield return e;
        }

        public override void Save(BinaryWriter bw)
        {
            //Head
            bw.WriteBig(Command);
            bw.WriteBig(Action);
            bw.WriteBig(StartFrame);
            bw.WriteBig(EndFrame);
            bw.WriteBig(zero);

            if (Entries.Count > 0)
            {
                foreach (var item in Entries)
                {
                    item.Terminator = 0;
                }

                var last = Entries[^1];
                last.Terminator = 0xFF;
            }
            foreach (var item in Entries)
            {
                item.Save(bw);
            }
        }

        public enum Type
        {
            FocusPoint,
            Position,
            Invalid
        }
    }
}