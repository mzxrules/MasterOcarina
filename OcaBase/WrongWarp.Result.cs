using OcaBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcaBase
{
    partial class WrongWarp
    {
        public class Result
        {
            public enum Scene
            {
                Debug,
                Unknown,
                Normal
            }
            public enum Cutscene
            {
                Normal,
                Pointer,
            }

            private Params WarpParams;
            public EntranceStruct Start;
            public EntranceStruct End;
            public OcaBase.Scene FinalScene;
            public Entrance FinalEntrance;

            public bool ValidEntranceTableRecord = true;
            public Scene SceneType = Scene.Unknown;
            public bool ValidSetup = false;
            public Cutscene CutsceneType = Cutscene.Pointer;
            public bool ValidRoom = false;
            public bool ValidEntranceId = false;

            public bool Crashes = true;
            public bool FaroresWindOnly = false;

            public Result(Params warpParams)
            {
                Start = null;
                End = null;
                FinalEntrance = null;
                WarpParams = warpParams;
            }
            public void SetResult()
            {
                //if the final entrance is not off the table
                if (ValidEntranceTableRecord)
                {
                    //Test scene existance
                    //if the scene does not exist in the commercial version
                    if (FinalScene.ID < 101)
                        SceneType = Scene.Normal;
                    //if it only exists in the debug version
                    else if (FinalScene.ID < 110)
                        SceneType = Scene.Debug;
                    else
                        SceneType = Scene.Unknown;

                    //scene exists
                    if (SceneType == Scene.Normal)
                    {
                        //if alternate headers command 0x18 exists,
                        //we'll wrong warp into a cutscene setup...
                        if (FinalScene.x18)
                            CutsceneType = Cutscene.Normal;
                        else
                            CutsceneType = Cutscene.Pointer;

                        //or if it doesn't exist, it will crash
                        if (FinalEntrance == null)
                            ValidSetup = false;
                        else
                        {
                            ValidSetup = true;
                            if (FinalEntrance.EntranceNum < FinalScene.EntranceNum)
                                ValidEntranceId = true;
                            else
                                ValidEntranceId = false;
                            //if the entrance record is sending you to a room that doesn't exist
                            if (FinalEntrance.Room >= FinalScene.Rooms)
                                ValidRoom = false;
                            else
                                ValidRoom = true;
                        }
                    }
                }
                FaroresWindOnly = (!ValidRoom || !ValidEntranceId);
            }


            #region PrintFunctions

            public string PrintN64()
            {
                string resultString;
                Result result = this;
                if (!result.ValidEntranceTableRecord)
                {
                    resultString = String.Format("Wrong warping with {0} cannot be calculated accurately due to version differences,"
                        + " but will take you to either the Deku Tree, Dodongo's Cavern, Jabu Jabu, or the Forest Temple",
                        result.Start.GetShortDescription(false, true));
                }
                else
                {
                    resultString = String.Format("{0} with cutscene {2} will wrong warp you to {1} (room {3})",
                        result.Start.GetShortDescription(false, true),
                        result.End.GetShortDescription(false, false),
                        WarpParams.CutsceneIndex,
                        (result.FinalEntrance != null) ? result.FinalEntrance.Room.ToString() : "null");

                    //What scene are we warping to?
                    switch (result.SceneType)
                    {
                        case WrongWarp.Result.Scene.Debug:
                            resultString += ", but will crash because the scene only exists on the Debug Rom."; break;
                        case WrongWarp.Result.Scene.Unknown:
                            resultString += ", but will crash because the scene doesn't exist."; break;
                        default: //Good Scene
                            resultString += GoodSceneBranch(WarpParams, result); break;
                    }
                }
                return resultString;
            }
            public string Print3dsDhww()
            {
                string result;
                Result wwr = this;

                if (!wwr.ValidEntranceTableRecord)
                {
                    result = String.Format("Death hole wrong warping with {0} cannot be calculated, but in theory it should try to"
                        + " take you to either the Deku Tree, Dodongo's Cavern, Jabu Jabu, or the Forest Temple",
                        wwr.Start.GetShortDescription(false, true));
                }
                else
                {
                    //We know enough to say where the wrong warp will place us, so create that part of the output
                    result = String.Format("Death hole wrong warping with {0} will wrong warp you to {1}",
                        wwr.Start.GetShortDescription(false, true),
                        wwr.End.GetShortDescription(false, false));

                    //What scene are we warping to?
                    switch (wwr.SceneType)
                    {
                        case WrongWarp.Result.Scene.Debug:
                            result += ", but will crash because the scene only exists on the Debug Rom."; break;
                        case WrongWarp.Result.Scene.Unknown:
                            result += ", but will crash because the scene doesn't exist."; break;
                        default: //Good Scene
                            result += DhwwGoodSceneBranch(wwr);
                            break;
                    }
                }
                return result;
            }


            /// <summary>
            /// Standard 64 good scene wrong warp branch result
            /// </summary>
            /// <param name="wwp"></param>
            /// <param name="wwr"></param>
            /// <returns></returns>
            private string GoodSceneBranch(WrongWarp.Params wwp, WrongWarp.Result wwr)
            {
                SceneCutscene cutsceneRecord;
                string result = "";

                //Valid Setup?
                if (!wwr.ValidSetup) //bad setup
                    return String.Format(" but will crash due to a bad scene setup ({0}).",
                        wwp.CutsceneIndex + 4);

                //cutscene type
                if (wwr.CutsceneType == WrongWarp.Result.Cutscene.Normal)
                {
                    if (Queries.TryGetCutsceneRecord(wwr.FinalScene.ID, wwp.CutsceneIndex, out cutsceneRecord))
                    {
                        result += String.Format(" and play the {0} cutscene.",
                            cutsceneRecord.Description);
                    }
                    else
                        result += String.Format(" and play cutscene {0}.", wwp.CutsceneIndex);
                }
                else //cutscene pointer
                {
                    result += " but is affected by the cutscene pointer.";
                }

                //Crash without Farore's Wind?
                if (FaroresWindOnly)
                    result += PrintFaroresWindRequirement(wwr);

                return result;
            }

            /// <summary>
            /// Death Hole Wrong Warping good scene result branch
            /// </summary>
            /// <param name="wwr"></param>
            /// <returns></returns>
            private string DhwwGoodSceneBranch(WrongWarp.Result wwr)
            {
                SceneCutscene cutsceneRecord;
                string result = "";
                //Valid Setup?
                if (!wwr.ValidSetup)
                {
                    //bad setup
                    return ", but will crash due to a bad scene setup (4).";// break;
                }

                //cutscene type
                if (wwr.CutsceneType == WrongWarp.Result.Cutscene.Normal)
                {
                    if (Queries.TryGetCutsceneRecord(wwr.FinalScene.ID, 0, out cutsceneRecord))
                    {
                        result = String.Format(", with a black screen and the {0} scene setup loaded.",
                            cutsceneRecord.Description);
                    }
                    else //still valid, just possible that the record wasn't pulled/missing cs
                        result = ", with a black screen and cutscene 0 loaded.";
                }
                else
                {
                    //cutscene pointer
                }

                //if room or entrance id aren't valid
                if (FaroresWindOnly)
                    result += PrintFaroresWindRequirement(wwr);

                return result;
            }

            private string PrintFaroresWindRequirement(WrongWarp.Result wwr)
            {
                return string.Format(
                    " HOWEVER, without Farore's Wind {0}{1}{2}.",
                    (!wwr.ValidRoom) ? String.Format("a crash will occur due to an invalid room id") : "",
                    (!wwr.ValidRoom && !wwr.ValidEntranceId) ? ", and " : "",
                    (!wwr.ValidEntranceId) ? "a softlock will occur due to a invalid starting position" : ""
                );
            }

            #endregion
        }
    }
}
