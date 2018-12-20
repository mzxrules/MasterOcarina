using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace mzxrules.OcaLib.Address
{
    [Serializable()]
    [XmlRoot("Addresses", Namespace = "", IsNullable = false)]
    public partial class Addresses
    {
        [XmlElement(ElementName = "Game")]
        public List<Game> Games { get; set; }

        public Addresses()
        {
            Games = new List<Game>();
        }
    }

    [Serializable()]
    public partial class Game
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "File")]
        public List<File> Files;

        public Game()
        {
            Files = new List<File>();
        }

    }

    [Serializable()]
    public partial class File
    {
        [XmlAttribute("n")]
        public string Filename { get; set; }

        [XmlElement(ElementName = "Item")]
        public List<Item> Items { get; set; }

        public File()
        {
            Items = new List<Item>();
        }
    }


    [Serializable()]
    public partial class Item
    {
        [XmlAttribute("var")]
        public string Variable { get; set; }

        [XmlElement(ElementName = "Version")]
        public List<Value> Values { get; set; }
    }

    [Serializable()]
    public partial class Value
    {
        [XmlAttribute("v")]
        public string Version { get; set; }

        [XmlTextAttribute]
        public string _Address { get; set; }


        [XmlIgnore]
        public int? Address
        {
            get { return TryParse(_Address); }
            set { _Address = string.Format("{0:X}", value); }
        }

        private int? TryParse(string _Address)
        {
            int v;
            string addr = _Address;

            if (int.TryParse(addr, System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture, out v))
                return v;

            return null;
        }
    }
}
