using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using mzxrules.OcaLib;
using mzxrules.Helper;
using System.IO;


namespace Experimental.TextureExplorer
{
    class Script
    {
        static StringBuilder sb;
        
        class ClassHelper
        {
            public ORom Rom { get; private set; }
            public TextureData Data { get; set; }
            public RomFile File { get; set; }
            public ClassHelper(ORom rom)
            {
                Rom = rom;
            }
            public void GetFile(int addr)
            {
                File = Rom.Files.GetFile(addr);
            }
        }

        class Set
        {
            public long Start { get; set; }
            public long End { get; set; }
            public long Size { get { return End - Start; } }
            public Set(long start, long end) { Start = start; End = end; }
        }

        class SearchField
        {
            public List<Set> Field { get; private set; }
            public SearchField(long start, long end)
            {
                Field = new List<Set>();
                Field.Add(new Set(start, end));
            }

            public void CutSearchField(long start, long end)
            {
                Set input = new Set(start, end);
                foreach (var range in Field)
                {
                    if (range.Start == input.Start)
                    {
                        range.Start += input.Size;
                        if (range.Size <= 0)
                            Field.Remove(range);
                        return;
                    }
                    else if (range.End == input.End)
                    {
                        range.End -= input.Size;
                        if (range.Size <= 0)
                            Field.Remove(range);
                        return;
                    }
                    else if (range.Start < input.Start
                        && input.End < range.End)
                    {
                        Field.Add(new Set(input.End, range.End));
                        range.End = input.Start;
                        return;
                    }
                }
            }
        }

        public static void Test(IExperimentFace Face, List<String> Files)
        {
            ClassHelper debug = new ClassHelper(new ORom(Files[0], ORom.Build.DBGMQ));
            ClassHelper ntsc10 = new ClassHelper(new ORom(Files[1], ORom.Build.N0));
            TextureData sourceData;

            sourceData = GetTextureData("TextureExplorer/SourceTextureList.xml");
            AdjustOffsets(sourceData);
            ntsc10.Data = new TextureData();

            debug.Data = TrimSearchData(sourceData, ntsc10.Data);

            Dictionary<string, int> FileAddressPair = GetFileDatabase();
            sb = new StringBuilder();

            DoThings(debug, ntsc10, FileAddressPair);
            WriteTextureData(ntsc10.Data);
            Face.OutputText(sb.ToString());
        }

        private static void AdjustOffsets(TextureData sourceData)
        {
            //For every top-level folder
            foreach (var file in sourceData.FileDirectory)
            {
                AdjustOffsets_R(file, 0);
            }
        }

        private static void AdjustOffsets_R(TextureDirectory file, int addr)
        {
            //addr contains the real start address relative to the current top-level folder
            foreach (var folder in file.Folders)
            {
                AdjustOffsets_R(folder, folder.Address+addr);
            }
            //quick out; nothing needs to be adjusted
            if (addr == 0)
                return;

            foreach (var texture in file.Textures)
            {
                texture.Address += addr;
            }
        }

        private static TextureData TrimSearchData(TextureData sourceData, TextureData currentData)
        {
            return sourceData;
        }

        private static void DoThings(ClassHelper source, ClassHelper current, Dictionary<string, int> FileAddressPair)
        {
            foreach (var TextureDirectory in source.Data.FileDirectory.Where(x=> x.Name == "icon_item_static"))
            {
                int currentAddr;
                int sourceAddr;

                if (!FileAddressPair.TryGetValue(TextureDirectory.Name, out currentAddr))
                {
                    sb.AppendLine($"Folder {TextureDirectory} not found.");
                    continue;
                }
                sb.AppendLine($"Folder: {TextureDirectory.Name}");

                sourceAddr = TextureDirectory.Address;

                source.GetFile(sourceAddr);
                current.GetFile(currentAddr);


                TextureDirectory dir = new TextureDirectory(TextureDirectory.Name,
                    current.File.Record.VRom.Start,
                    current.File.Record.VRom.Size);

                current.Data.FileDirectory.Add(dir);

                //subdirs processed, find textures within the current dir
                SearchField searchField = new SearchField(0, current.File.Record.VRom.Size);

                Dictionary<int, int?> paletteDictionary = new Dictionary<int, int?>();

                ProcessSubdirectory
                    (source, TextureDirectory,
                    current, dir,
                    searchField, paletteDictionary, true);
            }
        }

        private static void ProcessSubdirectory(ClassHelper source, TextureDirectory sourceDir,
            ClassHelper current, TextureDirectory currentDir,
            SearchField searchField, Dictionary<int, int?> paletteDictionary, bool top = false)
        {
            //check if any subdirectories exist
            foreach (var subdir in sourceDir.Folders)
            {
                //create a temp subdirectory
                TextureDirectory currentSubDir = new TextureDirectory(subdir.Name, 0, currentDir.Size);
                //attach
                currentDir.Folders.Add(currentSubDir);

                //process subdirectory
                ProcessSubdirectory(source, subdir, current, currentSubDir, searchField, paletteDictionary);

                //all textures are found/loaded into the subfolder.
                //subdir addr/size set
            }

            foreach (var sourceTexture in sourceDir.Textures.OrderBy(x => x.Address))
            {
                Texture foundTexture = null;
                try
                {
                    if (sourceTexture.Format == "jpeg")
                        foundTexture = FindJpeg(source, sourceTexture, current, searchField);
                    else
                        foundTexture = FindTexture(source, sourceTexture, current, searchField);
                }
                catch { }


                if (foundTexture == null)
                    sb.AppendLine($"Texture: {sourceTexture} not found.");
                else
                {
                    if ((sourceTexture.Format == "ci8" || sourceTexture.Format == "ci4")
                        && ShouldFindPalette(paletteDictionary, sourceTexture, ref foundTexture))
                    {
                        var foundPalTexture = FindPalette(source, sourceDir, current, searchField, sourceTexture);
                        if (foundPalTexture != null)
                        {
                            paletteDictionary.Add((int)sourceTexture.Palette, foundPalTexture.Address);
                            foundTexture.Palette = foundPalTexture.Address;
                            currentDir.Textures.Add(foundPalTexture);
                        }
                    }
                    currentDir.Textures.Add(foundTexture);
                }
            }

            //needs to re-adjust texture offsets as well. oops.
            ////if not the top node, update current folder address/size
            //if (!top && currentDir.Textures.Count != 0)
            //{
            //    var min = currentDir.Textures.Min(x => x.Address);
            //    var max = currentDir.Textures
            //        .Aggregate((maxVal, x) => x.Address > maxVal.Address ? x : maxVal);

            //    currentDir.Address = min;
            //    currentDir.Size = max.Address + max.Size - min;
            //}
        }

        private static bool ShouldFindPalette(Dictionary<int, int?> paletteDictionary, Texture sourceTexture, ref Texture foundTexture)
        {
            int? palOffset;

            if (sourceTexture.Palette == null)
                return false;

            //if the paletteDictionary contains a key, then a palette search was performed already
            else if (paletteDictionary.TryGetValue((int)sourceTexture.Palette, out palOffset))
            {
                foundTexture.Palette = palOffset;
                return false;
            }
            return true;
        }

        private static Texture FindPalette(ClassHelper source, TextureDirectory sourceDir, ClassHelper current,
            SearchField searchField, Texture sourceTexture)
        {
            var sq = (sourceTexture.Format == "ci4") ? 4 : 8;

            //Check for an existing texture in the source db
            Texture find = sourceDir.Textures.SingleOrDefault(x => x.Address == sourceTexture.Palette);

            //if the texture to find is null, create a new one to look for
            if (find == null)
                find = new Texture(sourceTexture.Name + " Palette", 0, "rgb5a1", sq, sq);

            try
            { 
                return FindTexture(source, find, current, searchField); 
            }
            catch { }
            return null;
        }

        private static Texture FindJpeg(ClassHelper source, Texture texture, ClassHelper current, SearchField searchField)
        {
            return null;
        }

        private static Texture FindTexture(ClassHelper source, Texture texture, ClassHelper current, SearchField searchField)
        {
            source.File.Stream.Position = 0;
            BinaryReader dbg = new BinaryReader(source.File);
            BinaryReader ntsc = new BinaryReader(current.File);
            {
                return FindTexture(dbg, texture, ntsc, searchField);
            }
        }

        private static Texture FindTexture(BinaryReader sourceFile, Texture texture, BinaryReader searchFile, SearchField searchField)
        {
            sourceFile.BaseStream.Position = texture.Address;
            using (BinaryReader data = new BinaryReader(new MemoryStream(sourceFile.ReadBytes(texture.Size))))
            {
                uint a;
                uint b;
                bool foundTexture = false;

                foreach (var field in searchField.Field)
                {
                    if (field.Size < texture.Size)
                        continue;

                    searchFile.BaseStream.Position = field.Start;
                    while (searchFile.BaseStream.Position < field.End)
                    {
                        a = searchFile.ReadUInt32();
                        b = data.ReadUInt32();
                        if (a != b)
                        {
                            searchFile.BaseStream.Position -= data.BaseStream.Position;
                            searchFile.BaseStream.Position += 4;
                            data.BaseStream.Position = 0;

                        }
                        if (data.BaseStream.Position >= texture.Size)
                        {
                            foundTexture = true;
                            break;
                        }
                    }
                    if (foundTexture)
                    {
                        int address = (int)(searchFile.BaseStream.Position - texture.Size);
                        searchField.CutSearchField(address, address + texture.Size);
                        return new Texture(texture.Name, address, texture.Format, texture.Width, texture.Height);
                    }
                }
            }
            return null;
        }

        private static Dictionary<string, int> GetFileDatabase()
        {
            OcaBase.OcarinaDataContext db = new OcaBase.OcarinaDataContext();
            Dictionary<string, int> FileAddressPair = new Dictionary<string, int>();
            //var fileList = from a in db.Files
            //               where a.N0_Address != null && a.N0_Address != 0 && a.N0_Address !=1
            //               orderby a.Address_N0.Index ascending
            //               select a;

            //foreach (var f in fileList)
            //{
            //        FileAddressPair.Add(f.Filename, f.Address_N0.StartAddr);
            //}
            return FileAddressPair;
        }

        private static TextureData GetTextureData(string filename)
        {
            TextureData data;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TextureData));
            using (XmlReader xmlReader = XmlReader.Create(filename))
            {
                data = (TextureData)xmlSerializer.Deserialize(xmlReader);
            }
            return data;
        }
        private static void WriteTextureData(TextureData data)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TextureData));
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t"
            };

            using (XmlWriter xmlWriter = XmlWriter.Create("NSTC10 Textures.xml", settings))
            {
                xmlSerializer.Serialize(xmlWriter, data);
            }
        }
    }
}
