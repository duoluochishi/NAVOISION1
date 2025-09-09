using NV.CT.DicomUtility.Graphic;
using NV.CT.FacadeProxy.Common.Enums;
using NV.MPS.Environment;

namespace NV.CT.Examination.ApplicationService.Impl.ProtocolExtensions.ModificationRule
{
    public class ScanStartPositionLinkedRule:ILinkedModificationRule
    {
        public bool CanAccept(BaseModel model, string parameterName)
        {

            if (model is ScanModel)
            {
                if (parameterName == ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION)
                {
                    return true;
                }
            }
            return false;
        }

        public Dictionary<BaseModel, List<ParameterModel>> GetLinkedModificationParam(BaseModel model, ParameterModel parameter)
        {
            var result = new Dictionary<BaseModel, List<ParameterModel>>();

            var scanModel = model as ScanModel;
            int start, end, length;
            var table_direction = scanModel.TableDirection;
            start = ParameterConverter.Convert<int>(parameter.Value);
            length = (int)scanModel.ScanLength;
            end = table_direction == TableDirection.In ? start - length : start + length;

            List<ParameterModel> scanParamList = new List<ParameterModel>
            {
                new ParameterModel { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, Value = start.ToString() },
                new ParameterModel { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = end.ToString() }
            };
            result.Add(scanModel, scanParamList);

            var forModel = scanModel.Parent.Parent;
            //单位精度转化
            var startD = UnitConvert.Micron2Millimeter((double)start);
            var lengthD = UnitConvert.Micron2Millimeter((double)length);
            var endD = UnitConvert.Micron2Millimeter((double)end);


            if (scanModel.ScanOption == ScanOption.Surview)             //定位像
            {
                var topoRTDRecon = scanModel.Children.Single(x => x.IsRTD);
                result.Add(topoRTDRecon, GetChangeListForTopo(topoRTDRecon,startD,lengthD));
            }
            else if(scanModel.ScanOption == ScanOption.DualScout)
            {
                var recons = scanModel.Children;
                result.Add(recons[0], GetChangeListForTopo(recons[0], startD, lengthD));
                result.Add(recons[1], GetChangeListForTopo(recons[1], startD, lengthD));
            }
            else                                                //断层图像
            {
                var recons = scanModel.Children;
                var deltaPos = UnitConvert.Micron2Millimeter(start - scanModel.ReconVolumeStartPosition);

                foreach (var recon in recons)
                {
                    if (recon.IsRTD)
                    {
                        result.Add(recon, GetChangeListForRTDTomo(recon, startD, endD));
                    }
                    else
                    {
                        result.Add(recon, GetChangeListForTomo(recon, deltaPos));
                    }
                }
            }
            return result;
        }

        private List<ParameterModel> GetChangeListForTopo(ReconModel recon, double start, double length)
        {
            var result = new List<ParameterModel>();

            var forModel = recon.Parent.Parent.Parent;
            var topoCenter = ScanReconCoordinateHelper.GetTopoReconParamByScanRange(forModel.PatientPosition, start, length, recon.ImageOrder);

            var pixelSpacing = UnitConvert.Micron2Millimeter((float)(recon.FOVLengthHorizontal / recon.ImageMatrixHorizontal));
            int matrixVer = (int)(length / pixelSpacing);
            double correctedLength = matrixVer * pixelSpacing;

            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_X, Value = ((int)UnitConvert.Millimeter2Micron(topoCenter[0])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y, Value = ((int)UnitConvert.Millimeter2Micron(topoCenter[1])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z, Value = ((int)UnitConvert.Millimeter2Micron(topoCenter[2])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_X, Value = ((int)UnitConvert.Millimeter2Micron(topoCenter[0])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Y, Value = ((int)UnitConvert.Millimeter2Micron(topoCenter[1])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = ((int)UnitConvert.Millimeter2Micron(topoCenter[2])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_LENGTH_VERTICAL, Value = ((int)correctedLength).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_VERTICAL, Value = ((int)matrixVer).ToString() });

            return result;
        }

        private List<ParameterModel> GetChangeListForRTDTomo(ReconModel recon, double start, double end)
        {
            var result = new List<ParameterModel>();
            var forModel = recon.Parent.Parent.Parent;

            var pos = ScanReconCoordinateHelper.GetTomoDefaultFirstLastCenterByScanRange(forModel.PatientPosition, recon.ImageOrder, start, end);
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_X, Value = ((int)UnitConvert.Millimeter2Micron(pos[0])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y, Value = ((int)UnitConvert.Millimeter2Micron(pos[1])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z, Value = ((int)UnitConvert.Millimeter2Micron(pos[2])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_X, Value = ((int)UnitConvert.Millimeter2Micron(pos[3])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Y, Value = ((int)UnitConvert.Millimeter2Micron(pos[4])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = ((int)UnitConvert.Millimeter2Micron(pos[5])).ToString() });

            return result;
        }

        private List<ParameterModel> GetChangeListForTomo(ReconModel recon, double delta)
        {
            var result = new List<ParameterModel>();

            var forModel = recon.Parent.Parent.Parent;
            var deltaPosInPatient = CoordinateConverter.Instance.TransformSliceLocationDeviceToPatient(forModel.PatientPosition, delta);

            var firstZ = recon.CenterFirstZ + (int)UnitConvert.Millimeter2Micron(deltaPosInPatient);
            var lastZ = recon.CenterLastZ + (int)UnitConvert.Millimeter2Micron(deltaPosInPatient);

            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z, Value = firstZ.ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = lastZ.ToString() });

            return result;
        }
    }
}
