namespace mzxrules.OcaLib.Text
{
    public class DialogWriterSettings(Rom.Language lang, DialogWriterSettings.OutputType style, bool verbose = true)
    {
        public Rom.Language Language { get; private set; } = lang;
        public OutputType OutputStyle { get; private set; } = style;
        public bool VerboseMode = verbose;

        public enum OutputType
        {
            CsvTable,
            Wiki,
            Textbox
        }
    }
}
