using mzxrules.Helper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mzxrules.OcaLib
{
    public partial class GameText
    {
        ORom rom;
        Dictionary<Rom.Language, Dictionary<ushort, TextboxMessage>> Dialogs;

        public GameText(ORom v)
        {
            rom = v;

            if (ORom.GetLocalization(v.Version) == ORom.Localization.NTSC)
                LoadNtsc();
            else
                LoadPal();
        }

        private void LoadPal()
        {
            Dialogs = new Dictionary<Rom.Language, Dictionary<ushort, TextboxMessage>>();

            if (Addresser.TryGetRom(ORom.FileList.code, rom.Version, "TextbankTable", out int textbank))
            {
                RomFile codefile = rom.Files.GetFile(ORom.FileList.code);
                using (BinaryReader file = new BinaryReader(codefile.Stream))
                {
                    textbank = (int)codefile.Record.GetRelativeAddress(textbank);
                    file.BaseStream.Position = textbank;

                    LoadTextTable(file, Rom.Language.English);
                    LoadTextTable_Pal2(file, Dialogs[Rom.Language.English], Rom.Language.German);
                    LoadTextTable_Pal2(file, Dialogs[Rom.Language.English], Rom.Language.French);
                }
            }
        }

        private void LoadTextTable_Pal2(BinaryReader code, Dictionary<ushort, TextboxMessage> nes_message, Rom.Language language)
        {
            uint current, next;
            Dictionary<ushort, TextboxMessage> TextBank = new Dictionary<ushort, TextboxMessage>();

            next = code.ReadBigUInt32() & 0xFFFFFF;

            var nes_message_query = from x in nes_message.Values
                                    orderby x.Id ascending
                                    select x;

            foreach (TextboxMessage item in nes_message_query)
            {
                current = next;
                next = code.ReadBigUInt32();

                if ((next & 0xFFFFFF) == 0)
                    break;

                TextBank.Add(item.Id, new TextboxMessage(item.Message, current, next));

            }
            Dialogs.Add(language, TextBank);
        }

        private void LoadNtsc()
        {
            Dialogs = new Dictionary<ORom.Language, Dictionary<ushort, TextboxMessage>>();

            if (Addresser.TryGetRom(ORom.FileList.code, rom.Version, "TextbankTable", out int textbank))
            {
                using (BinaryReader file = new BinaryReader(rom.Files.GetFile(ORom.FileList.code)))
                {
                    textbank = (int)rom.Files.GetFileStart(textbank).GetRelativeAddress(textbank);
                    file.BaseStream.Position = textbank;

                    LoadTextTable(file, Rom.Language.Japanese);
                    LoadTextTable(file, Rom.Language.English);
                }
            }
        }

        public IEnumerable<ushort> GetMessageEnumerator(Rom.Language lang)
        {
            return Dialogs[lang].Keys.AsEnumerable();
        }

        private void LoadTextTable(BinaryReader file, Rom.Language lang)
        {
            List<TextboxMessage> messages = new List<TextboxMessage>();
            Dictionary<ushort, TextboxMessage> TextBank = new Dictionary<ushort, TextboxMessage>();
            
            InitializeTextBank(TextBank, file);
            Dialogs.Add(lang, TextBank);
        }

        private void InitializeTextBank(Dictionary<ushort, TextboxMessage> TextBank, BinaryReader file)
        {
            MessageSettings current, next;
            uint currBankOff, nextBankOff;

            next = new MessageSettings(file);
            nextBankOff = file.ReadBigUInt32();
            do
            {
                current = next;
                currBankOff = nextBankOff;

                next = new MessageSettings(file);
                nextBankOff = file.ReadBigUInt32();

                if (next.Id != 0xFFFF)
                    TextBank.Add(current.Id, new TextboxMessage(current, currBankOff, nextBankOff));
            }
            while (next.Id != 0xFFFF);
        }

        /// <summary>
        /// Returns the raw data for a single message as a list of integers
        /// </summary>
        /// <param name="id">message id</param>
        /// <param name="language">language</param>
        /// <returns>the message data as a list of integers</returns>
        public List<int> GetMessageData(ushort id, Rom.Language language)
        {
            TextboxMessage mesg = Dialogs[language][id];
            List<int> result = new List<int>();

            using (BinaryReader message_data_static = new BinaryReader(rom.Files.GetMessageFile(language)))
            {
                message_data_static.BaseStream.Position = mesg.StartOffset;

                for (int i = 0; i < (mesg.EndOffset - mesg.StartOffset) / 4; i++)
                {
                    result.Add(message_data_static.ReadBigInt32());
                }
            }
            return result;
        }

        public string GetMessage(ushort id, Rom.Language language)
        {
            id = (Dialogs[language].ContainsKey(id)) ? id : (ushort)1;

            return GetMessage(Dialogs[language][id], language);
        }

        /// <summary>
        /// Gets the offset to the message relative to the start of the file
        /// </summary>
        public string GetMessageOffset(ushort id, Rom.Language language)
        {
            id = (Dialogs[language].ContainsKey(id)) ? id : (ushort)1;
            TextboxMessage mesg = Dialogs[language][id];
            return $"{mesg.StartOffset:X6}:{mesg.EndOffset:X6}";
        }

        private string GetMessage(TextboxMessage textboxMessage, Rom.Language language)
        {
            Dialog dialog;
            byte[] data;

            
            using (BinaryReader message_data_static = new BinaryReader(rom.Files.GetMessageFile(language)))
            {
                message_data_static.BaseStream.Position = textboxMessage.StartOffset;
                data = message_data_static.ReadBytes((int)textboxMessage.Length);
            }

            if (language == Rom.Language.Japanese)
                dialog = JpnEscapeCustomCodes(data);
            else
                dialog = NesEscapeCustomCodes(data);

            var settings = new DialogWriterSettings(language,
                        DialogWriterSettings.OutputType.Textbox); 

            return dialog.Print(settings);
        }
        
        private static Dialog NesEscapeCustomCodes(byte[] s)
        {
            Dialog dialog = new Dialog();

            for (int i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    //[00]		---
                    case 0x00: break;
                    //[01]		line break
                    case 0x01:
                        dialog.Append(new LineBreak());
                        break;
                    //[02]		end marker
                    case 0x02: dialog.Complete(); i = s.Length; break;
                    //[03]		---
                    //[04]		wait for keypress / box break
                    case 0x04:
                        dialog.Append(new BoxBreak());
                        break;
                    //[05 xx]		use text color xx
                    case 0x05:
                        i++;
                        dialog.Append(new SetTextColor((byte)(s[i] & 0x0F)));
                        break;
                    //  xx: 40 = white
                    //      41 = red
                    //      42 = green
                    //      43 = blue
                    //      44 = light blue
                    //      45 = pink
                    //      46 = yellow
                    //      47 = black
                    //[06 xx]		print xx spaces
                    case 0x06:
                        i++;
                        dialog.Append(new Padding(s[i]));
                        break;
                    //[07 xx xx]	continue with message with id xxxx
                    case 0x07: i++; dialog.Append(new GotoDialog(Endian.ConvertUShort(s, i))); i++; break;
                    //[08]		pring following text instantly
                    case 0x08: dialog.Append(new CustomCode(0x08)); break;
                    //[09]		disable 08, instant text printing
                    case 0x09: dialog.Append(new CustomCode(0x09)); break;
                    //[0a]		keep box opened, no reaction to keypresses (used in shop item descriptions)
                    case 0x0A: dialog.Append(new CustomCode(0x0A)); break;
                    //[0b]		(wait for external action? used in minigame texts)
                    case 0x0B: dialog.Append(new CustomCode(0x0B)); break;
                    //[0c xx]		delay text printing by xx
                    case 0x0C: i++; dialog.Append(new CustomCode(0x0C, s[i])); break;
                    //[0d]		wait for keypress / continue in same box/line
                    case 0x0D: dialog.Append(new CustomCode(0x0D)); break;
                    //[0e xx]		fade out interface and wait until xx maxes out(??), ignore all following text
                    case 0x0E: i++; dialog.Append(new CustomCode(0x0E, s[i])); break;
                    //[0f]		show player name
                    case 0x0F: dialog.Append("[Link]"); break;
                    //[10]		init ocarina playing
                    case 0x10: dialog.Append(new CustomCode(0x10)); break;
                    //[11]		(fade out interface and wait, ignore all following text, no parameters?)
                    case 0x11: dialog.Append(new CustomCode(0x11)); break;
                    //[12 xx xx]	play sound xx xx
                    case 0x12: i++; dialog.Append(new Sound(Endian.ConvertUShort(s, i))); i++; break;
                    //[13 xx]		show item icon xx
                    case 0x13: i++; dialog.Append(new ItemIcon(s[i])); break;
                    //  xx: 00 = deku stick
                    //      01 = deku nut
                    //      ...
                    //[14 xx]		delay printing of each letter by xx
                    case 0x14: i++; dialog.Append(new CustomCode(0x14, s[i])); break;
                    //[15 xx yy zz]	load box background image (valid values unknown, 00 20 00 gives red 'X' mark)
                    case 0x15: dialog.Append(new Background(s[i + 1], s[i + 2], s[i + 3])); i += 3; break;
                    //[16]		(show result time of something?)
                    case 0x16: dialog.Append("[Marathon Time]"); break;
                    //[17]		(show result time of something else?)
                    case 0x17: dialog.Append("[Race Time]"); break;
                    //[18]		(show result count of something?)
                    case 0x18: dialog.Append("[Points]"); break;
                    //[19]		show golden skulltula count
                    case 0x19: dialog.Append("[Gold Skulltulas]"); break;
                    //[1a]		following text can't be skipped with B button
                    case 0x1A: dialog.Append("[No Skip]"); break;
                    //[1b]		init two-choice answer selection (i.e. yes/no arrow)
                    case 0x1B: dialog.Append(new CustomCode(0x1B)); break;
                    //[1c]		init three-choice answer selection
                    case 0x1C: dialog.Append(new CustomCode(0x1C)); break;
                    //[1d]		(show result count of something?)
                    case 0x1D: dialog.Append("[Fish Score]"); break;
                    //[1e xx]		show minigame result xx
                    case 0x1E: i++; dialog.Append(DisplayValue(s[i])); break;
                    //  xx: 00 = horseback archery points
                    //      02 = largest fish caught
                    //      03 = horse race time
                    //      04 = marathon time
                    //      06 = dampé race time
                    //      ...?
                    //[1f]		show current hyrule time
                    case 0x1F: dialog.Append("[World Time]"); break;
                    default: dialog.Append(ConvertSpecialChars(s[i])); break;
                }
            }
            return dialog;
        }

        private static Dialog JpnEscapeCustomCodes(byte[] data)
        {
            Dialog dialog = new Dialog();
            StringBuilder sb = new StringBuilder();
            GetUnescapedJpnChars(data, out ushort[] chrRaw, out char[] chr);

            for (int i = 0; i < chrRaw.Length; i++)
            {
                switch (chrRaw[i])
                {
                    #region SpecialChars
                    case 0x839F: sb.Append(ConvertSpecialChars((byte)chrRaw[i])); break;
                    case 0x83A0: goto case 0x839F;// "[B]";
                    case 0x83A1: goto case 0x839F;// "[C]";
                    case 0x83A2: goto case 0x839F;// "[L]";
                    case 0x83A3: goto case 0x839F;// "[R]";
                    case 0x83A4: goto case 0x839F;// "[Z]";
                    case 0x83A5: goto case 0x839F;// "[C-Up]";
                    case 0x83A6: goto case 0x839F;// "[C-Down]";
                    case 0x83A7: goto case 0x839F;// "[C-Left]";
                    case 0x83A8: goto case 0x839F;// "[C-Right]";
                    case 0x83A9: goto case 0x839F;// "[Triangle]";
                    case 0x83AA: goto case 0x839F;// "[Control Stick]";
                    case 0x83AB: goto case 0x839F;// "[D-Pad]";
                    #endregion

                    //[00]		---

                    //[01]		line break
                    case 0x000A:
                        dialog.Append(new LineBreak()); break;

                    //[02]		end marker
                    case 0x8170: i = chr.Length; dialog.Complete(); break;
                    //[03]		---
                    //[04]		wait for keypress / box break
                    case 0x81A5:
                        dialog.Append(new BoxBreak()); break;
                    //[05 xx]		use text color xx
                    case 0x000B: i++; dialog.Append(new SetTextColor((byte)chrRaw[i])); break;
                    //[06 xx]		print xx spaces
                    case 0x86C7: i++; dialog.Append(new CustomCode(0x86C7, chrRaw[i])); break;
                    //[07 xx xx]	continue with message with id xx xx (ex. 033a)
                    case 0x81CB: dialog.Append(new GotoDialog(chrRaw[++i])); break;
                    //[08]		pring following text instantly
                    case 0x8189: dialog.Append(new CustomCode(0x8189)); break;
                    //[09]		disable 08, instant text printing
                    case 0x818A: dialog.Append(new CustomCode(0x818A)); break;
                    //[0a]		keep box opened, no reaction to keypresses (used in shop item descriptions)
                    case 0x86C8: dialog.Append(new CustomCode(chrRaw[i])); break;
                    //[0b]		(wait for external action? used in minigame texts)
                    case 0x819F: dialog.Append(new CustomCode(chrRaw[i])); break;
                    //[0c xx]		delay text printing by xx
                    case 0x81A3: dialog.Append(new CustomCode(chrRaw[i], chrRaw[i + 1])); i++; break;
                    //[0d]		wait for keypress / continue in same box/line

                    //[0e xx]		fade out interface and wait until xx maxes out(??), ignore all following text
                    case 0x819E: dialog.Append(new CustomCode(chrRaw[i], chrRaw[i + 1])); i++; break;
                    //[0f]		show player name
                    case 0x874F: dialog.Append("[Link]"); break;
                    //[10]		init ocarina playing
                    case 0x81F0: dialog.Append(new CustomCode(chrRaw[i])); break;
                    //[11]		(fade out interface and wait, ignore all following text, no parameters?)

                    //[12 xx xx]	play sound xx xx
                    case 0x81F3: i++; dialog.Append(new Sound(chrRaw[i])); break;
                    //[13 xx]		show item icon xx
                    case 0x819A: i++; dialog.Append(new ItemIcon((byte)chrRaw[i])); break;
                    //  xx: 00 = deku stick
                    //      01 = deku nut
                    //      ...
                    //[14 xx]		delay printing of each letter by xx
                    case 0x86C9: dialog.Append(new CustomCode(chrRaw[i], chrRaw[i + 1])); i++; break;
                    //[15 xx yy zz]	load box background image (valid values unknown, 00 20 00 gives red 'X' mark)
                    case 0x86B3: dialog.Append(new Background(chrRaw[i], chrRaw[i + 1])); i++; break;
                    //[16]		(show result time of something?)
                    case 0x8791: dialog.Append("[Marathon Time]"); break;
                    //[17]		(show result time of something else?)
                    case 0x8792: dialog.Append("[Race Time]"); break;
                    //[18]		(show result count of something?)
                    case 0x879B: dialog.Append("[Points]"); break;
                    //[19]		show golden skulltula count
                    case 0x86A3: dialog.Append("[Gold Skulltulas]"); break;
                    //[1a]		following text can't be skipped with B button
                    case 0x8199: dialog.Append("[No Skip]"); break;
                    //[1b]		init two-choice answer selection (i.e. yes/no arrow)
                    case 0x81BC: dialog.Append(new CustomCode(chrRaw[i])); break;
                    //[1c]		init three-choice answer selection
                    case 0x81B8: dialog.Append(new CustomCode(chrRaw[i])); break;
                    //[1d]		(show result count of something?)
                    case 0x86A4: dialog.Append("[Fish Score]"); break;
                    //[1e xx]		show minigame result xx
                    case 0x869F: i++; dialog.Append(DisplayValue((byte)chrRaw[i])); break;
                    //  xx: 00 = horseback archery points
                    //      02 = largest fish caught
                    //      03 = horse race time
                    //      04 = marathon time
                    //      06 = dampé race time
                    //      ...?
                    //[1f]		show current hyrule time
                    case 0x81A1: dialog.Append("[World Time]"); break;
                    default:
                        dialog.Append(chr[i]);
                        break;
                }
            }
            return dialog;
        }

        private static void GetUnescapedJpnChars(byte[] data, out ushort[] rawChars, out char[] convertedChars)
        {
            Decoder shift_jis_dec;
            Encoding shift_jis_enc;

            rawChars = new ushort[data.Length / 2];
            convertedChars = new char[data.Length / 2];
            shift_jis_enc = Encoding.GetEncoding("shift-jis");
            shift_jis_dec = shift_jis_enc.GetDecoder();

            for (int i = 0; i < convertedChars.Length; i++)
                shift_jis_dec.Convert(data, i * 2, 2, convertedChars, i, 1, false, out int bytesUsed, out int charsUsed, out bool completed);

            for (int i = 0; i < rawChars.Length; i++)
            {
                rawChars[i] = Endian.ConvertUShort(data, i * 2);
            }
        }

        private static string DisplayValue(byte p)
        {
            switch (p)
            {
                case 00: return "[Horseback Archery Score]";
                case 02: return "[Largest Fish]";
                case 03: return "[Horse Race Time]";
                case 04: return "[Marathon Time]";
                case 06: return "[Dampé Race Time]";
                default: return "[!?]";
            }
        }

        public class TextboxMessage
        {
            public MessageSettings Message { get; set; }
            public uint BankStart { get; set; }
            public uint BankEnd { get; set; }

            public ushort Id { get { return Message.Id; } }
            public byte Box { get { return Message.Box; } }
            public uint Length { get { return EndOffset - StartOffset; } }
            public uint StartOffset
            {
                get { return BankStart & 0xFFFFFF; }
                set { BankStart = (BankStart & 0xFF000000) + (value & 0xFFFFFF); }
            }
            public uint EndOffset
            {
                get { return BankEnd & 0xFFFFFF; }
                set { BankEnd = (BankEnd & 0xFF000000) + (value & 0xFFFFFF); }
            }

            public TextboxMessage(MessageSettings mesg, uint start, uint end)
            {
                Message = mesg;
                BankStart = start;
                BankEnd = end;
            }
        }

        public class MessageSettings
        {
            public ushort Id;
            public byte Box;
            public byte Unused;

            public MessageSettings(BinaryReader br)
            {
                Id = br.ReadBigUInt16();
                Box = br.ReadByte();
                Unused = br.ReadByte();
            }
        }

        
        /// <summary>
        /// Gets a text representation of a color code
        /// </summary>
        private static string GetColorCode(byte p)
        {
            string code;
            switch (p)
            {
                case 00: code = "wh"; break; //white
                case 01: code = "re"; break; //red
                case 02: code = "ge"; break; //green
                case 03: code = "bu"; break; //blue
                case 04: code = "cy"; break; //light blue
                case 05: code = "pi"; break; //pink
                case 06: code = "ye"; break; //yellow
                case 07: code = "ba"; break; //black
                default: code = $"{p:X2}"; break;
            }
            return $"[c:{code}]";
        }

        static string ConvertSpecialChars(byte s)
        {
            switch (s)
            {
                case 0x9F: return "[A]";
                case 0xA0: return "[B]";
                case 0xA1: return "[C]";
                case 0xA2: return "[L]";
                case 0xA3: return "[R]";
                case 0xA4: return "[Z]";
                case 0xA5: return "[C-Up]";
                case 0xA6: return "[C-Down]";
                case 0xA7: return "[C-Left]";
                case 0xA8: return "[C-Right]";
                case 0xA9: return "[Triangle]";
                case 0xAA: return "[Control Stick]";
                case 0xAB: return "[D-pad]";
                default:
                    return (s > 0x9F) ? $"[!! {s:X2}]" : ((char)s).ToString();
            }
        }

        //old parser code
        //private static string Old_JpnEscapeCustomCodes(byte[] data)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    Decoder shift_jis_dec;
        //    Encoding shift_jis_enc;

        //    ushort[] chars = new ushort[data.Length / 2];
        //    char[] c = new char[data.Length / 2];

        //    int bytesUsed, charsUsed;
        //    bool completed;

        //    shift_jis_enc = Encoding.GetEncoding("shift-jis");
        //    shift_jis_dec = shift_jis_enc.GetDecoder();

        //    for (int i = 0; i < c.Length; i++)
        //        shift_jis_dec.Convert(data, i * 2, 2, c, i, 1, false, out bytesUsed, out charsUsed, out completed);

        //    for (int i = 0; i < chars.Length; i++)
        //    {
        //        chars[i] = Endian.ConvertUShort(data, i * 2);
        //    }

        //    for (int i = 0; i < chars.Length; i++)
        //    {
        //        switch (chars[i])
        //        {
        //            #region SpecialChars
        //            case 0x839F: sb.Append(ConvertSpecialChars((byte)chars[i])); break;
        //            case 0x83A0: goto case 0x839F;// "[B]";
        //            case 0x83A1: goto case 0x839F;// "[C]";
        //            case 0x83A2: goto case 0x839F;// "[L]";
        //            case 0x83A3: goto case 0x839F;// "[R]";
        //            case 0x83A4: goto case 0x839F;// "[Z]";
        //            case 0x83A5: goto case 0x839F;// "[C-Up]";
        //            case 0x83A6: goto case 0x839F;// "[C-Down]";
        //            case 0x83A7: goto case 0x839F;// "[C-Left]";
        //            case 0x83A8: goto case 0x839F;// "[C-Right]";
        //            case 0x83A9: goto case 0x839F;// "[Triangle]";
        //            case 0x83AA: goto case 0x839F;// "[Control Stick]";
        //            case 0x83AB: goto case 0x839F;// "[D-Pad]";
        //            #endregion

        //            //[00]		---

        //            //[01]		line break
        //            case 0x000A:
        //                //sb.Append("<br>"); break;
        //                sb.AppendLine(); break;

        //            //[02]		end marker
        //            case 0x8170: i = c.Length; break;
        //            //[03]		---
        //            //[04]		wait for keypress / box break
        //            case 0x81a5:
        //                //sb.AppendFormat("<br>{0}<br>", c[i]); break;
        //                sb.AppendLine(); sb.AppendLine(c[i].ToString()); break;
        //            //[05 xx]		use text color xx
        //            case 0x000B: i++; sb.Append(GetColorCode((byte)chars[i])); break;
        //            //  xx: 00 = white
        //            //      01 = red
        //            //      02 = green
        //            //      03 = blue
        //            //      04 = light blue
        //            //      05 = pink
        //            //      06 = yellow
        //            //      07 = black
        //            //[06 xx]		print xx spaces
        //            case 0x86C7: i++; break;
        //            //[07 xx xx]	continue with message with id xx xx (ex. 033a)
        //            case 0x81CB: sb.AppendFormat("[{0} mesg {1:X4}]", c[i], chars[++i]); break;
        //            //[08]		pring following text instantly
        //            case 0x8189: break;
        //            //[09]		disable 08, instant text printing
        //            case 0x818a: break;
        //            //[0a]		keep box opened, no reaction to keypresses (used in shop item descriptions)

        //            //[0b]		(wait for external action? used in minigame texts)

        //            //[0c xx]		delay text printing by xx

        //            //[0d]		wait for keypress / continue in same box/line

        //            //[0e xx]		fade out interface and wait until xx maxes out(??), ignore all following text

        //            //[0f]		show player name
        //            case 0x874F: sb.Append("[Link]"); break;
        //            //[10]		init ocarina playing
        //            //case 0x10: i++; break;
        //            //[11]		(fade out interface and wait, ignore all following text, no parameters?)
        //            //case 0x11: i++; break;
        //            //[12 xx xx]	play sound xx xx
        //            case 0x81F3: i++; sb.AppendFormat("[sound {0:X4}]", chars[i]); break;
        //            //  xx xx: 0858 item fanfare
        //            //         28E3 frog ribbit sound
        //            //         28E4 frog ribbit sound
        //            //         3880 deku squeak
        //            //         3882 deku cry
        //            //         38EC Generic event
        //            //         4807 Poe vanishing
        //            //         486F Twinrova
        //            //         5965 Twinrova
        //            //         6844 Navi hello
        //            //         6852 Talon Ehh?
        //            //         6855 Carpenter WAAAAA!
        //            //         685F Navi HEY!
        //            //         6863 Saria giggle
        //            //         6867 YAAAAAAA!
        //            //         6869 Zelda heh
        //            //         686B Zelda awww
        //            //         686C Zelda huh
        //            //         686D Generic giggle
        //            //         6864 ??? used
        //            //         (values documented by OriginalLink)
        //            //[13 xx]		show item icon xx
        //            case 0x819A: i++; sb.AppendFormat("[Item Icon {0:X2}]", chars[i]); break;
        //            //  xx: 00 = deku stick
        //            //      01 = deku nut
        //            //      ...
        //            //[14 xx]		delay printing of each letter by xx
        //            case 0x86C9: i++; sb.Append($"[Delay {chars[i]:D2}]"); break;
        //            //case 0x14: i += 2; break;
        //            //[15 xx yy zz]	load box background image (valid values unknown, 00 20 00 gives red 'X' mark)
        //            //case 0x15: i += 4; break;
        //            //[16]		(show result time of something?)
        //            //case 0x16: i++; break;
        //            //[17]		(show result time of something else?)
        //            //case 0x17: i++; break;
        //            //[18]		(show result count of something?)
        //            //case 0x18: i++; break;
        //            //[19]		show golden skulltula count
        //            //case 0x19: i++; sb.Append("[Gold Skulltulas]"); break;
        //            //[1a]		following text can't be skipped with B button
        //            case 0x8199: break;
        //            //[1b]		init two-choice answer selection (i.e. yes/no arrow)
        //            //case 0x1B: i++; break;
        //            //[1c]		init three-choice answer selection
        //            //case 0x1C: i++; break;
        //            //[1d]		(show result count of something?)
        //            //case 0x1D: i++; break;
        //            //[1e xx]		show minigame result xx
        //            //case 0x1E: i += 2; break;
        //            //  xx: 00 = horseback archery points
        //            //      02 = largest fish caught
        //            //      03 = horse race time
        //            //      04 = marathon time
        //            //      06 = dampé race time
        //            //      ...?
        //            //[1f]		show current hyrule time
        //            //case 0x1F: i++; sb.Append("[World Time]"); break;
        //            default:
        //                sb.Append(c[i]);
        //                break;
        //        }
        //    }
        //    return sb.ToString();
        //}


        //public static string NesEscapeCustomCodes(byte[] s)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    for (int i = 0; i < s.Length; i++)
        //    {
        //        switch (s[i])
        //        {
        //            //[00]		---
        //            case 0x00: break;
        //            //[01]		line break
        //            case 0x01:
        //                //sb.Append("<br>");
        //                sb.AppendLine();
        //                break;
        //            //[02]		end marker
        //            case 0x02: i = s.Length; break;
        //            //[03]		---
        //            //[04]		wait for keypress / box break
        //            case 0x04:
        //                //sb.Append("<br><br>");
        //                sb.AppendLine(); sb.AppendLine();
        //                break;
        //            //[05 xx]		use text color xx
        //            case 0x05: i++; 
        //                sb.Append(ColorCode((byte)(s[i]&0x0F)));
        //                break;
        //            //  xx: 40 = white
        //            //      41 = red
        //            //      42 = green
        //            //      43 = blue
        //            //      44 = light blue
        //            //      45 = pink
        //            //      46 = yellow
        //            //      47 = black
        //            //[06 xx]		print xx spaces
        //            case 0x06: i++; /*sb.Append(String.Empty.PadRight(s[i]));*/ break;
        //            //[07 xx xx]	continue with message with id xxxx
        //            case 0x07: i++; sb.AppendFormat("[goto {0:X4}]", Endian.ConvertUShort(BitConverter.ToUInt16(s, i))); i++; break;
        //            //[08]		pring following text instantly
        //            case 0x08: break;
        //            //[09]		disable 08, instant text printing
        //            case 0x09: break;
        //            //[0a]		keep box opened, no reaction to keypresses (used in shop item descriptions)
        //            case 0x0A: break;
        //            //[0b]		(wait for external action? used in minigame texts)
        //            case 0x0B: sb.Append("[0B]"); break;
        //            //[0c xx]		delay text printing by xx
        //            case 0x0C: i++; break;
        //            //[0d]		wait for keypress / continue in same box/line
        //            case 0x0D: break;
        //            //[0e xx]		fade out interface and wait until xx maxes out(??), ignore all following text
        //            case 0x0E: i++; break;
        //            //[0f]		show player name
        //            case 0x0F: sb.Append("[Link]"); break;
        //            //[10]		init ocarina playing
        //            case 0x10: break;
        //            //[11]		(fade out interface and wait, ignore all following text, no parameters?)
        //            case 0x11: break;
        //            //[12 xx xx]	play sound xx xx
        //            case 0x12: i++; sb.AppendFormat("[sfx {0:X4}]", Endian.ConvertUShort(BitConverter.ToUInt16(s, i))); i++; break;
        //            //  xx xx: 0858 item fanfare
        //            //         28E3 frog ribbit sound
        //            //         28E4 frog ribbit sound
        //            //         3880 deku squeak
        //            //         3882 deku cry
        //            //         38EC Generic event
        //            //         4807 Poe vanishing
        //            //         486F Twinrova
        //            //         5965 Twinrova
        //            //         6844 Navi hello
        //            //         6852 Talon Ehh?
        //            //         6855 Carpenter WAAAAA!
        //            //         685F Navi HEY!
        //            //         6863 Saria giggle
        //            //         6867 YAAAAAAA!
        //            //         6869 Zelda heh
        //            //         686B Zelda awww
        //            //         686C Zelda huh
        //            //         686D Generic giggle
        //            //         6864 ??? used
        //            //         (values documented by OriginalLink)
        //            //[13 xx]		show item icon xx
        //            case 0x13: i++; sb.AppendFormat("[Item Icon {0:X2}]", s[i]); break;
        //            //  xx: 00 = deku stick
        //            //      01 = deku nut
        //            //      ...
        //            //[14 xx]		delay printing of each letter by xx
        //            case 0x14: i++; break;
        //            //[15 xx yy zz]	load box background image (valid values unknown, 00 20 00 gives red 'X' mark)
        //            case 0x15: i += 3; break;
        //            //[16]		(show result time of something?)
        //            case 0x16: break;
        //            //[17]		(show result time of something else?)
        //            case 0x17: break;
        //            //[18]		(show result count of something?)
        //            case 0x18: break;
        //            //[19]		show golden skulltula count
        //            case 0x19: sb.Append("[Gold Skulltulas]"); break;
        //            //[1a]		following text can't be skipped with B button
        //            case 0x1A: break;
        //            //[1b]		init two-choice answer selection (i.e. yes/no arrow)
        //            case 0x1B: break;
        //            //[1c]		init three-choice answer selection
        //            case 0x1C: break;
        //            //[1d]		(show result count of something?)
        //            case 0x1D: break;
        //            //[1e xx]		show minigame result xx
        //            case 0x1E: i++; sb.Append(DisplayValue(s[i])); break;
        //            //  xx: 00 = horseback archery points
        //            //      02 = largest fish caught
        //            //      03 = horse race time
        //            //      04 = marathon time
        //            //      06 = dampé race time
        //            //      ...?
        //            //[1f]		show current hyrule time
        //            case 0x1F: sb.Append("[World Time]"); break;
        //            default: sb.Append(ConvertSpecialChars(s[i])); break;
        //        }
        //    }
        //    return sb.ToString();
        //}

    }
}
