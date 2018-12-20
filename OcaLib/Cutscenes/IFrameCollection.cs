using System.Collections.Generic;

namespace mzxrules.OcaLib.Cutscenes
{
    public interface IFrameCollection
    {
        IEnumerable<IFrameData> IFrameDataEnum { get; }
        IEnumerable<IFrameData> GetIFrameDataEnumerator();

        void AddEntry(IFrameData d);
        void RemoveEntry(IFrameData d);
    }
}