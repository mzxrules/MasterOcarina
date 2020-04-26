using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    /// <summary>
    /// Class intended to log various data structures used in the Zelda64 engine
    /// </summary>
    /// <typeparam name="T">Type of the object that will be serialized</typeparam>
    [DataContract]
    public class ReferenceLogger<T>
    {
        [DataContract]
        public class Reference<U>
        {
            [DataMember]
            public RomVersion Version { get; set; }

            [DataMember]
            public int VRom { get; set; }

            [DataMember]
            public int Offset { get; set; }

            [DataMember]
            public U Data { get; set; }

            //public override bool Equals(object obj)
            //{
            //    if (obj == null)
            //        return false;

            //    Reference<U> e = obj as Reference<U>;
            //    if ((object)e == null)
            //        return false;

            //    return (Version == e.Version)
            //        && (VRom + Offset == e.VRom + e.Offset);
            //}

            //public override int GetHashCode()
            //{
            //    return base.GetHashCode();
            //}

            public static bool operator ==(Reference<U> a, Reference<U> b)
            {
                // If both are null, or both are same instance, return true.
                if (ReferenceEquals(a, b))
                {
                    return true;
                }

                // If one is null, but not both, return false.
                if (a is null || b is null)
                {
                    return false;
                }

                return a.Version == b.Version && a.VRom == b.VRom && a.Offset == b.Offset;
            }
            public static bool operator !=(Reference<U> a, Reference<U> b)
            {
                return !(a == b);
            }


            public override string ToString()
            {
                return $"{Version.ToString()}\t{VRom:X8}\t{Offset:X6}\t{Data.ToString()}";
            }
        }

        [DataMember]
        public List<Reference<T>> LoggedReferences { get; set; } = new List<Reference<T>>();

        internal void AddReference(RomVersion version, IFile file, int address, T data)
        {
            var item = new Reference<T>()
            {
                Version = version,
                VRom = file.VRom.Start,
                Offset = (address & 0xFFFFFF) - (file.Ram.Start.Offset),
                Data = data
            };
            if (LoggedReferences.Exists(x =>
                x.Version == item.Version
                && x.VRom + x.Offset == item.VRom + item.Offset))
            {
                LoggedReferences.Add(item);
            }
        }
        
        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
            LoggedReferences = LoggedReferences
                .OrderBy(x => x.Version.ToString())
                .ThenBy(x => x.VRom)
                .ThenBy(x => x.Offset).ToList();
        }
    }
}
