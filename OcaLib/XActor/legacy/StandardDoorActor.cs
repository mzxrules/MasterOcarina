using mzxrules.OcaLib.Actor;
using mzxrules.Helper;

namespace mzxrules.XActor.OActors
{
    class StandardDoorActor: TransitionActor
    {
        SwitchFlag flag;
        bool locked;
        public StandardDoorActor(byte[] record)
            : base(record)
        {
            flag = Shift.AsByte(Variable, 0x003F);
            locked = Shift.AsBool(Variable, 0x80);
        }
        protected override string GetActorName()
        {
            return (locked) ? "Locked Door" : "Door";
        }
        protected override string GetVariable()
        {
            return flag.ToString();
        }
        //public override string Print()
        //{
        //    return string.Format("{3}, {0}Door: {1}, {2}",
        //        ,
        //        flag.Print(),
        //        PrintWithoutActor(),
        //        PrintTransition());
        //}
    }
}
