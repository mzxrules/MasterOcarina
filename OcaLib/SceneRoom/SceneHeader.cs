using mzxrules.Helper;
using mzxrules.OcaLib.Actor;
using mzxrules.OcaLib.SceneRoom.Commands;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mzxrules.OcaLib.SceneRoom
{   
    public partial class SceneHeader
    {
        public Game Game { get; private set; }

        public AlternateHeadersCommand Alternate
        {
            get { return (AlternateHeadersCommand)this[HeaderCommands.AlternateHeaders]; }
        }

        public SceneCommand this[HeaderCommands c]
        {
            get
            {
                return cmds.SingleOrDefault(x => x.Code == (int)c);
            }
        }

        List<SceneCommand> cmds = new List<SceneCommand>();

        public long Offset { get; private set; }

        public SceneHeader(Game game)
        {
            Game = game;
        }

        /// <summary>
        /// Creates an in-memory model of the scene header
        /// </summary>
        /// <param name="br">Binary reader containing the scene or room file</param>
        /// <param name="seek">offset to the start of the scene header</param>
        public void Load(BinaryReader br, long seek)
        {
            bool KeepReading = true;
            long seekBackTop;
            Offset = seek;

            seekBackTop = br.BaseStream.Position;
            br.BaseStream.Position = seek;

            while (KeepReading)
            {
                SceneWord command = new SceneWord();
                br.Read(command, 0, 8);

                SetCommand(command, br.BaseStream.Position - 8);
                if ((HeaderCommands)command.Code == HeaderCommands.End)
                {
                    KeepReading = false;
                }
            }
            br.BaseStream.Position = seekBackTop;

            if (HasAlternateHeaders())
            {
                Alternate.HeaderListEndAddress = AltHeaderEnd();
                Alternate.Initialize(br);
            }
        }

        public void WriteHeader(BinaryWriter bw)
        {
            foreach (SceneCommand cmd in cmds)
            {
                bw.Write(cmd.Command, 0, SceneWord.LENGTH);
            }
        }

        public void SetCommand(SceneWord sceneWord, long HeaderOffset)
        {
            SceneCommand command;
             
            switch ((HeaderCommands)sceneWord.Code)
            {
                case HeaderCommands.PositionList:       //0x00
                    command = new ActorSpawnCommand(Game);
                    break;
                case HeaderCommands.ActorList:          //0x01
                    command = new ActorSpawnCommand(Game);
                    break;
                case HeaderCommands.Collision:          //0x03
                    command = new CollisionCommand();
                    break;
                case HeaderCommands.RoomList:           //0x04
                    command = new RoomListCommand();
                    break;
                case HeaderCommands.WindSettings:       //0x05
                    command = new SettingsCommand();
                    break;
                case HeaderCommands.EntranceDefs:       //0x06
                    command = new EntranceDefinitionsCommand();
                    break;
                case HeaderCommands.SpecialObject:      //0x07
                    command = new SettingsCommand();
                    break;
                case HeaderCommands.RoomBehavior:       //0x08
                    command = new RoomBehaviorCommand(Game);
                    break;
                case HeaderCommands.RoomMesh:           //0x0A
                    command = new RoomMeshCommand();
                    break;
                case HeaderCommands.ObjectList:         //0x0B
                    command = new ObjectListCommand();
                    break;
                case HeaderCommands.PathList:           //0x0D
                    command = new PathListCommand();
                    break;
                case HeaderCommands.TransitionActorList://0x0E
                    command = new TransitionActorListCommand(Game);
                    break;
                case HeaderCommands.EnvironmentSettings://0x0F
                    command = new EnvironmentSettingsCommand();
                    break;
                case HeaderCommands.TimeSettings:       //0x10
                    command = new SettingsCommand();
                    break;
                case HeaderCommands.SkyboxSettings:     //0x11
                    command = new SettingsCommand();
                    break;
                case HeaderCommands.SkyboxModifier:     //0x12
                    command = new SettingsCommand();
                    break;
                case HeaderCommands.ExitList:           //0x13
                    command = new ExitListCommand();
                    break;
                case HeaderCommands.End:                //0x14
                    command = new EndCommand();
                    break;
                case HeaderCommands.SoundSettings:      //0x15
                    command = new SettingsCommand();
                    break;
                case HeaderCommands.SoundSettingsEcho:  //0x16
                    command = new SettingsCommand();
                    break;
                case HeaderCommands.Cutscene:           //0x17
                    command = new CutsceneCommand(Game);
                    break;
                case HeaderCommands.AlternateHeaders:   //0x18
                    command = new AlternateHeadersCommand(Game);
                    break;
                case HeaderCommands.JpegBackground:     //0x19
                    command = new SettingsCommand();
                    break;
                default: command = new SceneCommand(); 
                    break;
            }

            command.SetCommand(sceneWord);
            command.OffsetFromFile = HeaderOffset;
            cmds.Add(command);
        }

        public void InitializeAssets(BinaryReader br)
        {
            foreach (IDataCommand asset in cmds.Where(x => x is IDataCommand && !(x is AlternateHeadersCommand)))
            {
                if (asset is ExitListCommand)
                {
                    ((ExitListCommand)asset).EndOffset = ExitListEnd();
                }
                else if (asset is EntranceDefinitionsCommand)
                {
                    var v = (ActorSpawnCommand)cmds.Single(x => x.Code == (int)HeaderCommands.PositionList);
                    ((EntranceDefinitionsCommand)asset).Entrances = v.Actors;
                }
                if (asset is ActorSpawnCommand)
                {

                    //ActorList list = new ActorList(Game, )
                }
                asset.Initialize(br);
            }
        }

        public string Read()
        {
            StringBuilder s = new StringBuilder();
            HeaderCommands cmd;

            foreach (SceneCommand command in cmds)
            {
                cmd = (HeaderCommands)command.Code;
                if (cmd != HeaderCommands.End)
                {
                    s.Append($"{(int)cmd:X2}: ");

                    if (this[cmd] != null)
                        s.AppendLine(this[cmd].Read());
                }
                else
                    break;
            }
            return s.ToString();
        }

        #region AlternateHeaders
        public bool HasAlternateHeaders()
        {
            return (this[HeaderCommands.AlternateHeaders] != null);
        }

        /// <summary>
        /// Gets alternate setup count for scene files only
        /// </summary>
        /// <returns></returns>
        public int AltHeaderEnd()
        {
            SceneCommand sc;

            sc = this[HeaderCommands.PositionList];

            if (sc != null)
                return ((IDataCommand)sc).SegmentAddress.Offset;
            else if ((IDataCommand)this[HeaderCommands.ObjectList] != null)
                return ((IDataCommand)this[HeaderCommands.ObjectList]).SegmentAddress.Offset;
            else 
                return ((IDataCommand)this[HeaderCommands.RoomMesh]).SegmentAddress.Offset;
        }
        #endregion

        public long ExitListEnd()
        {
            if (this[HeaderCommands.EnvironmentSettings] != null)
                return ((IDataCommand)this[HeaderCommands.EnvironmentSettings]).SegmentAddress.Offset;
            else return 0;
        }


        /// <summary>
        /// Gets all room addresses from the header and child headers
        /// </summary>
        /// <returns>Returns a list of rooms if the RoomListCommand is found, else returns null</returns>
        public List<FileAddress> GetRoomAddresses()
        {
            List<FileAddress> resultAddresses = new List<FileAddress>();
            RoomListCommand cmd;

            cmd = (RoomListCommand)this[HeaderCommands.RoomList];
            if (cmd == null)
                return null;

            foreach (FileAddress addr in cmd.RoomAddresses)
                resultAddresses.Add(addr);

            if (HasAlternateHeaders())
            {
                //for every scene setup
                foreach (SceneHeader altHeader in Alternate.HeaderList.Where(x => x != null))
                {
                    //for every room in that scene setup
                    cmd = (RoomListCommand)altHeader[HeaderCommands.RoomList];

                    for (int i = 0; i < cmd.Rooms; i++)
                    {
                        if (!resultAddresses.Contains(cmd.RoomAddresses[i]))
                        {
                            resultAddresses.Add(cmd.RoomAddresses[i]);
                            break;
                        }
                    }
                }
            }
            return resultAddresses;
        }

        private IEnumerable<SceneHeader> GetAltHeaders()
        {
            return from alt in Alternate.HeaderList
                   where alt != null
                   select alt;
        }

        #region GetActors
        public List<List<ActorSpawn>> GetActorsWithId(int id)
        {
            AlternateHeadersCommand altCmd;
            List<List<ActorSpawn>> result;

            result = new List<List<ActorSpawn>>
            {
                GetActorsById(id)
            };
            altCmd = (AlternateHeadersCommand)this[HeaderCommands.AlternateHeaders];
            if (altCmd != null)
            {
                for (int i = 0; i < altCmd.HeaderList.Count; i++)
                {
                    if (altCmd.HeaderList[i] != null)
                        result.Add(altCmd.HeaderList[i].GetActorsById(id));
                    else
                        result.Add(new List<ActorSpawn>());
                }
            }
            return result;
        }

        private List<ActorSpawn> GetActorsById(int id)
        {
            List<ActorSpawn> result;
            result = new List<ActorSpawn>();
            IEnumerable<SceneCommand> cmdQuery;

            //Linq query
            cmdQuery = from cmd in cmds
                       where cmd is IActorList
                       select cmd;

            foreach (IActorList actorList in cmdQuery)
            {
                if (id == -1)
                    result.AddRange(actorList.GetActors());
                else
                    result.AddRange(actorList.GetActors().Where(x => x.Actor == id));
            }

            return result;
        }
        #endregion

        #region GetObjects
        public List<List<ushort>> GetObjectsWithId(int id)
        {
            AlternateHeadersCommand altCmd;
            List<List<ushort>> result;

            result = new List<List<ushort>>
            {
                GetObjectsById(id)
            };
            altCmd = (AlternateHeadersCommand)this[HeaderCommands.AlternateHeaders];
            if (altCmd != null)
            {
                for (int i = 0; i < altCmd.HeaderList.Count; i++)
                {
                    if (altCmd.HeaderList[i] != null)
                        result.Add(altCmd.HeaderList[i].GetObjectsById(id));
                    else
                        result.Add(new List<ushort>());
                }
            }
            return result;
        }

        private List<ushort> GetObjectsById(int id)
        {
            List<ushort> result;
            IEnumerable<SceneCommand> cmdQuery;

            result = new List<ushort>();
            cmdQuery = from cmd in cmds
                       where cmd is ObjectListCommand
                       select cmd;

            foreach (ObjectListCommand objectList in cmdQuery)
            {
                if (id == -1)
                    result.AddRange(objectList.ObjectList);
                else
                    result.AddRange(objectList.ObjectList.Where(x => x == id));
            }
            return result;
        }
        #endregion

        internal List<SceneCommand> GetCommandWithId(int id)
        {
            List<SceneCommand> current;
            SceneCommand cmd;
            current = new List<SceneCommand>();

            cmd = this.cmds.SingleOrDefault(x => x.Code == id);
            if (cmd != null)
                current.Add(cmd);

            return current;

        }

        internal List<List<SceneCommand>> GetAllCommandsWithId(int id)
        {
            List<List<SceneCommand>> result = new List<List<SceneCommand>>
            {
                GetCommandWithId(id)
            };
            if (HasAlternateHeaders())
            {
                foreach (SceneHeader h in Alternate.HeaderList)
                {
                    if (h == null)
                        result.Add(new List<SceneCommand>());
                    else
                        result.Add(h.GetCommandWithId(id));
                }
            }
            return result;
        }

        public List<SceneCommand> Commands()
        {
            return cmds;
        }
    }
}
