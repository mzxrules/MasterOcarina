using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcaBase
{
    public static class Queries
    {
        public static bool TryGetCutsceneRecord(int scene, int setup, out SceneCutscene record)
        {
            OcarinaDataContext db = new OcarinaDataContext();

            record = db.SceneCutscenes.SingleOrDefault
                (x => x.Scene == scene
                   && x.Setup == (setup +4)
                   && x.Setup >= 0);

            if (record != null)
                return true;
            return false;
        }

        /// <summary>
        /// Gets a record to one baseEntranceIndex point
        /// </summary>
        /// <param name="baseEntranceIndex"></param>
        /// <param name="ent"></param>
        /// <returns>True if found, else false</returns>
        public static bool TryGetEntranceByIndex(int entranceIndex, out EntranceStruct ent)
        {
            OcarinaDataContext db = new OcarinaDataContext();

            ent = new EntranceStruct();

            ent.Index = db.EntranceIndexes.SingleOrDefault(x => x.ID == entranceIndex);
            if (ent.Index == null)
                return false;

            EntranceIndex e = ent.Index;

            ent.Definition = db.EntranceDefs.SingleOrDefault
                (x => x.SceneId == e.SceneId && x.EntranceNum == e.EntranceNum);

            if (ent.Definition == null)
                return false;
            return true;
        }

        /// <summary>
        /// Gets a record to one baseEntranceIndex point
        /// </summary>
        /// <param name="baseEntranceIndex"></param>
        /// <param name="givenEntrance"></param>
        /// <returns>True if found, else false</returns>
        public static bool TryGetEntranceByIndex(EntranceIndex entranceIndex, out EntranceStruct ent)
        {
            OcarinaDataContext db = new OcarinaDataContext();

            ent = new EntranceStruct();
            ent.Index = entranceIndex;
            

            EntranceIndex e = ent.Index;

            ent.Definition = db.EntranceDefs.SingleOrDefault
                (x => x.SceneId == e.SceneId && x.EntranceNum == e.EntranceNum);

            if (ent.Definition == null)
                return false;
            return true;
        }
    }
}
