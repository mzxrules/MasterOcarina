using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Z64Tables
{
    class Program
    {
        static void Main(string[] args)
        {
            string readline;
            readline = Console.ReadLine();
            while (readline.Trim() != string.Empty)
            {
                try
                {
                    List<ParserTask> tasks;
                    List<OutputToken> output;

                    string fileIn = "../../scripts/" + readline;


                    ParseResult result;
                    using (StreamReader sr = new StreamReader(fileIn))
                    {
                        result = ParseScript(sr, out tasks, out output); // out sbOut, out fileOut);
                    }
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
                string romLoc;
                if (!RomLocation.TryGetRomLocation(task.FileLocationToken, out romLoc))
                {
                    Console.WriteLine(string.Format("Can't find {0} rom", task.FileLocationToken));
                    continue;
                }

                task.FileIn = romLoc;
                switch (task.Format)
                {
                    case FormatTypesEnum.csv: WriteCsvTask(task, output, WriteCsvRow); break;
                    case FormatTypesEnum.tsv: WriteCsvTask(task, output, WriteTsvRow); break;
                    case FormatTypesEnum.wiki: WriteCsvTask(task, output, WriteWikiRow); break;
                    default:
                        Console.Write("{0} Format not supported.", task.Format); break;
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

        static ParseResult ParseScript(StreamReader scriptStream,
            out List<ParserTask> tasks, out List<OutputToken> outputFormat) //out StringBuilder sb, out string fileOut)
        {
            tasks = new List<ParserTask>();
            outputFormat = new List<OutputToken>();
            
            string script;

            string fileOut = scriptStream.ReadLine().Trim();

            {
                StringBuilder scriptBuild = new StringBuilder();

                while (scriptStream.Peek() >= 0)
                {
                    scriptBuild.Append(scriptStream.ReadLine());
                }

                script = scriptBuild.ToString();
            }


            if (!script.Contains('|'))
                return ParseResult.Malformed;

            var topParams = script.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).ToArray();

            if (topParams.Length != 2)
                return ParseResult.Malformed;


            var settings = topParams[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).ToArray();
            var output = topParams[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).ToArray();
           
            tasks = ParserSettings.GetParserTasks(settings);
            outputFormat = CreateOutputTokens(output);

            foreach (var task in tasks)
                task.FileOut = string.Format("{0} ({2}).{1}.txt", fileOut, task.Format, task.FileLocationToken);

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
                Console.Write("{0} created", task.FileOut);
                Console.WriteLine();
                    
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
        private static List<OutputToken> CreateOutputTokens(string[] outputTextTokens)
        {
            List<OutputToken> tokens = new List<OutputToken>();
            foreach (string item in outputTextTokens)
            {
                //Token is of the following form atm:
                //<tokenName>:<format> <parameter>
                OutputTokensEnum tokenName;
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

                if (Enum.TryParse(tokenNameFormatSplit[0], out tokenName))
                {
                    tokens.Add(new OutputToken(tokenName, format, parameter));
                }
            }
            return tokens;
        }

        public static bool TryParseNum(string num, out long result)
        {
            result = 0;
            System.Globalization.NumberStyles style = System.Globalization.NumberStyles.Integer;
            if (num.StartsWith("0x"))
            {
                style = System.Globalization.NumberStyles.HexNumber;
                num = num.Substring(2);
            }
            if (!long.TryParse(num, style, null, out result))
                return false;
            else
                return true;
        }
    }
}
