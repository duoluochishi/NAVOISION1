using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Helpers;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Protocol.Interfaces;
using NV.CT.Protocol.Models;
using NV.MPS.Exception;

namespace NV.CT.Protocol.Services;
public class ProtocolStructureService : IProtocolStructureService
{
    public event EventHandler<EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>> ProtocolStructureChanged;

    public bool AddProtocol(ProtocolModel instanceProtocol, ProtocolModel sourceProtocol, List<string> removedMeasurementIds, bool isKeepLocalizer, bool isKeepFOR)
    {
        if (sourceProtocol is null || sourceProtocol.Children.IsEmpty())
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "SourceProtocol"), new ArgumentNullException("sourceProtocol"));
        }

        sourceProtocol = RemovingMeasurements(sourceProtocol, removedMeasurementIds);

        if (instanceProtocol is null || !instanceProtocol.Children.Any())
        {
            instanceProtocol.Parameters = sourceProtocol.Parameters.Clone();
            instanceProtocol.Descriptor = sourceProtocol.Descriptor.Clone();
            instanceProtocol.Children = sourceProtocol.Children.Clone();

            instanceProtocol.IsEmergency = sourceProtocol.IsEmergency;

            ProtocolHelper.ResetParent(instanceProtocol);
            ProtocolHelper.ResetId(instanceProtocol);
            ProtocolHelper.ResetScanNumber(instanceProtocol);
            ProtocolHelper.ResetReconSeriesNumber(instanceProtocol);
            ProtocolStructureChanged?.Invoke(this, new EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>((instanceProtocol, instanceProtocol, StructureChangeType.Add)));
            return true;
        }

        //TODO: 暂不处理isKeepLocalizer和isKeepFOR
        var lastFOR = instanceProtocol.Children.LastOrDefault();
        if (lastFOR is null)
        {
            return true;
        }

        foreach (var forItem in sourceProtocol.Children)
        {
            var measurements = forItem.Children.Clone();
            measurements.ForEach(measurement => ProtocolHelper.ResetId(measurement));
            ProtocolHelper.ResetParent(lastFOR, measurements);
            lastFOR.Children.AddRange(measurements);
        }
        if (lastFOR.Children.Any(m => m.Status != PerformStatus.Unperform))
        {
            lastFOR.Status = PerformStatus.Performing;
        }
        else
        {
            lastFOR.Status = PerformStatus.Unperform;
        }
        ProtocolHelper.ResetScanNumber(instanceProtocol);
        ProtocolHelper.ResetReconSeriesNumber(instanceProtocol);
        ProtocolStructureChanged?.Invoke(this, new EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>((instanceProtocol, lastFOR, StructureChangeType.Add)));

        return true;
    }

    public bool ReplaceProtocol(ProtocolModel instanceProtocol, ProtocolModel sourceProtocol, List<string> removedMeasurementIds, bool isKeepLocalizer, bool isKeepFOR)
    {
        if (sourceProtocol is null || sourceProtocol.Children.IsEmpty())
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "SourceProtocol"), new ArgumentNullException("sourceProtocol"));
        }

        if (!instanceProtocol.Children.Any())
        {
            //执行添加即可
            return AddProtocol(instanceProtocol, sourceProtocol, removedMeasurementIds, isKeepLocalizer, isKeepFOR);
        }

        var protocolItems = ProtocolHelper.Expand(instanceProtocol);
        var deletingItems = protocolItems.Where(s => s.Scan.Status == PerformStatus.Unperform).ToList();
        //移除Scan
        foreach (var deletingItem in deletingItems)
        {
            deletingItem.Measurement.Children.Remove(deletingItem.Scan);
        }
        //移除Measurement
        foreach (var deletingItem in deletingItems)
        {
            if (!deletingItem.Measurement.Children.Any())
            {
                deletingItem.Frame.Children.Remove(deletingItem.Measurement);
            }
        }
        //移除FOR
        foreach (var deletingItem in deletingItems)
        {
            if (!deletingItem.Frame.Children.Any())
            {
                instanceProtocol.Children.Remove(deletingItem.Frame);
            }
        }

        //TODO: 暂不处理isKeepLocalizer和isKeepFOR
        var lastFOR = instanceProtocol.Children.LastOrDefault();

        BaseModel current = lastFOR is not null ? lastFOR : instanceProtocol;
        if (lastFOR is not null)
        {
            sourceProtocol = RemovingMeasurements(sourceProtocol, removedMeasurementIds);

            foreach (var forItem in sourceProtocol.Children)
            {
                var measurements = forItem.Children.Clone();
                measurements.ForEach(measurement => ProtocolHelper.ResetId(measurement));
                lastFOR.Children.AddRange(measurements);
                ProtocolHelper.ResetParent(lastFOR, measurements);
            }
            if (lastFOR.Children.Any(m => m.Status != PerformStatus.Unperform))
            {
                lastFOR.Status = PerformStatus.Performing;
            }
            else
            {
                lastFOR.Status = PerformStatus.Unperform;
            }
        }
        else
        {
            sourceProtocol = RemovingMeasurements(sourceProtocol, removedMeasurementIds);
            //todo:待处理移除的Measurements
            instanceProtocol.Children.AddRange(sourceProtocol.Children.Clone());
            instanceProtocol.Children.ForEach(item => ProtocolHelper.ResetId(item));
            ProtocolHelper.ResetParent(instanceProtocol);
        }
        ProtocolHelper.ResetScanNumber(instanceProtocol);
        ProtocolHelper.ResetReconSeriesNumber(instanceProtocol);
        ProtocolStructureChanged?.Invoke(this, new EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>((instanceProtocol, current, StructureChangeType.Replace)));

        return true;
    }

    public ProtocolModel RemovingMeasurements(ProtocolModel protocol, List<string> measurementIds)
    {
        if (protocol is null || protocol.Children.IsEmpty())
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Protocol"), new ArgumentNullException("protocol"));
        }

        if (measurementIds is not null && measurementIds.Count > 0)
        {
            var protocolItems = ProtocolHelper.ExpandMeasurements(protocol);
            var removingMeasurements = measurementIds.Clone();

            while (removingMeasurements.Count > 0)
            {
                foreach (var itemModels in protocolItems)
                {
                    if (itemModels.Measurement.Descriptor.Id.Equals(removingMeasurements[0]))
                    {
                        itemModels.Frame.Children.Remove(itemModels.Measurement);
                        removingMeasurements.RemoveAt(0);
                        break;
                    }
                }
            }
        }

        return protocol;
    }

    public bool DeleteScan(ProtocolModel instanceProtocol, string scanId)
    {
        if (instanceProtocol is null || instanceProtocol.Children.IsEmpty())
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "InstanceProtocol"), new ArgumentNullException("instanceProtocol"));
        }

        //todo:后补
        if (string.IsNullOrEmpty(scanId))
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Scan Id"), new ArgumentNullException("scanId"));
        }

        var protocolItems = ProtocolHelper.Expand(instanceProtocol);
        var deletingItem = protocolItems.FirstOrDefault(s => s.Scan.Descriptor.Id == scanId);

        if (deletingItem.Scan is null)
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Instance_NotExist_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Instance_NotExist_Description, "Scan"), new ArgumentNullException("scanId"));
        }

        BaseModel current = deletingItem.Scan;
        BaseModel parent = deletingItem.Measurement;
        deletingItem.Measurement.Children.Remove(deletingItem.Scan);
        if (!deletingItem.Measurement.Children.Any())
        {
            current = deletingItem.Measurement;
            parent = deletingItem.Frame;
            deletingItem.Frame.Children.Remove(deletingItem.Measurement);
        }
        if (!deletingItem.Frame.Children.Any())
        {
            current = deletingItem.Frame;
            parent = instanceProtocol;
            instanceProtocol.Children.Remove(deletingItem.Frame);
        }
        ProtocolHelper.ResetScanNumber(instanceProtocol);
        ProtocolStructureChanged?.Invoke(this, new EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>((parent, current, StructureChangeType.Delete)));

        return true;
    }

    public bool DeleteScan(ProtocolModel instanceProtocol, List<string> scanIds)
    {
        if (instanceProtocol is null || instanceProtocol.Children.IsEmpty())
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "InstanceProtocol"), new ArgumentNullException("instanceProtocol"));
        }
        foreach (string scanId in scanIds)
        {
            //todo:后补
            if (string.IsNullOrEmpty(scanId))
            {
                throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Scan Id"), new ArgumentNullException("scanId"));
            }
        }
        var protocolItems = ProtocolHelper.Expand(instanceProtocol);
        BaseModel parent = new BaseModel();
        BaseModel current = new BaseModel();
        foreach (string scanId in scanIds)
        {
            var deletingItem = protocolItems.FirstOrDefault(s => s.Scan.Descriptor.Id == scanId);

            if (deletingItem.Scan is null)
            {
                throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Instance_NotExist_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Instance_NotExist_Description, "Scan"), new ArgumentNullException("scanId"));
            }
            current = deletingItem.Scan;
            parent = deletingItem.Measurement;
            deletingItem.Measurement.Children.Remove(deletingItem.Scan);
            if (!deletingItem.Measurement.Children.Any())
            {
                current = deletingItem.Measurement;
                parent = deletingItem.Frame;
                deletingItem.Frame.Children.Remove(deletingItem.Measurement);
            }
            if (!deletingItem.Frame.Children.Any())
            {
                current = deletingItem.Frame;
                parent = instanceProtocol;
                instanceProtocol.Children.Remove(deletingItem.Frame);
            }
            ProtocolHelper.ResetScanNumber(instanceProtocol);
        }
        ProtocolStructureChanged?.Invoke(this, new EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>((parent, current, StructureChangeType.Delete)));
        return true;
    }

    public bool PasteScan(ProtocolModel instanceProtocol, string sourceScanId, string destinationScanId)
    {
        if (instanceProtocol is null || instanceProtocol.Children.IsEmpty())
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "InstanceProtocol"), new ArgumentNullException("instanceProtocol"));
        }

        if (string.IsNullOrEmpty(sourceScanId))
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Source Scan Id"), new ArgumentNullException("sourceScanId"));
        }

        if (string.IsNullOrEmpty(destinationScanId))
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Destination Scan Id"), new ArgumentNullException("destinationScanId"));
        }

        var protocolItems = ProtocolHelper.Expand(instanceProtocol);
        var sourceItem = protocolItems.FirstOrDefault(s => s.Scan.Descriptor.Id == sourceScanId);
        var destinationItem = protocolItems.FirstOrDefault(s => s.Scan.Descriptor.Id == destinationScanId);

        //todo: 可能已经完成扫描了，需要考虑

        var copyScan = sourceItem.Scan.Clone();
        ProtocolHelper.ResetPerformStatus(copyScan);
        ProtocolHelper.ResetId(copyScan);
        var copyMeasurement = new MeasurementModel
        {
            Descriptor = new DescriptorModel
            {
                Id = IdGenerator.Next(3),
                Type = typeof(MeasurementModel).Name,
                Name = "Measurement"
            },
            Children = new List<ScanModel>
            {
                copyScan
            }
        };
        ProtocolHelper.ResetParent(copyMeasurement, copyMeasurement.Children);
        var indexMeasurement = destinationItem.Frame.Children.IndexOf(destinationItem.Measurement);
        copyMeasurement.Parent = destinationItem.Frame;
        destinationItem.Frame.Children.Insert(indexMeasurement + 1, copyMeasurement);
        ProtocolHelper.ResetScanNumber(instanceProtocol);
        ProtocolHelper.SetReconSeriesNumber(instanceProtocol, copyScan);
        ProtocolStructureChanged?.Invoke(this, new EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>((destinationItem.Frame, copyMeasurement, StructureChangeType.Add)));

        return true;
    }

    public bool PasteScan(ProtocolModel instanceProtocol, List<string> sourceScanIds, string destinationScanId)
    {
        if (instanceProtocol is null || instanceProtocol.Children.IsEmpty())
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "InstanceProtocol"), new ArgumentNullException("instanceProtocol"));
        }
        foreach (var sourceScanId in sourceScanIds)
        {
            if (string.IsNullOrEmpty(sourceScanId))
            {
                throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Source Scan Id"), new ArgumentNullException("sourceScanId"));
            }
        }

        if (string.IsNullOrEmpty(destinationScanId))
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Destination Scan Id"), new ArgumentNullException("destinationScanId"));
        }

        var protocolItems = ProtocolHelper.Expand(instanceProtocol);
        var destinationItem = protocolItems.FirstOrDefault(s => s.Scan.Descriptor.Id == destinationScanId);
        var indexMeasurement = destinationItem.Frame.Children.IndexOf(destinationItem.Measurement);
        int i = 1;
        foreach (var sourceScanId in sourceScanIds)
        {
            var sourceItem = protocolItems.FirstOrDefault(s => s.Scan.Descriptor.Id == sourceScanId);
            //todo: 可能已经完成扫描了，需要考虑
            var copyScan = sourceItem.Scan.Clone();
            ProtocolHelper.ResetPerformStatus(copyScan);
            ProtocolHelper.ResetId(copyScan);
            //临时实现ROI不用一起拷贝
            if (copyScan.ScanOption == ScanOption.NVTestBolus
                || copyScan.ScanOption == ScanOption.TestBolus
                || copyScan.ScanOption == ScanOption.NVTestBolusBase)
            {
                foreach (var recon in copyScan.Children)
                {
                    if (recon.IsRTD && recon.Status == PerformStatus.Unperform)
                    {
                        recon.CycleROIs = new List<CycleROIModel>();
                    }
                }
            }
            var copyMeasurement = new MeasurementModel
            {
                Descriptor = new DescriptorModel
                {
                    Id = IdGenerator.Next(3),
                    Type = typeof(MeasurementModel).Name,
                    Name = "Measurement"
                },
                Children = new List<ScanModel> { copyScan }
            };

            ProtocolHelper.ResetParent(copyMeasurement, copyMeasurement.Children);
            copyMeasurement.Parent = destinationItem.Frame;
            destinationItem.Frame.Children.Insert(indexMeasurement + i, copyMeasurement);
            ProtocolHelper.ResetScanNumber(instanceProtocol);
            ProtocolHelper.SetReconSeriesNumber(instanceProtocol, copyScan);
            i++;
        }
        ProtocolStructureChanged?.Invoke(this, new EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>((destinationItem.Frame, destinationItem.Measurement, StructureChangeType.Add)));
        return true;
    }

    public bool RepeatScan(ProtocolModel instanceProtocol, string scanId)
    {
        if (instanceProtocol is null || instanceProtocol.Children.IsEmpty())
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "InstanceProtocol"), new ArgumentNullException("instanceProtocol"));
        }

        if (string.IsNullOrEmpty(scanId))
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Scan Id"), new ArgumentNullException("scanId"));
        }

        var protocolItems = ProtocolHelper.Expand(instanceProtocol);
        var sourceItem = protocolItems.FirstOrDefault(s => s.Scan.Descriptor.Id == scanId);
        var destinationItem = protocolItems.FirstOrDefault(s => s.Scan.Status == PerformStatus.Unperform);
        var repeatScan = sourceItem.Scan.Clone();
        ProtocolHelper.ResetPerformStatus(repeatScan);
        ProtocolHelper.ResetId(repeatScan);
        repeatScan.Children.ForEach(recon =>
        {
            ProtocolHelper.SetParameter(recon, ProtocolParameterNames.RECON_SERIES_NUMBER, "0");
        });
        var repeatMeasurement = new MeasurementModel
        {
            Descriptor = new DescriptorModel
            {
                Id = IdGenerator.Next(3),
                Type = typeof(MeasurementModel).Name,
                Name = "Measurement"
            },
            Children = new List<ScanModel>
            {
                repeatScan
            }
        };
        ProtocolHelper.ResetParent(repeatMeasurement, repeatMeasurement.Children);
        //TODO:需要考虑是否有未执行的Scan，是否在Measurement中间
        BaseModel parent;
        BaseModel current;
        if (destinationItem.Scan is null)
        {
            var lastFOR = instanceProtocol.Children.LastOrDefault();
            repeatMeasurement.Parent = lastFOR;
            lastFOR.Children.Add(repeatMeasurement);
            if (lastFOR.Children.Any(m => m.Status != PerformStatus.Unperform))
            {
                lastFOR.Status = PerformStatus.Performing;
            }
            else
            {
                lastFOR.Status = PerformStatus.Unperform;
            }
            parent = lastFOR;
            current = repeatMeasurement;
        }
        else
        {
            var indexMeasurement = destinationItem.Frame.Children.IndexOf(destinationItem.Measurement);
            destinationItem.Frame.Children.Insert(indexMeasurement, repeatMeasurement);
            repeatMeasurement.Parent = destinationItem.Frame;
            if (destinationItem.Frame.Children.Any(m => m.Status != PerformStatus.Unperform))
            {
                destinationItem.Frame.Status = PerformStatus.Performing;
            }
            else
            {
                destinationItem.Frame.Status = PerformStatus.Unperform;
            }
            parent = destinationItem.Frame;
            current = repeatMeasurement;
        }
        ProtocolHelper.ResetScanNumber(instanceProtocol);
        ProtocolHelper.SetReconSeriesNumber(instanceProtocol, repeatScan);
        ProtocolStructureChanged?.Invoke(this, new EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>((parent, current, StructureChangeType.Add)));

        return true;
    }

    public bool RepeatScan(ProtocolModel instanceProtocol, List<string> scanIds)
    {
        if (instanceProtocol is null || instanceProtocol.Children.IsEmpty())
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "InstanceProtocol"), new ArgumentNullException("instanceProtocol"));
        }
        foreach (var scanId in scanIds)
        {
            if (string.IsNullOrEmpty(scanId))
            {
                throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Scan Id"), new ArgumentNullException("scanId"));
            }
        }
        var protocolItems = ProtocolHelper.Expand(instanceProtocol);
        BaseModel parent = new BaseModel();
        BaseModel current = new BaseModel();
        MeasurementModel repeatMeasurement = new MeasurementModel();
        List<MeasurementModel> repeatMeasurements = new List<MeasurementModel>();
        //表示TestBolus跟Base中间不能插入其他的扫描流程
        var destinationItem = protocolItems.FirstOrDefault(s => s.Scan.Status == PerformStatus.Unperform && !(s.Scan.ScanOption == ScanOption.NVTestBolus || s.Scan.ScanOption == ScanOption.TestBolus));
        foreach (var scanId in scanIds)
        {
            var sourceItem = protocolItems.FirstOrDefault(s => s.Scan.Descriptor.Id == scanId);
            var repeatScan = sourceItem.Scan.Clone();
            ProtocolHelper.ResetPerformStatus(repeatScan);
            ProtocolHelper.ResetId(repeatScan);
            repeatScan.Children.ForEach(recon =>
            {
                ProtocolHelper.SetParameter(recon, ProtocolParameterNames.RECON_SERIES_NUMBER, "0");
            });

            //临时实现ROI不用一起拷贝
            if (repeatScan.ScanOption == ScanOption.NVTestBolus
                || repeatScan.ScanOption == ScanOption.TestBolus
                || repeatScan.ScanOption == ScanOption.NVTestBolusBase)
            {
                foreach (var recon in repeatScan.Children)
                {
                    if (recon.IsRTD && recon.Status == PerformStatus.Unperform)
                    {
                        recon.CycleROIs = new List<CycleROIModel>();
                    }
                }
            }
            repeatMeasurement = new MeasurementModel()
            {
                Descriptor = new DescriptorModel
                {
                    Id = IdGenerator.Next(3),
                    Type = typeof(MeasurementModel).Name,
                    Name = "Measurement"
                },
                Children = new List<ScanModel> {
                    repeatScan
                }
            };

            ProtocolHelper.ResetParent(repeatMeasurement, repeatMeasurement.Children);
            repeatMeasurements.Add(repeatMeasurement);
        }

        //TODO:需要考虑是否有未执行的Scan，是否在Measurement中间          
        if (destinationItem.Scan is null)
        {
            var lastFOR = instanceProtocol.Children.LastOrDefault();
            if (lastFOR is not null)
            {
                repeatMeasurement.Parent = lastFOR;
                foreach (var item in repeatMeasurements)
                {
                    lastFOR.Children.Add(item);
                    if (lastFOR.Children.Any(m => m.Status != PerformStatus.Unperform))
                    {
                        lastFOR.Status = PerformStatus.Performing;
                    }
                    else
                    {
                        lastFOR.Status = PerformStatus.Unperform;
                    }
                    parent = lastFOR;
                    current = item;
                }
            }
        }
        else
        {
            var measurementIndex = destinationItem.Frame.Children.IndexOf(destinationItem.Measurement);
            parent = destinationItem.Frame;
            int i = 0;
            foreach (var item in repeatMeasurements)
            {
                destinationItem.Frame.Children.Insert(measurementIndex + i, item);
                item.Parent = destinationItem.Frame;
                if (destinationItem.Frame.Children.Any(m => m.Status != PerformStatus.Unperform))
                {
                    destinationItem.Frame.Status = PerformStatus.Performing;
                }
                else
                {
                    destinationItem.Frame.Status = PerformStatus.Unperform;
                }
                current = item;
                i++;
            }
        }
        ProtocolHelper.ResetScanNumber(instanceProtocol);
        ProtocolHelper.ResetReconSeriesNumber(instanceProtocol);
        ProtocolStructureChanged?.Invoke(this, new EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>((parent, current, StructureChangeType.Add)));
        return true;
    }

    public bool ModifyFOR(ProtocolModel instanceProtocol, string measurementId, PatientPosition patientPosition)
    {
        if (instanceProtocol is null || instanceProtocol.Children.IsEmpty())
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "InstanceProtocol"), new ArgumentNullException("instanceProtocol"));
        }

        if (string.IsNullOrEmpty(measurementId))
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Measurement Id"), new ArgumentNullException("measurementId"));
        }

        var protocolItems = ProtocolHelper.ExpandMeasurements(instanceProtocol);
        var sourceItem = protocolItems.FirstOrDefault(s => s.Measurement.Descriptor.Id == measurementId);
        var index = protocolItems.IndexOf(sourceItem);
        if (sourceItem.Measurement is null || index < 0)
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Measurement Id"), new ArgumentNullException("measurementId"));
        }
		var transferItems = protocolItems.GetRange(index, protocolItems.Count - index);

        var newFOR = new FrameOfReferenceModel
        {
            Parent = instanceProtocol,
            Descriptor = new DescriptorModel
            {
                Id = UIDHelper.CreateFORUID(),
                Name = patientPosition.ToString(),
                Type = typeof(FrameOfReferenceModel).Name
            },
            Parameters = new List<ParameterModel> {
                new ParameterModel
                {
                    Name = ProtocolParameterNames.PATIENT_POSITION,
                    Value = patientPosition.ToString(),
                    Type = typeof(PatientPosition).Name
                }
            },
            Children = new List<MeasurementModel>()
        };

        //todo:此处处理需细化，重复添加
        foreach (var transferItem in transferItems)
        {
            if (!newFOR.Children.Any(m => m.Descriptor.Id == transferItem.Measurement.Descriptor.Id))
            {
                newFOR.Children.Add(transferItem.Measurement.Clone());
            }
        }
        ProtocolHelper.ResetParent(newFOR, newFOR.Children);

        //移除Measurement
        foreach (var transferItem in transferItems)
        {
            transferItem.Frame.Children.Remove(transferItem.Measurement);
        }
        //移除FOR
        foreach (var transferItem in transferItems)
        {
            if (!transferItem.Frame.Children.Any())
            {
                instanceProtocol.Children.Remove(transferItem.Frame);
            }
        }

        instanceProtocol.Children.Add(newFOR);
        ProtocolHelper.ResetScanNumber(instanceProtocol);
        ProtocolStructureChanged?.Invoke(this, new EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>((instanceProtocol, newFOR, StructureChangeType.Modify)));

        return true;
    }

    public bool CopyRecon(ProtocolModel instanceProtocol, string scanId, string sourceReconId, bool isRTD = false)
    {
        if (instanceProtocol is null || instanceProtocol.Children.IsEmpty())
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "InstanceProtocol"), new ArgumentNullException("instanceProtocol"));
        }

        if (string.IsNullOrEmpty(scanId))
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Scan Id"), new ArgumentNullException("scanId"));
        }

        if (string.IsNullOrEmpty(sourceReconId))
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Source Recon Id"), new ArgumentNullException("sourceReconId"));
        }

        var protocolItems = ProtocolHelper.Expand(instanceProtocol);
        var selectedItem = protocolItems.FirstOrDefault(s => s.Scan.Descriptor.Id == scanId);
        var sourceRecon = selectedItem.Scan.Children.FirstOrDefault(r => r.Descriptor.Id == sourceReconId);

        if (sourceRecon is null)
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Instance_NotExist_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Instance_NotExist_Description, "Source Recon Id"), new ArgumentNullException("sourceReconId"));
        }

        var cloneRecon = sourceRecon.Clone();
        cloneRecon.Status = PerformStatus.Unperform;
        cloneRecon.FailureReason = FailureReasonType.None;
        cloneRecon.IsRTD = isRTD;
        if (isRTD)
        {
            ProtocolHelper.SetParameter(cloneRecon, ProtocolParameterNames.RECON_RECON_TYPE, ReconType.HCT.ToString());
        }
        else if (cloneRecon.ReconType == ReconType.HCT)
        {
            ProtocolHelper.SetParameter(cloneRecon, ProtocolParameterNames.RECON_RECON_TYPE, ReconType.IVR_TV.ToString());
        }
        ProtocolHelper.SetParameter(cloneRecon, ProtocolParameterNames.RECON_IMAGE_PATH, string.Empty);
        cloneRecon.Descriptor.Id = IdGenerator.Next(5);
        cloneRecon.Parent = selectedItem.Scan;
        selectedItem.Scan.Children.Add(cloneRecon);
        ProtocolHelper.SetNextSeriesNumber(instanceProtocol, cloneRecon);
        ProtocolStructureChanged?.Invoke(this, new EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>((selectedItem.Scan, cloneRecon, StructureChangeType.Add)));

        return true;
    }

    public bool DeleteRecon(ProtocolModel instanceProtocol, string scanId, string reconId)
    {
        if (instanceProtocol is null || instanceProtocol.Children.IsEmpty())
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "InstanceProtocol"), new ArgumentNullException("instanceProtocol"));
        }

        if (string.IsNullOrEmpty(scanId))
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Scan Id"), new ArgumentNullException("scanId"));
        }

        if (string.IsNullOrEmpty(reconId))
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Description, "Recon Id"), new ArgumentNullException("reconId"));
        }

        var protocolItems = ProtocolHelper.Expand(instanceProtocol);
        var selectedItem = protocolItems.FirstOrDefault(s => s.Scan.Descriptor.Id == scanId);
        var sourceRecon = selectedItem.Scan.Children.FirstOrDefault(r => r.Descriptor.Id == reconId);
        if (sourceRecon is not null)
        {
            selectedItem.Scan.Children.Remove(sourceRecon);
            //ScanReconDeleted?.Invoke(this, new EventArgs<(ScanModel, ReconModel)>((selectedItem.Scan, sourceRecon)));
            ProtocolStructureChanged?.Invoke(this, new EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>((selectedItem.Scan, sourceRecon, StructureChangeType.Delete)));
        }
        return true;
    }
}