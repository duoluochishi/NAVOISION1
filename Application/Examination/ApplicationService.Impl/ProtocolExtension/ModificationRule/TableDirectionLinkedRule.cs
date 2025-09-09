using NV.CT.DicomUtility.Graphic;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Protocol.Models;
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.Examination.ApplicationService.Impl.ProtocolExtension.ModificationRule
{
    public class TableDirectionLinkedRule : ILinkedModificationRule
    {
        public bool CanAccept(BaseModel model, string parameterName)
        {
            if (model is ScanModel)
            {
                if (parameterName == ProtocolParameterNames.SCAN_TABLE_DIRECTION)
                {
                    return true;
                }
            }
            return false;
        }

        //扫描床方向发生变化，会影响扫描起始与终止位置，和RTD图像的图像方向，同时断层RTD图像的起始与终止位置也会反转。
        public Dictionary<BaseModel, List<ParameterModel>> GetLinkedModificationParam(BaseModel model, ParameterModel parameter)
        {
            var result = new Dictionary<BaseModel, List<ParameterModel>>();
            if (model is ScanModel scanModel)
            {
                var rtdRecon = scanModel.Children.FirstOrDefault(x => x.IsRTD);
                if (rtdRecon is not null)
                {
                    var newTableDirection = ParameterConverter.Convert<TableDirection>(parameter.Value);
                    switch (scanModel.ScanOption)
                    {
                        case ScanOption.Surview:
                            result.Add(scanModel, GetChangeListForTopoScan(scanModel, newTableDirection));
                            result.Add(rtdRecon, GetChangeListForTopoRTD(rtdRecon, newTableDirection));
                            break;
                        case ScanOption.Axial:
                        case ScanOption.Helical:
                            result.Add(scanModel, GetChangeListForTomoScan(scanModel, newTableDirection));
                            result.Add(rtdRecon, GetChangeListForTomoRTD(scanModel, newTableDirection));
                            break;
                        case ScanOption.DualScout:
                            result.Add(scanModel, GetChangeListForTopoScan(scanModel, newTableDirection));
                            var recons = scanModel.Children;
                            result.Add(recons[0], GetChangeListForTopoRTD(recons[0], newTableDirection));
                            result.Add(recons[1], GetChangeListForTopoRTD(recons[1], newTableDirection));
                            break;
                        default:
                            throw new InvalidOperationException($"The ScanOption is invalid{scanModel.ScanOption}");
                    }
                }
            }
            return result;
        }

        private List<ParameterModel> GetChangeListForTopoScan(ScanModel scanModel, TableDirection newTableDirection)
        {
            var result = new List<ParameterModel>();
            var start = scanModel.ReconVolumeStartPosition;
            var end = scanModel.ReconVolumeEndPosition;
            if (newTableDirection == TableDirection.In)
            {
                end = scanModel.ReconVolumeStartPosition - (int)scanModel.ScanLength;
            }
            else
            {
                end = scanModel.ReconVolumeStartPosition + (int)scanModel.ScanLength;
            }
            //TODO:先保留一定的长度去做加减速和曝光区域的缓冲（5cm）
            int effectiveLength = 50000;
            int tableMaxZ = SystemConfig.TableConfig.Table.MaxZ.Value - effectiveLength;
            int tableMinZ = SystemConfig.TableConfig.Table.MinZ.Value + effectiveLength;

            result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, Value = start.ToString() });
            if (end > tableMaxZ)
            {
                result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_LENGTH, Value = Math.Abs(tableMaxZ - start).ToString() });
                result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = tableMaxZ.ToString() });
            }
            else if (end < tableMinZ)
            {
                result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_LENGTH, Value = Math.Abs(tableMinZ - start).ToString() });
                result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = tableMinZ.ToString() });
            }
            else
            {
                result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = end.ToString() });
            }
            return result;
        }

        private List<ParameterModel> GetChangeListForTomoScan(ScanModel scanModel, TableDirection newTableDirection)
        {
            var result = new List<ParameterModel>();

            var start = scanModel.ReconVolumeStartPosition;
            var end = scanModel.ReconVolumeEndPosition;

            var largeValue = start > end ? start : end;
            var smallValue = start < end ? start : end;

            if (newTableDirection == TableDirection.In)
            {
                result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, Value = largeValue.ToString() });
                result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = smallValue.ToString() });
            }
            else
            {
                result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, Value = smallValue.ToString() });
                result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = largeValue.ToString() });
            }

            return result;
        }

        private List<ParameterModel> GetChangeListForTopoRTD(ReconModel rtdRecon, TableDirection newTableDirection)
        {
            var result = new List<ParameterModel>();
            var patientPosition = rtdRecon.Parent.Parent.Parent.PatientPosition;


            var newImageOrder = CoordinateConverter.Instance.GetImageOrderByTableDirection(patientPosition, newTableDirection);

            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_IMAGE_ORDER, Value = newImageOrder.ToString() });

            return result;
        }

        private List<ParameterModel> GetChangeListForTomoRTD(ScanModel scanModel, TableDirection newTableDirection)
        {
            var result = new List<ParameterModel>();
            var rtdRecon = scanModel.Children.Single(x => x.IsRTD);
            var patientPosition = scanModel.Parent.Parent.PatientPosition;

            var scanStart = UnitConvert.Micron2Millimeter((double)scanModel.ReconVolumeStartPosition);
            var scanEnd = UnitConvert.Micron2Millimeter((double)scanModel.ReconVolumeEndPosition);


            var newImageOrder = CoordinateConverter.Instance.GetImageOrderByTableDirection(patientPosition, newTableDirection);
            var pos = ScanReconCoordinateHelper.GetTomoDefaultFirstLastCenterByScanRange(patientPosition, newImageOrder, scanStart, scanEnd);

            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_IMAGE_ORDER, Value = newImageOrder.ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_X, Value = ((int)UnitConvert.Millimeter2Micron(pos[0])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y, Value = ((int)UnitConvert.Millimeter2Micron(pos[1])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z, Value = ((int)UnitConvert.Millimeter2Micron(pos[2])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_X, Value = ((int)UnitConvert.Millimeter2Micron(pos[3])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Y, Value = ((int)UnitConvert.Millimeter2Micron(pos[4])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = ((int)UnitConvert.Millimeter2Micron(pos[5])).ToString() });

            return result;

        }
    }
}