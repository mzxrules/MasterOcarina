using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;

namespace Atom
{
    public static class JQuery
    {
        public static List<FunctionInfo> GetFunctionInfo(RomVersion version)
        {
            string game = version.GameAbbr;
            string ver = version.VerAbbr;

            string filepath = $"data/{game}-{ver}.json";
            if (File.Exists(filepath))
            {
                return (List<FunctionInfo>)Deserialize(typeof(List<FunctionInfo>), filepath);
            }

            return new List<FunctionInfo>();
        }

        private static object Deserialize(Type type, string path)
        {
            DataContractJsonSerializer serializer;

            serializer = new DataContractJsonSerializer(type);
            object result;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                result = serializer.ReadObject(fs);
            }
            return result;

        }

        public static T Deserialize<T>(string path)
        {
            var data = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<T>(data);
        }


        public static void Serialize<T>(string path, T obj)
        {
            JsonSerializer serializer = new JsonSerializer();
            using var fs = File.CreateText(path);
            serializer.Serialize(fs, obj, typeof(T));
        }
    }
}
