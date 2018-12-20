using mzxrules.Helper;
using System;

namespace mzxrules.OcaLib.SceneRoom.Commands
{
    public class CollisionCommand : SceneCommand, IDataCommand
    {
        public SegmentAddress SegmentAddress { get; set; }
        public CollisionMesh Mesh;

        public override void SetCommand(SceneWord command)
        {
            base.SetCommand(command);
            SegmentAddress = Command.Data2;
            if (SegmentAddress.Segment != (byte)ORom.Bank.scene)
                throw new Exception();
        }

        public void Initialize(System.IO.BinaryReader br)
        {
            Mesh = new CollisionMesh(SegmentAddress);
            Mesh.Initialize(br);
        }
        public override string Read()
        {
            return ToString() + Environment.NewLine + Mesh.Print();
        }
        public override string ToString()
        {
            return $"Collision Header starts at {SegmentAddress.Offset:X8}";
        }
    }
}
