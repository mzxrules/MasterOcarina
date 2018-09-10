using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcaBase
{
    partial class EntranceDef
    {
        /// <summary>
        /// Returns the minimal amount of information needed to define a unique baseEntranceIndex point
        /// </summary>
        /// <param name="hideDestinationScene">If true, only display the starting location to reach the entrance</param>
        /// <returns>Returns a description of the entrance</returns>
        public string GetShortDescription(bool hideDestinationScene)
        {
            string result;

            if (PrevId == null
                && PrevInfo == null)
            {
                result = String.Format("{0}{1}",
                    (hideDestinationScene) ? "" : this.Scene.Name + " (" + this.Scene.ID.ToString() + ")",
                    (DestInfo == null) ? "" : " " + DestInfo);
            }
            else
            {
                result = String.Format("{0}{1} from{2}{3}",
                    (hideDestinationScene) ? "" : this.Scene.Name + " (" + this.Scene.ID.ToString() + ")",
                    (DestInfo == null) ? "" : " " + DestInfo,
                    (PrevId == null) ? "" : " " + this.Scene1.Name,
                    (PrevInfo == null) ? "" : " " + PrevInfo);
            }
            return result.TrimStart();
        }

        /// <summary>
        /// Returns a full description of the entrance
        /// </summary>
        /// <returns></returns>
        public string GetDescription()
        {
            return String.Format("Entrance {0}, {1} ({2}){3}{4}{5}{6}",
                EntranceNum,
                //DestScene
                this.Scene.Name,
                this.Scene.ID,
                (DestInfo == null) ? "" : " " + DestInfo,

                //From
                (PrevId == null && PrevInfo == null) ? "" : " from",

                //PrevScene
                (PrevId == null) ? "" : " " + this.Scene1.Name,
                (PrevInfo == null) ? "" : " " + PrevInfo);
        }
    }
}
