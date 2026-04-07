namespace mzxrules.OcaLib.Text.Code
{
    record SetTextColor(byte ColorId) : IDialogItem
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

        public string Print(DialogWriterSettings settings)
        {
            ColorCode Color;

            if (0 <= ColorId && ColorId < 7)
                Color = (ColorCode)ColorId;
            else
                Color = ColorCode.Undefined;

            string code = Color switch
            {
                ColorCode.Undefined => $" {ColorId:X2}",
                _ => Color.ToString()[..3],
            };
            return $"[c:{code}]";
        }
    }
}
