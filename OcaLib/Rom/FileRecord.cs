using mzxrules.Helper;

namespace mzxrules.OcaLib
{
    public record struct FileRecord
    {
        /// <summary>
        /// The file's location when a rom is decompressed. Also determines decompressed size.
        /// </summary>
        public FileAddress VRom { get; set; }

        /// <summary>
        /// The file's actual location in rom. A negative start address denotes that the file does not exist on rom.
        /// A zeroed end address represents that the file is compressed
        /// </summary>
        public FileAddress Rom { get; set; }

        /// <summary>
        /// The file's location, independent of compression state.
        /// </summary>
        /// 
        public readonly FileAddress Data => GetDataAddress();

        public readonly bool IsCompressed => Rom.End != 0;

        public readonly bool IsNull => VRom.Start == 0 && VRom.End == 0;

        public FileRecord(FileRecord rec)
        {
            VRom = rec.VRom;
            Rom = rec.Rom;
        }

        public FileRecord(FileAddress vrom, FileAddress rom)
        {
            VRom = vrom;
            Rom = rom;
        }

        public readonly long GetRelativeAddress(long offset)
        {
            return offset - VRom.Start;
        }

        private readonly FileAddress GetDataAddress()
        {
            var start = Rom.Start;
            var end = IsCompressed ? Rom.End : Rom.Start + VRom.Size;
            return new(start, end);
        }
    }
}
