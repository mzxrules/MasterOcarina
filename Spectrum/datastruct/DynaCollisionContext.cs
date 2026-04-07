using mzxrules.Helper;
using mzxrules.OcaLib;
using System;

namespace Spectrum
{
    class DynaCollisionContext
    {
        const int BG_ACTOR_MAX = 50;
        /* O 0x0000          */ public byte bitFlag = 0;
        /* O 0x0004          */ public BgActor[] bgActors = new BgActor[BG_ACTOR_MAX];
        /* O 0x138C          */ public ushort[] bgActorFlags = new ushort[BG_ACTOR_MAX]; // & 0x0008 = no dyna ceiling
        /* O 0x13F0          */ public Ptr polyList; //dyn_poly
        /* O 0x13F4          */ public Ptr vtxList; //dyn_vtx
        /*          M 0x13F8 */ public int mm_0x1448;
        /*          M 0x13FC */ public Ptr waterBoxList;
        /* O 0x13F8 M 0x1400 */ public Ptr polyNodes_tbl; //DynaSSNodeList, dyn_list, dyn_count, dyn_max
        /* O 0x13FC M 0x1404 */ public int polyNodes_count;
        /* O 0x1400 M 0x1408 */ public int polyNodes_max;
        /* O 0x1404 M 0x140C */ public int polyNodesMax; //dyn_list_max
        /* O 0x1408 M 0x1410 */ public int polyListMax; //dyn_poly_max
        /* O 0x140C M 0x1414 */ public int vtxListMax; //dyn_vtx_max

        RomVersion version;

        public DynaCollisionContext(Ptr ctx, RomVersion version)
        {
            //TODO: remove this
            Ptr colCtx = ctx.RelOff(-0x50);

            for (int i = 0; i < BG_ACTOR_MAX; i++)
            {
                bgActors[i] = new BgActor(ctx.RelOff(i * 0x64 + 0x4));
                bgActorFlags[i] = ctx.RelOff(0x138C).ReadUInt16(i * 2);
            }

            polyList = ctx.Deref(0x13F0);
            vtxList = ctx.Deref(0x13F4);

            if (version.Game == Game.OcarinaOfTime)
            {
                polyNodes_tbl = ctx.Deref(0x13F8);
                polyNodes_count = ctx.ReadInt32(0x13FC);
                polyNodes_max = ctx.ReadInt32(0x1400);
                polyNodesMax = ctx.ReadInt32(0x1404);
                polyListMax = ctx.ReadInt32(0x1408);
                vtxListMax = ctx.ReadInt32(0x140C);
            }
            else if (version.Game == Game.MajorasMask)
            {
                mm_0x1448 = colCtx.ReadInt32(0x1448);
                waterBoxList = colCtx.Deref(0x144C);
                polyNodes_tbl = colCtx.Deref(0x1450);
                polyNodes_count = colCtx.ReadInt32(0x1454);
                polyNodes_max = colCtx.ReadInt32(0x1458);
                polyNodesMax = colCtx.ReadInt32(0x145C);
                polyListMax = colCtx.ReadInt32(0x1460);
                vtxListMax = colCtx.ReadInt32(0x1464);
            }
            this.version = version;
        }
        public override string ToString()
        {
            string result = $"DynaPolyContext:{Environment.NewLine}";
            result +=
                $" polyList        {polyList}{Environment.NewLine}" +
                $" vtxList         {vtxList}{Environment.NewLine}";
            if (version.Game == Game.MajorasMask)
            {
                result +=
                    $" waterBox_0x1448 {mm_0x1448:X8}{Environment.NewLine}" +
                    $" waterBoxList    {waterBoxList}{Environment.NewLine}";
            }
            result +=
                $" PolyNodes:{Environment.NewLine}" +
                $"  tbl            {polyNodes_tbl}{Environment.NewLine}" +
                $"  count          {polyNodes_count,6}{Environment.NewLine}" +
                $"  max            {polyNodes_max,6}{Environment.NewLine}" +
                $" polyNodesMax    {polyNodesMax,6}{Environment.NewLine}" +
                $" polyListMax     {polyListMax,6}{Environment.NewLine}" +
                $" vtxListMax      {vtxListMax,6}";
            return result;
        }
    }
}
