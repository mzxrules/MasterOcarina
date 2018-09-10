using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    class CollisionCtx
    {
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

        public CollisionCtx(Ptr ctx)
        {
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
        }

        public override string ToString()
        {
            return $"Scene Mesh: {SceneMeshPtr}{Environment.NewLine}" +
                $"Bounding Box: ({boxmin.x},{boxmin.y},{boxmin.z}) ({boxmax.x}, {boxmax.y}, {boxmax.z}){Environment.NewLine}" +
                $"Subdivisions: ({max.x}, {max.y}, {max.z}) Section Size: ({unitSize.x}, {unitSize.y}, {unitSize.z}){Environment.NewLine}" +
                $"Max PolyLinks: {LinksMax:X4} Allocated: {LinksAlloc:X4}{Environment.NewLine}" +
                $"Table: {Table} Links: {Links} Checks: {Checks}";
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
