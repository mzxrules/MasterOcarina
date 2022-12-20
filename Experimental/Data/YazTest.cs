using mzxrules.OcaLib;
using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Experimental.Data
{
    static partial class Get
    {
        public static void TestYazDecompress(IExperimentFace face, List<string> filePath)
        {
            StringBuilder sb = new StringBuilder();
            ORom compRom = new(filePath[0], ORom.Build.N0);
            ORom decompRom = new(@"C:\Users\mzxrules\Documents\Roms\N64\Games\Ocarina\Ocarina (U10)\Ocarina (U10) (d).z64", ORom.Build.N0);
            RomFile compFile = compRom.Files.GetFile(ORom.FileList.code);
            RomFile decompFile = decompRom.Files.GetFile(ORom.FileList.code);

            BinaryReader compReader = new(compFile);
            BinaryReader decompReader = new(decompFile);

            byte[] compText = compReader.ReadBytes(compFile.Record.VRom.Size);
            byte[] decompText = decompReader.ReadBytes(decompFile.Record.VRom.Size);

            for (int i = 0; i < compFile.Record.VRom.Size; i++ )
            {
                byte a = decompText[i];
                byte b = compText[i];
                if (a == b)
                    continue;
                
                sb.AppendLine($"DECOMPRESS MISMATCH {i:X6}: {a:X2} {b:X2}");
                    break;
            }

            sb.AppendLine("Test Complete");
            face.OutputText(sb.ToString());
        }

        public static void TestYazSimple(IExperimentFace face, List<string> filePath)
        {
            StringBuilder sb = new();
            ORom rom = new(filePath[0], ORom.Build.N0);

            RomFile rfile = rom.Files.GetFile(ORom.FileList.code);
            var dmarec_code = rfile.Record;

            BinaryReader compressed = new((MemoryStream)rom.Files.GetPhysicalFile(dmarec_code.VRom));
            BinaryReader decompressed = new(rfile);
            byte[] decompressedbuffer = decompressed.ReadBytes(rfile.Record.VRom.Size);

            //var buffer = new MemoryStream(0x20_0000);
            var buffer = new byte[0];

            Stopwatch stopwatch = Stopwatch.StartNew();
            //int compressedSize = Yaz.Encode(decompressedbuffer, rfile.Record.VirtualAddress.Size, buffer);
            int compressedSize = Yaz.Encode(decompressedbuffer, rfile.Record.VRom.Size, out buffer);
            Yaz.GetEncodingData(buffer, compressedSize, out int metaSize);

            stopwatch.Stop();

            sb.AppendLine($"Compression time: {stopwatch.Elapsed.Seconds}.{stopwatch.Elapsed.Milliseconds:D3}");
            sb.AppendLine($"Compressed Size: {compressedSize:X6}, Meta Size {metaSize:X6}");

            decompressed.Seek(0);
            //buffer.Position = 0;

            //compare compressed output
            if (compressedSize != dmarec_code.Rom.Size)
                sb.AppendLine($"Compression size mismatch: Original {dmarec_code.Rom.Size:X6} || New {compressedSize:X6}");

            int mismatchMax = 8;
            for (int i = 0; i < dmarec_code.Rom.Size; i++)
            {
                byte a = compressed.ReadByte();
                //byte b = (byte)buffer.ReadByte(); 
                byte b = buffer[i];
                if (a == b)
                    continue;

                mismatchMax--;
                sb.AppendLine($"COMPRESS MISMATCH {i:X6}: {a:X2} {b:X2}");
                if (mismatchMax <= 0)
                    break;

            }
            compressed.Seek(0);
            //buffer.Position = 0;

            byte[] dbuffer = Yaz.Decode(new MemoryStream(buffer), compressedSize);
            decompressed.BaseStream.Position = 0;

            for (int i = 0; i < dbuffer.Length; i++)
            {
                if (dbuffer[i] != decompressed.ReadByte())
                {
                    sb.AppendLine($"File Size: {dbuffer.Length:X}, Compressed: {compressedSize:X}, Failed Match: {i:X}");
                    break;
                }
            }

            sb.AppendLine("Test Complete");
            face.OutputText(sb.ToString());
        }

        public static void TestYazCompression(IExperimentFace face, List<string> filePath)
        {
            ORom rom = new ORom(filePath[0], ORom.Build.N0);
            StringBuilder sb = new StringBuilder();

            foreach(var file in rom.Files)
            {
                if (!file.IsCompressed)
                    continue;

                try
                {
                    Stream vanillaFile = rom.Files.GetPhysicalFile(file.VRom);
                    var decompressedFile = Yaz.Decode(vanillaFile, file.Rom.Size);
                    MemoryStream ms = new(file.Rom.Size);

                    sb.AppendLine($"{ Yaz.Encode(decompressedFile, decompressedFile.Length, ms):X8}");
                    while (ms.Length < ms.Capacity)
                    {
                        ms.WriteByte(0);
                    }

                    vanillaFile.Position = 0;
                    ms.Position = 0;

                    BinaryReader original = new BinaryReader(vanillaFile);
                    BinaryReader test = new BinaryReader(ms);

                    sb.AppendLine($"{file.VRom} - original: {original.BaseStream.Length:X8} test: {test.BaseStream.Length:X8}");

                    for (int i = 0; i < file.Rom.Size; i+= 4)
                    {
                        int left = original.ReadBigInt32();
                        int right = test.ReadBigInt32();
                        if (left != right)
                        {
                            sb.AppendLine($"{file.VRom} - {i:X8} does not match, comparison stopped");
                        }
                    }

                }
                catch (Exception e)
                {
                    sb.AppendLine($"{file.VRom} - Exception {e.Message} {e.InnerException}");
                }

            }
            face.OutputText(sb.ToString());
        }
    }
}
