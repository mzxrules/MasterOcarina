namespace mzxrules.OcaLib.Text.Code
{
    record GotoDialog(ushort MesgId) : IDialogItem
    {
        public string Print(DialogWriterSettings settings)
        {
            return $"[goto {MesgId:X4}]";
        }
    }
}
