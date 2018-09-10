using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OcaBase
{
    partial class Scene
    {
        public static bool TryGetSingle(byte index, out Scene record)
        {
            OcarinaDataContext db = new OcarinaDataContext();

            record = db.Scenes.SingleOrDefault(x => x.ID == index);
            return record != null;
        }

        public string GetDescription()
        {
            return String.Format("#{0} {1} ({2}), Alt Setups?: {3}, Rooms: {4}, Entrances: {5}{6}.",
                ID,
                File.Description,
                File.Filename,
                (x18) ? "Yes" : "No",
                Rooms,
                EntranceNum,
                (MapSelect != null) ? ", Map Select #" + MapSelect : "");
        }
    }
}
