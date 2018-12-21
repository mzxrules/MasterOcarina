using mzxrules.Helper;
using mzxrules.OcaLib.Addr2;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Addr = mzxrules.OcaLib.Addr2.Address;
using Domain = mzxrules.OcaLib.Addr2.SpaceDomain;


namespace mzxrules.OcaLib
{
    public static class Addresser
    {
        static Addresses AddressDoc { get; set; }
        static Addresser()
        {
            using (FileStream stream = File.OpenRead("Data/Addresses.xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Addresses));

                using (XmlReader reader = XmlReader.Create(stream))
                {
                    AddressDoc = (Addresses)serializer.Deserialize(reader);
                }
            }
        }


        public static bool TryGetOffset(string addrVar, RomVersion version, out int v)
        {
            v = 0;

            if (!TryGetBlock(version, addrVar, out Block block))
                return false;

            var lookupAddr = block.Identifier.SingleOrDefault(x => x.id == addrVar);

            if (!(lookupAddr.Item.Count > 0 && lookupAddr.Item[0] is Offset))
                return false;

            var lookupSet = lookupAddr.Item.Cast<Offset>().ToList();

            var group = version.GetGroup();

            Offset offset = lookupSet.SingleOrDefault(x => x.id == version.GetGroup());

            if (offset == null && group != null)
            {
                offset = lookupSet.SingleOrDefault(x => x.id == null);
            }
            
            if (offset == null)
                return false;
            

            if (!TryGetOffsetValue(offset, out v))
                return false;
            return true;
        }

        #region (Try)GetRam

        public static bool TryGetRam(RomFileToken file, RomVersion version, out int v)
        {
            var block = GetBlock(version, file.ToString());

            return TryGetStart(block, version, Domain.RAM, out v);
        }

        public static bool TryGetRam(string addrVar, RomFileToken file, RomVersion version, out int v)
        {
            var block = GetBlock(version, file.ToString());

            return TryMagicConverter(block, addrVar, version, Domain.RAM, out v);
        }

        public static bool TryGetRam(string addrVar, RomVersion version, out int v)
        {
            v = 0;
            if (!TryGetBlock(version, addrVar, out Block block))
                return false;

            if (!TryMagicConverter(block, addrVar, version, Domain.RAM, out v))
                return false;

            return true;

            //return TryGetAddress("ram", version, addrVar, out v);
        }

        #endregion

        #region (Try)GetRom
        public static bool TryGetRom(RomFileToken file, RomVersion version, N64Ptr ramAddr, out int v)
        {
            ramAddr &= 0xFFFFFF;
            var block = GetBlock(version, file.ToString());
            if (!TryGetStart(block, version, Domain.ROM, out int romStart)
                || !TryGetStart(block, version, Domain.RAM, out int ramStart)
                || ramAddr < ramStart)
            {
                v = 0;
                return false;
            }

            v = romStart + ramAddr - ramStart;
            return true;
        }

        public static bool TryGetRom(RomFileToken file, RomVersion version, string addrVar, out int v)
        {
            var block = GetBlock(version, file.ToString());

            if (TryMagicConverter(block, addrVar, version, Domain.ROM, out v))
                return true;

            return false;
        }

        public static int GetRom(RomFileToken file, RomVersion version)
        {
            var block = GetBlock(version, file.ToString());

            TryGetStart(block, version, Domain.ROM, out int addr);
            return addr;
        }

        public static int GetRom(RomFileToken file, RomVersion version, string addrVar)
        {
            int addr;
            var block = GetBlock(version, file.ToString());

            if (addrVar == "__Start")
                TryGetStart(block, version, Domain.ROM, out addr);
            else
                TryMagicConverter(block, addrVar, version, Domain.ROM, out addr);
            return addr;
        }

        #endregion

        private static bool TryGetStart(Block block, RomVersion version, Domain domain, out int start)
        {
            start = 0;
            var startAddr = block.Start.SingleOrDefault(x => x.domain == domain);

            if (startAddr == null
                || !TryGetAddrValue(startAddr, version, out start))
                return false;
            return true;
        }

        private static bool TryGetBlock(RomVersion version, string addressIdentifier, out Block block)
        {
            var game = (from a in AddressDoc.Game
                       where a.name.ToString() == version.Game.ToString()
                       select a).Single();

            block = (from b in game.Block
                     where b.Identifier.Any(x => x.id == addressIdentifier)
                     select b).SingleOrDefault();
            if (block != null)
                return true;

            return false;
        }

        private static Block GetBlock(RomVersion version, string blockId)
        {
            var game = from a in AddressDoc.Game
                       where a.name.ToString() == version.Game.ToString()
                       select a;

            var block = (from b in game.Single().Block
                         where b.name == blockId
                         select b).SingleOrDefault();
            return block;
        }

        private static bool TryMagicConverter(Block block, string ident, RomVersion version, Domain domain, out int result)
        {
            int lookup;
            result = 0;
            lookup = 0;

            var lookupAddr = block.Identifier.SingleOrDefault(x => x.id == ident);

            if (lookupAddr == null)
                return false;

            if (lookupAddr.Item.Count > 0 && lookupAddr.Item[0] is Addr)
            {
                Addr addr = (Addr)lookupAddr.Item[0];

                if (!TryGetAddrValue(addr, version, out lookup))
                    return false;

                //if lookup is absolute, and in the same space, we have it

                if (addr.reftype == AddressType.absolute
                    && addr.domain == domain)
                {
                    result = lookup;
                    return true;
                }

                //if lookup is absolute, but not in the same space, convert to offset
                if (addr.reftype == AddressType.absolute && addr.domain != domain)
                {
                    Addr altStartAddr;

                    altStartAddr = block.Start.SingleOrDefault(x => x.domain == addr.domain);

                    if (altStartAddr == null
                        || !TryGetAddrValue(altStartAddr, version, out int altStart))
                        return false;

                    lookup -= altStart;
                }
            }
            else if (lookupAddr.Item.Count > 0 && lookupAddr.Item[0] is Offset)
            {

                var lookupSet = lookupAddr.Item.Cast<Offset>().ToList();

                Offset offset = lookupSet.SingleOrDefault(x => x.id == version.GetGroup());

                if (offset == null)
                    return false;
                
                if (!TryGetOffsetValue(offset, out lookup))
                    return false;
            }

            if (!TryGetStart(block, version, domain, out int start))
                return false;

            result = start + lookup;

            return true;
        }

        private static bool TryGetOffsetValue(Offset offset, out int result)
        {
            try
            {
                result = Convert.ToInt32(offset.Value, 16);
                return true;
            }
            catch
            {
                result = -1;
                return false;
            }
        }

        private static bool TryGetAddrValue(Addr2.Address addr, RomVersion version, out int result)
        {
            try
            {
                result = Convert.ToInt32(addr.Data.Single(x => x.ver == version.ToString()).Value, 16);
            }
            catch
            {
                result = -1;
                return false;
            }
            return true;
        }
    }
}
