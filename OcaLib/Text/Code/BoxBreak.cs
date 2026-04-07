using System;

namespace mzxrules.OcaLib.Text.Code
{
    /// <summary>
    /// 0x04
    /// </summary>
    class BoxBreak : IDialogItem
    {
        public string Print(DialogWriterSettings settings)
        {
            return settings.OutputStyle switch
            {
                DialogWriterSettings.OutputType.Wiki => "<br><br>",
                _ => Environment.NewLine + Environment.NewLine,
            };
        }
    }
}
