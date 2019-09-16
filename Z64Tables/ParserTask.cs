using mzxrules.OcaLib;

namespace Z64Tables
{
    class ParserTask
    {
        public string FileIn { get; set; }
        public string FileOut { get; set; }
        public string Name { get; set; }
        public RomVersion Version { get; set; }
        public FormatTypesEnum Format { get; set; }


        public long StartAddress { get; set; }

        //loop
        public long LoopFor { get; set; }
        
        /// <summary>
        /// Incrementor
        /// </summary>
        public long Inc { get; set; }

        //iterator
        public long Index { get; set; }

        public long GetOffset(long address)
        {
            return address - StartAddress;
        }
        
    }
}
