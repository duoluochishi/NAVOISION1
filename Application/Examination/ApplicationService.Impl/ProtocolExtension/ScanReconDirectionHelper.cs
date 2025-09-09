using NV.CT.DicomUtility.Graphic;
using NV.CT.FacadeProxy.Common.Enums;
using NV.MPS.Environment;
using TubePos = NV.CT.FacadeProxy.Common.Enums.TubePosition;

namespace NV.CT.Examination.ApplicationService.Impl.ProtocolExtension
{
    public static class ScanReconDirectionHelper
    {
        /// <summary>
        /// 在点击confirm时根据当前参数调整整个protocol中所有扫描重建方向参数使其符合摆位与重建方向。
        /// 并调整扫描起始、终止位置与重建起始终止位置，使之符合逻辑。
        /// 注意：这里不做扫描、重建范围的限位判断, 认为调整前参数都是对的。
        /// </summary>
        /// <param name="protocol"></param>
        public static Dictionary<BaseModel, List<ParameterModel>> AdjustAllScanReconDirections(IProtocolHostService protocolHostService)
        {
            var items = protocolHostService.Models;

            var changingDic = new Dictionary<BaseModel, List<ParameterModel>>();
            foreach (var item in items)
            {
                var scan = item.Scan;
                if (scan.Status != PerformStatus.Unperform)  //仅针对未执行的扫描！
                {
                    continue;
                }

                //Scan 根据当前体位与RTD的ImageOrder，更新TableDirection                
                changingDic.Add(scan, GetModifiedScanParameters(scan));

                var recons = item.Scan.Children;
                if (scan.ScanOption == ScanOption.Surview)
                {
                    //定位像图像只有一个RTD recon. 添加该重建的待更新参数，主要为重建方向。
                    recons.ForEach(recon => {
                        changingDic.Add(recon, GetModifiedTopoRTDReconParameters(recon));
                    });
                }
                else if (scan.ScanOption == ScanOption.DualScout)
                {
                    changingDic.Add(recons[0], GetModifiedTopoRTDReconParameters(recons[0], 0));
                    changingDic.Add(recons[1], GetModifiedTopoRTDReconParameters(recons[1], 1));
                }
                else
                {
                    //断层扫描更新所有重建的重建方向。
                    //该方法只会在Confirm和切换FOR时发生，此前的所有参数认为已经无效，直接刷新为与当前扫描范围相符的重建参数。
                    recons.ForEach(recon => {
                        changingDic.Add(recon, GetModifiedTomoReconParameters(recon));
                    });
                }
            }
            return changingDic;
        }

        private static List<ParameterModel> GetModifiedScanParameters(ScanModel scan)
        {
            //Scan 根据当前体位与RTD的ImageOrder，更新TableDirection
            //同时根据原来的Start和length，更新扫描范围, 若方向改变，则对调start，end
            var rtdRecon = scan.Children.FirstOrDefault(x => x.IsRTD);
            var pp = scan.Parent.Parent.PatientPosition;
            var tableDirection = CoordinateConverter.Instance.GetTableDirectionByImageOrder(pp, rtdRecon.ImageOrder);

            var start =  scan.ReconVolumeStartPosition;
            var length =  (int)scan.ScanLength;
            var end = scan.TableDirection == TableDirection.In ? start - length : start + length;

            var smallerValue = start > end ? end : start;
            var largerValue = start > end ? start : end;

            if(tableDirection == TableDirection.In)
            {
                start = largerValue;
                end = smallerValue;
            }
            else
            {
                start = smallerValue;
                end = largerValue;
            }

            List<ParameterModel> scanParamList = new List<ParameterModel>();
            scanParamList.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_TABLE_DIRECTION, Value = tableDirection.ToString() });
            
            scanParamList.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, Value = start.ToString() });
            scanParamList.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = end.ToString() });
            return scanParamList;
        }

        private static List<ParameterModel> GetModifiedTopoRTDReconParameters(ReconModel recon,int index = 0)
        {
            var pp = recon.Parent.Parent.Parent.PatientPosition;
            var tubePosition = (TubePos)(recon.Parent.TubePositions[index]);
            var dir = ScanReconCoordinateHelper.GetDefaultTopoReconOrientation(pp, tubePosition);
            var scan = recon.Parent;

            var start = scan.ReconVolumeStartPosition;
            var length = (int)scan.ScanLength;
            var end = scan.TableDirection == TableDirection.In ? start - length : start + length;

            var startD = UnitConvert.Micron2Millimeter((double)start);
            var endD = UnitConvert.Micron2Millimeter((double)end);

            var pos = ScanReconCoordinateHelper.GetTopoReconParamByScanRange(pp, startD, endD);        //图像坐标系下中心点            

            //除了方向，还需要根据扫描长度自动校正定位像的矩阵大小。
            var fovLengthHor = recon.FOVLengthHorizontal;
            var pixelSpacing = UnitConvert.Micron2Millimeter((float)(recon.FOVLengthHorizontal / recon.ImageMatrixHorizontal));
            var fovLengthVer = length;
            var matrixVer = fovLengthVer / pixelSpacing;
            List<ParameterModel> result = new();
            
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X, Value = dir[0].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y, Value = dir[1].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z, Value = dir[2].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X, Value = dir[3].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y, Value = dir[4].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z, Value = dir[5].ToString() });

            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_LENGTH_VERTICAL, Value = ((int)fovLengthVer).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_VERTICAL, Value = ((int)matrixVer).ToString() });

            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_X, Value = ((int)UnitConvert.Millimeter2Micron(pos[0])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y, Value = ((int)UnitConvert.Millimeter2Micron(pos[1])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z, Value = ((int)UnitConvert.Millimeter2Micron(pos[2])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_X, Value = ((int)UnitConvert.Millimeter2Micron(pos[0])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Y, Value = ((int)UnitConvert.Millimeter2Micron(pos[1])).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = ((int)UnitConvert.Millimeter2Micron(pos[2])).ToString() });            

            return result;
        }
                
        private static List<ParameterModel> GetModifiedTomoReconParameters(ReconModel recon)
        {
            var pp = recon.Parent.Parent.Parent.PatientPosition;
            var dir = ScanReconCoordinateHelper.GetDefaultTomoReconOrientation(pp);
            var scan = recon.Parent;

            var start = scan.ReconVolumeStartPosition;
            var length = (int)scan.ScanLength;
            var end = scan.TableDirection == TableDirection.In ? start - length : start + length;

            var startD = UnitConvert.Micron2Millimeter((double)start);
            var endD = UnitConvert.Micron2Millimeter((double)end);

            var pos = ScanReconCoordinateHelper.GetTomoDefaultFirstLastCenterByScanRange(pp,recon.ImageOrder,startD,endD);

            List<ParameterModel> result = new List<ParameterModel>();
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X, Value = dir[0].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y, Value = dir[1].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z, Value = dir[2].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X, Value = dir[3].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y, Value = dir[4].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z, Value = dir[5].ToString() });

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
