using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace JOcaBase
{
    public static class JQuery
    {
        public static List<JScene> OScenes;
        public static List<JFiles> OFiles;
        public static List<JActor> OActors;
        public static List<JScene> MScenes;
        public static List<JFiles> MFiles;
        public static List<JActor> MActors;
        static JQuery()
        {
            OScenes = Deserialize<List<JScene>>(@"base/OOT/Scenes.json");
            OFiles = Deserialize<List<JFiles>>(@"base/OOT/Files.json");
            OActors = Deserialize<List<JActor>>(@"base/OOT/Actors.json");
            MScenes = Deserialize<List<JScene>>(@"base/MM/Scenes.json");
            MFiles = Deserialize<List<JFiles>>(@"base/MM/Files.json");
            MActors = Deserialize<List<JActor>>(@"base/MM/Actors.json");
        }

        public static List<JDmaData> GetOcaDmaData(string version)
        {
            return GetDmaData("OOT", version);
        }

        public static List<JDmaData> GetMaskDmaData(string version)
        {
            return GetDmaData("MM", version);
        }

        private static List<JDmaData> GetDmaData(string game, string version)
        {
            version = version.ToUpper();
            return Deserialize<List<JDmaData>>($@"base/{game}/{version}/dmadata.json");
        }


        public static JActor GetActor(int actor)
        {
            return OActors.SingleOrDefault(x => x.Int_Id == actor);
        }

        private static T Deserialize<T>(string path)
        {
            var data = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}