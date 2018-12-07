using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Atom
{
    public static class JQuery
    {
        public static List<FunctionInfo> GetFunctionInfo(RomVersion ver)
        {
            string game = ver.GetGameAbbr();

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
            DataContractJsonSerializer serializer;

            serializer = new DataContractJsonSerializer(typeof(T));

            T result;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                result = (T)serializer.ReadObject(fs);
            }
            return result;
        }


        public static void Serialize<T>(string path, T obj)
        {
            DataContractJsonSerializer serializer;

            serializer = new DataContractJsonSerializer(typeof(T));

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                serializer.WriteObject(fs, obj);
            }
        }
    }
}
