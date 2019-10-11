using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using mzxrules.OcaLib;

namespace Atom
{
    public partial class Atom
    {
        //path [gameid] [versionid] [path]  //sets a path to a rom
        //all [gameid] [versionid]          //creates a gcc compatible disassembly of the entire rom
        //df [gameid] [versionid]
        //dovl [gameid] [versionid] [name]  //creates a disassembly of a specific overlay file
        //script [path]                     //converts elf (.o) files to overlay form, and injects into the rom
        //script --new [path]               //creates a dummy script file
        //ovl [vram] [path]                 //creates an overlay file named [path].ovl from an elf (.o) file

        static readonly Type[] VerbTypes = new Type[] {
            typeof(PathOptions),
            typeof(AllOptions),
            typeof(DisassembleFileOptions),
            typeof(DisassembleOverlayOptions),
            typeof(ScriptOptions),
            typeof(OvlOptions),
            typeof(VersionOptions),
        };

        #region Parser Types
        const string GAME_MT_STR = "game";
        const string GAME_HT_STR = "game id";

        const string VER_MT_STR = "ver";
        const string VER_HT_STR = "version id";

        const string ROM_MT_STR = "rom";
        const string ROM_HT_STR = "path to the rom";
        #endregion

        public class GameVersionOptions
        {
            [Value(1, HelpText = GAME_HT_STR, MetaName = GAME_MT_STR)]
            public string GameId { get; set; }

            [Value(2, HelpText = VER_HT_STR, MetaName = VER_MT_STR)]
            public string VersionId { get; set; }
        }

        [Verb("all", HelpText = "creates a disassembly of the entire rom")]
        public class AllOptions : GameVersionOptions
        {
            [Option('r', "readable",
                Default = false,
                HelpText = "Generates more readable, but uncompilable code")]
            public bool ReadableOutput { get; set; }

            [Value(3, HelpText = ROM_HT_STR, MetaName = ROM_MT_STR, Required = false)]
            public string RomPath { get; set; }
        }

        [Verb("df", Hidden = true)]
        public class DisassembleFileOptions : FileLookupOptions { }

        [Verb("dovl")]
        public class DisassembleOverlayOptions : FileLookupOptions { }

        public class FileLookupOptions : GameVersionOptions
        {
            [Option('s', "start", HelpText = "file start", Required = false, Hidden = true)]
            public string FileStart { get; set; }

            [Option('e', "end", HelpText = "file end", Required = false, Hidden = true)]
            public string FileEnd { get; set; }

            [Value(3, HelpText = "the internal name of the file to be decompiled", MetaName = "file")]
            public string File { get; set; }

            [Value(4, HelpText = "optional. " + ROM_HT_STR, MetaName = ROM_MT_STR, Required = false)]
            public string RomPath { get; set; }

            [Usage()]
            public static IEnumerable<Example> Examples => new List<Example>()
            {
                new Example("Auto-lookup file using the /base folder database", new FileLookupOptions
                {
                    GameId = $"oot",
                    VersionId = $"{ORom.Build.N0}",
                    File = "code"
                }),
                //new Example("Specify file's location in rom", new FileLookupOptions
                //{
                //    GameId = $"oot",
                //    VersionId = $"{ORom.Build.N0}",
                //    File = "code",
                //    FileStart = "A87000",
                //    FileEnd = "B8AD30",
                //}),
            };
        }

        [Verb("path", HelpText = "configures a path for a specific rom version")]
        public class PathOptions : GameVersionOptions
        {
            [Value(3, HelpText = ROM_HT_STR, MetaName = ROM_MT_STR)]
            public string RomPath { get; set; }
        }

        [Verb("script", HelpText = "converts elf (.o) files to overlay form, and injects into the rom")]
        public class ScriptOptions
        {
            [Option('n', "new", HelpText = "creates a new script .xml",
                Default = false, Required = false)]
            public bool GenerateNewScript { get; set; }

            [Value(1, HelpText = "path to the script .xml file")]
            public string ScriptPath { get; set; }
        }

        [Verb("ver", HelpText = "lists supported " + GAME_MT_STR + " and " + VER_MT_STR + " values")]
        public class VersionOptions
        {

        }

        [Verb("ovl", HelpText = "creates an overlay file named [path].ovl from an elf (.o) file")]
        public class OvlOptions
        {
            [Value(1, MetaName = "vram", HelpText = "sets the virtual ram address of the overlay file")]
            public string VRam { get; set; }

            [Value(2, MetaName = "path", HelpText = "location of the elf (.o) file")]
            public string Path { get; set; }
        }

    }
}
