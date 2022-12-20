using mzxrules.OcaLib;
using mzxrules.OcaLib.Cutscenes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Globalization.NumberStyles;

namespace Experimental.Data
{
    static partial class Get
    {
        public static void CutscenesDumpAll(IExperimentFace face, List<string> file)
        {
            StringBuilder sb = new();
            List<(int rom, int ram, string name)> cutsceneList = new();

            foreach (string line in File.ReadLines("Data/Cutscenes DBGMQ.txt"))
            {
                var vals = line.Split('\t');
                int rom = int.Parse(vals[0], HexNumber);
                int ram = int.Parse(vals[1], HexNumber);
                cutsceneList.Add((rom, ram, vals[2]));
            }

            foreach (var (rom, ram, name) in cutsceneList) {
                sb.AppendLine("".PadLeft(80, '='));
                sb.AppendLine($"| {name.PadRight(76)} |");
                sb.AppendLine($"| ROM: {$"{rom:X8}".PadRight(71)} |");
                sb.AppendLine($"| RAM: {$"{ram:X8}".PadRight(71)} |");
                sb.AppendLine("".PadLeft(80, '='));
                sb.AppendLine();
                var cutscene = GetCutscene(file, ORom.Build.DBGMQ, rom);
                sb.AppendLine(cutscene.PrintByOccurrence());
            }
            face.OutputText(sb.ToString());
        }
        public static void CutscenesDumpActorAction(IExperimentFace face, List<string> file)
        {
            StringBuilder sb = new();
            List<(int rom, int ram, string name)> cutsceneList = new();
            List<Cutscene> cutscenes = new();
            Dictionary<int, List<ActionCommand>> test = new();


            foreach (string line in File.ReadLines("Data/Cutscenes DBGMQ.txt"))
            {
                var vals = line.Split('\t');
                int rom = int.Parse(vals[0], HexNumber);
                int ram = int.Parse(vals[1], HexNumber);
                cutsceneList.Add((rom, ram, vals[2]));
            }

            foreach (var (rom, ram, name) in cutsceneList)
            {
                cutscenes.Add(GetCutscene(file, ORom.Build.DBGMQ, rom));
            }
            
            foreach (var cutscene in cutscenes)
            {
                foreach (var action in cutscene.Commands.OfType<ActionCommand>())
                {
                    int command = action.Command;

                    if (!test.ContainsKey(command))
                    {
                        test.Add(command, new List<ActionCommand>());
                    }
                    test[command].Add(action);
                }
            }

            foreach (var (key, value) in test)
            {
                sb.AppendLine();
                sb.AppendLine($"Command {key:X2}");
                sb.AppendLine("".PadLeft(80, '='));
                foreach (var item in value)
                {
                    sb.AppendLine(item.ReadCommand());
                }
            }

            face.OutputText(sb.ToString());
        }
    }
}
