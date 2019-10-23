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
        //df [gameid] [versionid]           //creates a disassembly of a specific file, defined in the /base database
        //script [path]                     //converts elf (.o) files to overlay form, and injects into the rom
        //script --new [path]               //creates a dummy script file
        //ovl [vram] [path]                 //creates an overlay file named [path].ovl from an elf (.o) file

        static readonly Type[] VerbTypes = new Type[] {
            typeof(PathOptions),
            typeof(AllOptions),
            typeof(DisassembleFileOptions),
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

        public class DisassemblyOptions : GameVersionOptions
        {
            [Option('r', "readable",
                Default = false,
                HelpText = "Generates more readable, but uncompilable code")]
            public bool ReadableOutput { get; set; }

            [Value(3, HelpText = ROM_HT_STR + ". optional", MetaName = ROM_MT_STR, Required = false)]
            public string RomPath { get; set; }
        }

        [Verb("all", HelpText = "creates a disassembly of the entire rom")]
        public class AllOptions : DisassemblyOptions
        {
            [Usage()]
            public static IEnumerable<Example> Examples => new List<Example>
            {
                new Example("Dissemble ROM (gcc compatible)", new AllOptions
                {
                     GameId = $"oot",
                     VersionId = $"{ORom.Build.N0}",
                }),

                new Example("Dissemble ROM (readable output)", new AllOptions
                {
                     GameId = $"oot",
                     VersionId = $"{ORom.Build.N0}",
                     ReadableOutput = true,
                })
            };
        }

        [Verb("df", HelpText = "creates a disassembly of a file. dependent on the /base folder definitions")]
        public class DisassembleFileOptions : FileLookupOptions { }

        public class FileLookupOptions : DisassemblyOptions
        {
            [Option('s', "start", HelpText = "file start", Required = false, SetName = "pick addr")]
            public string FileStart { get; set; }

            //[Option('e', "end", HelpText = "file end", Required = false, Hidden = true)]
            //public string FileEnd { get; set; }

            [Option('f', "file", HelpText = "the internal name of the file to be decompiled", Required = false, SetName = "pick file")]
            public string File { get; set; }

            [Option('a', "af", HelpText = "the actor id of the file to be decompiled", Required = false, SetName = "pick actor")]
            public string ActorIndex { get; set; }

            [Usage()]
            public static IEnumerable<Example> Examples => new List<Example>()
            {
                new Example("Auto-lookup file by filename", new FileLookupOptions
                {
                    GameId = $"oot",
                    VersionId = $"{ORom.Build.N0}",
                    File = "ovl_player_actor"
                }),
                new Example("Auto-lookup file by actor index", new FileLookupOptions
                {
                    GameId = "oot",
                    VersionId = $"{ORom.Build.DBGMQ}",
                    ActorIndex = "66",
                }),
                new Example("Auto-lookup file by file start", new FileLookupOptions
                {
                    GameId = "oot",
                    VersionId = $"{ORom.Build.N0}",
                    FileStart = "A87000",
                }),
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

            [Value(1, HelpText = "path to the script .xml file", MetaName = "path", Required = true)]
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
