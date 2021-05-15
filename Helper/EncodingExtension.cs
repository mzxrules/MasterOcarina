using System.Text;

namespace mzxrules.Helper
{
    public static class EncodingExtension
    {
        public static void RegisterCodePagesEncodingProvider()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}
