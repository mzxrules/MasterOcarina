using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mzxrules.OcaLib.Cutscenes
{
    public class Cutscene
    {
        int CommandCap;
        public List<CutsceneCommand> Commands = new List<CutsceneCommand>();
        public bool CommandCapReached = false;
        public bool InvalidCommandReached = false;
        public int Frames { get; set; }
        public int CommandCount { get { return Commands.Count; } }

        Dictionary<int, int> CommandMap = new Dictionary<int, int>()
        {
            { 0x01, 0x01 },
            { 0x02, 0x02 },
            { 0x03, 0x03 },
            { 0x04, 0x04 },
            { 0x05, 0x05 },
            { 0x06, 0x06 },
            { 0x07, 0x07 },
            { 0x08, 0x08 },
            { 0x09, 0x09 },
            { 0x0A, 0x0A },
            { 0x0B, 0x0B },
            { 0x0C, 0x0B },
            { 0x0D, 0x0B },
            { 0x0E, 0x0E },
            { 0x0F, 0x0F },
            { 0x10, 0x0E },
            { 0x11, 0x0F },
            { 0x12, 0x0F },
            { 0x13, 0x13 },
            { 0x14, 0x0B },
            { 0x15, 0x0B },
            { 0x16, 0x0B },
            { 0x17, 0x0F },
            { 0x18, 0x0E },
            { 0x19, 0x19 },
            { 0x1A, 0x0B },
            { 0x1B, 0x0B },
            { 0x1C, 0x0B },
            { 0x1D, 0x1D },
            { 0x1E, 0x1E },
            { 0x1F, 0x1F },
            { 0x20, 0x0B },
            { 0x21, 0x0B },
            { 0x22, 0x0F },
            { 0x23, 0x0E },
            { 0x24, 0x19 },
            { 0x25, 0x1D },
            { 0x26, 0x1E },
            { 0x27, 0x0F },
            { 0x28, 0x0E },
            { 0x29, 0x19 },
            { 0x2A, 0x1D },
            { 0x2B, 0x1E },
            { 0x2C, 0x2C },
            { 0x2D, 0x2D },
            { 0x2E, 0x0F },
            { 0x2F, 0x1E },
            { 0x30, 0x0E },
            { 0x31, 0x31 },
            { 0x32, 0x19 },
            { 0x33, 0x1D },
            { 0x34, 0x1F },
            { 0x35, 0x1D },
            { 0x36, 0x1E },
            { 0x37, 0x2C },
            { 0x38, 0x0B },
            { 0x39, 0x1F },
            { 0x3A, 0x1F },
            { 0x3B, 0x0B },
            { 0x3C, 0x31 },
            { 0x3D, 0x0B },
            { 0x3E, 0x3E },
            { 0x3F, 0x1D },
            { 0x40, 0x0E },
            { 0x41, 0x1D },
            { 0x42, 0x1D },
            { 0x43, 0x19 },
            { 0x44, 0x0E },
            { 0x45, 0x19 },
            { 0x46, 0x0E },
            { 0x47, 0x0B },
            { 0x48, 0x19 },
            { 0x49, 0x0B },
            { 0x4A, 0x19 },
            { 0x4B, 0x1D },
            { 0x4C, 0x0F },
            { 0x4D, 0x2C },
            { 0x4E, 0x0E },
            { 0x4F, 0x1E },
            { 0x50, 0x0E },
            { 0x51, 0x19 },
            { 0x52, 0x1D },
            { 0x53, 0x1E },
            { 0x54, 0x2C },
            { 0x55, 0x0F },
            { 0x56, 0x56 },
            { 0x57, 0x57 },
            { 0x58, 0x1F },
            { 0x59, 0x31 },
            { 0x5A, 0x2C },
            { 0x5B, 0x0B },
            { 0x5C, 0x0B },
            { 0x5D, 0x0F },
            { 0x5E, 0x0E },
            { 0x5F, 0x0B },
            { 0x60, 0x0B },
            { 0x61, 0x0B },
            { 0x62, 0x0B },
            { 0x63, 0x0B },
            { 0x64, 0x0B },
            { 0x65, 0x0B },
            { 0x66, 0x0B },
            { 0x67, 0x0B },
            { 0x68, 0x0B },
            { 0x69, 0x0F },
            { 0x6A, 0x19 },
            { 0x6B, 0x0F },
            { 0x6C, 0x1D },
            { 0x6D, 0x0B },
            { 0x6E, 0x0F },
            { 0x6F, 0x31 },
            { 0x70, 0x0B },
            { 0x71, 0x0B },
            { 0x72, 0x31 },
            { 0x73, 0x1F },
            { 0x74, 0x0E },
            { 0x75, 0x19 },
            { 0x76, 0x0E },
            { 0x77, 0x0F },
            { 0x78, 0x0E },
            { 0x79, 0x19 },
            { 0x7A, 0x0B },
            { 0x7B, 0x0F },
            { 0x7C, 0x7C },
            { 0x7D, 0x0E },
            { 0x7E, 0x19 },
            { 0x7F, 0x1D },
            { 0x80, 0x1E },
            { 0x81, 0x2C },
            { 0x82, 0x1F },
            { 0x83, 0x0E },
            { 0x84, 0x19 },
            { 0x85, 0x1D },
            { 0x86, 0x31 },
            { 0x87, 0x1E },
            { 0x88, 0x2C },
            { 0x89, 0x1F },
            { 0x8A, 0x0F },
            { 0x8B, 0x0F },
            { 0x8C, 0x8C },
            { 0x8D, 0x0E },
            { 0x8E, 0x31 },
            { 0x8F, 0x8F },
            { 0x90, 0x0F }
        };

        /// <summary>
        /// For reporting a parse error
        /// </summary>
        private int error_CommandCount;

        public Cutscene(int commandcap = 250)
        {
            CommandCap = commandcap;
        }

        public Cutscene(Stream s, int commandCap = 250)
        {
            BinaryReader br;
            CutsceneCommand cmd;
            Int32 commandId;
            int commands;

            CommandCap = commandCap;
            br = new BinaryReader(s);

            //Read the header
            commands = br.ReadBigInt32();
            Frames = br.ReadBigInt32();

            error_CommandCount = commands;

            //early termination
            if (Frames < 0 || commands < 0)
                return;

            if (commands > commandCap)
            {
                CommandCapReached = true;
                return;
            }

            for (int i = 0; i < commands + 1; i++)
            {
                cmd = null;
                commandId = br.ReadBigInt32();

                if (commandId == 0x3E8)
                {
                    cmd = new ExitCommand(commandId, br);
                }
                else if (commandId == -1)
                {
                    cmd = new EndCommand(commandId, br);
                }
                else if (commandId > 0 && commandId < 0x91)
                {
                    int functionalId = CommandMap[commandId];
                    switch (functionalId)
                    {
                        case 0x01: cmd = new CameraCommand(commandId, br); break; //Camera Positions
                        case 0x02: goto case 1; //Camera Focus Points
                        case 0x03: goto default; //Special Execution
                        case 0x04: cmd = new ActionCommand(commandId, br); break; //Lighting Settings
                        case 0x05: goto case 1; //Camera Positions (Link)
                        case 0x06: goto case 1; //Camera Focus Points (Link)
                        case 0x07: goto default; //unknown 1
                        case 0x08: goto default; //unknown 2
                        case 0x09: cmd = new Command09(commandId, br); break;
                        case 0x0A: goto default; //Struct + 0x24, Link
                        case 0x0B: goto default; //No Command
                        case 0x0E: goto default; //Struct + 0x2C
                        case 0x0F: goto default; //Struct + 0x28
                        case 0x13: cmd = new TextCommand(commandId, br); break;
                        case 0x19: goto default; //Struct + 0x30
                        case 0x1D: goto default; //Struct + 0x34
                        case 0x1E: goto default; //Struct + 0x38
                        case 0x1F: goto default; //Struct + 0x40
                        case 0x2C: goto default; //Struct + 0x3C
                        case 0x2D: cmd = new ScreenTransitionCommand(commandId, br); break;
                        case 0x31: goto default; //Struct + 0x44
                        case 0x3E: goto default; //Struct + 0x48
                        case 0x56: goto default; //Play Background Music
                        case 0x57: goto default; //Stop Background Music
                        case 0x7C: goto default; //Fade Background Music
                        case 0x8C: cmd = new TimeCommand(commandId, br); break;
                        case 0x8F: goto default; //Struct + 0x4C
                        default: cmd = new ActionCommand(commandId, br); break;
                    }
                }

                if (cmd != null)
                    Commands.Add(cmd);
                else
                {
                    InvalidCommandReached = true;
                    return;
                }

                if (cmd is EndCommand)
                    break;

                if (i > CommandCap)
                {
                    CommandCapReached = true;
                    return;
                }
            }
            TryMergeCameraCommands();
        }

        public bool TryMergeCameraCommands()
        {
            List<CameraCommand> CameraPositions; //0x01, 0x05
            List<CameraCommand> FocusPoints; //0x02,0x06

            for (int i = 0; i < 5; i += 4)
            {
                FocusPoints = Commands.OfType<CameraCommand>().Where(x => x.Command == 2 + i).ToList();
                CameraPositions = Commands.OfType<CameraCommand>().Where(x => x.Command == 1 + i).ToList();

                if (FocusPoints.Count != CameraPositions.Count
                    || !TryMergeCameraCommandLists(FocusPoints, CameraPositions))
                    return false;
            }
            return true;
        }

        private bool TryMergeCameraCommandLists(List<CameraCommand> FocusPoints, List<CameraCommand> CameraPositions)
        {
            for (int i = 0; i < FocusPoints.Count; i++)
            {
                if (FocusPoints[i].Entries.Count == CameraPositions[i].Entries.Count)
                    MergeCameraCommands(FocusPoints[i], CameraPositions[i]);
                else
                    return false;
            }
            return true;
        }

        private void MergeCameraCommands(CameraCommand focusPoints, CameraCommand cameraPositions)
        {
            for (int i = 0; i < focusPoints.Entries.Count; i++)
            {
                cameraPositions.Entries[i].StartFrame = focusPoints.Entries[i].StartFrame;
                cameraPositions.Entries[i].EndFrame = focusPoints.Entries[i].EndFrame;
            }
        }

        public void TrimStart()
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Generates a verbose dump of a cutscene, ordering instructions by occurrence within the file
        /// </summary>
        /// <returns>The output text</returns>
        public string PrintByOccurrence()
        {
            StringBuilder output = new StringBuilder();

            if (Commands.Count == 0)
                return "No Cutscene Found";

            if (InvalidCommandReached)
                return "Invalid Command";

            if (CommandCapReached)
                return $"Exceeded {CommandCap} command limit: {error_CommandCount}";

            output.AppendLine(String.Format("Length: {0:X4} Hit End? {1}, Commands {2:X8}, End Frame {3:X8}",
                Commands.Sum(x => x.Length),
                (Commands.Exists(x => x is EndCommand)) ? "YES" : "NO",
                CommandCount,
                Frames));

            var commandOffset = 8L; //Header data is 0x8 bytes
            foreach (CutsceneCommand command in Commands)
            {
                output.AppendLine($"{commandOffset:X4}: {command.ReadCommand()}");
                commandOffset += command.Length;
            }

            return output.ToString();
        }
        /// <summary>
        /// Generates a verbose dump of a cutscene, ordering intructions by start frame
        /// </summary>
        /// <returns>The output text</returns>
        public string PrintByTimeline()
        {
            short time = -1;
            StringBuilder sb = new StringBuilder();
            CutsceneCommand lastRoot = null;

            foreach (IFrameData f in Commands.OfType<IFrameCollection>()
                .SelectMany(x =>  x.IFrameDataEnum)
                .OrderBy(x => x.StartFrame))
            {
                if (f.StartFrame > time)
                {
                    time = f.StartFrame;
                }

                if (lastRoot == null || f.RootCommand != lastRoot)
                {
                    sb.AppendLine();
                    sb.AppendLine($"{time:X4} {0:X6}: {f.RootCommand}");
                    lastRoot = f.RootCommand;
                }
                if ((f != f.RootCommand))
                    sb.AppendLine($"{time:X4} FFFFFF:   {f}");
            }
            return sb.ToString();
        }

        public void Save(BinaryWriter bw)
        {
            bw.WriteBig(Commands.Count - 1);
            bw.WriteBig(Frames);
            foreach (CutsceneCommand command in Commands)
                command.Save(bw);
        }
    }
}
