namespace mzxrules.OcaLib.Text.Code
{
    record CustomCode(ushort Id, int? Param = null) : IDialogItem
    {
        public string Print(DialogWriterSettings settings)
        {
            if (settings.VerboseMode)
            {
                return (Param == null) ? $"[{Id:X2}]" : $"[{Id:X2}:{Param:X4}]";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}

