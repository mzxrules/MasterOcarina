using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Collections.Generic;

namespace Spectrum
{
    class DynaCollisionContext
    {
        /* O 0x0000          */ public byte bitFlag = 0;
        /* O 0x0004          */ public BgActor[] bgActors = new BgActor[50];
        /* O 0x138C          */ public ushort[] bgActorFlags = new ushort[50]; // & 0x0008 = no dyna ceiling
        /* O 0x13F0          */ public Ptr polyList; //dyn_poly
        /* O 0x13F4          */ public Ptr vtxList; //dyn_vtx
        /*          M 0x13F8 */ public int mm_0x1448;
        /*          M 0x13FC */ public int mm_0x144C;
        /* O 0x13F8 M 0x1400 */ public Ptr polyNodes_tbl; //DynaSSNodeList, dyn_list, dyn_count, dyn_max
        /* O 0x13FC M 0x1404 */ public int polyNodes_count;
        /* O 0x1400 M 0x1408 */ public int polyNodes_max;
        /* O 0x1404 M 0x140C */ public int polyNodesMax; //dyn_list_max
        /* O 0x1408 M 0x1410 */ public int polyListMax; //dyn_poly_max
        /* O 0x140C M 0x1414 */ public int vtxListMax; //dyn_vtx_max

        RomVersion version;

        public DynaCollisionContext(Ptr ctx, RomVersion version)
        {
            polyList = ctx.Deref(0x1440);
            vtxList = ctx.Deref(0x1444);

            if (version.Game == Game.OcarinaOfTime)
            {
                polyNodes_tbl = ctx.Deref(0x1448);
                polyNodes_count = ctx.ReadInt32(0x144C);
                polyNodes_max = ctx.ReadInt32(0x1450);
                polyNodesMax = ctx.ReadInt32(0x1454);
                polyListMax = ctx.ReadInt32(0x1458);
                vtxListMax = ctx.ReadInt32(0x145C);
            }
            else if (version.Game == Game.MajorasMask)
            {
                mm_0x1448 = ctx.ReadInt32(0x1448);
                mm_0x144C = ctx.Deref(0x144C);
                polyNodes_tbl = ctx.Deref(0x1450);
                polyNodes_count = ctx.ReadInt32(0x1454);
                polyNodes_max = ctx.ReadInt32(0x1458);
                polyNodesMax = ctx.ReadInt32(0x145C);
                polyListMax = ctx.ReadInt32(0x1460);
                vtxListMax = ctx.ReadInt32(0x1464);
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
                    $" mm_0x1448       {mm_0x1448:X8}{Environment.NewLine}" +
                    $" mm_0x144C       {mm_0x144C}{Environment.NewLine}";
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
    class CollisionCtx
    {
        RomVersion version;
        /* 0x00 */
        public N64Ptr SceneMeshPtr;
        /* 0x04 */
        public Vector3<float> boxmin;
        /* 0x10 */
        public Vector3<float> boxmax;
        /* 0x1C */
        public Vector3<int> max;
        /* 0x28 */
        public Vector3<float> unitSize;
        /* 0x34 */
        public Vector3<float> factor;
        /* 0x40 */
        public Ptr Table;
        /* 0x44 */
        public short SSNodeMax;
        /* 0x46 */
        public short SSNodeCount;
        /* 0x48 */
        public Ptr SSNodeTbl;
        /* 0x4C */
        public Ptr polyCheckTbl;
        /* 0x50 */
        DynaCollisionContext dyna;
        public int mem_size;
        public int mm_0x146C;

        public static readonly string[] StaticLookupTypes =
        {
            "Floor",
            "Wall",
            "Ceil"
        };

        public static readonly string[] DynaLookupTypes =
        {
            "Ceil",
            "Wall",
            "Floor"
        };

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
            SSNodeMax = ctx.ReadInt16(0x44);
            SSNodeCount = ctx.ReadInt16(0x46);
            SSNodeTbl = ctx.Deref(0x48);
            polyCheckTbl = ctx.Deref(0x4C);
            dyna = new DynaCollisionContext(ctx, version);

            if (version.Game == Game.OcarinaOfTime)
            {
                mem_size = ctx.ReadInt32(0x1460);
            }
            else if (version.Game == Game.MajorasMask)
            {
                mem_size = ctx.ReadInt32(0x1468);
                mm_0x146C = ctx.ReadInt32(0x146C);
            }
        }

        public override string ToString()
        {
            string result = $"CollisionContext:{Environment.NewLine}" +
                $" CollisionHeader {SceneMeshPtr}{Environment.NewLine}" +
                $" Bounding Box:{Environment.NewLine}" +
                $"  minBounds      ({boxmin.x,6}, {boxmin.y,6}, {boxmin.z,6}){Environment.NewLine}" +
                $"  maxBounds      ({boxmax.x,6}, {boxmax.y,6}, {boxmax.z,6}){Environment.NewLine}" +
                $"  Subdivs        ({max.x}, {max.y}, {max.z}){Environment.NewLine}" +
                $"  Subdiv len     ({unitSize.x}, {unitSize.y}, {unitSize.z}){Environment.NewLine}" +
                $"  Subdiv SSLists {Table} {Environment.NewLine}" +
                $" SSNode Max      {SSNodeMax, 6}  {SSNodeMax:X4}{Environment.NewLine}" +
                $" SSNode Count    {SSNodeCount, 6}  {SSNodeCount:X4}{Environment.NewLine}" +
                $" SSNode Tbl      {SSNodeTbl}{Environment.NewLine}" +
                $" polyCheckTbl    {polyCheckTbl}{Environment.NewLine}" +
                $"{dyna}{Environment.NewLine}" +
                $" mem_size        {mem_size:X8}";

            if (version.Game == Game.MajorasMask)
            {
                result += $"{Environment.NewLine}unk_0x146C = {mm_0x146C:X8}";
            }
            return result;
        }

        public Vector3<int> ComputeColSec(Vector3<float> xyz)
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
            return new(colsec[0], colsec[1], colsec[2]);
        }

        public bool ColSecInBounds(Vector3<int> colsec)
        {
            return !(colsec.x < 0 || colsec.x >= max.x
                || colsec.y < 0 || colsec.y >= max.y
                || colsec.z < 0 || colsec.z >= max.z);
        }

        public N64Ptr GetColSecDataPtr(Vector3<int> colsec)
        {
            int addr = colsec.z * max.y * max.x;
            addr += colsec.y * max.x;
            addr += colsec.x;
            addr *= 6;
            addr += Table;

            return addr;
        }

        public N64Ptr GetLinkDataPtr(int index)
        {
            return index * 4 + SSNodeTbl;
        }

        public IEnumerable<(short, short)> YieldPolyChain(Vector3<int> colsec, int index)
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
                short polyId = SSNodeTbl.ReadInt16(linkId * 4);
                linkId = SSNodeTbl.ReadInt16(linkId * 4 + 2);
                yield return (polyId, linkId);
            }
        }

        public List<SimpleRamItem> GetRamMap()
        {
            return new List<SimpleRamItem>
            {
                new SimpleRamItem()
                {
                    Ram = new N64PtrRange(Table, Table + (max.x * max.y * max.z * 6)),
                    Description = "COLCTX Table"
                },

                new SimpleRamItem()
                {
                    Ram = new N64PtrRange(SSNodeTbl, SSNodeTbl + SSNodeMax * 4),
                    Description = "COLCTX Links"
                },

                new SimpleRamItem()
                {
                    Ram = new N64PtrRange(polyCheckTbl, SSNodeTbl),
                    Description = "COLCTX Checks"
                },

                new SimpleRamItem()
                {
                    Ram = new N64PtrRange(dyna.polyList, dyna.polyList + dyna.polyListMax * 0x10),
                    Description = "COLCTX dyn_poly"
                },

                new SimpleRamItem()
                {
                    Ram = new N64PtrRange(dyna.vtxList, dyna.vtxList + dyna.vtxListMax * 6),
                    Description = "COLCTX dyn_vtx",
                },

                new SimpleRamItem()
                {
                    Ram = new N64PtrRange(dyna.polyNodes_tbl, dyna.polyNodes_tbl + dyna.polyNodesMax * 4),
                    Description = "COLCTX dyn_list"
                }
            };
        }
    }
}
