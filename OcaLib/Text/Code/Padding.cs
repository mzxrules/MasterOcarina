namespace mzxrules.OcaLib.Text.Code
{
    record Padding(byte Pixels) : IDialogItem
    {
        public string Print(DialogWriterSettings settings)
        {
            return $"[Pad:{Pixels:D3}]";
        }
    }
}
