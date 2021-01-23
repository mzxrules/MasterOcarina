using mzxrules.OcaLib;
using mzxrules.OcaLib.Cutscenes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Experimental.Data
{
    static partial class Get
    {
        private delegate Cutscene CutsceneGenerator(List<string> file);

        public static void CutsceneSerializationTest(IExperimentFace face, List<string> file)
        {
            Cutscene cutscene;
            cutscene = SaveCutscene(file, KokiriEmeraldCs, "cs/kokiriemerald.z64");
            cutscene = SaveCutscene(file, GoronRubyCs, "cs/goronruby.z64");
            cutscene = SaveCutscene(file, ZoraSapphireCs, "cs/zorasapphire.z64");

            cutscene = SaveCutscene(file, LightMedallionCs, "cs/lightmedallion.z64");
            face.OutputText(cutscene.PrintByOccurrence());
            cutscene = SaveCutscene(file, FireMedallionCs, "cs/firemedallion.z64");
            cutscene = SaveCutscene(file, WaterMedallionCs, "cs/watermedallion.z64");
            cutscene = SaveCutscene(file, SpiritMedallionCs, "cs/spiritmedallion.z64");
            cutscene = SaveCutscene(file, ShadowMedallionCs, "cs/shadowmedallion.z64");

            //cutscene = SaveCutscene(file, Ganonlol, "Ganonlol.z64");
            cutscene = SaveCutscene(file, LightArrowCs, "cs/lightarrow.z64");
            cutscene = SaveCutscene(file, TheEnd, "cs/theend.z64");
            cutscene = SaveCutscene(file, MeetSheikTempleTimeCs, "cs/sheiktime.z64");
            cutscene = SaveCutscene(file, ForestMedallionCs, "cs/forestmedallion.z64");

        }

        private static Cutscene TheEnd(List<string> file)
        {
            Cutscene theEnd;
            int csAddr = 0x2E8DC04;

            theEnd = GetCutscene(file, csAddr);

            var cmd = theEnd.Commands.OfType<ActionCommand>().Single(x => x.Command == 0x03
            && x.Entries[0].Action == 0x23);

            var entry = cmd.Entries[0];

            ExitCommand exit = new ExitCommand(0x27, entry.StartFrame, entry.EndFrame);

            theEnd.Commands.Remove(theEnd.Commands.SingleOrDefault(x => x.Command == -1));

            theEnd.Commands.Remove(cmd);
            theEnd.Commands.Add(exit);
            theEnd.Commands.Add(new EndCommand());
            return theEnd;
        }

        private static Cutscene SaveCutscene(List<string> file, CutsceneGenerator GenerateCs, string filename)
        {
            Cutscene cutscene = GenerateCs(file);

            using (BinaryWriter bw = new BinaryWriter(new FileStream(filename, FileMode.Create)))
            {
                cutscene.Save(bw);
            }
            return cutscene;
        }

        private static Cutscene Ganonlol(List<string> file)
        {
            int csAddr = 0x2AA1A60;
            return GetCutscene(file, csAddr);
        }

        private static Cutscene MeetSheikTempleTimeCs(List<string> file)
        {
            Cutscene cs;
            int csAddr = 0x252D6F0;
            //short startFrame = 0;
            short endFrame = 0x2B5;
            short sheikCrossArms = 0x25F;

            cs = GetCutscene(file, csAddr);

            ActionCommand music = (ActionCommand)cs.Commands.Single(
                x => (x is ActionCommand)
                && ((ActionCommand)x).Command == 0x56
                && ((ActionCommand)x).Entries[0].StartFrame == 0x2C6);

            music.Entries[0].StartFrame = 0x283;
            music.Entries[0].EndFrame = 0x284;

            cs.Commands.RemoveAll(x => x is TextCommand);

            ActionCommand terminator = (ActionCommand)cs.Commands.Single(x => x.Command == 0x03);
            terminator.Entries[0].StartFrame = (short)(endFrame - 1);

            ActionCommand sheik = (ActionCommand)cs.Commands.Single(x => x.Command == 0x2F);
            sheik.Entries[1].EndFrame = sheikCrossArms;
            sheik.Entries[2].StartFrame = sheikCrossArms;

            CutsceneTrimFromEnd(cs, endFrame);

            cs.Commands.RemoveAll(x => (x is CameraCommand)
            && (((CameraCommand)x).StartFrame >= endFrame));

            //ActorCommand music = new ActorCommand(0x56, new ActionEntry())


            return cs;
        }

        private static Cutscene KokiriEmeraldCs(List<string> file)
        {
            Cutscene stoneCs;
            int csAddr = 0x20783A0;
            short startFrame = 0x0B5;
            short endFrame = 0x3AC;

            //not matching beta kwest because of camera command being truncated

            stoneCs = GetCutscene(file, csAddr);

            stoneCs.Commands.OfType<TextCommand>().Single()
                .Entries.RemoveAll(x => x.TextId != 0x80);

            stoneCs.Commands.RemoveAll
                (x => (x is CameraCommand)
                && (((CameraCommand)x).StartFrame == 0x281
                || ((CameraCommand)x).StartFrame < startFrame
                || ((CameraCommand)x).StartFrame >= endFrame));


            var exit = stoneCs.Commands.OfType<ExitCommand>().Single();
            short exitDur = 10;
            exit.StartFrame = (short)(endFrame-exitDur);
            exit.EndFrame = endFrame;
            
            CutsceneDeleteSection(stoneCs, endFrame, 0x7000);
            CutsceneDeleteSection(stoneCs, 0x281, 0x2BD);
            CutsceneDeleteSection(stoneCs, 0, startFrame);

            return stoneCs;
        }

        private static Cutscene GoronRubyCs(List<string> file)
        {
            Cutscene stoneCs;
            int csAddr = 0x22210B0;
            short endFrame = 0x430;

            stoneCs = GetCutscene(file, csAddr);

            stoneCs.Commands.OfType<TextCommand>().Single()
                .Entries.RemoveAll(x => x.TextId != 0x81);

            var exit = stoneCs.Commands.OfType<ExitCommand>().Single();
            exit.StartFrame = (short)(endFrame - 1);
            exit.EndFrame = endFrame;

            CutsceneDeleteSection(stoneCs, endFrame, short.MaxValue);

            return stoneCs;
        }

        private static Cutscene ZoraSapphireCs(List<string> file)
        {
            Cutscene stoneCs;
            int csAddr = 0x2112D10;
            short endFrame = 0x403;

            stoneCs = GetCutscene(file, csAddr);

            stoneCs.Commands.OfType<TextCommand>().Single()
                .Entries.RemoveAll(x => x.TextId != 0x82);

            var exit = stoneCs.Commands.OfType<ExitCommand>().Single();
            exit.StartFrame = (short)(endFrame - 1);
            exit.EndFrame = endFrame;

            CutsceneDeleteSection(stoneCs, endFrame, short.MaxValue);

            return stoneCs;
        }

        #region Medallion Cs

        private static Cutscene WaterMedallionCs(List<string> file)
        {
            Cutscene waterMedallion;
            int csAddr = 0xD59538;
            short startFrame = 0x241;
            short endFrame = 0x442; //Exit command should be shifted to be 1 more than this
            short adjustedEndFrame = (short)(endFrame - startFrame);

            waterMedallion = GetCutscene(file, csAddr);
            CutsceneTrimFromStart(waterMedallion, startFrame);

            var exitCommand = waterMedallion.Commands.OfType<ExitCommand>().Single();
            TrimExitCommand(exitCommand,
                (short)(exitCommand.StartFrame - adjustedEndFrame));
            
            exitCommand.Asm = 0x29;

            var textCommand = waterMedallion.Commands.OfType<TextCommand>().Single();
            textCommand.Entries = textCommand.Entries.Where(x => x.TextId == 0x3D).ToList();

            CutScreenTransitions(waterMedallion, adjustedEndFrame);

            return waterMedallion;
        }

        private static Cutscene FireMedallionCs(List<string> file)
        {
            int csAddr = 0xD0FD04;
            short startFrame = 0x120;//0x1D1;
            short trimEnd = 0x45 + 0x1F;
            short textTrimStart = (short)(0x1D1 - startFrame);
            Cutscene fireMedallion;

            fireMedallion = GetCutscene(file, csAddr);
            CutsceneTrimFromStart(fireMedallion, startFrame);

            var exitCommand = fireMedallion.Commands.OfType<ExitCommand>().Single();
            TrimExitCommand(exitCommand, trimEnd);

            foreach (TextCommand t in fireMedallion.Commands.OfType<TextCommand>())
            {
                t.Entries = t.Entries.Where(
                    x =>
                        x.StartFrame >= textTrimStart
                        && x.TextId != 0x303D).ToList();
            }

            CutScreenTransitions(fireMedallion, trimEnd);

            return fireMedallion;
        }

        private static Cutscene SpiritMedallionCs(List<string> file)
        {
            Cutscene spiritmedallion;
            int csAddr = 0xD39CA8;
            short startFrame = 0x12A;
            short trimEnd = 0x65;

            spiritmedallion = GetCutscene(file, csAddr);
            CutsceneTrimFromStart(spiritmedallion, startFrame);

            var exit = spiritmedallion.Commands.OfType<ExitCommand>().Single();
            TrimExitCommand(exit, trimEnd);

            var remove = new List<CutsceneCommand>(); 

            foreach (CameraCommand t in spiritmedallion.Commands.OfType<CameraCommand>())
            {
                if (t.Entries.Count <= 4)
                    remove.Add(t);
            }
            foreach (CutsceneCommand c in remove)
                spiritmedallion.Commands.Remove(c);

            foreach (TextCommand t in spiritmedallion.Commands.OfType<TextCommand>())
            {
                t.Entries = t.Entries.Where(
                    x =>
                        x.TextId == 0xFFFF
                        || x.TextId == 0x003F).ToList();
            }

            CutScreenTransitions(spiritmedallion, trimEnd);

            return spiritmedallion;
        }

        private static Cutscene ShadowMedallionCs(List<string> file)
        {
            Cutscene shadowmedallion;

            int csAddr = 0xD13A38;
            short startFrame = 0x1E2;
            short trimEnd = 0x65;

            shadowmedallion = GetCutscene(file, csAddr);
            CutsceneTrimFromStart(shadowmedallion, startFrame);

            var exit = shadowmedallion.Commands.OfType<ExitCommand>().Single();
            TrimExitCommand(exit, trimEnd);

            var remove = new List<CutsceneCommand>();

            foreach (CameraCommand t in shadowmedallion.Commands.OfType<CameraCommand>())
            {
                if (t.Entries.Count <= 4)
                    remove.Add(t);
            }

            foreach (CutsceneCommand c in remove)
                shadowmedallion.Commands.Remove(c);

            foreach (TextCommand t in shadowmedallion.Commands.OfType<TextCommand>())
            {
                t.Entries = t.Entries.Where(
                    x =>
                        x.TextId == 0xFFFF
                        || x.TextId == 0x0041).ToList();
            }

            CutScreenTransitions(shadowmedallion, trimEnd);

            return shadowmedallion;
        }

        private static Cutscene ForestMedallionCs(List<string> file)
        {
            Cutscene forestmedallion;
            int csAddr = 0xD4EA58;
            short startFrame = 0x120 + 0x2D; //0x412;
            short trimEnd = 0x44 + 0x1F;
            short textTrim = (short)(0x01D1 - startFrame);

            forestmedallion = GetCutscene(file, csAddr);
            CutsceneTrimFromStart(forestmedallion, startFrame);
            var exitCommand = forestmedallion.Commands.OfType<ExitCommand>().Single();

            TrimExitCommand(exitCommand, trimEnd);

            short endcutoff = exitCommand.StartFrame;
            endcutoff--;

            var screenFxTrim =
                forestmedallion.Commands
                .OfType<ScreenTransitionCommand>()
                .Where(x => x.StartFrame == endcutoff)
                .Single();

            forestmedallion.Commands.Remove(screenFxTrim);

            foreach (TextCommand t in forestmedallion.Commands.OfType<TextCommand>())
            {
                t.Entries = t.Entries.Where(
                    x =>
                        x.StartFrame >= textTrim
                        && x.TextId != 0x106B).ToList();
            }

            ExitCommand exit = forestmedallion.Commands.OfType<ExitCommand>().Single();
            //ExitCommand newExit = new ExitCommand(exit);
            exit.Asm = 0x45;
            //forestmedallion.Commands.Add(newExit);
            return forestmedallion;
        }
        
        private static Cutscene LightMedallionCs(List<string> file)
        {
            int csAddr = 0x2511120;
            short startFrame = 0x42F;//0x412;
            short endFrame = (short)(0x171 + 0x42F - startFrame);
            Cutscene lightmedallion;

            lightmedallion = GetCutscene(file, csAddr);
            CutsceneTrimFromStart(lightmedallion, startFrame);

            ExitCommand exit = lightmedallion.Commands.OfType<ExitCommand>().Single();
            short exitTrim = (short)(exit.StartFrame - endFrame-1);
            exit.StartFrame -= exitTrim;
            exit.EndFrame -= exitTrim;

            List<CutsceneCommand> remove = new List<CutsceneCommand>();

            foreach (var item in  lightmedallion.Commands.OfType<ScreenTransitionCommand>())
            {
                if (!(item.StartFrame < endFrame))
                    remove.Add(item);
            }
            foreach (var item in remove)
                lightmedallion.Commands.Remove(item);

            foreach (var item in lightmedallion.Commands.OfType<TextCommand>())
            {
                item.Entries = item.Entries.Where(
                    x =>
                        x.StartFrame < endFrame).ToList();
            }
    
            lightmedallion.Commands.Insert(lightmedallion.Commands.Count - 2, new ScreenTransitionCommand(9, 0x32, 0x9));
            return lightmedallion;
        }

        #endregion

        private static Cutscene LightArrowCs(List<string> file)
        {
            Cutscene LightArrowCsFull,
                SheikCs,
                TransformCs,
                GetLightArrowCs;
            int SheikCsAddr = 0x25314E0;
            int TransformationCsAddr = 0x2531BC0;
            int GetLightArrowCsAddr = 0x2532F70;
            List<CutsceneCommand> remove = new List<CutsceneCommand>();

            short sheikCsTrimStart = 0xA0;

            LightArrowCsFull = new Cutscene()
            {
                Frames = 0xC00
            };
            SheikCs = GetCutscene(file, SheikCsAddr);

            //LightArrowCsFull.Commands = SheikCs.Commands.Where
            //    (x => !(x is TextCommand) &&
            //    x.IFrameDataEnum.Where(y => y.StartFrame < sheikCsTrimStart).Count() > 0)
            //    .ToList();

            //
            // Transformation part
            //
            short curFrame = sheikCsTrimStart;
            short transformTrimStart = 0x12C;
            short transformTrimEnd = 0x302;

            TransformCs = GetCutscene(file, TransformationCsAddr);

            //TransformCs.Commands = TransformCs.Commands.Where
            //    (x => !(x is TextCommand)
            //    && x.IFrameDataEnum.Where(y => y.StartFrame < transformTrimEnd).Count() > 0)
            //    .ToList();

            remove = TransformCs.Commands.OfType<CameraCommand>()
                .Where(x => x.StartFrame < transformTrimStart)
                .Cast<CutsceneCommand>().ToList();

            remove.Add(TransformCs.Commands.OfType<ActionCommand>()
                .Single(x => x.Command == 0x21));

            foreach (var i in remove)
                TransformCs.Commands.Remove(i);

            foreach (var i in TransformCs.Commands.OfType<ActionCommand>().Where(x => x.Command == 0x4))
                foreach (var action in i.IFrameDataEnum.Cast<ActionEntry>())
                    action.Action += 6;

            CutsceneTrimFromEnd(TransformCs, transformTrimEnd);
            CutsceneTrimFromStart(TransformCs, transformTrimStart);
            CutsceneTrimFromStart(TransformCs, (short)(-curFrame)); //a hack

            MergeCutsceneCommands(LightArrowCsFull, TransformCs);
            TransformCs = null;
        
            // 
            // Obtain Light Arrows (Section 1)
            //
            short getLightArrowTrimStart = 0x122;
            short getLightArrowTrimEnd = 0x582;//0x488;
            curFrame += (short)(transformTrimEnd - transformTrimStart);
            GetLightArrowCs = GetCutscene(file, GetLightArrowCsAddr);
            remove.Clear();

            {
                var ExitCmd = GetLightArrowCs.Commands.OfType<ExitCommand>().Single();
                TrimExitCommand(ExitCmd, (short)(ExitCmd.EndFrame - getLightArrowTrimEnd+1));
            }
            
            GetLightArrowCs.Commands = GetLightArrowCs.Commands.OfType<IFrameCollection>().Where
                (x => x.IFrameDataEnum.Where(y => y.StartFrame < getLightArrowTrimEnd).Count() > 0).Cast<CutsceneCommand>()
                .ToList();
            {
                var TextCmd = GetLightArrowCs.Commands.OfType<TextCommand>().Single();
                TextCmd.Entries = TextCmd.Entries.Where(x => x.TextId == 0x72).ToList();
            }
            //modify zeldo movement
            {
                var ZeldaCmd = GetLightArrowCs.Commands.OfType<ActionCommand>()
                    .Where(x => x.Command == 0x55).Single();

                var ZeldaCmds = ZeldaCmd.IFrameDataEnum.Cast<ActionEntry>()
                    .Where(x => x.Action == 0xD).OrderBy(y =>y.StartFrame).ToList();

                ZeldaCmds[0].Action = 0xE;
                ZeldaCmds[0].EndFrame = (short)(ZeldaCmds[0].StartFrame + 0x13);
                ZeldaCmds[0].EndPosition = ZeldaCmds[0].StartPosition;

                for (int i = 1; i < ZeldaCmds.Count; i++)
                    ZeldaCmd.RemoveEntry(ZeldaCmds[i]);
            }

            remove = GetLightArrowCs.Commands.OfType<CameraCommand>()
                .Where(x => x.StartFrame < getLightArrowTrimStart || x.StartFrame == 0x316)
                .Cast<CutsceneCommand>().ToList();

            remove.Add(GetLightArrowCs.Commands.OfType<ActionCommand>()
                .Single(x => x.Command == 0x21));

            foreach (var i in remove)
                GetLightArrowCs.Commands.Remove(i);

            GetLightArrowCs.Commands.RemoveAll(x => x.Command == 0x03 || x.Command == 0x09);

            //{
            //    var environmentCmd = GetLightArrowCs.Commands.OfType<ActorCommand>()
            //        .Where(x => x.Command == 0x04
            //        && x.IFrameDataEnum.Cast<ActionEntry>().Single().StartFrame == 0x316).Single();
            //    foreach (var item in environmentCmd.IFrameDataEnum)
            //    {
            //        item.StartFrame--;
            //        item.EndFrame--;
            //    }
            //}

            CutsceneDeleteSection(GetLightArrowCs, (0x316+1), 0x3AC);
            CutsceneDeleteSection(GetLightArrowCs, 0x348-1, 0x37A-1);
            CutsceneTrimFromEnd(GetLightArrowCs, getLightArrowTrimEnd);
            CutsceneTrimFromStart(GetLightArrowCs, getLightArrowTrimStart);
            CutsceneTrimFromStart(GetLightArrowCs, (short)(-curFrame)); //a hack
            
            {
                var Zelda = GetLightArrowCs.Commands.OfType<ActionCommand>().Single(x => x.Command == 0x55);
                var ZeldaEntry = (ActionEntry)Zelda.IFrameDataEnum.Aggregate(
                    (curMin, x) =>
                    (curMin == null || (x.StartFrame < curMin.StartFrame) ? x : curMin));
                ZeldaEntry.Action = 3;
                ZeldaEntry.StartPosition = new mzxrules.Helper.Vector3<int>(0, -40, 2310);

            }

            MergeCutsceneCommands(LightArrowCsFull, GetLightArrowCs);
            
            LightArrowCsFull.Commands.Add(new EndCommand());
            return LightArrowCsFull;
        }

        /// <summary>
        /// Removes a section of a cutscene. Camera focus data is not altered
        /// </summary>
        /// <param name="cutscene"></param>
        /// <param name="cutStart">First frame to remove</param>
        /// <param name="cutEnd">First frame to keep</param>
        private static void CutsceneDeleteSection(Cutscene cutscene, short cutStart, short cutEnd)
        {
            var deleteIFrameCollection = CutsceneDeleteSection
                (cutscene.Commands.OfType<IFrameCollection>(), cutStart, cutEnd);

            var deleteIFrameData = CutsceneDeleteSection
                (cutscene.Commands.OfType<IFrameData>(), cutStart, cutEnd);
            

            foreach (var item in deleteIFrameCollection)
                cutscene.Commands.Remove((CutsceneCommand) item);

            foreach (var item in deleteIFrameData)
                cutscene.Commands.Remove((CutsceneCommand)item);
            
        }
        

        /// <summary>
        /// Fuck this is crazy
        /// </summary>
        /// <param name="iFrameCollectionList"></param>
        /// <param name="cutStart">Inclusive</param>
        /// <param name="cutEnd">Exclusive</param>
        /// <returns>List of IFrameCollections to delete</returns>
        private static List<IFrameCollection> CutsceneDeleteSection
            (IEnumerable<IFrameCollection> iFrameCollectionList, short cutStart, short cutEnd)
        {
            List<IFrameCollection> delete = new List<IFrameCollection>();
            foreach (IFrameCollection item in iFrameCollectionList)
            {
                List<IFrameData> remove = CutsceneDeleteSection(item.IFrameDataEnum, cutStart, cutEnd);
                foreach (var i in remove)
                    item.RemoveEntry(i);
            }
            delete = iFrameCollectionList.Where(x => x.IFrameDataEnum.Count() == 0).ToList();
            return delete;
        }

        /// <summary>
        /// Trims cutscene, and lists records to be removed
        /// </summary>
        /// <param name="iFrameDataEnumerable"></param>
        /// <param name="cutStart">Inclusive</param>
        /// <param name="cutEnd">Exclusive</param>
        /// <returns></returns>
        private static List<IFrameData> CutsceneDeleteSection
            (IEnumerable<IFrameData> iFrameDataEnumerable,  short cutStart, short cutEnd)
        {
            List<IFrameData> remove = new List<IFrameData>();
            short shift = (short)(cutEnd - cutStart);

            foreach (var item in iFrameDataEnumerable)
            {
                //cut range = 2-5
                //0,3  3,4  4,7  0,7
                // becomes
                //0,2  n/a  2,4  0,4

                //ends span the cut portion
                if (item.StartFrame < cutStart && item.StartFrame >= item.EndFrame)
                    item.EndFrame -= shift;
                else //one or two ends are within the cut portion
                if (item.StartFrame < cutStart) //end frame is in cut space
                    item.EndFrame = (item.EndFrame > cutStart) ? cutStart : item.EndFrame;
                else if (item.EndFrame >= cutEnd) //start frame is in cut space
                    item.StartFrame = (item.StartFrame < cutEnd) ? cutEnd : item.StartFrame;
                else
                    remove.Add(item);

                if (item.StartFrame >= cutEnd)
                {
                    item.StartFrame -= shift;
                    item.EndFrame -= shift;
                }
            }
            return remove;
        }

        private static void MergeCutsceneCommands(Cutscene merge, Cutscene copyFrom)
        {
            foreach (CutsceneCommand c in copyFrom.Commands)
                merge.Commands.Add(c);
        }

        private static void CutsceneTrimFromEnd(Cutscene cutscene, short frameEnd)
        {
            //List<CutsceneCommand> remove = new List<CutsceneCommand>();
            //foreach (IFrameCollection command in cutscene.Commands.OfType<IFrameCollection>())
            //{
            //    List<IFrameData> delData = new List<IFrameData>();
            //    foreach (var frameItem in command.IFrameDataEnum)
            //    {
            //        frameItem.EndFrame = (frameEnd > frameItem.EndFrame) ?
            //            frameItem.EndFrame : frameEnd;
            //        if (frameItem.EndFrame <= frameItem.StartFrame)
            //            delData.Add(frameItem);
            //    }
            //    foreach (var i in delData)
            //        command.RemoveEntry(i);

            //    if (command.IFrameDataEnum.Count() == 0)
            //        remove.Add((CutsceneCommand)command);
            //}
            //foreach (CutsceneCommand c in remove)
            //    cutscene.Commands.Remove(c);

            List<CutsceneCommand> remove = new List<CutsceneCommand>();
            foreach (var command in cutscene.Commands)
            {
                if (command is IFrameCollection frameCollection)
                {
                    List<IFrameData> delData = new List<IFrameData>();
                    foreach (var item in frameCollection.IFrameDataEnum)
                    {
                        item.EndFrame = (frameEnd > item.EndFrame) ?
                            item.EndFrame : frameEnd;
                        if (item.EndFrame <= item.StartFrame)
                            delData.Add(item);
                    }
                    foreach (var item in delData)
                        frameCollection.RemoveEntry(item);

                    if (frameCollection.IFrameDataEnum.Count() == 0)
                        remove.Add((CutsceneCommand)frameCollection);
                }
            }
            foreach (CutsceneCommand c in remove)
                cutscene.Commands.Remove(c);

        }

        private static void CutsceneTrimFromStart(Cutscene cs, short frame)
        {
            cs.Frames -= frame;
            TrimFromStart(cs.Commands, frame);
        }

        private static void TrimFromStart(List<CutsceneCommand> Commands, short frame)
        {
            List<CutsceneCommand> RemoveCommand = new List<CutsceneCommand>();
            foreach (var commandIFrameCollection in Commands.OfType<IFrameCollection>())
            {
                var command = (CutsceneCommand)commandIFrameCollection;
                List<IFrameData> RemoveIFrameData = new List<IFrameData>();
                //loop over individual items
                foreach (var frameItem in commandIFrameCollection.IFrameDataEnum)
                {
                    frameItem.StartFrame = (short)Math.Max(frameItem.StartFrame - frame, 0);
                    frameItem.EndFrame -= frame;

                    if (!(frameItem is CutsceneCommand) && frameItem.EndFrame <= 0)
                        RemoveIFrameData.Add(frameItem);
                }

                if (command is ActionCommand
                    || command is CameraCommand
                    || command is TextCommand)
                {
                    foreach (var entry in RemoveIFrameData)
                    {
                        commandIFrameCollection.RemoveEntry(entry);
                    }
                    if (commandIFrameCollection.IFrameDataEnum.Count() == 0)
                        RemoveCommand.Add(command);
                }
                if (command is CameraCommand
                    && ((CameraCommand)command).Entries.Count <= 4)
                {
                    RemoveCommand.Add(command);
                }
            }
            foreach (IFrameData item in Commands.OfType<IFrameData>())
            {
                item.StartFrame = (short)Math.Max(item.StartFrame - frame, 0);
                item.EndFrame -= frame;
                if (item.EndFrame <= 0)
                    RemoveCommand.Add((CutsceneCommand)item);
            }
            
            foreach (var command in RemoveCommand)
            {
                Commands.Remove(command);
            }
        }

        private static void TrimExitCommand(ExitCommand exitCommand, short trimEnd)
        {
            trimEnd--;
            exitCommand.StartFrame -= trimEnd;
            exitCommand.EndFrame -= trimEnd;
        }

        private static void CutScreenTransitions(Cutscene cutscene, short endFrame)
        {
            List<ScreenTransitionCommand> remove = new List<ScreenTransitionCommand>();

            remove = cutscene.Commands
                .OfType<ScreenTransitionCommand>()
                .Cast<ScreenTransitionCommand>()
                .Where(x => !(x.StartFrame < endFrame))
                .ToList();

            foreach (var item in remove)
                cutscene.Commands.Remove(item);
        }
        
        private static Cutscene GetCutscene(List<string> file, int csAddr)
        {
            Cutscene cs;
            FileRecord record;
            ORom rom = new ORom(file[0], ORom.Build.N0);

            record = rom.Files.GetFileStart(csAddr);
            RomFile f = rom.Files.GetFile(record.VRom);

            f.Stream.Position = f.Record.GetRelativeAddress(csAddr);

            cs = new Cutscene(f);
            return cs;
        }
    }
}