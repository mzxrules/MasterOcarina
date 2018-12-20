using System;

namespace mzxrules.OcaLib.Actor
{ 
    public class ChestFlag
    {
        byte value;
        ChestFlag(byte b) 
        {
            value = b;
        }
        
        public static implicit operator byte(ChestFlag s)
        {
            return s.value;
        }
        public static implicit operator ChestFlag(byte b)
        {
            return new ChestFlag(b);
        }
        public override string ToString()
        {
            if (value < 0x20)
            {
                return $"chest flag: {value:X2}";
            }
            else
                return "invalid";
        }
    }
}
