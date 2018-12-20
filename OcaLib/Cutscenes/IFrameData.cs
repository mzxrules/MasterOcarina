namespace mzxrules.OcaLib.Cutscenes
{
    public interface IFrameData
    {
        CutsceneCommand RootCommand { get; set; }
        short StartFrame { get; set; }
        short EndFrame { get; set; }
    }
}