using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    [DataContract]
    class CollisionAutoDoc
    {
        public Dictionary<int, Record> Meshes { get; set; }

        [DataMember]
        public List<Record> Mesh { get; set; }

        [DataContract]
        public class Record
        {
            [DataMember]
            public FileData File { get; set; }

            [DataMember]
            public int Offset { get; set; }

            [DataMember]
            public List<RomVersion> Versions { get; set; }

            [DataMember]
            public List<int> Actors { get; set; }

            private void Initialize()
            {
                if (Actors == null)
                    Actors = new List<int>();

                if (Versions == null)
                    Versions = new List<RomVersion>();
            }

            [DataContract]
            public class FileData
            {
                [DataMember]
                public int Index { get; set; }

                [DataMember]
                public FileType Type { get; set; }

                public FileData(int index, FileType type)
                {
                    Index = index;
                    Type = type;
                }
            }

            public enum FileType
            {
                Object = 0,
                Actor = 1,
                Code = 2,
                Other = 3
            }

            public Record(BgActor actor, IFile file, RomVersion version)
            {
                Initialize();
                int ramStart;
                if (file is RamObject obj)
                {
                    ramStart = obj.Ram.Start;
                    File = new FileData(obj.Object, FileType.Object);
                }
                else if (file is OvlActor af)
                {
                    ramStart = af.Ram.Start;
                    File = new FileData(af.Actor, FileType.Actor);
                }
                else
                {
                    throw new NotImplementedException();
                }

                Offset = actor.MeshPtr.Offset - (ramStart & 0xFFFFFF);
                Versions.Add(version);
                Actors.Add(actor.ActorId);
            }

            public void Merge(Record record)
            {
                Versions = Versions.Union(record.Versions).ToList();
                Actors = Actors.Union(record.Actors).ToList();
            }

            public Record() { Initialize(); }

            public override int GetHashCode()
            {
                return (((int)File.Type & 1) << 31) + ((File.Index & 0x7FF) << 18) + (Offset & 0xFFFFFF);
            }
            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }

                if (obj is not Record e)
                {
                    return false;
                }

                return File.Type == e.File.Type && File.Index == e.File.Index && (Offset == e.Offset);
            }

            public override string ToString()
            {
                return string.Format($"{File.Type}\t{File.Index:X4}\t{Offset:X6}\t{Actors[0]:X4}");
            }
        }

        public void AddNewRecord(BgActor actor, IFile obj, RomVersion version)
        {
            try
            {
                var record = new Record(actor, obj, version);

                if (Meshes.ContainsKey(record.GetHashCode()))
                {
                    Meshes[record.GetHashCode()].Merge(record);
                }
                else
                    Meshes.Add(record.GetHashCode(), record);
            }
            catch
            {
                Console.WriteLine("UNRECORDED");
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
            Initialize();
        }
        private void Initialize()
        {
            if (Meshes == null)
                Meshes = new Dictionary<int, Record>();
            if (Mesh == null)
                Mesh = new List<Record>();

            foreach (var item in Mesh)
            {
                Meshes.Add(item.GetHashCode(), item);
            }
        }
        public CollisionAutoDoc() { Initialize(); }
    }
}
