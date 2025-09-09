//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/4/19 16:30:59     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using System.Xml;
using Microsoft.Extensions.Logging;
using NV.CT.Protocol.Models;
using NV.CT.ProtocolService.Contract;
using System.Xml.Serialization;
using NV.MPS.Exception;

namespace NV.CT.ProtocolService.Impl;

public class ProtocolOperationService : IProtocolOperation
{
    private readonly ILogger<ProtocolOperationService> _logger;
    private readonly ProtocolService _protocolService;
    private readonly ProtocolSettingRepository _settingRepository;
    private List<ProtocolTemplateModel> _protocolTemplates;
    private List<ProtocolPresentationModel> _protocolPresentations;
    public event EventHandler<string> ProtocolChanged;

    public ProtocolOperationService(ILogger<ProtocolOperationService> logger, ProtocolService protocolService)
    {
        _logger = logger;
        _protocolService = protocolService;
        _settingRepository = new ProtocolSettingRepository();
        InitializeProtocolTemplates();
        _protocolService.ProtocolChanged += ProtocolService_ProtocolChanged;
    }

    private void InitializeProtocolTemplates()
    {
        _protocolTemplates = _protocolService.GetAllProtocolTemplates();
        var emergencyProtocol = _settingRepository.EmergencyProtocol;

        _protocolPresentations = new List<ProtocolPresentationModel>();
        _protocolTemplates.ForEach(template =>
        {
            var isEmerencyProtocol = false;
            if (!string.IsNullOrEmpty(emergencyProtocol) && template.Protocol.Descriptor.Id.Equals(emergencyProtocol))
            {
                isEmerencyProtocol = true;
            }
            template.Protocol.IsEmergency = isEmerencyProtocol;
            var protocolPresentation = new ProtocolPresentationModel
            {
                Id = template.Descriptor.Id,
                Name = template.Protocol.Descriptor.Name,
                IsAdult = template.IsAdult,
                IsFactory = template.IsFactory,
                IsEmergency = template.IsEmergency,
                IsEnhanced = template.IsEnhanced,
                IsIntervention = template.IsIntervention,
                BodyPart = template.BodyPart,
                Description = template.Protocol.Description
            };
            protocolPresentation.Measurements = new List<MeasurementPresentationModel>();
            foreach (var forEntity in template.Protocol.Children)
            {
                foreach (var measurementEntity in forEntity.Children)
                {
                    var measurementPresentation = new MeasurementPresentationModel
                    {
                        Id = measurementEntity.Descriptor.Id,
                        Name = measurementEntity.Descriptor.Name,
                        Scans = new List<ScanPresentationModel>()
                    };

                    foreach (var scanEntity in measurementEntity.Children)
                    {
                        var scanPresentation = new ScanPresentationModel
                        {
                            Id = scanEntity.Descriptor.Id,
                            Name = scanEntity.Descriptor.Name,
                            Recons = new List<ReconPresentationModel>()
                        };
                        foreach (var reconEntity in scanEntity.Children)
                        {
                            scanPresentation.Recons.Add(new ReconPresentationModel
                            {
                                Id = reconEntity.Descriptor.Id,
                                Name = reconEntity.Descriptor.Name,
                            });
                        }
                        measurementPresentation.Scans.Add(scanPresentation);
                    }

                    protocolPresentation.Measurements.Add(measurementPresentation);
                }
            }
            _protocolPresentations.Add(protocolPresentation);
        });
    }

    private void ProtocolService_ProtocolChanged(object? sender, string e)
    {
        InitializeProtocolTemplates();
        ProtocolChanged?.Invoke(this, e);
    }

    public void Delete(string templateId)
    {
        var template = _protocolTemplates.FirstOrDefault(t => t.Descriptor.Id == templateId);
        if (template is not null)
        {
            _protocolService.Delete(templateId, template.FullPath);
        }
    }

    public void Export(List<string> templateIds)
    {
        templateIds.ForEach(templateId =>
        {
            var protocolTemplate = GetProtocolTemplate(templateId);
            _protocolService.Export(protocolTemplate);
        });
    }

    public void ExportProtocol(string templateId, string outPath)
    {
        var protocolTemplate = GetProtocolTemplate(templateId);
        _protocolService.ExportProtocol(protocolTemplate, outPath);
    }

    public List<ProtocolTemplateModel> GetAllProtocolTemplates() => _protocolTemplates;

    public List<ProtocolPresentationModel> GetPresentations()
    {        
        return _protocolPresentations;
    }

    public ProtocolTemplateModel GetProtocolTemplate(string templateId)
    {
        return _protocolTemplates.FirstOrDefault(t => t.Descriptor.Id == templateId);
    }

    public ProtocolTemplateModel GetEmergencyProtocolTemplate()
    {
        return _protocolTemplates.FirstOrDefault(t => t.IsEmergency);
    }

    public (bool, string) Import(List<string> files)
    {
        var serializer = new XmlSerializer(typeof(ProtocolTemplateModel));
        var flag = true;
        var reason = string.Empty;
        foreach (var file in files)
        {
            try
            {
                using var reader = new XmlTextReader(file);
                serializer.Deserialize(reader);
            }
            catch (NanoException ex)
            {
                reason = ex.Message;
                flag = false;
                break;
            }
        }
        if (!flag) return (false, reason);
        files.ForEach(f => _protocolService.Import(f));
        return (true, string.Empty);
    }

    public void Save(ProtocolTemplateModel protocolTemplate, bool isTopping = false)
    {
        if (protocolTemplate.IsEmergency)
        {
            _settingRepository.SaveEmergencyProtocol(protocolTemplate.Protocol.Descriptor.Id);
        }else
        {
            _settingRepository.UpdateDefaultEmergencyProtocol();
        }
        _protocolService.Save(protocolTemplate);
        if (isTopping)
        {
            _settingRepository.SaveOrUpdate(new ProtocolSettingItem
            {
                Id = protocolTemplate.Descriptor.Id,
                Name = protocolTemplate.Descriptor.Name,
                BodySize = protocolTemplate.Protocol.BodySize,
                BodyPart = protocolTemplate.BodyPart
            });
        }
        else
        {
            _settingRepository.RemoveProtocolSetting(new ProtocolSettingItem
            {
                Id = protocolTemplate.Descriptor.Id,
                Name = protocolTemplate.Descriptor.Name,
                BodySize = protocolTemplate.Protocol.BodySize,
                BodyPart = protocolTemplate.BodyPart
            });
        }
    }

    public List<ProtocolSettingItem> GetProtocolSettingItems()
    {
        return _settingRepository.ProtocolSettings;
    }

    public void SaveSettingItemList(List<ProtocolSettingItem> protocolSettings)
    {
        if (protocolSettings.Count > 0)
        {
            foreach (var item in _settingRepository.ProtocolSettings.FindAll(t => t.OtherBodyPart.Equals(protocolSettings[0].OtherBodyPart)))
            {
                _settingRepository.RemoveProtocolSetting(item);
            }
        }
        foreach (ProtocolSettingItem item in protocolSettings)
        {
            _settingRepository.SaveOrUpdate(item);
        }
    }

    public void SaveOrUpdate(ProtocolSettingItem protocolSetting)
    {
        _settingRepository.SaveOrUpdate(protocolSetting);
    }

    public void RemoveProtocolSetting(ProtocolSettingItem protocolSetting)
    {
        _settingRepository.RemoveProtocolSetting(protocolSetting);
    }
}