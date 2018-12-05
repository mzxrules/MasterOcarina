using mzxrules.Helper;
using System;
using System.Runtime.Serialization;

namespace Atom
{
    public class Label
    {
        public enum Type
        {
            LBL, 
            FUNC,
            VAR
        }
        public N64Ptr Addr { get; protected set; }
        public Type Kind { get; set; }

        //function variables
        public string Name;
        public string InlineDesc;
        public string Desc;
        public string Desc2;
        public string Args;
        public int hits;

        public bool Confirmed = false;

        public Label() { }

        public Label(Type kind, N64Ptr addr, bool confirmed = false)
        {
            Addr = addr;
            Kind = kind;
            Confirmed = confirmed;
        }
        
        public Label(FunctionInfo info) : this(Type.FUNC, info.Address, true)
        {
            Addr = info.Address;
            Name = info.Name ?? "";

            Desc = info.Desc ?? "";
            Desc2 = info.Desc2 ?? "";
            Args = info.Args ?? "";
            
            InlineDesc = Name;

            if (string.IsNullOrWhiteSpace(info.Name))
            {
                InlineDesc = (string.IsNullOrWhiteSpace(info.Desc)) ? ToString() : Desc;
            }
        }
        
        public override string ToString()
        {
            return $"{Kind.ToString().ToLower()}_{(int)(Addr | 0x80000000):X8}";
        }
    }

    [DataContract]
    public class FunctionInfo
    {
        [DataMember]
        protected string Addr { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Desc { get; set; }

        [DataMember]
        public string Desc2 { get; set; }

        [DataMember]
        public string Args { get; set; }

        public N64Ptr Address { get { return int.Parse(Addr, System.Globalization.NumberStyles.HexNumber); } }
        
    }
}
