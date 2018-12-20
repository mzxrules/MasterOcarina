using mzxrules.Helper;

namespace mzxrules.OcaLib.Actor
{
    public class CollectableFlag
    {
        byte value;
        CollectableFlag(byte b) 
        {
            value = b;
        }
        public CollectableFlag(ushort variable, ushort mask)
        {
            value = Shift.AsByte(variable, mask);
        }
        public static implicit operator byte(CollectableFlag s)
        {
            return s.value;
        }
        public static implicit operator CollectableFlag(byte b)
        {
            return new CollectableFlag(b);
        }
        public override string ToString()
        {
            if (value < 0x20)
            {
                return $"Perm: {value:X2}";
            }
            else 
            {
                return $"Temp: {(value - 0x20):X2}";
            }
        }
    }
}