using mzxrules.Helper;
using mzxrules.OcaLib;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace Atom
{
    public class JFileInfo
    {
        [JsonProperty(Order = 1)]
        public string File;

        [JsonProperty(Order = 2)]
        public string Game;

        [JsonProperty(Order = 3)]
        public string Version;

        [JsonProperty(Order = 4)]
        public JFileAddress Rom;

        [JsonProperty(Order = 5)]
        public JFileAddress Ram;

        [JsonProperty(Order = 6)]
        public List<JSectionInfo> Sections = new List<JSectionInfo>();

        public JFileInfo() { }

        public JFileInfo(string file, RomVersion version, FileAddress rom, FileAddress ram, JSectionInfo section)
        {
            File = file;
            Game = version.Game.ToString();
            Version = version.ToString();
            Rom = rom;
            Ram = ram;
            Sections.Add(section);
        }
    }

    public class JSectionInfo
    {
        [JsonProperty(Order = 1)]
        public string Name;

        [JsonProperty(Order = 2)]
        public int Subsection;

        [JsonProperty(Order = 3)]
        public bool IsCode;

        [JsonProperty(Order = 4)]
        public JFileAddress Ram;


        public JSectionInfo() { }

        public JSectionInfo(string name, int subsection, bool isCode, FileAddress ram)
        {
            Name = name;
            Subsection = subsection;
            IsCode = isCode;
            Ram = ram;
        }
    }

    public class JFileAddress
    {
        [JsonProperty(Order = 1)]
        public string Start;
        [JsonProperty(Order = 2)]
        public string End;

        public JFileAddress() { }

        public JFileAddress(FileAddress f)
        {
            Start = f.Start.ToString("X8");
            End = f.End.ToString("X8");
        }

        public JFileAddress(N64PtrRange range)
        {
            Start = range.Start.ToString();
            End = range.End.ToString();
        }

        public static implicit operator JFileAddress(FileAddress v)
        {
            return new JFileAddress(v);
        }

        public static implicit operator JFileAddress(N64PtrRange v)
        {
            return new JFileAddress(v);
        }

        public FileAddress Convert()
        {
            return new FileAddress(int.Parse(Start, NumberStyles.HexNumber), int.Parse(End, NumberStyles.HexNumber));
        }
    }

}