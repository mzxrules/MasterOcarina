using mzxrules.OcaLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Z64Tables
{
    class ParserSettings
    {
        public static List<ParserTask> GetParserTasks(IEnumerable<string> settings)
        {
            List<SettingsToken> settingsTokens = GenerateSettingsTokens(settings);
            List<ParserTask> parserTasks = GetParserTasks(settingsTokens);
            return parserTasks;
        }

        private static List<SettingsToken> GenerateSettingsTokens(IEnumerable<string> settings)
        {
            List<SettingsToken> tokens = new List<SettingsToken>();

            foreach (string setting in settings)
            {
                var spl = setting.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim()).ToArray();
                var settingId = spl[0].ToLower();
                if (!Enum.TryParse(settingId, out SettingsTokens token))
                {
                    if (settingId == "i")
                        token = SettingsTokens.iterator;
                    else
                        throw new InvalidOperationException($"Invalid setting option: {spl[0]}");
                }

                tokens.Add(new SettingsToken(token, (spl.Length == 1) ? "" : spl[1]));
            }

            return tokens;
        }

        private static List<ParserTask> GetParserTasks(List<SettingsToken> settings)
        {
            List<ParserTask> tasks = new List<ParserTask>();
            List<string> builds = (List<string>)Get(settings, SettingsTokens.build);
            List<FormatTypesEnum> outputFormats =
                (List<FormatTypesEnum>)Get(settings, SettingsTokens.format);

            for (int i = 0; i < builds.Count; i++) 
            {
                var build = builds[i];
                foreach (var format in outputFormats)
                {
                    var game = (string)GetSingle(settings, SettingsTokens.game, i);
                    ParserTask task = new ParserTask
                    {
                        Name = (string)GetSingle(settings, SettingsTokens.name, i),
                        Version = new RomVersion(game, build),
                        Format = format,
                        StartAddress = (long)GetSingle(settings, SettingsTokens.start, i),
                        LoopFor = (long)GetSingle(settings, SettingsTokens.loop, i),
                        Inc = (long)GetSingle(settings, SettingsTokens.inc, i),
                        Index = (long)GetSingle(settings, SettingsTokens.iterator, i)
                    };
                    tasks.Add(task);
                }
            }
            return tasks;
        }

        private static object GetSingle(List<SettingsToken> settings, SettingsTokens type, int i)
        {
            IList list = Get(settings, type);
            object result;
            int maxIndex = list.Count - 1;

            i = (i > maxIndex) ? maxIndex : i;

            result = list[i];
            return result;
        }

        private static IList Get(List<SettingsToken> settings, SettingsTokens t)
        {
            var a = settings.SingleOrDefault(x => x.Type == t);
            if (a == null)
            {
                switch (t)
                {
                    case SettingsTokens.format: return new List<FormatTypesEnum>() { FormatTypesEnum.tsv };
                    case SettingsTokens.iterator: return new List<long>() { 0L };
                    case SettingsTokens.inc: return new List<long>() { 1L };
                    default: return null;
                }
            }
            return a.Data;
        }

    }
}
