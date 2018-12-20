using mzxrules.Helper;
using System.IO;

namespace mzxrules.OcaLib
{
    public class EntranceTableRecord
    {
        public byte Scene;
        public byte Spawn;
        public short Variables;

        public EntranceTableRecord (BinaryReader br)
        {
            Scene = br.ReadByte();
            Spawn = br.ReadByte();
            Variables = br.ReadBigInt16();
        }
    }
}
