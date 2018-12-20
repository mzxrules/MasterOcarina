using mzxrules.Helper;
namespace mzxrules.OcaLib.SceneRoom.Commands
{
    class RoomBehaviorCommand : SceneCommand
    {
        private Game game;

        public RoomBehaviorCommand(Game game)
        {
            this.game = game;
        }

        public override string ToString()
        {
             string result = $"Room Behavior: ? {Command.Data1:X2}, " 
                + $"Disable Warp Songs? {Shift.AsBool(Command.Data2, 0x400)}, "
                + $"Show invisible actors? {Shift.AsBool(Command.Data2, 0x100)}, "
                + $"Idle Animation {(Command.Data2 & 0xFF):X2}";
            if (game == Game.OcarinaOfTime)
                return result;
            else
                return result + ", "
                    + $"Lighting? {Shift.AsBool(Command.Data2, 0x800)}, "
                    + $"Day 2 Rain? {Shift.AsBool(Command.Data2, 0x1000)}";
        }
        public override string Read()
        {
            return ToString();
        }
    }
}
