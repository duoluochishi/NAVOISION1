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
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.Protocol.Models;
using System.Xml.Serialization;
using NV.CT.Protocol;

namespace NV.CT.ProtocolService.Impl;

public class ProtocolService
{
    private readonly ProtocolRepository _repository;
    private readonly ILogger<ProtocolService> _logger;
    private List<ProtocolTemplateModel> _protocolTemplates = new();

    public ProtocolService(ProtocolRepository repository, ILogger<ProtocolService> logger)
    {
        _logger = logger;
        _repository = repository;
        _repository.ProtocolChanged += OnProtocolChanged;
        GetAllProtocolTemplates();
    }

    public event EventHandler<string>? ProtocolChanged;
    public event EventHandler<EventArgs<ProtocolTemplateModel>>? ImportClick;

    private void OnProtocolChanged(object? sender, string e)
    {
        ProtocolChanged?.Invoke(sender, e);
    }

    public void Save(ProtocolTemplateModel protocolTemplate)
    {
        _repository.Save(protocolTemplate);
    }

    public void Delete(string templateId, string templateFile)
    {
        _repository.Delete(templateId, templateFile);
    }

    public List<ProtocolTemplateModel> GetAllProtocolTemplates()
    {
        try
        {
            //TODO:目前开放这个有bug，需要确认这个作用
            //if (_protocolTemplates is null || !_protocolTemplates.Any())
            //{
            _protocolTemplates = _repository.GetAllProtocolTemplates();
            //}
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        return _protocolTemplates;
    }

    /// <summary>
    /// 导入协议
    /// </summary>
    public void Import(string fileName)
    {
        var serializer = new XmlSerializer(typeof(ProtocolTemplateModel));
        using var stream = new FileStream(fileName, FileMode.Open);
        var protocolTemplate = serializer.Deserialize(stream) as ProtocolTemplateModel;

        if (protocolTemplate is null) return;
        protocolTemplate.Protocol.Parameters.Find(p=>p.Name== "IsFactory").Value = 0.ToString();

        ProtocolHelper.ResetId(protocolTemplate, true);

        Save(protocolTemplate);
    }

    public void Export(ProtocolTemplateModel protocolTemplate)
    {
        if (protocolTemplate is null) return;

        _repository.Export(protocolTemplate);
    }

    //导出到桌面
    public void ExportProtocol(ProtocolTemplateModel protocolTemplate, string outPath)
    {
        if (protocolTemplate is null) return;

        _repository.ExportProtocol(protocolTemplate, outPath);
    }
}