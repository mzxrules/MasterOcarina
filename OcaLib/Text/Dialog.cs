using System.Collections.Generic;
using System.Text;

namespace mzxrules.OcaLib.Text
{
    class Dialog
    {
        readonly StringBuilder Text = new();
        readonly List<IDialogItem> dialogItems = [];

        public void Append(IDialogItem item)
        {
            FlushText();
            dialogItems.Add(item);
        }
        public void Append(string str) => Text.Append(str);
        public void Append(char chr) => Text.Append(chr);

        public void FlushText()
        {
            if (Text.Length > 0)
            {
                dialogItems.Add(new TextNode(Text.ToString()));
                Text.Clear();
            }
        }

        public void Complete() => FlushText();

        internal string Print(DialogWriterSettings settings)
        {
            StringBuilder sb = new();
            foreach (IDialogItem item in dialogItems)
            {
                sb.Append(item.Print(settings));
            }
            return sb.ToString();
        }
    }    
}
