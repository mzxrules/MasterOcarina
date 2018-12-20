using mzxrules.Helper;

namespace mzxrules.OcaLib
{
    public class FileRecord
    {
        //public const int LENGTH = 0x10;
        /// <summary>
        /// The file's mapped location in virtual space
        /// </summary>
        public FileAddress VirtualAddress { get; set; } 
        /// <summary>
        /// The file's mapped location on file. A zeroed end address means that the file is not compressed
        /// </summary>
        public FileAddress PhysicalAddress { get; set; } 
        /// <summary>
        /// The file's location independent of compression state
        /// </summary>
        public FileAddress DataAddress { get; private set; }
        public bool IsCompressed => PhysicalAddress.End != 0; 
        public int Index = -1;

        public FileRecord(FileRecord fileRecord)
        {
            VirtualAddress = fileRecord.VirtualAddress;
            PhysicalAddress = fileRecord.PhysicalAddress;
            DataAddress = fileRecord.DataAddress;
            Index = fileRecord.Index;
        }

        public FileRecord(FileAddress virtualAddr, FileAddress physicalAddr, int index)
        {
            VirtualAddress = virtualAddr;
            PhysicalAddress = physicalAddr;
            Index = index;

            if (IsCompressed)
            {
                DataAddress = new FileAddress(PhysicalAddress.Start, PhysicalAddress.End);
            }
            else
                DataAddress = new FileAddress(PhysicalAddress.Start, PhysicalAddress.Start + VirtualAddress.Size);
        }

        public long GetRelativeAddress(long offset)
        {
            return offset - VirtualAddress.Start;
        }
    }
}
