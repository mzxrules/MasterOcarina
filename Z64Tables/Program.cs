using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using mzxrules.OcaLib.PathUtil;

namespace Z64Tables
{
    class Program
    {
        static void Main(string[] args)
        {
            PathUtil.Initialize();
            string readline;
            readline = Console.ReadLine();
            while (readline.Trim() != string.Empty)
            {
                try
                {
                    string fileIn = "../../scripts/" + readline;

                    ParseResult result;
                    string script = "";
                    using (StreamReader sr = new StreamReader(fileIn))
                    {
                        script = sr.ReadToEnd();
                    }
                    result = ParseScript(script, out List<ParserTask> tasks, out List<OutputToken> output);

                    if (result == ParseResult.Success)
                    {
                        WriteTasks(tasks, output);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e);
                }

                readline = Console.ReadLine();
            }

        }

        private static void WriteTasks(List<ParserTask> tasks, List<OutputToken> output)
        {
            foreach (var task in tasks)
            {
                if (!PathUtil.TryGetRomLocation(task.Version, out string romLoc))
                {
                    Console.WriteLine($"Can't find {task.Version} rom");
                    continue;
                }

                task.FileIn = romLoc;
                switch (task.Format)
                {
                    case FormatTypesEnum.csv: WriteCsvTask(task, output, WriteCsvRow); break;
                    case FormatTypesEnum.tsv: WriteCsvTask(task, output, WriteTsvRow); break;
                    case FormatTypesEnum.wiki: WriteCsvTask(task, output, WriteWikiRow); break;
                    default:
                        Console.Write($"{task.Format} format not supported."); break;
                }
            }
            
        }
        

        enum ParseResult
        {
            Success,
            Malformed,
            NoIterator,
            NoRom,
        }

        static ParseResult ParseScript(string script,
            out List<ParserTask> tasks, out List<OutputToken> outputFormat)
        {
            tasks = new List<ParserTask>();
            outputFormat = new List<OutputToken>();

            if (!script.Contains('|'))
                return ParseResult.Malformed;

            var topParams = script.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).ToArray();

            if (topParams.Length != 2)
                return ParseResult.Malformed;

            var settings = topParams[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).ToList();
            var output = topParams[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());

            tasks = ParserSettings.GetParserTasks(settings);
            outputFormat = CreateOutputTokens(output);

            foreach (var task in tasks)
                task.FileOut = $"{task.Name} ({task.Version}).{task.Format}.txt";

            return ParseResult.Success;
        }

        delegate void WriteRow(StringBuilder sb, List<string> outlist);

        private static void WriteCsvTask(ParserTask task, List<OutputToken> OutputFormat, WriteRow writeRow)
        {
            StringBuilder sb = new StringBuilder();
            using (BinaryReader br = new BinaryReader(new FileStream(task.FileIn, FileMode.Open, FileAccess.Read)))
            {
                br.BaseStream.Position = task.StartAddress;
                for (long i = 0; i < task.LoopFor; i++)
                {
                    List<string> outlist = new List<string>();
                    foreach (OutputToken item in OutputFormat)
                    {
                        var result = item.Process(br, task);
                        if (item.IsValueReturning)
                            outlist.Add(result);
                    }
                    writeRow(sb, outlist);
                    task.Index += task.Inc;
                }
            }
            using (StreamWriter sw = new StreamWriter(task.FileOut))
            {
                sw.Write(sb.ToString());
                Console.WriteLine($"{task.FileOut} created");
            }
        }

        private static void WriteCsvRow(StringBuilder sb, List<string> outlist)
        {
            sb.AppendLine(string.Join(",", outlist));
        }

        private static void WriteTsvRow(StringBuilder sb, List<string> outlist)
        {
            sb.AppendLine(string.Join("\t", outlist));
        }

        private static void WriteWikiRow(StringBuilder sb, List<string> outlist)
        {
            sb.AppendLine("|-");
            sb.AppendLine("|" + string.Join("||", outlist));
        }



        /// <summary>
        /// Generates a list of tokens for generating output data
        /// </summary>
        /// <param name="outputTextTokens"></param>
        /// <returns></returns>
        private static List<OutputToken> CreateOutputTokens(IEnumerable<string> outputTextTokens)
        {
            List<OutputToken> tokens = new List<OutputToken>();
            foreach (string item in outputTextTokens)
            {
                //Token is of the following form atm:
                //<tokenName>:<format> <parameter>
                string format = string.Empty;
                long parameter = 0;

                var formatParameterSplit = item
                    .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim()).ToArray();

                if (formatParameterSplit.Length == 2)
                    TryParseNum(formatParameterSplit[1], out parameter);

                var tokenNameFormatSplit = formatParameterSplit[0]
                    .Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim()).ToArray();

                if (tokenNameFormatSplit.Length == 2)
                    format = tokenNameFormatSplit[1];

                if (Enum.TryParse(tokenNameFormatSplit[0], out OutputTokensEnum tokenName))
                {
                    tokens.Add(new OutputToken(tokenName, format, parameter));
                }
            }
            return tokens;
        }

        public static bool TryParseNum(string num, out long result)
        {
            var style = System.Globalization.NumberStyles.Integer;
            if (num.StartsWith("0x"))
            {
                style = System.Globalization.NumberStyles.HexNumber;
                num = num.Substring(2);
            }
            return long.TryParse(num, style, null, out result);
        }
    }
}
