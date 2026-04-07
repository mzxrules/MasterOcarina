namespace mzxrules.OcaLib.Text.Code
{
    record ItemIcon(byte Item) : IDialogItem
    {
        public string Print(DialogWriterSettings settings)
        {
            return $"[Item Icon {Item:X2}]";
        }
    }
}
