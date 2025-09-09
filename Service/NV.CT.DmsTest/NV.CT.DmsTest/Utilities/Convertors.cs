using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NV.CT.DmsTest.Utilities
{
    internal class Convertors
    {
        public static T SerializationRead<T>(string path)
        {
            XmlSerializer ?serializer = new XmlSerializer(typeof(T));
            FileStream fs = new FileStream(path, FileMode.Open);
            T r = (T)serializer.Deserialize(fs)!;
            fs.Close();
            return r!;
        }
        public static void SerializationSave<T>(T t, string path)
        {
            XmlSerializer xmlFormat = new XmlSerializer(typeof(T));
            TextWriter writer = new StreamWriter(path);
            xmlFormat.Serialize(writer, t);
            writer.Close();
        }
    }
}
