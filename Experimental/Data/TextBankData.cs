using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Experimental.Data
{
    static partial class Get
    {
        delegate void DumpTableDelegate(ORom rom, BinaryReader br, StreamWriter fs);

        class MesgBuildsPair
        {
            /// <summary>
            /// Contains the textbox message data for a single message
            /// </summary>
            public List<int> Data { get; set; }
            /// <summary>
            /// Contains a list of rom builds that byte for byte share the same message
            /// </summary>
            public List<ORom.Build> Builds = new List<ORom.Build>();
            public MesgBuildsPair(List<int> data, ORom.Build build)
            {
                Data = data;
                Builds.Add(build);
            }
        }

        public static string TextDump(ORom rom, ORom.Language language)
        {
            StringBuilder sb = new StringBuilder();

            //sb.AppendLine("{|
            foreach (ushort mesgId in rom.Text.GetMessageEnumerator(language))
            {
                //if (mesgId < 0x4000) continue;
                //if (mesgId >= 0x4000) break;
                //sb.AppendLine(string.Join("\t",
                //    mesgId.ToString("X4"),
                //    "\"" + rom.Text.GetMessage(mesgId, language).Replace("\"", "\"\"") + "\""));

                //Wiki syntax
                sb.AppendLine("|-");
                sb.AppendLine("|" + string.Join("||",
                    mesgId.ToString("X4"),
                    rom.Text.GetMessage(mesgId, language).Replace(Environment.NewLine, "<br>")));
            }

            return sb.ToString();
        }

        static string CsvEscape(this string value)
        {
            if (value.Contains(","))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }

        public static void TextCompareDump()
        {
            Dictionary<Rom.Language, Dictionary<ushort, List<MesgBuildsPair>>> jesuschristmarietheyreminerals;

            jesuschristmarietheyreminerals = new Dictionary<Rom.Language, Dictionary<ushort, List<MesgBuildsPair>>>();

            foreach (Rom.Language l in ORom.GetAllSupportedLanguages())
                jesuschristmarietheyreminerals.Add(l, new Dictionary<ushort, List<MesgBuildsPair>>());

            //for all roms i found
            foreach (ORom.Build build in ORom.GetSupportedBuilds()) 
            {
                ORom rom;
                string romLoc = string.Empty;

                if (!VerboseOcarina.RomLocation.TryGetRomLocation(build, ref romLoc))
                    continue;

                rom = new ORom(romLoc, build);
                
                mzxrules.OcaLib.GameText textModule = new GameText(rom);

                //for all languages supported by the rom
                foreach(Rom.Language lang in ORom.GetSupportedLanguages(build))
                {
                    var LanguageSet = jesuschristmarietheyreminerals[lang];

                    //for all valid message ids
                    foreach (ushort mesgId in textModule.GetMessageEnumerator(lang))
                    {
                        bool matchedItem = false;
                        List<MesgBuildsPair> MessageSet;
                        var data = textModule.GetMessageData(mesgId, lang);

                        //If no data is stored yet for this mesgId, add it
                        if (!LanguageSet.TryGetValue(mesgId, out MessageSet))
                        {
                            MessageSet = new List<MesgBuildsPair>();
                            MessageSet.Add(new MesgBuildsPair(data, build));
                            LanguageSet.Add(mesgId, MessageSet);
                            continue;
                        }
                        //else there's existing data we need to compare to
                        foreach (MesgBuildsPair MessageIteration in MessageSet)
                        {
                            if (data.SequenceEqual(MessageIteration.Data))
                            {
                                MessageIteration.Builds.Add(build);
                                matchedItem = true;
                                break;
                            }
                        }
                        if (!matchedItem)
                        {
                            MessageSet.Add(new MesgBuildsPair(data, build));
                        }
                    }
                }
            }

            OutputTextDump_Wiki(jesuschristmarietheyreminerals);
            //language -> List
            
            //message id, List< data, list<build>>
        }

        private static void OutputTextDump_Wiki(Dictionary<ORom.Language, Dictionary<ushort, List<MesgBuildsPair>>> jesuschrist)
        {
            Dictionary<ORom.Build, mzxrules.OcaLib.GameText> dialogBanks = new Dictionary<ORom.Build, GameText>();

            foreach (ORom.Build b in ORom.GetSupportedBuilds())
            {
                string romLoc = string.Empty;
                ORom rom;

                if (!VerboseOcarina.RomLocation.TryGetRomLocation(b, ref romLoc))
                    continue;

                rom = new ORom(romLoc, b);
                dialogBanks.Add(b, new GameText(rom));
            }

            using (StreamWriter sw = new StreamWriter("MasterTextDump.txt"))
            {
                foreach (var language_MessageSet in jesuschrist)
                {
                    sw.WriteLine(String.Format("== {0} ==", language_MessageSet.Key));
                    foreach (var MessageSet_MessageIteration in language_MessageSet.Value.Where(x => x.Value.Count > 1))
                    {
                        sw.WriteLine("{|class=\"wikitable\"");
                        sw.WriteLine(String.Format("! colspan=\"{0}\" |{1:X4}", MessageSet_MessageIteration.Value.Count,
                            MessageSet_MessageIteration.Key));
                        sw.WriteLine("|-");

                        List<string> buildStrs = new List<string>();
                        List<string> dialogStrs = new List<string>();

                        foreach (var MessageIteration in MessageSet_MessageIteration.Value)
                        {
                            mzxrules.OcaLib.GameText textModule;
                            ORom.Build b;

                            b = MessageIteration.Builds[0];
                            textModule = dialogBanks[b];

                            dialogStrs.Add(
                                textModule.GetMessage(MessageSet_MessageIteration.Key, language_MessageSet.Key));

                            string buildString = string.Empty;

                            foreach (ORom.Build build in MessageIteration.Builds)
                            {
                                buildString += build.ToString() + ":";
                            }
                            buildStrs.Add(buildString);
                        }
                        //write header row
                        sw.Write("! ");
                        for (int i = 0; i < buildStrs.Count; i++ )
                        {
                            sw.Write(buildStrs[i]);
                            if (i < buildStrs.Count - 1)
                                sw.Write("!!");
                        }
                        sw.WriteLine();
                        sw.WriteLine("|-");

                        //write data row

                        sw.Write("| ");
                        for (int i = 0; i < dialogStrs.Count; i++)
                        {
                            sw.Write(dialogStrs[i]);
                            if (i < dialogStrs.Count - 1)
                                sw.Write("||");
                        }
                        sw.WriteLine();
                        sw.WriteLine("|}");

                        sw.WriteLine();
                    }
                }
            }
        }

        private static void Table(ORom rom, int tableStart, string outFilename, DumpTableDelegate DumpTable)
        {
            FileRecord start;

            start = rom.Files.GetFileStart(tableStart);

            if (start == null)
                return;

            using (StreamWriter fs = new StreamWriter($"{rom.Version}-{outFilename}.txt"))
            using (BinaryReader file = new BinaryReader(rom.Files.GetFile(start.VRom)))
            {
                file.BaseStream.Position = start.GetRelativeAddress(tableStart);
                DumpTable(rom, file, fs);
            }

        }


        public static void TextBankData(ORom rom)
        {
            ///Get.Table(rom, 0xB849EC, "TextBankDump", DumpTextBank);

        }
        //private static void DumpTextBank(Rom rom, BinaryReader file, StreamWriter fs)
        //{
        //    GameText.MessageSettings current, next;
        //    BinaryReader textFile;
        //    string dialog;

        //    textFile = new BinaryReader(rom.Files.GetFile(Rom.FileList.nes_message_data_static/*0x0092D000*/));

        //    current = new GameText.MessageSettings(file);
        //    next = new GameText.MessageSettings(file);

        //    while (next.Id != 0xFFFF)
        //    {
        //        textFile.BaseStream.Position = current.BankOffset;
        //        dialog = GameText.EscapeCustomCodes(textFile.ReadBytes((int)(next.BankOffset - current.BankOffset)));

        //        //fs.WriteLine("|-");
        //        fs.WriteLine(String.Format("{0:X4},{1:X2},{2:X8}, {3}",
        //            current.Id,
        //            current.Box,
        //            current.Bank,
        //            dialog));
        //        current = next;
        //        next = new GameText.MessageSettings(file);
        //    }
        //}
    }
}