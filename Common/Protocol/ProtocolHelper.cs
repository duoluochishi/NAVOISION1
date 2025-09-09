using Newtonsoft.Json;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Helpers;
using NV.CT.Protocol.Models;

namespace NV.CT.Protocol;
public static class ProtocolHelper
{
    public static void SaveProtocol(string fileName, ProtocolModel instanceProtocol)
    {
        XmlHelper.SerializeFile(instanceProtocol, fileName);
    }

    public static string Serialize(ProtocolModel instanceProtocol)
    {
        return XmlHelper.Serialize(instanceProtocol);
    }

    public static ProtocolModel Deserialize(string xmlProtocol)
    {
        var model = XmlHelper.Deserialize<ProtocolModel>(xmlProtocol);
        ResetParent(model);
        return model;
    }

    public static ProtocolTemplateModel DeserializeXmlFile(string filePath)
    {
        var model = XmlHelper.DeseerializeFile<ProtocolTemplateModel>(filePath);
        ResetParent(model.Protocol);
        return model;
    }

    public static List<ProtocolTemplateModel> GetProtocolsByFiles(string[] files)
    {
        List<ProtocolTemplateModel> protocolModels = new List<ProtocolTemplateModel>();
        files.ForEach(f =>
        {
            protocolModels.Add(DeserializeXmlFile(f));
        });
        return protocolModels;
    }

    public static void ResetParent(ScanModel parent, List<ReconModel> instances)
    {
        instances.ForEach(instance => instance.Parent = parent);
    }

    public static void ResetParent(MeasurementModel parent, List<ScanModel> instances)
    {
        instances.ForEach(instance =>
        {
            instance.Parent = parent;
            ResetParent(instance, instance.Children);
        });
    }

    public static void ResetParent(FrameOfReferenceModel parent, List<MeasurementModel> instances)
    {
        instances.ForEach(instance =>
        {
            instance.Parent = parent;
            ResetParent(instance, instance.Children);
        });
    }

    public static void ResetParent(ProtocolModel parent, List<FrameOfReferenceModel> instances)
    {
        instances.ForEach(instance =>
        {
            instance.Parent = parent;
            ResetParent(instance, instance.Children);
        });
    }

    public static void ResetParent(ProtocolModel instance)
    {
        ResetParent(instance, instance.Children);
    }

    public static void ResetId(ScanModel instance, bool isGuid = false)
    {
        if (instance.Status == CTS.Enums.PerformStatus.Unperform)
        {
            instance.Descriptor.Id = isGuid ? Guid.NewGuid().ToString() : IdGenerator.Next(4);
        }
        foreach (var reconRange in instance.Children)
        {
            if (reconRange.Status == CTS.Enums.PerformStatus.Unperform)
            {
                reconRange.Descriptor.Id = isGuid ? Guid.NewGuid().ToString() : IdGenerator.Next(5);
            }
        }
    }

    public static void ResetId(MeasurementModel instance, bool isGuid = false)
    {
        if (instance.Status == CTS.Enums.PerformStatus.Unperform)
        {
            instance.Descriptor.Id = isGuid ? Guid.NewGuid().ToString() : IdGenerator.Next(3);
        }
        foreach (var scanRange in instance.Children)
        {
            ResetId(scanRange, isGuid);
        }
    }

    public static void ResetId(FrameOfReferenceModel instance, bool isGuid = false)
    {
        if (instance.Status == CTS.Enums.PerformStatus.Unperform)
        {
            instance.Descriptor.Id = isGuid ? Guid.NewGuid().ToString() : UIDHelper.CreateFORUID();
        }
        foreach (var measurement in instance.Children)
        {
            ResetId(measurement, isGuid);
        }
    }

    public static void ResetId(ProtocolModel instance, bool isGuid = false)
    {
        if (instance.Status == CTS.Enums.PerformStatus.Unperform)
        {
            instance.Descriptor.Id = isGuid ? Guid.NewGuid().ToString() : IdGenerator.Next(1);
        }
        foreach (var forItem in instance.Children)
        {
            ResetId(forItem, isGuid);
        }
    }

    public static void ResetId(ProtocolTemplateModel instance, bool isGuid = false)
    {
        instance.Descriptor.Id = isGuid ? Guid.NewGuid().ToString() : IdGenerator.Next(1);
        ResetId(instance.Protocol, isGuid);
    }

    public static void ResetPerformStatus(ScanModel instance)
    {
        instance.Status = CTS.Enums.PerformStatus.Unperform;
        instance.FailureReason = CTS.Enums.FailureReasonType.None;
        instance.Children.ForEach(reconRange => {
            reconRange.Status = CTS.Enums.PerformStatus.Unperform;
            reconRange.FailureReason = CTS.Enums.FailureReasonType.None;
            SetParameter(reconRange, ProtocolParameterNames.RECON_IMAGE_PATH, string.Empty, false);
        });
    }

    public static void ResetPerformStatus(MeasurementModel instance)
    {
        instance.Status = CTS.Enums.PerformStatus.Unperform;
        instance.FailureReason = CTS.Enums.FailureReasonType.None;
        instance.Children.ForEach(scanRange => ResetPerformStatus(scanRange));
    }

    public static List<(FrameOfReferenceModel Frame, MeasurementModel Measurement, ScanModel Scan, ReconModel Recon)> ExpandRecons(ProtocolModel instance)
    {
        var items = new List<(FrameOfReferenceModel Frame, MeasurementModel Measurement, ScanModel Scan, ReconModel Recon)>();
        foreach (var forEntity in instance.Children)
        {
            foreach (var measurementEntity in forEntity.Children)
            {
                foreach (var scanEntity in measurementEntity.Children)
                {
                    foreach(var reconEntity in scanEntity.Children)
                    {
                        items.Add((forEntity, measurementEntity, scanEntity, reconEntity));
                    }
                }
            }
        }
        return items;
    }

    public static List<(FrameOfReferenceModel Frame, MeasurementModel Measurement, ScanModel Scan)> Expand(ProtocolModel instance)
    {
        var items = new List<(FrameOfReferenceModel Frame, MeasurementModel Measurement, ScanModel Scan)>();
        foreach (var forEntity in instance.Children)
        {
            foreach (var measurementEntity in forEntity.Children)
            {
                foreach (var scanEntity in measurementEntity.Children)
                {
                    items.Add((forEntity, measurementEntity, scanEntity));
                }
            }
        }
        return items;
    }

    public static List<(FrameOfReferenceModel Frame, MeasurementModel Measurement)> ExpandMeasurements(ProtocolModel instance)
    {
        var items = new List<(FrameOfReferenceModel Frame, MeasurementModel Measurement)>();
        foreach (var forEntity in instance.Children)
        {
            foreach (var measurementEntity in forEntity.Children)
            {
                items.Add((forEntity, measurementEntity));
            }
        }
        return items;
    }

    public static void SetReconSeriesNumber(ProtocolModel instance, ScanModel scan)
    {
        var itemModels = ExpandRecons(instance);

        //定位像RTD
        var topoReconIndex = GlobalSeriesNumber.TopoIndex_Min;
        if (itemModels.Any(item => item.Scan.ScanOption is FacadeProxy.Common.Enums.ScanOption.Surview or FacadeProxy.Common.Enums.ScanOption.DualScout && item.Recon.IsRTD))
        {
            var tempIndex = itemModels.Where(item => item.Scan.ScanOption is FacadeProxy.Common.Enums.ScanOption.Surview or FacadeProxy.Common.Enums.ScanOption.DualScout && item.Recon.IsRTD).Max(model => model.Recon.SeriesNumber);
            topoReconIndex = tempIndex == 0 ? topoReconIndex : tempIndex;
        }
        //断层像RTD
        var tomoReconIndex = GlobalSeriesNumber.TomoIndex_Min;
        if (itemModels.Any(item => item.Scan.ScanOption is not FacadeProxy.Common.Enums.ScanOption.Surview and not FacadeProxy.Common.Enums.ScanOption.DualScout && item.Recon.IsRTD))
        {
            var tempIndex = itemModels.Where(item => item.Scan.ScanOption is not FacadeProxy.Common.Enums.ScanOption.Surview and not FacadeProxy.Common.Enums.ScanOption.DualScout && item.Recon.IsRTD).Max(model => model.Recon.SeriesNumber);
            tomoReconIndex = tempIndex == 0 ? tomoReconIndex : tempIndex;
        }
        //断层像OfflineRecon
        var offlineReconIndex = GlobalSeriesNumber.ReconIndex_Min;
        if (itemModels.Any(item => item.Scan.ScanOption is not FacadeProxy.Common.Enums.ScanOption.Surview and not FacadeProxy.Common.Enums.ScanOption.DualScout && !item.Recon.IsRTD))
        {
            var tempIndex = itemModels.Where(item => item.Scan.ScanOption is not FacadeProxy.Common.Enums.ScanOption.Surview and not FacadeProxy.Common.Enums.ScanOption.DualScout && !item.Recon.IsRTD).Max(model => model.Recon.SeriesNumber);
            offlineReconIndex = tempIndex == 0 ? offlineReconIndex : tempIndex;
        }

        foreach(var reconModel in scan.Children)
        {
            if (reconModel.Parent.ScanOption == FacadeProxy.Common.Enums.ScanOption.None) continue;

            if (reconModel.Parent.ScanOption is FacadeProxy.Common.Enums.ScanOption.Surview or FacadeProxy.Common.Enums.ScanOption.DualScout)
            {
                topoReconIndex++;
                SetParameter(reconModel, ProtocolParameterNames.RECON_SERIES_NUMBER, topoReconIndex.ToString());
                continue;
            }
            else if (reconModel.IsRTD)
            {
                tomoReconIndex++;
                SetParameter(reconModel, ProtocolParameterNames.RECON_SERIES_NUMBER, tomoReconIndex.ToString());
                continue;
            }
            else
            {
                offlineReconIndex++;
                SetParameter(reconModel, ProtocolParameterNames.RECON_SERIES_NUMBER, offlineReconIndex.ToString());
                continue;
            }
        }
    }

    public static int SetNextSeriesNumber(ProtocolModel instance, ReconModel model)
    {
        var itemModels = ExpandRecons(instance);

        //定位像RTD
        var topoReconIndex = GlobalSeriesNumber.TopoIndex_Min;
        if (itemModels.Any(item => item.Scan.ScanOption is FacadeProxy.Common.Enums.ScanOption.Surview or FacadeProxy.Common.Enums.ScanOption.DualScout && item.Recon.IsRTD))
        {
            var tempIndex = itemModels.Where(item => item.Scan.ScanOption is FacadeProxy.Common.Enums.ScanOption.Surview or FacadeProxy.Common.Enums.ScanOption.DualScout && item.Recon.IsRTD).Max(model => model.Recon.SeriesNumber);
            topoReconIndex = tempIndex == 0 ? topoReconIndex : tempIndex;
        }
        //断层像RTD
        var tomoReconIndex = GlobalSeriesNumber.TomoIndex_Min;
        if (itemModels.Any(item => item.Scan.ScanOption is not FacadeProxy.Common.Enums.ScanOption.Surview and not FacadeProxy.Common.Enums.ScanOption.DualScout && item.Recon.IsRTD))
        {
            var tempIndex = itemModels.Where(item => item.Scan.ScanOption is not FacadeProxy.Common.Enums.ScanOption.Surview and not FacadeProxy.Common.Enums.ScanOption.DualScout && item.Recon.IsRTD).Max(model => model.Recon.SeriesNumber);
            tomoReconIndex = tempIndex == 0 ? tomoReconIndex : tempIndex;
        }
        //断层像OfflineRecon
        var offlineReconIndex = GlobalSeriesNumber.ReconIndex_Min;
        if (itemModels.Any(item => item.Scan.ScanOption is not FacadeProxy.Common.Enums.ScanOption.Surview and not FacadeProxy.Common.Enums.ScanOption.DualScout && !item.Recon.IsRTD))
        {
            var tempIndex = itemModels.Where(item => item.Scan.ScanOption is not FacadeProxy.Common.Enums.ScanOption.Surview and not FacadeProxy.Common.Enums.ScanOption.DualScout && !item.Recon.IsRTD).Max(model => model.Recon.SeriesNumber);
            offlineReconIndex = tempIndex == 0 ? offlineReconIndex : tempIndex;
        }

        //ScanOption为None的情况，不应存在
        if (model.Parent.ScanOption == FacadeProxy.Common.Enums.ScanOption.None) return 0;

        if (model.Parent.ScanOption is FacadeProxy.Common.Enums.ScanOption.Surview or FacadeProxy.Common.Enums.ScanOption.DualScout)
        {
            topoReconIndex++;
            SetParameter(model, ProtocolParameterNames.RECON_SERIES_NUMBER, topoReconIndex.ToString());
            return topoReconIndex;
        }
        else if (model.IsRTD)
        {
            tomoReconIndex++;
            SetParameter(model, ProtocolParameterNames.RECON_SERIES_NUMBER, tomoReconIndex.ToString());
            return topoReconIndex;
        }
        else
        {
            offlineReconIndex++;
            SetParameter(model, ProtocolParameterNames.RECON_SERIES_NUMBER, offlineReconIndex.ToString());
            return topoReconIndex;
        }
    }

    public static void ResetReconSeriesNumber(ProtocolModel instance)
    {
        var itemModels = ExpandRecons(instance);

        //获取扫描完成的定位像
        var topoReconIndex = GlobalSeriesNumber.TopoIndex_Min;
        if (itemModels.Any(item => item.Scan.ScanOption is FacadeProxy.Common.Enums.ScanOption.Surview or FacadeProxy.Common.Enums.ScanOption.DualScout && item.Scan.Status == CTS.Enums.PerformStatus.Performed && item.Recon.Status == CTS.Enums.PerformStatus.Performed))
        {
            var tempIndex = itemModels.Where(item => item.Scan.ScanOption is FacadeProxy.Common.Enums.ScanOption.Surview or FacadeProxy.Common.Enums.ScanOption.DualScout && item.Scan.Status == CTS.Enums.PerformStatus.Performed && item.Recon.Status == CTS.Enums.PerformStatus.Performed).Max(model => model.Recon.SeriesNumber);
            topoReconIndex = tempIndex == 0 ? topoReconIndex : tempIndex;
        }
        //获取完成扫描的断层像(RTD)
        var tomoReconIndex = GlobalSeriesNumber.TomoIndex_Min;
        if (itemModels.Any(item => item.Scan.ScanOption is not FacadeProxy.Common.Enums.ScanOption.Surview and not FacadeProxy.Common.Enums.ScanOption.DualScout && item.Scan.Status == CTS.Enums.PerformStatus.Performed && item.Recon.IsRTD && item.Recon.Status == CTS.Enums.PerformStatus.Performed))
        {
            var tempIndex = itemModels.Where(item => item.Scan.ScanOption is not FacadeProxy.Common.Enums.ScanOption.Surview and not FacadeProxy.Common.Enums.ScanOption.DualScout && item.Scan.Status == CTS.Enums.PerformStatus.Performed && item.Recon.IsRTD && item.Recon.Status == CTS.Enums.PerformStatus.Performed).Max(model => model.Recon.SeriesNumber);
            tomoReconIndex = tempIndex == 0 ? tomoReconIndex : tempIndex;
        }
        //获取完成扫描的断层像(离线重建)
        var offlineReconIndex = GlobalSeriesNumber.ReconIndex_Min;
        if (itemModels.Any(item => item.Scan.ScanOption is not FacadeProxy.Common.Enums.ScanOption.Surview and not FacadeProxy.Common.Enums.ScanOption.DualScout && item.Scan.Status == CTS.Enums.PerformStatus.Performed && !item.Recon.IsRTD && item.Recon.Status == CTS.Enums.PerformStatus.Performed))
        {
            var tempIndex = itemModels.Where(item => item.Scan.ScanOption is not FacadeProxy.Common.Enums.ScanOption.Surview and not FacadeProxy.Common.Enums.ScanOption.DualScout && item.Scan.Status == CTS.Enums.PerformStatus.Performed && !item.Recon.IsRTD && item.Recon.Status == CTS.Enums.PerformStatus.Performed).Max(model => model.Recon.SeriesNumber);
            offlineReconIndex = tempIndex == 0 ? offlineReconIndex : tempIndex;
        }
        foreach (var itemModel in itemModels)
        {
            //ScanOption为None的情况，不应存在
            if (itemModel.Scan.ScanOption == FacadeProxy.Common.Enums.ScanOption.None) continue;

            //已经完成的扫描，是否有未执行的重建
            if (itemModel.Scan.Status == CTS.Enums.PerformStatus.Performed && itemModel.Recon.Status != CTS.Enums.PerformStatus.Unperform) continue;

            if (itemModel.Scan.ScanOption is FacadeProxy.Common.Enums.ScanOption.Surview or FacadeProxy.Common.Enums.ScanOption.DualScout)
            {
                topoReconIndex++;
                SetParameter(itemModel.Recon, ProtocolParameterNames.RECON_SERIES_NUMBER, topoReconIndex.ToString());
            }
            else if (itemModel.Recon.IsRTD)
            {
                tomoReconIndex++;
                SetParameter(itemModel.Recon, ProtocolParameterNames.RECON_SERIES_NUMBER, tomoReconIndex.ToString());
            }
            else
            {
                offlineReconIndex++;
                SetParameter(itemModel.Recon, ProtocolParameterNames.RECON_SERIES_NUMBER, offlineReconIndex.ToString());
            }
        }
    }

    public static ReconModel GetRecon(ProtocolModel instance, string scanId, string reconId)
    {
        var items = Expand(instance);
        var item = items.FirstOrDefault(o => o.Scan.Descriptor.Id == scanId);
        return item.Scan?.Children.FirstOrDefault(r => r.Descriptor.Id == reconId);
    }

    public static void ResetScanNumber(ProtocolModel instance)
    {
        var items = Expand(instance);
        var itemIndex = 1;
        if (items.Any(t => t.Scan.Status != CTS.Enums.PerformStatus.Unperform))
        {
            itemIndex = items.Where(t => t.Scan.Status != CTS.Enums.PerformStatus.Unperform).Max(t => t.Scan.ScanNumber) + 1;
        }
        foreach (var item in items)
        {
            if (item.Scan.Status == CTS.Enums.PerformStatus.Unperform)
            {
                SetParameter(item.Scan, ProtocolParameterNames.SCAN_NUMBER, itemIndex.ToString());
                itemIndex++;
            }
        }
    }

    public static void SetParameter(BaseModel model, string parameterName, string parameterValue, bool isAddition = true)
    {
        var parameter = model.Parameters.FirstOrDefault(p => p.Name == parameterName);
        if (parameter is not null)
        {
            parameter.Value = parameterValue;
        }
        else
        {
            if (isAddition)
            {
                model.Parameters.Add(new ParameterModel
                {
                    Name = parameterName,
                    Value = parameterValue
                });
            }
        }
    }

    public static void SetParameter<TParameter>(BaseModel model, string parameterName, TParameter parameterValue, bool isAddition = true)
    {
        var parameter = model.Parameters.FirstOrDefault(p => p.Name == parameterName);
        if (parameter is not null)
        {
            parameter.Value = JsonConvert.SerializeObject(parameterValue);
        }
        else
        {
            if (isAddition)
            {
                model.Parameters.Add(new ParameterModel
                {
                    Name = parameterName,
                    Value = JsonConvert.SerializeObject(parameterValue)
                });
            }
        }
    }
}