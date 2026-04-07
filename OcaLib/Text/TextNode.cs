namespace mzxrules.OcaLib.Text
{
    class TextNode(string text) : IDialogItem
    {
        public string Text { get; set; } = text;

        public string Print(DialogWriterSettings settings) => Text;
    }
}
