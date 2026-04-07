using System;

namespace mzxrules.OcaLib.Text.Code
{
    record LineBreak : IDialogItem
    {
        public string Print(DialogWriterSettings settings)
        {
            return settings.OutputStyle switch
            {
                DialogWriterSettings.OutputType.Wiki => "<br>",
                _ => Environment.NewLine,
            };
        }
    }
}
