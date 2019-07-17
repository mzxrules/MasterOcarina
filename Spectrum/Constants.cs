using System.Collections.Generic;
using mzxrules.Helper;
using mzxrules.OcaLib;

namespace Spectrum
{
    static class Constants
    {
        const int FRAMEBUFFER_SIZE = 0x3DA800 - 0x3B5000;
        public static List<N64Ptr> GetFramebufferPointers(RomVersion version)
        {
            if (version.Game == Game.OcarinaOfTime)
            {
                if (version == ORom.Build.DBGMQ)
                {
                    return new List<N64Ptr>() { 0x80400E80, FRAMEBUFFER_SIZE + 0x80400E80 };
                }
                else
                    return new List<N64Ptr>() { 0x803B5000, 0x803DA800 };
            }
            else if (version.Game == Game.MajorasMask)
            {
                if (version == MRom.Build.U0)
                {
                    return new List<N64Ptr>() { 0x80000500, 0x80785000, 0x80383AC0, 0x80383AC0 + FRAMEBUFFER_SIZE };
                }
            }
            return new List<N64Ptr>() { };
        }
    }
}
