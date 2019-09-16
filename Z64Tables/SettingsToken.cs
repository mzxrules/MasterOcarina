using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Z64Tables
{
    enum SettingsTokens
    {
        name,
        game,
        format,
        iterator,
        inc,
        build,
        size,
        start,
        end,
        loop,
    }
    enum FormatTypesEnum
    {
        csv,
        tsv,
        wiki,
        json
    }

    class SettingsToken
    {
        public SettingsTokens Type { get; set; }
        public IList Data { get; set; }

        public SettingsToken(SettingsTokens t, string d)
        {
            Type = t;

            switch (Type)
            {
                case SettingsTokens.name: Data = GetName(d); break;
                case SettingsTokens.game: Data = GetName(d); break;
                case SettingsTokens.build: Data = GetBuildData(d); break;
                case SettingsTokens.format: Data = GetOutputTypes(d); break;
                case SettingsTokens.start: goto case SettingsTokens.loop;
                case SettingsTokens.end: goto case SettingsTokens.loop;
                case SettingsTokens.size: goto case SettingsTokens.loop;
                case SettingsTokens.loop: Data = GetNumericalData(d); break;
                case SettingsTokens.iterator: goto case SettingsTokens.loop;
                case SettingsTokens.inc: goto case SettingsTokens.loop;
                default:
                    Data = null; break;
            }
        }

        private List<string> GetName(string s)
        {
            return SplitArguments(s).ToList();
        }

        private static List<FormatTypesEnum> GetOutputTypes(string s)
        {
            List<FormatTypesEnum> resultItems = new List<FormatTypesEnum>();

            foreach (string d in SplitArguments(s))
            {
                if (Enum.TryParse(d.ToLower(), out FormatTypesEnum outputType))
                {
                    resultItems.Add(outputType);
                }
                else
                    throw new InvalidOperationException($"Invalid output code: {s}");
            }
            return resultItems;
        }


        private static List<string> GetBuildData(string s)
        {
            return SplitArguments(s).ToList();
        }

        private static IEnumerable<string> SplitArguments(string s)
        {
            return s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());
        }

        private static List<long> GetNumericalData(string s)
        {
            List<long> resultItems = new List<long>();

            foreach (string d in SplitArguments(s))
            {
                if (Program.TryParseNum(d, out long i))
                    resultItems.Add(i);
                else
                    throw new InvalidOperationException($"Failed Cast: {s}");
            }
            return resultItems;
        }
    }
}
