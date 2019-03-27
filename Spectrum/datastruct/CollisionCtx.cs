using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    class CollisionCtx
    {
        RomVersion version;
        /* 0x00 */ public N64Ptr SceneMeshPtr;
        /* 0x04 */ public Vector3<float> boxmin;
        /* 0x10 */ public Vector3<float> boxmax;
        /* 0x1C */ public Vector3<int> max;
        /* 0x28 */ public Vector3<float> unitSize;
        /* 0x34 */ public Vector3<float> factor;
        /* 0x40 */ public Ptr Table;
        /* 0x44 */ public short LinksMax;
        /* 0x46 */ public short LinksAlloc;
        /* 0x48 */ public Ptr Links;
        /* 0x4C */ public Ptr Checks;

        /* 0x1440 */ public Ptr dyn_poly;
        public Ptr dyn_vtx;
        public int mm_0x1448;
        public Ptr mm_0x144C;
        public Ptr dyn_list;
        public int dyn_count;
        public int dyn_max;
        public int dyn_list_max;
        public int dyn_poly_max;
        public int dyn_vtx_max;
        public int mem_size;
        public int mm_0x146C;

        public CollisionCtx(Ptr ctx, RomVersion version)
        {
            this.version = version;
            SceneMeshPtr = ctx.ReadInt32(0);

            boxmin = new Vector3<float>(
                ctx.ReadFloat(0x04),
                ctx.ReadFloat(0x08),
                ctx.ReadFloat(0x0C)
            );

            boxmax = new Vector3<float>(
                ctx.ReadFloat(0x10),
                ctx.ReadFloat(0x14),
                ctx.ReadFloat(0x18)
            );

            max = new Vector3<int>(
                ctx.ReadInt32(0x1C),
                ctx.ReadInt32(0x20),
                ctx.ReadInt32(0x24)
            );

            unitSize = new Vector3<float>(
                ctx.ReadFloat(0x28),
                ctx.ReadFloat(0x2C),
                ctx.ReadFloat(0x30)
            );

            factor = new Vector3<float>(
                ctx.ReadFloat(0x34),
                ctx.ReadFloat(0x38),
                ctx.ReadFloat(0x3C)
            );

            Table = ctx.Deref(0x40);
            LinksMax = ctx.ReadInt16(0x44);
            LinksAlloc = ctx.ReadInt16(0x46);
            Links = ctx.Deref(0x48);
            Checks = ctx.Deref(0x4C);

            dyn_poly = ctx.Deref(0x1440);
            dyn_vtx = ctx.Deref(0x1444);

            if (version.Game == Game.OcarinaOfTime)
            {
                dyn_list = ctx.Deref(0x1448);
                dyn_count = ctx.ReadInt32(0x144C);
                dyn_max = ctx.ReadInt32(0x1450);
                dyn_list_max = ctx.ReadInt32(0x1454);
                dyn_poly_max = ctx.ReadInt32(0x1458);
                dyn_vtx_max = ctx.ReadInt32(0x145C);
                mem_size = ctx.ReadInt32(0x1460);
            }
            else if (version.Game == Game.MajorasMask)
            {
                mm_0x1448 = ctx.ReadInt32(0x1448);
                mm_0x144C = ctx.Deref(0x144C);
                dyn_list = ctx.Deref(0x1450);
                dyn_count = ctx.ReadInt32(0x1454);
                dyn_max = ctx.ReadInt32(0x1458);
                dyn_list_max = ctx.ReadInt32(0x145C);
                dyn_poly_max = ctx.ReadInt32(0x1460);
                dyn_vtx_max = ctx.ReadInt32(0x1464);
                mem_size = ctx.ReadInt32(0x1468);
                mm_0x146C = ctx.ReadInt32(0x1468);
            }
        }

        public override string ToString()
        {
            string result = $"Scene Mesh: {SceneMeshPtr}{Environment.NewLine}" +
                $"Bounding Box: ({boxmin.x},{boxmin.y},{boxmin.z}) ({boxmax.x}, {boxmax.y}, {boxmax.z}){Environment.NewLine}" +
                $"Subdivisions: ({max.x}, {max.y}, {max.z}) Section Size: ({unitSize.x}, {unitSize.y}, {unitSize.z}){Environment.NewLine}" +
                $"Max PolyLinks: {LinksMax:X4} Allocated: {LinksAlloc:X4}{Environment.NewLine}" +
                $"Table: {Table} Links: {Links} Checks: {Checks}{Environment.NewLine}" +
                $"dyn_poly = {dyn_poly}{Environment.NewLine}" +
                $"dyn_vtx = {dyn_vtx}{Environment.NewLine}";
            if (version.Game == Game.MajorasMask)
            {
                result +=
                    $"mm_0x1448 = {mm_0x1448:X8}{Environment.NewLine}" +
                    $"mm_0x144C = {mm_0x144C}{Environment.NewLine}";
            }
            result +=
                $"dyn_list = {dyn_list}{Environment.NewLine}" +
                $"dyn_count = {dyn_count}{Environment.NewLine}" +
                $"dyn_max = {dyn_max}{Environment.NewLine}" +
                $"dyn_list_max = {dyn_list_max}{Environment.NewLine}" +
                $"dyn_poly_max = {dyn_poly_max}{Environment.NewLine}" +
                $"dyn_vtx_max = {dyn_vtx_max}{Environment.NewLine}" +
                $"mem_size = {mem_size:X6}";

            if (version.Game == Game.MajorasMask)
            {
                result += $"{Environment.NewLine}unk_0x146C = {mm_0x146C:X8}";
            }
            return result;
        }

        public int[] ComputeColSec(Vector3<float> xyz)
        {
            int[] colsec = new int[3];

            for (int i = 0; i < 3; i++)
            {
                int off = i * 4;
                float abs = xyz.Index(i) - boxmin.Index(i);
                colsec[i] = (int)(abs * factor.Index(i));
                if (colsec[i] < 0)
                    colsec[i] = 0;
                if (colsec[i] >= max.Index(i))
                    colsec[i] = max.Index(i) - 1;
            }
            return colsec;
        }

        public bool ColSecInBounds(int[] colsec)
        {
            return !(colsec[0] < 0 || colsec[0] >= max.x
                || colsec[1] < 0 || colsec[1] >= max.y
                || colsec[2] < 0 || colsec[2] >= max.z);
        }

        public N64Ptr GetColSecDataPtr(int[] colsec)
        {
            int addr = colsec[2] * max.y * max.x;
            addr += colsec[1] * max.x;
            addr += colsec[0];
            addr *= 6;
            addr += Table;

            return addr;
        }

        public N64Ptr GetLinkDataPtr(int index)
        {
            return index * 4 + Links;
        }

        public IEnumerable<(short, short)> YieldPolyChain(int[] colsec, int index)
        {
            N64Ptr colsecAddr = GetColSecDataPtr(colsec);
            Ptr colsecPtr = SPtr.New(colsecAddr);

            int depthLimit = 800;
            short topLinkId = colsecPtr.ReadInt16(index * 2);
            short linkId = topLinkId;

            while (depthLimit > 0 && linkId != -1)
            {
                depthLimit--;

                //Get Next Link Record
                short polyId = Links.ReadInt16(linkId * 4);
                linkId = Links.ReadInt16(linkId * 4 + 2);
                yield return (polyId, linkId);
            }
        }
    }
}
