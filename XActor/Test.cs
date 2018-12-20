using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using XActors1 = mzxrules.XActor.XActors;
namespace mzxrules.XActor
{
    class Test
    {
        public static void Test1()
        {
            XActors1 root = new XActors1();
            root.Actor = new List<XActor>();

            root.Actor.Add(new XActor());

            XmlWriterSettings xout = new XmlWriterSettings();

            xout.Indent = true;

            XmlSerializer serializer = new XmlSerializer(typeof(XActors1));

            using (XmlWriter writer = XmlWriter.Create("testNew.txt", xout))
            {
                serializer.Serialize(writer, root);
            }


            XActors1 test;

            using (XmlReader reader = XmlReader.Create("testNew.txt"))
            {
                test = (XActors1) serializer.Deserialize(reader);
            }

        }

        public static void Serialize()
        {

        }

        public static string ParseActorVariables(string[] lines)
        {
            return string.Empty;


            //XmlSerializer test = new XmlSerializer(typeof(XActors1));

            //test.Serialize(
            
        }
    }
}
