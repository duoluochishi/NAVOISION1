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
using NV.CT.ProtocolService.Contract;
using NV.MPS.Environment;
using System.Text;

namespace NV.CT.ProtocolService.Impl;

public class ProtocolSettingRepository
{
    private readonly string _fileName;
    private ProtocolSettingModel _protocolSetting;

    public ProtocolSettingRepository()
    {
        _fileName = Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "ProtocolSetting", "protocolsetting.json");
        Deserialize();
    }

    private void Deserialize()
    {
        try
        {
            var content = File.ReadAllText(_fileName);
            _protocolSetting = JsonConvert.DeserializeObject<ProtocolSettingModel>(content);
        }
        catch
        {
            //todo:待处理
        }
    }

    public List<ProtocolSettingItem> ProtocolSettings => _protocolSetting.Items;

    public string EmergencyProtocol => _protocolSetting.EmergencyProtocol;

    public void SaveEmergencyProtocol(string emergencyProtocol)
    {
        _protocolSetting.EmergencyProtocol = emergencyProtocol;

        var content = JsonConvert.SerializeObject(_protocolSetting);
        File.WriteAllText(_fileName, content, Encoding.UTF8);

        Deserialize();
    }
    public void UpdateDefaultEmergencyProtocol()
    {
        _protocolSetting.EmergencyProtocol = _protocolSetting.DefaultEmergencyProtocol;

        var content = JsonConvert.SerializeObject(_protocolSetting);
        File.WriteAllText(_fileName, content, Encoding.UTF8);

        Deserialize();
    }
    public void SaveOrUpdate(ProtocolSettingItem protocolSetting)
    {
        //TODO:置顶，取消置顶，人体部位唯一置顶
        var setting = _protocolSetting.Items.FirstOrDefault(s => s.Id == protocolSetting.Id);
        if (setting is not null)
        {
            _protocolSetting.Items.Remove(setting);
        }
        _protocolSetting.Items.Add(protocolSetting);
        var content = JsonConvert.SerializeObject(_protocolSetting);
        File.WriteAllText(_fileName, content, Encoding.UTF8);

        Deserialize();
    }

    public void RemoveProtocolSetting(ProtocolSettingItem protocolSetting)
    {
        var setting = _protocolSetting.Items.FirstOrDefault(s => s.Id == protocolSetting.Id);
        if (setting is not null)
        {
            _protocolSetting.Items.Remove(setting);
        }

        var content = JsonConvert.SerializeObject(_protocolSetting);
        File.WriteAllText(_fileName, content, Encoding.UTF8);

        Deserialize();
    }
}