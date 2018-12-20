namespace mzxrules.OcaLib.SceneRoom.Commands
{
    class SettingsCommand: SceneCommand
    {
        public override string Read()
        {
            return ToString();
        }
        public override string ToString()
        {
            switch ((HeaderCommands)Code)
            {
                case HeaderCommands.WindSettings:
                    return string.Format("Wind Settings: Forces Westward {0:X2}, Vertical {1:X2}, Southward {2:X2}, Strength {3:X2}",
                        Command[4],
                        Command[5],
                        Command[6],
                        Command[7]);

                case HeaderCommands.SpecialObject:
                    return $"Elf_Message {Command.Data1}, Load Object {Command.Data2:X4}";

                case HeaderCommands.TimeSettings:
                    return $"Time Settings: {Command[4]:X2}{Command[5]:X2} {Command[6]:X2}";

                case HeaderCommands.SkyboxSettings:
                    return string.Format("Skybox {0}, Cast: {1}, Fog? {2}",
                        Command[4],
                        (Command[5] == 1) ? "Cloudy" : "Sunny",
                        (Command[6] > 0) ? "Yes" : "No");

                case HeaderCommands.SkyboxModifier:
                    return $"Skybox Settings: Disable Sky? {Command.Data2 >> 24 > 0}, Disable Sun/Moon? {(Command.Data2 & 0xFF0000) >> 16 > 0}";

                case HeaderCommands.SoundSettings:
                    return string.Format("Sound Settings: Reverb {0}, Playback option {1}, Song {2:X2}.",
                        Command.Data1,
                        (Command[6] == 0x13) ? "Always Playing" : $"{Command[6]:X2}",
                        Command[7]);

                case HeaderCommands.SoundSettingsEcho:
                    return $"Sound Settings: Echo {Command[7]}";

                case HeaderCommands.JpegBackground:
                    return $"JPEG background camera related: {Command.Data1:X2} Overworld Region:  {(byte)Command.Data2:X2}";
            }
            return base.ToString();
        }
    }
}
