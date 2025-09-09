using NV.CT.DicomUtility.Graphic;
using NV.CT.FacadeProxy.Common.Enums;
using NV.MPS.Exception;

namespace NV.CT.Examination.ApplicationService.Impl.ProtocolExtension.ModificationRule
{
    public class ImageOrderLinkedRule : ILinkedModificationRule
    {

        public bool CanAccept(BaseModel model, string parameterName)
        {
            if (model is ReconModel)
            {
                if (parameterName == ProtocolParameterNames.RECON_IMAGE_ORDER)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 若改变图像方向的重建不是rtd重建，只需要适配图像方向。
        /// 若改变图像方向的重建是rtd重建，还需要改变对应扫描的扫描方向与扫描位置。
        /// </summary>
        /// <param name="model"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public Dictionary<BaseModel, List<ParameterModel>> GetLinkedModificationParam(BaseModel model, ParameterModel parameter)
        {
            var result = new Dictionary<BaseModel, List<ParameterModel>>();
            var newImageOrder = ParameterConverter.Convert<ImageOrders>(parameter.Value);

            var recon = model as ReconModel;

            if (recon is not null)
            {
                if (recon.IsRTD)
                {
                    var scanModel = recon.Parent;
                    switch (scanModel.ScanOption)
                    {
                        case ScanOption.Surview:
                            //定位像无法对重建进行编辑，不应该发生ImageOrder的变化。
                            throw new NanoException("MCS000001001", "RTD Recon should not be edited");
                        case ScanOption.Axial:
                        case ScanOption.Helical:
                            result.Add(recon, GetChangeListForTomo(recon, newImageOrder));
                            break;
                        case ScanOption.DualScout:
                            //定位像无法对重建进行编辑，不应该发生ImageOrder的变化。
                            throw new NanoException("MCS000001002", "RTD Recon should not be edited");
                        default:
                            throw new NanoException("MCS000001003", "The ScanOption is invalid{scanModel.ScanOption}");
                    }

                    result.Add(scanModel, GetChangeListForScan(scanModel, newImageOrder));
                }
                else
                {
                    result.Add(recon, GetChangeListForTomo(recon, newImageOrder));
                }
            }

            return result;
        }


        private List<ParameterModel> GetChangeListForTomo(ReconModel recon, ImageOrders imageOrder)
        {
            var result = new List<ParameterModel>();

            var firstZ = recon.CenterFirstZ;
            var lastZ = recon.CenterLastZ;

            var largeValue = firstZ > lastZ ? firstZ : lastZ;
            var smallValue = firstZ < lastZ ? firstZ : lastZ;

            if (imageOrder is ImageOrders.HeadFoot)
            {
                result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z, Value = largeValue.ToString() });
                result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = smallValue.ToString() });
            }
            else
            {
                result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z, Value = smallValue.ToString() });
                result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = largeValue.ToString() });
            }

            return result;
        }

        private List<ParameterModel> GetChangeListForScan(ScanModel scan, ImageOrders imageOrder)
        {
            var result = new List<ParameterModel>();
            var patientPosition = scan.Parent.Parent.PatientPosition;

            var newTableDirection = CoordinateConverter.Instance.GetTableDirectionByImageOrder(patientPosition, imageOrder);

            var start = scan.ReconVolumeStartPosition;
            var end = scan.ReconVolumeEndPosition;

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

            result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_TABLE_DIRECTION, Value = newTableDirection.ToString() });

            return result;
        }
    }
}
