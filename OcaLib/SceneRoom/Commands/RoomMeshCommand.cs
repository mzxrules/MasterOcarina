using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace mzxrules.OcaLib.SceneRoom.Commands
{
    class RoomMeshCommand : SceneCommand, IDataCommand
    {
        public SegmentAddress SegmentAddress { get; set; }
        public SegmentAddress RoomMeshAddress { get { return SegmentAddress; } set { SegmentAddress = value; } }
        private Header header;

        public override void SetCommand(SceneWord command)
        {
            base.SetCommand(command);
            SegmentAddress = Command.Data2;
        }

        public void Initialize(BinaryReader br)
        {
            br.BaseStream.Position = RoomMeshAddress.Offset;
            header = Header.Get(br);
        }
        public override string Read()
        {
            return header.ToString() ;
        }
        public override string ToString()
        {
            return $"Room Mesh: {Command.Data2:X8}";
        }

        enum Type
        {
            T0,
            T1,
            T2,
        }

        class Header
        {
            public Type Type { get; protected set; }
            protected byte Data { get; set; }
            public SegmentAddress Start { get; protected set; }
            protected List<DisplayEntry> DisplayLists = new List<DisplayEntry>();

            public static Header Get(BinaryReader br)
            {
                Type t = (Type)br.ReadByte();
                br.BaseStream.Position--;
                switch(t)
                {
                    case Type.T0: return new T0Header(br); 
                    case Type.T1: return new T1Header(br); 
                    case Type.T2: return new T0Header(br);
                    default:
                        throw new NotImplementedException();
                }
            }

            protected Header(BinaryReader br)
            {
                /* 0x00 */
                Type = (Type)br.ReadByte();
                Data = br.ReadByte();
                br.BaseStream.Position += 2;

                /* 0x04 */
                Start = br.ReadBigInt32();
            }
            
        }

        class T0Header : Header
        {
            public byte Entries
            {
                get { return Data; }
                private set { Data = value; }
            }
            public SegmentAddress End { get; private set; }

            public T0Header(BinaryReader br) : base(br)
            {
                /* 0x08 */
                End = br.ReadBigInt32();
                br.BaseStream.Position = Start.Offset;
                for (int i = 0; i < Entries; i++)
                {
                    DisplayLists.Add(new DisplayEntry(br));
                }
            }

            public override string ToString()
            {
                string result = $"{Type} - Display Lists {Entries:D2} Offset {Start}";
                foreach(var item in DisplayLists)
                {
                    result += $"{Environment.NewLine}  {item}";
                }
                return result;
            }
        }

        class T1Header : Header
        {
            SegmentAddress bgAddr = 0;
            List<StaticBackground> Backgrounds = new List<StaticBackground>();
            public byte Format
            {
                get { return Data; }
                private set { Data = value; }
            }
            public T1Header(BinaryReader br) : base(br)
            {
                /* 0x08 */
                if (Format == 1)
                {
                    Backgrounds.Add(new StaticBackground(br));
                }
                else if (Format == 2)
                {
                    byte count = br.ReadByte();
                    br.BaseStream.Position += 3;
                    bgAddr = br.ReadBigInt32();
                    for (int i = 0; i < count; i++)
                    {
                        Backgrounds.Add(new StaticBackground(br, true));
                        br.BaseStream.Position += 2;
                    }
                }

                br.BaseStream.Position = Start.Offset;
                DisplayLists.Add(new DisplayEntry(br));
            }

            public override string ToString()
            {
                string result = $"{Type} - Format {Format:X2} Display List Offset {Start}";
                result += $"{Environment.NewLine}  {DisplayLists[0]}";
                if (Format == 2)
                {
                    result += $"{Environment.NewLine} Background Offset {bgAddr}";
                }
                foreach (var item in Backgrounds)
                {
                    result += $"{Environment.NewLine}  {item}";
                }
                return result;
            }
        }

        class StaticBackground
        {
            public byte BackgroundId { get; private set; } = 0;
            public SegmentAddress JFIF { get; private set; }
            public int unknown1;
            public int unknown2;
            
            public ushort Width { get; private set; }
            public ushort Height { get; private set; }
            public byte ImageFmt { get; private set; }
            public byte ImageSize { get; private set; }
            public ushort ImagePal { get; private set; }
            public ushort ImageFlip { get; private set; }

            public StaticBackground (BinaryReader br, bool readIndex = false)
            { 
                if (readIndex)
                {
                    br.ReadUInt16(); //0x0082
                    BackgroundId = br.ReadByte();
                    br.BaseStream.Position++;
                }
                JFIF = br.ReadBigInt32();
                unknown1 = br.ReadInt32();
                unknown2 = br.ReadInt32();

                Width = br.ReadBigUInt16();
                Height = br.ReadBigUInt16();
                ImageFmt = br.ReadByte();
                ImageSize = br.ReadByte();

                ImagePal = br.ReadUInt16();
                ImageFlip = br.ReadUInt16();
            }
            public override string ToString()
            {
                return $"JFIF {BackgroundId} {JFIF}, {Width}x{Height}, {ImageFmt:X2} {ImageSize:X2} {ImagePal:X4} {ImageFlip:X4}";
            }
        }

        class DisplayEntry
        {
            public SegmentAddress DisplayList1 { get; private set; } //Probably OPA
            public SegmentAddress DisplayList2 { get; private set; } //Probably XLU

            public DisplayEntry(BinaryReader br)
            {
                DisplayList1 = br.ReadBigInt32();
                DisplayList2 = br.ReadBigInt32();
            }

            public override string ToString()
            {
                return $"{DisplayList1} {DisplayList2}";
            }
        }
    }
}
