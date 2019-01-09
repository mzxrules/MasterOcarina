using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mzxrules.Helper;
using mzxrules.OcaLib;

namespace Spectrum
{
    static class Constants
    {
        const int FRAMEBUFFER_SIZE = 0x3DA800 - 0x3B5000;
        public static (int buffer0, int buffer1) GetFramebuffers(RomVersion version)
        {
            if (version.Game == Game.OcarinaOfTime)
            {
                if (version == ORom.Build.DBGMQ)
                {
                    return (0x400E80, FRAMEBUFFER_SIZE + 0x400E80);
                }
                else
                    return (0x3B5000, 0x3DA800);
            }
            return (0, 0);
        }

        //public static int GetRamSize(RomVersion version)
        //{
        //    if (version.Game == Game.MajorasMask
        //        || version == ORom.Build.DBGMQ)
        //    {
        //        return 0x800000;
        //    }
        //    return 0x400000;
        //}
    }
}
