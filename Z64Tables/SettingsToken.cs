using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Z64Tables
{
    enum SettingsTokensEnum
    {
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
        public SettingsTokensEnum Type { get; set; }
        public IList Data { get; set; }

        public SettingsToken(SettingsTokensEnum t, string d)
        {
            Type = t;

            switch (Type)
            {
                case SettingsTokensEnum.build: Data = GetBuildData(d); break;
                case SettingsTokensEnum.format: Data = GetOutputTypes(d); break;
                case SettingsTokensEnum.start: goto case SettingsTokensEnum.loop;
                case SettingsTokensEnum.end: goto case SettingsTokensEnum.loop;
                case SettingsTokensEnum.size: goto case SettingsTokensEnum.loop;
                case SettingsTokensEnum.loop: Data = GetNumericalData(d); break;
                case SettingsTokensEnum.iterator: goto case SettingsTokensEnum.loop;
                case SettingsTokensEnum.inc: goto case SettingsTokensEnum.loop;
                default:
                    Data = null; break;
            }
        }

        private static List<FormatTypesEnum> GetOutputTypes(string s)
        {
            string[] stringItems;
            List<FormatTypesEnum> resultItems = new List<FormatTypesEnum>();

            stringItems = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().ToLower()).ToArray();

            foreach (string d in stringItems)
            {
                FormatTypesEnum outputType;
                if (Enum.TryParse(d, out outputType))
                {
                    resultItems.Add(outputType);
                }
                else
                    throw new InvalidOperationException(string.Format("Invalid output code: {0}", s));
            }
            return resultItems;
        }


        private static List<string> GetBuildData(string s)
        {
            List<string> stringItems;


            stringItems = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().ToUpper()).ToList();

            
            return stringItems;
        }

        //private static List<RomVersion> GetBuildData(string s)
        //{
        //    string[] stringItems;
        //    List<RomVersion> resultItems = new List<RomVersion>();

        //    stringItems = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        //        .Select(x => x.Trim().ToUpper()).ToArray();

        //    foreach (string d in stringItems)
        //    {
        //        ORom.Build ocarinaBuild;
        //        MRom.Build maskBuild;
        //        if (Enum.TryParse(d, out ocarinaBuild))
        //        {
        //            resultItems.Add(ocarinaBuild);
        //        }
        //        else if (Enum.TryParse(d, out maskBuild))
        //        {
        //            resultItems.Add(maskBuild);
        //        }
        //        else
        //            throw new InvalidOperationException(string.Format("Invalid version code: {0}", s));
        //    }
        //    return resultItems;
        //}

        private static List<long> GetNumericalData(string s)
        {
            string[] stringItems;
            List<long> resultItems = new List<long>();

            stringItems = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).ToArray();

            foreach (string d in stringItems)
            {
                long i;
                if (Program.TryParseNum(d, out i))
                    resultItems.Add(i);
                else
                    throw new InvalidOperationException(string.Format("Failed Cast: {0}", s));
            }
            return resultItems;
        }
    }
}
