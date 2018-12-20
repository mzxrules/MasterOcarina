using mzxrules.Helper;
using System;

namespace mzxrules.OcaLib.Actor
{
    public enum SwitchFlagAttributes
    {
        ListenSwitch,
        ListenClear,
        WriteSwitch
    }
    public interface ISwitchFlag
    {
        SwitchFlag Flag { get; set; }
        SwitchFlagAttributes GetFlagAttributes();
    }
    public class SwitchFlag
    {
        byte value;
        SwitchFlag(byte b) 
        {
            value = b;
        }
        public SwitchFlag(UInt16 variable, UInt16 mask)
        {
            value = Shift.AsByte(variable, mask); 
        }

        public static implicit operator byte(SwitchFlag s)
        {
            return s.value;
        }
        public static implicit operator SwitchFlag(byte b)
        {
            return new SwitchFlag(b);
        }
        public override string ToString()
        {
            if (value < 0x20)
            {
                return $"Perm: {value:X2}";
            }
            else if (value < 0x38)
            {
                return $"Temp: {(value - 0x20):X2}";
            }
            else if (value < 0x40)
            {
                if (value == 0x3F)
                    return "No Flag";
                return $"Local: {(value - 0x38):X2}";
            }
            else
                return "invalid";
        }
    }
}
