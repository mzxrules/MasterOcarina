using OcaBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Experimental
{
    public partial class TestForm : Form, IExperimentFace
    {
        private void GetBaseIndex(IExperimentFace face)
        {
            face.OutputText("not implemented;");
        }
        private void GetWWResult(IExperimentFace face)
        {
            face.OutputText("not implemented;");
        }
        private void ActorTaggingTest(IExperimentFace face)
        {
            face.OutputText("not implemented;");
        }
        /*
        private void GetBaseIndex(IExperimentFace face)
        {
            StringBuilder sb = new StringBuilder();
            OcarinaDataContext db = new OcarinaDataContext();

            var entrances = from a in db.EntranceIndexes
                            select a;

            foreach (EntranceIndex entrance in entrances)
            {
                sb.AppendFormat("{0}\t{1}", entrance.ID, entrance.BaseIndex);
                sb.AppendLine();
            }
            db.Dispose();
            face.OutputText(sb.ToString());
        }

        private void GetWWResult(IExperimentFace face)
        {
            OcarinaDataContext db = new OcarinaDataContext();
            StringBuilder sb = new StringBuilder();

            var entrances = from a in db.EntranceIndexes
                            where a.BaseIndex == a.ID
                            select a;

            foreach (EntranceIndex entrance in entrances)
            {
                for (int i = 0; i < 0x0D; i++)
                {
                    var p = new WrongWarp.Params(entrance.BaseIndex, i);
                    WrongWarp.GetResult(p, out WrongWarp.Result result);
                    var strResult = result.PrintN64();
                    sb.AppendFormat("{0:X4}\t{1:X2}\t{2}", entrance.ID, i, strResult);
                    sb.AppendLine();
                }
            }
            face.OutputText(sb.ToString());
        }

        private void ActorTaggingTest()
        {
            StringBuilder sb = new StringBuilder();
            OcarinaDataContext db = new OcarinaDataContext();

            var list = from a in db.ActorTagRelationships
                       where a.ParentTagId == 124
                       select a;

            List<ActorTagRelationship> hiearchy = list.ToList();


            foreach (ActorTagRelationship atr in hiearchy)
            {
                sb.AppendLine($"{atr.ChildTagId}");
                var actorTagList = from a in db.ActorTags
                                   where a.TagId == atr.ChildTagId
                                   select a;

                foreach (ActorTag at in actorTagList)
                {
                    var actorList = from a in db.Actors
                                    where a.ID == at.ActorId
                                    select a;
                    foreach (Actor a in actorList)
                    {
                        sb.AppendLine(a.GetDescription());
                    }
                }
            }
            outRichTextBox.Text = sb.ToString();
        }
        */
    }
}
