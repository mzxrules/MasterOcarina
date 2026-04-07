namespace mzxrules.OcaLib.Text.Code
{
    record Background(int Value) : IDialogItem
    {
        public Background(byte a, byte b, byte c) : this(((a << 16) + (b << 8) + c) & 0xFFFFFF)
        {
        }
        public Background(ushort a, ushort b) : this(((a << 16) + b) & 0xFFFFFF)
        {
        }
        public string Print(DialogWriterSettings settings)
        {
            if (settings.VerboseMode)
                return $"[Background: {Value:X6}]";
            return "";
        }
    }
}
