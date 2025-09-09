using NV.CT.DicomUtility.Graphic;
using NV.CT.FacadeProxy.Common.Enums;
using NV.MPS.Environment;

namespace NV.CT.Examination.ApplicationService.Impl.ProtocolExtensions.ModificationRule
{
    public class ScanLengthLinkedRule : ILinkedModificationRule
    {
        /// <summary>
        /// 当且仅当扫描Range相关参数（包括Start,End,Length）发生变化的时候,调整扫描参数及其下属Recon参数。
        /// 注意： 在该方法中不对参数有效性进行验证。
        /// 有效性的验证在参数修改后的参数验证过程中体现。
        /// todo:当前做法可能有点问题，长度调整后，将扫描结束位置直接作为重建的一个边。 后续可能需要考虑重建长度与重加方向。
        /// </summary>
        /// <param name="model"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public bool CanAccept(BaseModel model, string parameterName)
        {
            if (model is ScanModel)
            {
                if (parameterName == ProtocolParameterNames.SCAN_LENGTH)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取关联修改参数。
        /// 注意，关联修改不关心参数的验证。
        /// 即修改扫描长度，不验证参数是否合理。
        /// 需要由调用者确定scanlength是否合理。
        /// </summary>
        /// <param name="model"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public Dictionary<BaseModel, List<ParameterModel>> GetLinkedModificationParam(BaseModel model, ParameterModel parameter)
        {
            var result = new Dictionary<BaseModel, List<ParameterModel>>();

            if (model is ScanModel scanModel)
            {
                int start, end, length;
                var table_direction = scanModel.TableDirection;
                start = scanModel.ReconVolumeStartPosition;
                length = ParameterConverter.Convert<int>(parameter.Value);

                if (scanModel.ScanOption == ScanOption.NVTestBolus
                    || scanModel.ScanOption == ScanOption.TestBolus
                    || scanModel.ScanOption == ScanOption.NVTestBolusBase)
                {
                    length = (int)scanModel.ScanLength;
                }
                end = table_direction == TableDirection.In ? start - length : start + length;

                //扫描参数变化
                List<ParameterModel> scanParamList = new List<ParameterModel>
                {
                    new ParameterModel { Name = ProtocolParameterNames.SCAN_LENGTH, Value = length.ToString() },
                    new ParameterModel { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = end.ToString() }
                };
                result.Add(scanModel, scanParamList);

                var forModel = scanModel.Parent.Parent;
                //单位转换
                var startD = UnitConvert.Micron2Millimeter((double)start);
                double lengthD = UnitConvert.Micron2Millimeter((double)length);
                double endD = UnitConvert.Micron2Millimeter((double)end);

                if (scanModel.ScanOption == ScanOption.Surview) //定位像
                {
                    var topoRTDRecon = scanModel.Children.Single(x => x.IsRTD);
                    result.Add(topoRTDRecon, GetChangeListForTopo(topoRTDRecon, startD, lengthD));
                }
                else if (scanModel.ScanOption == ScanOption.DualScout)
                {
                    var recons = scanModel.Children;
                    result.Add(recons[0], GetChangeListForTopo(recons[0], startD, lengthD));
                    result.Add(recons[1], GetChangeListForTopo(recons[1], startD, lengthD));
                }
                else //断层扫描,先简单实现，直接拉到
                {
                    var recons = scanModel.Children;
                    foreach (var recon in recons)
                    {
                        if (recon.IsRTD)
                        {
                            result.Add(recon, GetChangeListForRTDTomo(recon, startD, endD));
                        }
                        else
                        {
                            result.Add(recon, GetChangeListForTomo(recon, startD, endD));
                        }
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

        private List<ParameterModel> GetChangeListForTomo(ReconModel recon, double start, double end)
        {
            var result = new List<ParameterModel>();

            var forModel = recon.Parent.Parent.Parent;
            var scanModel = recon.Parent;
            //单位转换
            var oldStart = UnitConvert.Micron2Millimeter((double)scanModel.ReconVolumeStartPosition);
            var oldEnd = UnitConvert.Micron2Millimeter((double)scanModel.ReconVolumeEndPosition);
            var oldRange = ScanReconCoordinateHelper.GetFirstLastZByScanRange(forModel.PatientPosition, recon.ImageOrder, oldStart, oldEnd);

            //根据当前参数判断是否同步调整
            var isSyncFirstZ = Math.Abs(recon.CenterFirstZ - UnitConvert.Millimeter2Micron(oldRange.FirstZ)) < 1;
            var isSyncLastZ = Math.Abs(recon.CenterLastZ - UnitConvert.Millimeter2Micron(oldRange.LastZ)) < 1;


            double newFirst = UnitConvert.Micron2Millimeter((double)recon.CenterFirstZ);
            double newLast = UnitConvert.Micron2Millimeter((double)recon.CenterLastZ);
            var firstLast = ScanReconCoordinateHelper.GetFirstLastZByScanRange(forModel.PatientPosition, recon.ImageOrder, start, end);

            if (isSyncFirstZ)
            {
                newFirst = firstLast.FirstZ;
            }
            if (isSyncLastZ)
            {
                newLast = firstLast.LastZ;
            }

            if (recon.ImageOrder == ImageOrders.HeadFoot)    //头到脚，First>Last
            {
                newFirst = newFirst > firstLast.FirstZ ? firstLast.FirstZ : newFirst;
                newLast = newLast < firstLast.LastZ ? firstLast.LastZ : newLast;
            }
            else                                            //脚到头，First<Last
            {
                newFirst = newFirst < firstLast.FirstZ ? firstLast.FirstZ : newFirst;
                newLast = newLast > firstLast.LastZ ? firstLast.LastZ : newLast;
            }

            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z, Value = ((int)UnitConvert.Millimeter2Micron(newFirst)).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = ((int)UnitConvert.Millimeter2Micron(newLast)).ToString() });
            return result;
        }
    }
}
