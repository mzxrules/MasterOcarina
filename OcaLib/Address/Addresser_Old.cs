using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace mzxrules.OcaLib
{
    public static class Addresser2
    {
        static Address.Addresses addresses;

        static Addresser2()
        {
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("mzxrules.OcaLib.Addresses.xml");


            XmlSerializer serializer;

            serializer = new XmlSerializer(typeof(Address.Addresses));

            using (XmlReader reader = XmlReader.Create(stream))
            {
                addresses = (Address.Addresses)serializer.Deserialize(reader);
            }
        }

        public static bool TryConvertToRam(RomFileToken file, RomVersion version, string addrVar, out int v)
        {
            int romStart;
            int romAddr;
            int ramAddr;
            if (!TryGetAddress(file, version, "__Start", out romStart)
                || !TryGetAddress(file, version, addrVar, out romAddr)
                || !TryGetAddress("ram", version, "CONV_" + file, out ramAddr))
            {
                v = 0;
                return false;
            }

            v = ramAddr + romAddr - romStart;
            return true;
        }

        public static bool TryConvertToRom(RomFileToken file, RomVersion version, uint ramAddr, out int v)
        {
            int romStart;
            int ramStart;
            if (!TryGetAddress(file, version, "__Start", out romStart)
                || !TryGetAddress("ram", version, "CONV_" + file, out ramStart)
                ||ramAddr < ramStart)
            {
                v = 0;
                return false;
            }
            ramAddr &= 0xFFFFFF;
            v = romStart + (int)ramAddr - ramStart;
            return true;
        }

        public static bool TryGetRam(RomVersion version, string addrVar, out int v)
        {
            return TryGetAddress("ram", version, addrVar, out v);
        }

        public static bool TryGetRom(RomFileToken file, RomVersion version, string addrVar, out int v)
        {
            return TryGetAddress(file, version, addrVar, out v);
        }

        public static int GetRom(RomFileToken file, RomVersion version, string addrVar)
        {
            return GetAddress(file.ToString(), version, addrVar);
        }

        private static XElement GetElementOfVersion(string version, XElement e)
        {
            var x = from item in e.Elements("Version")
                    where item.Attribute("v").Value == version.ToString()
                    select item;

            return x.SingleOrDefault();
        }

        #region GetAddress
        private static bool TryGetAddress(RomFileToken file, RomVersion version, string addrVar, out int value)
        {
            return TryGetAddress(file.ToString(), version, addrVar, out value);
        }

        private static bool TryGetAddress(string file, RomVersion version, string addrVar, out int value)
        {
            try
            {
                value = GetAddress(file, version, addrVar);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        private static int GetAddress(string file, RomVersion version, string addrVar)
        {

            var gFile = from a in addresses.Games
                        where a.Name == version.Game.ToString()
                        select a;

            var aFile = from b in gFile.Single().Files
                        where b.Filename == file
                        select b;

            var iAddr = from c in aFile.Single().Items
                        where c.Variable == addrVar
                        select c;

            var qualAddr = from d in iAddr.Single().Values
                           where d.Version == version.ToString()
                           select d;

            int? address = qualAddr.Single().Address;


            if (address == null)
                throw new ArgumentException(String.Format("{1} does not exist under {0} in Addresses.xml",
                    file, addrVar));

            return (int)address;

        }
        #endregion

    }
}
