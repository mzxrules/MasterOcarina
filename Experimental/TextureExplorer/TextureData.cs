using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Experimental.TextureExplorer
{
    [XmlRoot(ElementName = "root")]
    public class TextureData
    {
        public TextureData()
        {
            version = "2";
            FileDirectory = new List<TextureDirectory>();
        }
        [XmlElement(ElementName = "directory")]
        public List<TextureDirectory> FileDirectory { get; set; }

        [XmlAttribute]
        public string version;

    }

    public class TextureDirectory
    {
        private TextureDirectory() { }
        public TextureDirectory(string folderName, int address, int size)
        {
            Name = folderName;
            Address = address;
            Size = size;
            Folders = new List<TextureDirectory>();
            Textures = new List<Texture>();
        }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "address")]
        public string _Address { get; set; }
        [XmlIgnore]
        public int Address
        {
            get { return int.Parse(_Address.Substring(2), System.Globalization.NumberStyles.HexNumber); }
            set { _Address = string.Format("0x{0:X8}", value); }
        }

        [XmlAttribute(AttributeName = "size")]
        public string _Size { get; set; }

        [XmlIgnore]
        public int Size
        {
            get { return int.Parse(_Size.Substring(2), System.Globalization.NumberStyles.HexNumber); }
            set { _Size = string.Format("0x{0:X8}", value); }
        }

        [XmlElement(ElementName = "directory")]
        public List<TextureDirectory> Folders { get; set; }

        [XmlElement(ElementName = "texture")]
        public List<Texture> Textures { get; set; }

        public override string ToString()
        {
            return String.Format("{1:X8}: size {2:X6} - {0} ",
                Name, Address, Size);
        }
    }

    public class Texture
    {
        [XmlIgnore]
        static Dictionary<string, int> FormatBitsize = new Dictionary<string, int>
        #region FormatBitsize { ... }
        {
            {"i4",  4},
            {"ia4", 4},
            {"ci4", 4},
            {"i8",  8},
            {"ia8", 8},
            {"ci8", 8},
            {"rgb5a1", 16},
            {"ia16", 16},
            {"yuv16", 16}, 
            {"rgba32", 32},
        };
        #endregion

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "address")]
        public string _Address { get; set; }
        [XmlIgnore]
        public int Address
        {
            get { return int.Parse(_Address.Substring(2), System.Globalization.NumberStyles.HexNumber); }
            set { _Address = string.Format("0x{0:X8}", value); }
        }

        [XmlAttribute(AttributeName = "format")]
        public string Format { get; set; }


        [XmlAttribute(AttributeName = "width")]
        public int Width { get; set; }


        [XmlAttribute(AttributeName = "height")]
        public int Height { get; set; }

        [XmlAttribute(AttributeName = "palette")]
        public string _Palette { get; set; }

        [XmlIgnore]
        public int? Palette
        {
            get
            {
                if (String.IsNullOrEmpty(_Palette))
                    return null;
                return int.Parse(_Palette.Substring(2), System.Globalization.NumberStyles.HexNumber);
            }
            set
            {
                if (value == null)
                    _Palette = null;
                _Palette = string.Format("0x{0:X8}", value);
            }
        }


        [XmlIgnore]
        public int Size
        {
            get
            {
                if (Format == "jpeg")
                    return 0;
                return Width * Height * FormatBitsize[Format] / 8;
            }
        }

        private Texture() { }

        public Texture(string name, int address, string format, int width, int height)
        {
            Name = name;
            Address = address;
            Format = format;
            Width = width;
            Height = height;
        }

        public override string ToString()
        {
            return string.Format("{0:X6}: {1} {2:D2}x{3:D2} {4:X4}{5} - {6}",
                Address, Format, Width, Height, Size,
                (Palette != null)? " " + ((int)Palette).ToString("X6"): "",
                Name);
        }

    }
}
