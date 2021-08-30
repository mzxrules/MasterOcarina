using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using XActors1 = mzxrules.XActor.XActors;
namespace mzxrules.XActor
{
    class Test
    {
        public static void Test1()
        {
            XActors1 root = new();
            root.Actor = new List<XActor>
            {
                new XActor()
            };

            XmlWriterSettings xout = new()
            {
                Indent = true
            };

            XmlSerializer serializer = new(typeof(XActors1));

            using (XmlWriter writer = XmlWriter.Create("testNew.txt", xout))
            {
                serializer.Serialize(writer, root);
            }

            using XmlReader reader = XmlReader.Create("testNew.txt");
            XActors1 test = (XActors1)serializer.Deserialize(reader);
        }

        public static void Serialize()
        {

        }

        public static string ParseActorVariables(string[] lines)
        {
            return string.Empty;
        }
    }
}
