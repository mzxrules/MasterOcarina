using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using mzxrules.Helper;
using mzxrules.OcaLib;
using System.Diagnostics;

namespace Gen
{
    public static class RomBuilder
    {
        public delegate Stream GetFileDelegate(string f);

        static string processedFiles = "\rFile {0:D4} of {1:D4} processed";

        public static void CompressRom(Stream rom, RomVersion version, List<int> exclusions, Stream sw)
        {
            DmaData dmadata = new DmaData(rom, version);
            CompressRom_new(rom, dmadata, exclusions, sw);
        }

        private static void CompressRom_new(Stream rom, DmaData dmadata, List<int> exclusions, Stream sw)
        { 
            BinaryReader br = new BinaryReader(rom);
            MemoryStream outRom = new MemoryStream(0x200_0000);
            
            List<FileRecord> newDmaTable = new List<FileRecord>();
            Console.WriteLine();

            int cur = 0;
            
            for (int i = 0; i < dmadata.Table.Count; i++)
            {
                FileRecord record = dmadata.Table[i];
                if (record.VRom.End == 0)
                {
                    var r = new FileRecord(new FileAddress(), new FileAddress());
                    newDmaTable.Add(r);
                    break;
                }
                
                br.BaseStream.Position = record.Rom.Start;

                byte[] data = br.ReadBytes(record.VRom.Size);
                int dstsize;
                FileAddress physical;
                if (!exclusions.Contains(i))
                {
                    dstsize = Yaz.Encode(data, data.Length, outRom);
                    dstsize = Align.To16(dstsize);
                    physical = new FileAddress(cur, cur + dstsize);
                }
                else
                {
                    dstsize = data.Length;
                    dstsize = Align.To16(dstsize);

                    outRom.Write(data, 0, data.Length);
                    physical = new FileAddress(cur, 0);
                    exclusions.Remove(i);
                }
                var newRec = new FileRecord(record.VRom, physical);
                newDmaTable.Add(newRec);

                cur += dstsize;
                outRom.Position = cur;
            }
            
            WriteFileTable(outRom, dmadata.Address.VRom, newDmaTable);
            CRC.Write(outRom);
            outRom.Position = 0;
            outRom.CopyTo(sw);
        }

        public static void CompressRom(Rom uncompressedRom, Rom refRom, Stream sw)
        {
            Dictionary<long, FileRecord> sourceRecords = new Dictionary<long, FileRecord>();

            foreach (FileRecord r in refRom.Files)
            {
                if (r.VRom.End == 0)
                    break;
                sourceRecords.Add(r.VRom.Start, r);
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            CompressRom(uncompressedRom, sourceRecords, sw);
            stopwatch.Stop();
            Console.WriteLine();
            Console.WriteLine(stopwatch.Elapsed);
        }

        private static void CompressRom(Rom rom, Dictionary<long, FileRecord> refFileList, Stream sw)
        {
            int filesProcessed = 0;
            int filesTotal;
            List<FileRecord> newDmaTable = new List<FileRecord>();
            Console.WriteLine();
            foreach (FileRecord record in rom.Files)
            {
                Stream outstream;
                bool IsCompressed = false;
                filesTotal = rom.Files.Count();

                //get file
                outstream = rom.Files.GetFile(record.VRom.Start);

                //Compress if the file can't be found in the don't compress list,
                //or if the file listed should be compressed (legacy)
                if (!refFileList.ContainsKey(record.VRom.Start)
                    || refFileList[record.VRom.Start].IsCompressed)
                {
                    //compress file
                    IsCompressed = true;
                    MemoryStream ms = new MemoryStream();
                    using (BinaryReader br = new BinaryReader(outstream))
                    {
                        byte[] data = br.ReadBytes(record.VRom.Size);
                        Yaz.Encode(data, data.Length, ms);
                        ms.Position = 0;
                    }
                    outstream = ms;
                }

                //data contains the data to write to rom
                AppendFile(sw, newDmaTable, outstream, record, IsCompressed);
                //writes progress to console window
                filesProcessed++;
                Console.Write(processedFiles, filesProcessed, filesTotal);
            }

            sw.PadToLength(0x2000000);
            WriteFileTable(sw,rom.Files.GetDmaDataStart(), newDmaTable);
            CRC.Write(sw);
        }

        public static void CompressRomOptimized(ORom uncompressedRom, ORom refRom, Stream sw)
        {
            int filesProcessed = 0;
            int filesTotal;
            List<FileRecord> newDmaTable = new List<FileRecord>();
            filesTotal = uncompressedRom.Files.Count();

            foreach (FileRecord record in uncompressedRom.Files)
            {

                Stream outstream = GetFile_CompressedOptimized(uncompressedRom, refRom, record, out bool IsCompressed);
                AppendFile(sw, newDmaTable, outstream, record, IsCompressed);


                //writes progress to console window
                filesProcessed++;
                Console.Write(processedFiles, filesProcessed, filesTotal);
            }

            sw.PadToLength(0x2000000);
            WriteFileTable(sw, uncompressedRom.Files.GetDmaDataStart(), newDmaTable);
            CRC.Write(sw);
        }

        private static Stream GetFile_CompressedOptimized(ORom uncompressedRom, ORom refRom,
            FileRecord record, out bool IsCompressed)
        {
            FileRecord refRecord;

            //If a matching file record can't be found, assume that the file should be compressed
            if (!refRom.Files.TryGetFileRecord(record.VRom, out refRecord))
            {
                IsCompressed = true;
                return CompressFile(uncompressedRom, record);
            }

            //else

            //Compressed state = refRecord compressed state
            IsCompressed = refRecord.IsCompressed;

            //if file isn't compressed, just send the uncompressed file from the uncompressed rom
            if (!IsCompressed)
            {
                return uncompressedRom.Files.GetFile(record.VRom.Start);
            }

            //the file is compressed, try and see if the reference rom already has an identical
            //file compressed for us

            using (Stream uncompressed = uncompressedRom.Files.GetFile(record.VRom))
            using (Stream comp = refRom.Files.GetFile(record.VRom))
            {
                if (!uncompressed.IsDifferentTo(comp))
                    return refRom.Files.GetPhysicalFile(record.VRom);
                else
                    return CompressFile(uncompressedRom, record);

            }
        }

        private static void AppendFile(Stream sw, List<FileRecord> dmaTable, Stream file,
            FileRecord record, bool IsCompressed)
        {
            FileAddress physicalAddress;
            int pos;
            int offset;

            //get the next position to write the file
            pos = (int)sw.Position;
            offset = (int)(0x10 - file.Length) & 0x0F;
            
            //set the physical address for the file
            physicalAddress = new FileAddress(pos,
                        (IsCompressed) ? pos + file.Length + offset : 0);

            //create a new file table record for the file
            dmaTable.Add(new FileRecord(record.VRom, physicalAddress));
            file.CopyTo(sw);
            sw.PadToNextLine();
            sw.Flush();

            //for (int i = 0; i < offset; i++)
            //    sw.WriteByte(0);
        }

        private static Stream CompressFile(ORom rom, FileRecord record)
        {
            byte[] data;

            using (BinaryReader br = new BinaryReader(rom.Files.GetFile(record.VRom.Start)))
            {
                data = br.ReadBytes(record.VRom.Size);
            }

            //compress file
            MemoryStream ms = new MemoryStream();
            Yaz.Encode(data, data.Length, ms);
            ms.Position = 0;

            return ms;
        }

        public static void DecompressRom(Rom compressedRom, Stream sw)
        {
            List<FileRecord> newDmaTable = new List<FileRecord>();
            int filesProcessed = 0;
            int filesTotal;

            filesTotal = compressedRom.Files.Count();

            //build the new dmadata table
            foreach (FileRecord record in compressedRom.Files)
            {
                FileAddress vrom = record.VRom; //current address
                
                //MM Check for empty file
                int start = (record.Rom.Start == -1) ? -1 : vrom.Start;
                FileRecord rec = new FileRecord(vrom, new FileAddress(start, 0));
                newDmaTable.Add(rec);
            }

            //write the decompressed files to the stream
            foreach (FileRecord record in compressedRom.Files.OrderBy(x => x.VRom.Start))
            {
                FileAddress vrom = record.VRom; //current address
                sw.PadToLength(vrom.Start);

                //If file exists (supports MM)
                if (record.Rom.Start != -1)
                {
                    var file = compressedRom.Files.GetFile(vrom);
                    ((Stream)file).CopyTo(sw);
                }

                //writes progress to console window
                filesProcessed++;
                Console.Write(processedFiles, filesProcessed, filesTotal);
            }

            sw.PadToLength(0x4000000);
            WriteFileTable(sw, compressedRom.Files.GetDmaDataStart(), newDmaTable);
            CRC.Write(sw);
        }

        public static void PatchFiles_SameVrom(Stream output, bool outputIsCompressed,
            ORom hostRom, Dictionary<long, string> updateFiles, GetFileDelegate getFile)
        {
            if (outputIsCompressed)
                PatchFiles_SameVrom_Compressed(output, hostRom, updateFiles, getFile);
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Patches files into a rom, where file VROM mappings are identical
        /// </summary>
        /// <param name="OutputFilename"></param>
        /// <param name="hostRom"></param>
        /// <param name="UpdateFiles"></param>
        /// <param name="GetFile"></param>
        private static void PatchFiles_SameVrom_Compressed(Stream output, ORom hostRom,
            Dictionary<long, string> UpdateFiles, GetFileDelegate GetFile)
        {
            List<FileRecord> newDmaTable = new List<FileRecord>();

            foreach (FileRecord record in hostRom.Files)
            {
                long physicalStart = output.Position;
                long vromFilesize;
                long romFilesize;
                Stream file;

                //if the file is being replaced with a custom file
                if (UpdateFiles.ContainsKey(record.VRom.Start))
                {
                    //get source file
                    file = GetFile(UpdateFiles[record.VRom.Start]);

                    //Get virtual file size
                    if (record.IsCompressed)
                        Yaz.DecodeSize(file, out vromFilesize);
                    else
                        vromFilesize = Align.To16(file.Length);

                    //get the physical file size with padding
                    romFilesize = Align.To16(file.Length);
                }
                else //copy a source rom file.
                {
                    vromFilesize = record.VRom.Size;
                    romFilesize = record.Data.Size;

                    if (romFilesize > 0x800000)
                        throw new Exception("Internal file too large");

                    file = hostRom.Files.GetPhysicalFile(record.VRom.Start);
                }

                //copy file
                file.CopyTo(output);
                file.Close();

                //pad out
                output.PadToLength(physicalStart + romFilesize);

                //generate a new file table record
                newDmaTable.Add(new FileRecord(
                    new FileAddress(record.VRom.Start, record.VRom.Start + vromFilesize),
                    new FileAddress(physicalStart, (record.IsCompressed) ? physicalStart + romFilesize : 0)));

            }
            output.PadToLength(0x2000000);
            WriteFileTable(output, hostRom.Files.GetDmaDataStart(), newDmaTable);

            //write crc
            CRC.Write(output);
        }

        public static void SetLanguage(Stream rom, Rom.Language language)
        {
            long pos = rom.Position;
            byte chr;
            rom.Position = 0x3E;

            switch (language)
            {
                case Rom.Language.Japanese: chr = 0x4A; break;
                case Rom.Language.English: chr = 0x45; break;
                default: throw new NotImplementedException();
            }
            rom.WriteByte(chr);
            rom.Position = pos;
        }

        private static void WriteFileTable(Stream sw, FileAddress dmaStart, List<FileRecord> fileTable)
        {
            int fileTableLoc = dmaStart.Start;
            sw.Position = fileTableLoc;

            BinaryWriter bw = new BinaryWriter(sw);
            foreach (FileRecord record in fileTable)
            {
                bw.WriteBig(record.VRom.Start);
                bw.WriteBig(record.VRom.End);
                bw.WriteBig(record.Rom.Start);
                bw.WriteBig(record.Rom.End);
            }
        }
        
    }
}
