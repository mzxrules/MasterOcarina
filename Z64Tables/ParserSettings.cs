using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Z64Tables
{
    class ParserSettings
    {
        public static List<ParserTask> GetParserTasks(string[] settings)
        {
            List<SettingsToken> settingsTokens;
            List<ParserTask> parserTasks;

            settingsTokens = GenerateSettingsTokens(settings);
            parserTasks = GetParserTasks(settingsTokens);
            return parserTasks;
        }

        private static List<SettingsToken> GenerateSettingsTokens(string[] settings)
        {
            List<SettingsToken> tokens = new List<SettingsToken>();

            foreach (string setting in settings)
            {
                var spl = setting.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim()).ToArray();
                SettingsTokensEnum token;
                var settingId = spl[0].ToLower();
                if (Enum.TryParse(settingId, out token))
                {
                    tokens.Add(new SettingsToken(token, (spl.Length == 1) ? "" : spl[1]));
                }
                else if (settingId == "i")
                {
                    tokens.Add(new SettingsToken(SettingsTokensEnum.iterator, (spl.Length == 1) ? "" : spl[1]));
                }
                else
                    throw new InvalidOperationException(string.Format("Invalid setting option: {0}", spl[0]));
            }

            return tokens;
        }

        private static List<ParserTask> GetParserTasks(List<SettingsToken> settings)
        {
            List<ParserTask> tasks = new List<ParserTask>();
            List<string> builds = (List<string>)Get(settings, SettingsTokensEnum.build);
            List<FormatTypesEnum> outputFormats =
                (List<FormatTypesEnum>)Get(settings, SettingsTokensEnum.format);

            int i = -1;
            foreach (var build in builds)
            {
                i++;
                foreach (var format in outputFormats)
                {
                    ParserTask task = new ParserTask();
                    task.FileLocationToken = build;
                    task.Format = format;
                    task.StartAddress = (long)GetSingle(settings, SettingsTokensEnum.start, i);
                    task.LoopFor = (long)GetSingle(settings, SettingsTokensEnum.loop, i);
                    task.Inc = (long)GetSingle(settings, SettingsTokensEnum.inc, i);
                    task.Index = (long)GetSingle(settings, SettingsTokensEnum.iterator, i);
                    tasks.Add(task);
                }
            }
            return tasks;
        }

        private static object GetSingle(List<SettingsToken> settings, SettingsTokensEnum type, int i)
        {
            IList list = Get(settings, type);
            object result;
            int maxIndex = list.Count - 1;

            i = (i > maxIndex) ? maxIndex : i;

            result = list[i];
            return result;
        }

        private static IList Get(List<SettingsToken> settings, SettingsTokensEnum t)
        {
            var a = settings.SingleOrDefault(x => x.Type == t);
            if (a == null)
            {
                switch (t)
                {
                    case SettingsTokensEnum.format: return new List<FormatTypesEnum>() { FormatTypesEnum.csv };
                    case SettingsTokensEnum.iterator: return new List<long>() { 0L };
                    case SettingsTokensEnum.inc: return new List<long>() { 1L };
                    default: return null;
                }
            }
            return a.Data;
        }

    }
}
