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
using NV.CT.CTS.Extensions;
using NV.CT.Protocol.Models;
using NV.MPS.Environment;
using System.Text;
using System.Xml.Serialization;

namespace NV.CT.ProtocolService.Impl;

public class ProtocolRepository
{
    private readonly string _rootDir;
    private readonly string _customDir;
    private Dictionary<string,string> _protocolsFiles = new Dictionary<string,string>();
    public ProtocolRepository()
    {
        _rootDir = RuntimeConfig.Console.MCSProtocol.Path;
        _customDir = Path.Combine(_rootDir, "Custom");
    }

    public event EventHandler<string> ProtocolChanged;

    public void Delete(string templateId, string templateFile)
    {
        var fullName = Path.Combine(_rootDir, templateFile);
        if (File.Exists(fullName))
        {
            File.Delete(fullName);
            ProtocolChanged?.Invoke(this, templateId);
        }
    }

    private void SaveTemplateFile(FileStream fileStream, ProtocolTemplateModel templateModel)
    {
        using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
        var serializer = new XmlSerializer(typeof(ProtocolTemplateModel));
        serializer.Serialize(streamWriter, templateModel);
        streamWriter.Flush();
        streamWriter.Close();

    }

    public void Save(ProtocolTemplateModel protocolTemplate)
    {
        //根据Id找到对应的xml文件，如果存在，则取得文件名并覆盖保存，如果不存在，则创建新文件名并新增保存
        string fileName = string.Empty;
        if (_protocolsFiles.Keys.Contains(protocolTemplate.Descriptor.Id))
        {
            fileName = _protocolsFiles[protocolTemplate.Descriptor.Id];
        }
        else
        {
            fileName = $"{protocolTemplate.Descriptor.Name}_{IdGenerator.Next(1)}.xml";
        }

        if (!protocolTemplate.IsFactory && !Directory.Exists(_customDir))
        {
            Directory.CreateDirectory(_customDir);
        }

        using var fileStream = new FileStream(Path.Combine(_rootDir, protocolTemplate.IsFactory ? "Factory" : "Custom", fileName), FileMode.Create);
        SaveTemplateFile(fileStream, protocolTemplate);

        ProtocolChanged?.Invoke(this, protocolTemplate.Descriptor.Id);
    }

    public void Export(ProtocolTemplateModel protocolTemplate)
    {
        var parts = new List<string> {
            protocolTemplate.IsAdult ? "Adult":"Child",
            $"{protocolTemplate.BodyPart}",
            protocolTemplate.Protocol.Descriptor.Name.Replace(" ", "_"),
            DateTime.Now.ToString("yyyyMMdd")
        };
        var fileName = $"{string.Join("_", parts)}.xml";

        using var fileStream = new FileStream(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), fileName), FileMode.Create);
        SaveTemplateFile(fileStream, protocolTemplate);
    }

    public void ExportProtocol(ProtocolTemplateModel protocolTemplate, string outPath)
    {
        var parts = new List<string> {
            protocolTemplate.IsAdult ? "Adult":"Child",
            $"{protocolTemplate.BodyPart}",
            protocolTemplate.Protocol.Descriptor.Name.Replace(" ", "_"),
            DateTime.Now.ToString("yyyyMMddHHmmss")
        };
        var filePath = $"{outPath}\\{string.Join("_", parts)}.xml";

        using var fileStream = new FileStream(filePath, FileMode.Create);
        SaveTemplateFile(fileStream, protocolTemplate);
    }

    public List<ProtocolTemplateModel> GetAllProtocolTemplates()
    {
        var protocolTemplates = new List<ProtocolTemplateModel>();
        try
        {
            var templateFiles = Directory.GetFiles(_rootDir, "*.xml", SearchOption.AllDirectories).ToList();
            templateFiles.ForEach(templateFile =>
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(ProtocolTemplateModel));
                    using var stream = File.Open(templateFile, FileMode.Open);
                    var protocolTemplate = serializer.Deserialize(stream) as ProtocolTemplateModel;

                    if (protocolTemplate is null) return;
                    protocolTemplate.FullPath = templateFile;
                    protocolTemplate.FileName = templateFile.Substring(templateFile.LastIndexOf("\\") + 1);
                    if (!_protocolsFiles.ContainsKey(protocolTemplate.Descriptor.Id))
                    {
                        _protocolsFiles.Add(protocolTemplate.Descriptor.Id, templateFile);
                    }
                    protocolTemplates.Add(protocolTemplate);
                }
                catch (Exception ex) {
                    CTS.Global.Logger.LogDebug(ex, $"Protocol file deserialize failed: {templateFile}");
                }
            });
        }
        catch (Exception ex)
        {
            throw ex;
        }

        return protocolTemplates;
    }

    public bool CheckIsExistProtocolId(string protocolId)
    {
        return _protocolsFiles.ContainsKey(protocolId);
    }
}
