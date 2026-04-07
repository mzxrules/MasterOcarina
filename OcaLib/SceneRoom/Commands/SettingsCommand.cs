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
            return (HeaderCommands)Code switch
            {
                HeaderCommands.WindSettings =>
                    $"Wind Settings: Forces Westward {Command[4]:X2}, Vertical {Command[5]:X2}, Southward {Command[6]:X2}, Strength {Command[7]:X2}",
                HeaderCommands.SpecialObject =>
                    $"Elf_Message {Command.Data1}, Load Object {Command.Data2:X4}",
                HeaderCommands.TimeSettings =>
                    $"Time Settings: {Command[4]:X2}{Command[5]:X2} {Command[6]:X2}",
                HeaderCommands.SkyboxSettings =>
                    string.Format("Skybox {0}, Cast: {1}, Fog? {2}",
                        Command[4],
                        (Command[5] == 1) ? "Cloudy" : "Sunny",
                        (Command[6] > 0) ? "Yes" : "No"),
                HeaderCommands.SkyboxModifier => $"Skybox Settings: Disable Sky? {Command.Data2 >> 24 > 0}, Disable Sun/Moon? {(Command.Data2 & 0xFF0000) >> 16 > 0}",
                HeaderCommands.SoundSettings => 
                    string.Format("Sound Settings: Reverb {0}, Playback option {1}, Song {2:X2}.",
                        Command.Data1,
                        (Command[6] == 0x13) ? "Always Playing" : $"{Command[6]:X2}",
                        Command[7]),
                HeaderCommands.SoundSettingsEcho => $"Sound Settings: Echo {Command[7]}",
                HeaderCommands.CameraAndWorldMap => $"JPEG background camera related: {Command.Data1:X2} Overworld Region:  {(byte)Command.Data2:X2}",
                _ => base.ToString(),
            };
        }
    }
}
