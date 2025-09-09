//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Newtonsoft.Json;
using System.Text;
using System.Xml.Serialization;

namespace NV.CT.ConfigService.Impl;

public class ConfigRepository<TConfig> where TConfig : notnull
{
    private readonly string _fileName;
    private readonly bool _isJson;
    private TConfig _config;
    public event EventHandler ConfigRefreshed;

    public ConfigRepository(string fileName, bool isJson = true)
    {
        _isJson = isJson;
        _fileName = fileName;
        Task.Run(() => {
            Deserialize();
        });
    }
    
    private void Deserialize()
    {
        if (_isJson)
        {
            DeserializeJson();
        }
        else
        {
            DeserializeXml();
        }
    }

    public TConfig GetConfigs()
    {
        return _config;
    }

    public bool Save(TConfig config)
    {
        var isSaved = false;

        if (_isJson)
        {
            isSaved = SaveJson(config);
        }
        else
        {
            isSaved = SaveXml(config);
        }
        Deserialize();
        ConfigRefreshed?.Invoke(this, EventArgs.Empty);
        return isSaved;
    }

    private void DeserializeJson()
    {
        try
        {
            var content = File.ReadAllText(_fileName);
            _config = JsonConvert.DeserializeObject<TConfig>(content);
        }
        catch
        {
            //todo:待处理
        }
    }

    private void DeserializeXml()
    {
        try
        {
            var content = File.ReadAllText(_fileName);
            var serializer = new XmlSerializer(typeof(TConfig));
            _config = (TConfig)serializer.Deserialize(new StringReader(content));
        }
        catch
        {
            //todo:待处理
        }
    }

    private bool SaveJson(TConfig config)
    {
        using var fileStream = new FileStream(_fileName, FileMode.Create);
        using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
        var content = JsonConvert.SerializeObject(config);
        streamWriter.Write(content);
        streamWriter.Flush();
        streamWriter.Close();
        fileStream.Close();
        return true;
    }

    private bool SaveXml(TConfig config)
    {
        using var fileStream = new FileStream(_fileName, FileMode.Create);
        using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
        var serializer = new XmlSerializer(typeof(TConfig));
        serializer.Serialize(streamWriter, config);
        streamWriter.Flush();
        streamWriter.Close();
        fileStream.Close();
        return true;
    }
}
