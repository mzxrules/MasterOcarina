using System.Linq;
using OcaBase;
using System.Runtime.CompilerServices;

namespace OcaBase
{
    public partial class WrongWarp
    {
        #region Helper Classes
        public class Params
        {
            public int BaseEntranceIndex { get { return _BaseEntranceIndex; } }
            int _BaseEntranceIndex;
            public int CutsceneIndex { get { return _CutsceneOffset; } }
            int _CutsceneOffset;
            public Params(int baseEntrance, int cutscene)
            {
                _BaseEntranceIndex = baseEntrance;
                _CutsceneOffset = cutscene;
            }
        }

        #endregion

        /// <summary>
        /// Returns the calculation of a wrong warp as a WrongWarp.Result object
        /// </summary>
        /// <param name="wwp">The parameters for the wrong warp calculation</param>
        /// <param name="wrongWarp">The returned result</param>
        /// <returns>True if the operation can be performed, false otherwise</returns>
        public static bool GetResult(WrongWarp.Params wwp, out WrongWarp.Result wrongWarp)
        {
            bool result;
            result = GetResults(wwp, out wrongWarp);
            if (result)
                wrongWarp.SetResult();
            return result;
        }

        private static bool GetResults(WrongWarp.Params wwp, out WrongWarp.Result wrongWarp)
        {
            int finalEntranceIndex;          //the destination entrance index after calculation

            //initialize our WrongWarp.Result
            wrongWarp = new WrongWarp.Result(wwp);

            //Test to see if the baseEntranceIndex is a valid entrance, and collect information on it

            //if the cutscene offset is not between 0 and 15, or we can't retrieve an entrance index table record
            //report that the operation can't be performed
            if ((wwp.CutsceneIndex < 0 || wwp.CutsceneIndex > 16)
                || !Queries.TryGetEntranceByIndex(wwp.BaseEntranceIndex, out wrongWarp.Start))
            {
                return false;
            }

            //baseEntranceIndex exists, so calculate our finalEntranceIndex
            finalEntranceIndex = wrongWarp.Start.Index.BaseIndex + wwp.CutsceneIndex + 4;

            //if the final index is off the table, don't calculate the final entrance point.
            if (finalEntranceIndex > 0x613)
            {
                wrongWarp.ValidEntranceTableRecord = false;
                return true;
            }

            //now retrieve the record at finalEntranceIndex
            if (Queries.TryGetEntranceByIndex(finalEntranceIndex, out wrongWarp.End))
            {
                if (Scene.TryGetSingle(wrongWarp.End.Index.SceneId, out wrongWarp.FinalScene))
                {
                    //we've got our base and final entrance indexes
                    //now to determine if the scene defines the final entrance correctly

                    GetFinalEntrance(wrongWarp, wwp.CutsceneIndex);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the WrongWarp.Result's resultant entrance data
        /// </summary>
        /// <param name="wrongWarp">the WrongWarp.Result being generated</param>
        /// <param name="cutsceneOffset">the cutscene offset of the wrong warp</param>
        /// <returns></returns>
        private static void GetFinalEntrance(WrongWarp.Result wrongWarp, int cutsceneOffset)
        {
            OcarinaDataContext db = new OcarinaDataContext();

            wrongWarp.FinalEntrance = db.Entrances.SingleOrDefault(ent =>
                ent.SceneId == wrongWarp.FinalScene.ID
                && ent.Setup == ((wrongWarp.FinalScene.x18) ? cutsceneOffset + 4 : 0)
                && ent.EntranceNum == wrongWarp.End.Index.EntranceNum);
        }
    }
}
