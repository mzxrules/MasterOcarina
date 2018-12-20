using mzxrules.Helper;
using System.Collections.Generic;
using System.IO;

namespace mzxrules.OcaLib
{
    public class DmaData
    {
        public List<FileRecord> Table = new List<FileRecord>();
        public FileRecord Address { get; private set; }

        public DmaData (Stream s, int address)
        {
            InitializeTable(s, address);
        }

        public DmaData (Stream s, RomVersion version)
        {
            RomFileToken token = ORom.FileList.invalid;
            if (version.Game == Game.OcarinaOfTime)
                token = ORom.FileList.dmadata;
            else if (version.Game == Game.MajorasMask)
                token = MRom.FileList.dmadata;
            
            int address = Addresser.GetRom(token, version, "__Start");
            InitializeTable(s, address);
        }

        private void InitializeTable(Stream fs, int address)
        {
            FileRecord fileRecord;
            int length;         //length of file table
            int position;       //offset into rom of file table
            int positionEnd;    //end offset for the file table

            FileAddress fileVirtualAddress;
            FileAddress filePhysicalAddress;
            
            BinaryReader br = new BinaryReader(fs);
            //set rom type dependent values
            position = address;

            //set stream position
            fs.Position = position;

            positionEnd = GetEndAddress(br);

            length = (positionEnd - position) / 0x10;

            for (int i = 0; i < length; i++)
            {
                fileVirtualAddress = new FileAddress(
                    br.ReadBigInt32(),
                    br.ReadBigInt32());
                filePhysicalAddress = new FileAddress(
                    br.ReadBigInt32(),
                    br.ReadBigInt32());

                fileRecord = new FileRecord(fileVirtualAddress, filePhysicalAddress, i);
                Table.Add(fileRecord);
            }
            TryGetFileRecord(address, out FileRecord addr);
            Address = addr;
        }

        public bool TryGetFileRecord(long virtualStart, out FileRecord fr)
        {
            foreach (FileRecord item in Table)
            {
                if (item.VirtualAddress.Start == virtualStart)
                {
                    fr = item;
                    return true;
                }
                if (item.VirtualAddress.Start == 0
                    && item.VirtualAddress.End == 0)
                    break;
            }
            fr = new FileRecord(new FileAddress(), new FileAddress(), 0);
            return false;
        }

        private int GetEndAddress(BinaryReader br)
        {
            long seekback;
            int result;

            seekback = br.BaseStream.Position;

            //set position
            br.BaseStream.Position += 0x24; //3rd record

            result = br.ReadBigInt32();

            br.BaseStream.Position = seekback;
            return result;
        }

    }
}
