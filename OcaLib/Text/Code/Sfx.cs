namespace mzxrules.OcaLib.Text.Code
{
    record Sfx(ushort SfxId) : IDialogItem
    {
        public string Print(DialogWriterSettings settings)
        {
            string desc = SfxId switch
            {
                0x0858 => "item fanfare",
                0x28E3 => "frog ribbit",
                0x28E4 => "frog ribbit",
                0x3880 => "deku squeak",
                0x3882 => "deku cry",
                0x38EC => "Generic event",
                0x4807 => "Poe vanishing",
                0x486F => "Twinrova",
                0x5965 => "Twinrova",
                0x6844 => "Navi hello",
                0x6852 => "Talon Ehh?",
                0x6855 => "Carpenter WAAAAA!",
                0x685F => "Navi HEY!",
                0x6863 => "Saria giggle",
                0x6867 => "YAAAAAAA!",
                0x6869 => "Zelda heh",
                0x686B => "Zelda awww",
                0x686C => "Zelda huh",
                0x686D => "Generic giggle",
                0x6864 => "??? used",
                _ => "Custom",
            };
            return string.Format("[sfx {0:X4}:{1}]", SfxId, desc);
        }
    }
}
