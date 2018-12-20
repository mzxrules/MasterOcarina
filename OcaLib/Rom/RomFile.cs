using System;
using System.IO;

namespace mzxrules.OcaLib
{
    public class RomFile
    {
        public FileRecord Record { get; private set; }
        public Stream Stream { get; private set; }
        public RomVersion Version { get; private set; }

        public RomFile(FileRecord record, Stream s, RomVersion version)
        {
            Record = record;
            Stream = s;
            Version = version;
        }

        public static implicit operator Stream(RomFile file)
        {
            return file.Stream;
        }

        //public static explicit operator Stream(RomFile v)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
