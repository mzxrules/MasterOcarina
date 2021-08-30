using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace mzxrules.OcaLib.SymbolMapParser
{

    public static class SymbolMapParser
    {
        static readonly Regex getSymbol = new(@" {16}0x([0-9a-f]{16}) {16}(\w+)$", RegexOptions.Compiled);
        static readonly Regex getSectionLine = new(@" (\.[A-z\s]{13}) 0x([0-9a-f]{16})\s{1,}0x([0-9a-f]{1,}) (.{1,})", RegexOptions.Compiled);
        static readonly Regex getSegmentLine = new(@".{15} 0x([0-9a-f]{16})\s{1,}0x([0-9a-f]{1,})(?: load address 0x)?(.*)", RegexOptions.Compiled);
        //lines starting with .. denote file start
        //
        public static SymbolMap Parse(string path)
        {
            var lines = File.ReadAllLines(path);
            bool GetSym = false;
            bool parseSegmentLine = false;

            List<Segment> segments = new();
            Segment curSegment = new();
            Section curSection = new();

            foreach (var line in lines)
            {
                if (line.Length == 0)
                {
                    continue;
                }    
                if (line.StartsWith("OUTPUT"))
                {
                    break;
                }
                if (line.StartsWith(".."))
                {
                    GetSym = true;
                    curSegment = new Segment();
                    segments.Add(curSegment);

                    curSegment.Name = GetName(line);
                    parseSegmentLine = true;
                    if (!line.Contains(' '))
                        continue;
                }
                if (parseSegmentLine)
                {
                    parseSegmentLine = false;
                    var segLineMatch = getSegmentLine.Match(line);
                    if (!segLineMatch.Success)
                    {
                        Console.WriteLine($"{nameof(getSegmentLine)} pattern failed: {line}");
                        continue;
                    }
                    curSegment.Address = long.Parse(segLineMatch.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                    curSegment.Size = uint.Parse(segLineMatch.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
                    string t = segLineMatch.Groups[3].Value;
                    if (t.Length > 0)
                    {
                        curSegment.LoadAddress = uint.Parse(segLineMatch.Groups[3].Value, System.Globalization.NumberStyles.HexNumber);
                    }
                    continue;
                }
                if (!GetSym)
                    continue;
                if (line.StartsWith(" ."))
                {
                    var sectionLineMatch = getSectionLine.Match(line);
                    if(!sectionLineMatch.Success)
                    {
                        Console.WriteLine($"{nameof(sectionLineMatch)} pattern failed: {line}");
                        continue;
                    }
                    curSection = new Section();
                    curSegment.Sections.Add(curSection);
                    curSection.Name = sectionLineMatch.Groups[1].Value.Trim();
                    curSection.Address = long.Parse(sectionLineMatch.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
                    curSection.Size = uint.Parse(sectionLineMatch.Groups[3].Value, System.Globalization.NumberStyles.HexNumber);
                    curSection.File = sectionLineMatch.Groups[4].Value.Trim();
                    continue;
                }
                else
                {
                    var match = getSymbol.Match(line);
                    if (match.Success)
                    {
                        Symbol symbol = new();
                        curSection.Symbols.Add(symbol);

                        symbol.Address = long.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                        symbol.Name = match.Groups[2].Value;
                    }
                }
            }
            return new SymbolMap()
            {
                Path = path,
                Map = segments
            };  
        }

        private static string GetName(string line)
        {
            string name = line[2..];
            var index = name.IndexOf(' ');
            if (index > 0)
            {
                name = name.Substring(0, index);
            }
            return name;
        }
    }
}
