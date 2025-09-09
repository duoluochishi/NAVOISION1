//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
//     2024/8/13 9:12:24    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using System.Text;
using System.Xml.Serialization;

namespace NV.CT.CTS.Helpers;

public static class XmlHelper
{
    public static string Serialize<T>(T data)
    {
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        var serializer = new XmlSerializer(typeof(T));
        serializer.Serialize(streamWriter, data);
        streamWriter.Flush();
        memoryStream.Position = 0;
        using var streamReader = new StreamReader(memoryStream);
        return streamReader.ReadToEnd();
    }

    public static T Deserialize<T>(string content)
    {
        var serializer = new XmlSerializer(typeof(T));
        var model = (T)serializer.Deserialize(new StringReader(content));
        return model;
    }

    public static void SerializeFile<T>(T data, string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
        using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
        var serializer = new XmlSerializer(typeof(T));
        serializer.Serialize(streamWriter, data);
        streamWriter.Flush();
        streamWriter.Close();
        fileStream.Close();
    }

    public static T DeseerializeFile<T>(string filePath)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var stream = new FileStream(filePath, FileMode.Open);
        var model = (T)serializer.Deserialize(stream);
        return model;
    }
}
