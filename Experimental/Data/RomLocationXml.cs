using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace VerboseOcarina
{
    public static class RomLocation
    {
        static XDocument romLocDoc;

        static RomLocation()
        {
            //var stream = Assembly.GetExecutingAssembly()
            //    .GetManifestResourceStream("RomReader.RomLocations.xml");
            romLocDoc = XDocument.Load("Data/RomLocations.xml");

        }

        public static bool TryGetRomLocation(ORom.Build version, ref string location)
        {
            XElement romLocation;
            var x = from a in romLocDoc.Elements("values").Elements("string")
                    where a.Attribute("v").Value == version.ToString()
                    select a;

            romLocation = x.SingleOrDefault();

            if (romLocation == null)
                return false;

            location = romLocation.Value;
            return File.Exists(location);
        }

        public static void SetRomLocation(ORom.Build version, string location)
        {
            XElement romLocation;
            var x = from a in romLocDoc.Elements("values").Elements("string")
                    where a.Attribute("v").Value == version.ToString()
                    select a;

            romLocation = x.SingleOrDefault();

            if (romLocation == null)
                throw new NotImplementedException();

            romLocation.Value = location;
            romLocDoc.Save("RomLocations.xml");
        }
    }
}
