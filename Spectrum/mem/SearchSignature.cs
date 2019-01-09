using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    class SearchSignature
    {
        const int ootN0SearchStringOff = 0x0FB4C0; //-0x20 of scene table
        static uint[] ootN0SearchString =
        {
                0x57094183, 0x57094183, 0x57094183, 0x57094183,
                0x5C084183, 0x5C084183, 0x5C084183, 0x5C084183,
                0x02499000, 0x024A6A10, 0x01994000, 0x01995B00,
                0x01130200, 0x01F12000, 0x01F27140, 0x01998000,
                0x01999B00, 0x01140300, 0x0273E000, 0x027537C0,
                0x01996000, 0x01997B00, 0x01150400, 0x023CF000,
                0x023E4F90, 0x0198A000, 0x0198BB00, 0x02160500,
                0x022D8000, 0x022F2970, 0x0198E000, 0x0198FB00,
                0x02120600, 0x025B8000, 0x025CDCF0, 0x01990000,
                0x01991B00, 0x01170700, 0x02ADE000, 0x02AF7B40,
                0x01992000, 0x01993B00, 0x01190800, 0x027A7000,
                0x027BF3C0, 0x0198C000, 0x0198DB00, 0x02180900,
                0x032C6000, 0x032D2560, 0x019F4000, 0x019F5B00,
                0x02180A00, 0x02BEB000, 0x02BFC610, 0x0199C000,
                0x0199DB00, 0x00250000, 0x02EE3000, 0x02EF37B0
        };

        public uint[] Pattern;
        public N64Ptr Address;
        public (N64Ptr, uint)[] Verification;
        
        public static SearchSignature GetN0Signature()
        {
            SearchSignature result = new SearchSignature()
            {
                Pattern = ootN0SearchString,
                Address = ootN0SearchStringOff,
                Verification = new (N64Ptr, uint)[]
                {
                    (0x8011A5D0, 0xCD)
                }
            };
            return result;
        }
    }
}
