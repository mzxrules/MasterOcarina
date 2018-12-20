using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace mzxrules.OcaLib
{
    partial class GameText
    {
        public class DialogWriterSettings
        {
            public Rom.Language Language { get; private set; }
            public OutputType OutputStyle { get; private set; }
            public bool VerboseMode = true;

            public enum OutputType
            {
                CsvTable,
                Wiki,
                Textbox
            }

            public DialogWriterSettings(Rom.Language lang, OutputType style, bool verbose = true)
            {
                Language = lang;
                OutputStyle = style;
                VerboseMode = verbose;
            }
        }

        public interface IDialogItem
        {
            string Print(DialogWriterSettings settings);
        }

        class Dialog
        {
            private StringBuilder Text = new StringBuilder();
            List<IDialogItem> dialogItems = new List<IDialogItem>();
            
            public void Append(IDialogItem item)
            {
                if (Text.Length > 0)
                {
                    dialogItems.Add(new TextNode(Text.ToString()));
                    Text.Clear();
                }
                dialogItems.Add(item);
            }
            public void Append(string text)
            {
                Text.Append(text);
            }
            public void Append(char p)
            {
                Text.Append(p);
            }

            public void Complete()
            {
                if (Text.Length > 0)
                {
                    dialogItems.Add(new TextNode(Text.ToString()));
                    Text.Clear();
                }
            }

            internal string Print(DialogWriterSettings settings)
            {
                StringBuilder sb = new StringBuilder();
                foreach (IDialogItem item in dialogItems)
                {
                    sb.Append(item.Print(settings));
                }
                return sb.ToString();
            }

        }

        class TextNode : IDialogItem
        {
            public string Text { get; set; }

            public TextNode(string p)
            {
                Text = p;
            }

            public string Print(DialogWriterSettings settings)
            {
                return Text;
            }
        }
        /// <summary>
        /// 0x02
        /// </summary>
        class LineBreak : IDialogItem
        {
            public string Print(DialogWriterSettings settings)
            {
                switch (settings.OutputStyle)
                {
                    case DialogWriterSettings.OutputType.Wiki: return "<br>";
                    default: return Environment.NewLine;
                }
            }
        }

        /// <summary>
        /// 0x04
        /// </summary>
        class BoxBreak : IDialogItem
        {
            public string Print(DialogWriterSettings settings)
            {
                switch (settings.OutputStyle)
                {
                    case DialogWriterSettings.OutputType.Wiki: return "<br><br>";
                    default: return Environment.NewLine + Environment.NewLine;
                }

            }
        }

        class CustomCode : IDialogItem
        {
            public ushort Id { get; private set; }
            int? parameter = null;

            public CustomCode(ushort id, int? p = null)
            {
                Id = id;
                parameter = p;
            }

            public string Print(DialogWriterSettings settings)
            {
                if (settings.VerboseMode)
                {
                    return (parameter == null) ? $"[{Id:X2}]" : $"[{Id:X2}:{parameter:X4}]";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        class GotoDialog : IDialogItem
        {
            public ushort Dialog { get; private set; }
            public GotoDialog(ushort d)
            {
                Dialog = d;
            }
            public string Print(DialogWriterSettings settings)
            {
                return String.Format("[goto {0:X4}]", Dialog);
            }
        }

        /// <summary>
        /// 0x05
        /// </summary>
        class SetTextColor: IDialogItem
        {
            public enum ColorCode
            {
                White = 0,
                Red = 1,
                Green = 2,
                Blue = 3,
                Cyan = 4,
                Pink = 5,
                Yellow = 6,
                Black = 7,
                Undefined
            }
            public ColorCode Color { get; private set; }
            private byte colorId;

            public SetTextColor(byte value)
            {
                colorId = value;
                if (0 <= colorId && colorId < 7)
                    Color = (ColorCode)colorId;
                else
                    Color = ColorCode.Undefined;

            }

            public string Print(DialogWriterSettings settings)
            {
                string code;
                if (Color == ColorCode.Undefined)
                {
                    code = $" {colorId:X2}";
                }
                else
                {
                    code = Color.ToString().Substring(0, 3);
                }
                return $"[c:{code}]";
            }
        }

        /// <summary>
        /// 0x06
        /// </summary>
        class Padding : IDialogItem
        {
            public byte Pixels { get; private set; }
            public Padding(byte b)
            {
                Pixels = b;
            }
            public string Print(DialogWriterSettings settings)
            {
                //switch (settings.OutputStyle)
                //{
                //    case DialogWriterSettings.OutputType.Wiki:
                //        {
                //            string r = string.Empty;
                //            for (int i = 0; i < Spaces; i++)
                //            {
                //                r += "&nbsp;";
                //            }
                //            return r;
                //        }
                //    default: return String.Empty.PadRight(Spaces/2);

                //}
                return $"[Pad:{Pixels:D3}]";
            }
        }

        /// <summary>
        /// 0x12
        /// </summary>
        class Sound : IDialogItem
        {
            public ushort SfxId {get; private set;}
            public Sound(ushort id)
            {
                SfxId = id;
            }
            public string Print(DialogWriterSettings settings)
            {
                string desc;
                switch (SfxId)
                {
                    case 0x0858: desc = "item fanfare"; break;
                    case 0x28E3: desc = "frog ribbit"; break;
                    case 0x28E4: desc = "frog ribbit"; break;
                    case 0x3880: desc = "deku squeak"; break;
                    case 0x3882: desc = "deku cry"; break;
                    case 0x38EC: desc = "Generic event"; break;
                    case 0x4807: desc = "Poe vanishing"; break;
                    case 0x486F: desc = "Twinrova"; break;
                    case 0x5965: desc = "Twinrova"; break;
                    case 0x6844: desc = "Navi hello"; break;
                    case 0x6852: desc = "Talon Ehh?"; break;
                    case 0x6855: desc = "Carpenter WAAAAA!"; break;
                    case 0x685F: desc = "Navi HEY!"; break;
                    case 0x6863: desc = "Saria giggle"; break;
                    case 0x6867: desc = "YAAAAAAA!"; break;
                    case 0x6869: desc = "Zelda heh"; break;
                    case 0x686B: desc = "Zelda awww"; break;
                    case 0x686C: desc = "Zelda huh"; break;
                    case 0x686D: desc = "Generic giggle"; break;
                    case 0x6864: desc = "??? used"; break;
                    default: desc = "Custom"; break;
                }
                return string.Format("[sfx {0:X4}:{1}]", SfxId, desc);
            }
        }

        /// <summary>
        /// 0x13
        /// </summary>
        class ItemIcon : IDialogItem
        {
            public byte Item { get; private set; }
            public ItemIcon(byte item)
            {
                Item = item;
            }
            public string Print(DialogWriterSettings settings)
            {
                return $"[Item Icon {Item:X2}]";
            }
        }

        class Background: IDialogItem
        {
            public int Value { get; private set; }
            public Background(byte a, byte b, byte c)
            {
                Value = ((a << 16) + (b << 8) + c) & 0xFFFFFF;
            }
            public Background(ushort a, ushort b)
            {
                Value = ((a << 16) + b) & 0xFFFFFF;
            }
            public string Print(DialogWriterSettings settings)
            {
                if (settings.VerboseMode)
                    return $"[Background: {Value:X6}]";
                return "";
            }
        }
    }
}
