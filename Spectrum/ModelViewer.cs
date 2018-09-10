using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uCode;

namespace Spectrum
{
    struct ModelViewerDlist
    {
        public N64Ptr DlistBranch;
        public List<N64Ptr> DListHierarchy;

        public ModelViewerDlist(N64Ptr branch, List<N64Ptr> hierarchy)
        {
            DlistBranch = branch;
            DListHierarchy = hierarchy;
        }
    }
    class ModelViewerOoT
    {
        List<ModelViewerDlist> DListBranches = new List<ModelViewerDlist>();
        int DListIndex = 0;

        N64Ptr WORK_DISP_ptr = 0;
        N64Ptr OVERLAY_DISP_ptr = 0;
        N64Ptr POLY_OPA_DISP_ptr = 0;
        N64Ptr POLY_XLU_DISP_ptr = 0;
        private GfxDList[] gfxDList;

        //public ModelViewerOoT(SPtr GlobalContext)
        //{
        //    var gfx = GlobalContext.Deref();
        //    WORK_DISP_ptr = gfx.ReadInt32(0x1B8);
        //    OVERLAY_DISP_ptr = gfx.ReadInt32(0x2AC);
        //    POLY_OPA_DISP_ptr = gfx.ReadInt32(0x2BC);
        //    POLY_XLU_DISP_ptr = gfx.ReadInt32(0x2CC);

        //    DListBranches.AddRange(GetDlistBranches(POLY_OPA_DISP_ptr));
        //    DListBranches.AddRange(GetDlistBranches(POLY_XLU_DISP_ptr));
        //    DListBranches.AddRange(GetDlistBranches(OVERLAY_DISP_ptr));
        //}

        public ModelViewerOoT(GfxDList[] gfxDList)
        {
            this.gfxDList = gfxDList;
            foreach (var item in gfxDList)
            {
                if (item.Name == "WORK_DISP")
                    continue;
                var dlists = GetDlistBranches(item.StartPtr);
                DListBranches.AddRange(dlists);
                Console.WriteLine($"{item.Name}: {dlists.Count}");
            }
            //NullBranches();
        }


        public void ScrollDlist(int v)
        {
            if (DListBranches.Count == 0)
                return;

            NullBranches();

            //var record = DListBranches[DListIndex];
            //var addr = record.DlistBranch;

            //Zpr.WriteRam8((int)addr, 0x00);

            DListIndex += v;
            if (DListIndex >= DListBranches.Count)
                DListIndex = 0;
            if (DListIndex < 0)
                DListIndex = DListBranches.Count - 1;

            var addr = DListBranches[DListIndex].DlistBranch;

            Zpr.WriteRam8(addr, 0xDE);

            Console.Clear();
            Console.WriteLine($"{Zpr.ReadRamInt32(addr + 4):X8}");

            foreach(var item in DListBranches[DListIndex].DListHierarchy)
            {
                Zpr.WriteRam8(item, 0xDE);
            }
        }


        private List<ModelViewerDlist> GetDlistBranches(N64Ptr topDlist)
        {
            List<ModelViewerDlist> result = new List<ModelViewerDlist>();

            var data = Zpr.ReadRam(0, 0x400000);

            //bool foundProjection = false;

            List<(N64Ptr ptr, bool check)> stack = new List<(N64Ptr, bool)>();

            List<int> endDlists = gfxDList.Select(x => (int)x.StartPtr).ToList();

            using (BinaryReader br = new BinaryReader(new MemoryStream(data)))
            {
                foreach (var item in MicrocodeParser.DeepTrace(br, (int)topDlist))
                {
                    //if (item.Item2.Name == G_.G_MTX)
                    //{
                    //    if ((item.Item2.EncodingHigh & 0x4) > 0)
                    //        foundProjection = true;
                    //}

                    //if (!foundProjection)
                    //    continue;

                    if (item.gbi.Name == G_.G_DL)
                    {
                        if (endDlists.Contains((int)item.gbi.EncodingLow))
                            break;
                        bool check = (item.gbi.EncodingLow >> 24) != 0x80;
                        
                        var list = stack.Where(x => x.check == check).Select(y => y.ptr).ToList();

                        if (check)
                        {
                            result.Add(new ModelViewerDlist(item.ptr, list));
                        }
                        stack.Add((item.ptr, check));
                    }
                    if (item.gbi.Name == G_.G_ENDDL)
                    {
                        var remove = stack.Count-1;
                        if (remove >= 0)
                            stack.RemoveAt(remove);
                    }
                }
            }
            return result;
        }

        private void NullBranches()
        {
            foreach (var addr in DListBranches)
            {
                Zpr.WriteRam8((int)addr.DlistBranch, 0x00);
            }
        }

        internal void RestoreBranches()
        {
            foreach (var addr in DListBranches)
                Zpr.WriteRam8((int)addr.DlistBranch, 0xDE);
        }
    }
}
