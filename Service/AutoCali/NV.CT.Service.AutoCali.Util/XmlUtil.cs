using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace NV.CT.Service.AutoCali.Util
{
    /// <summary>
    /// Xml文件的工具类
    /// </summary>
    public static class XmlUtil
    {
        public static string SerializeXml(object data)
        {
            var xmlSerializer = new XmlSerializer(data.GetType());
            var stringBuilder = new StringBuilder();

            var writerSettings = new XmlWriterSettings() { Indent = true };
            using (var xmlWriter = XmlWriter.Create(new UTF8StringWriter(stringBuilder), writerSettings))
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                xmlSerializer.Serialize(xmlWriter, data, ns);
                return stringBuilder.ToString();
            }
        }

        public static T DeserializeFromFile<T>(string filePath)
        {
            T obj = default(T)!;
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"文件不存在!{filePath}");//todo log
                    return obj;
                }

                FileStream fs = File.Open(filePath, FileMode.Open);
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    XmlSerializer xz = new XmlSerializer(typeof(T));
                    obj = (T)xz.Deserialize(sr)!;
                }
                return obj;
            }
            catch
            {
                throw;
            }
        }

        public static void SaveToFile(string filePath, object data)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                string content = XmlUtil.SerializeXml(data);
                writer.Write(content);
            }
        }
    }

    public sealed class UTF8StringWriter : StringWriter
    {
        public UTF8StringWriter() : base() { }
        public UTF8StringWriter(StringBuilder stringBuilder) : base(stringBuilder) { }

        public override Encoding Encoding => Encoding.UTF8;
    }
}
