using NV.CT.DicomUtility.Graphic;
using NV.CT.FacadeProxy.Common.Enums;
using NV.MPS.Configuration;
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
            //改变适配逻辑，以扫描参数为准,不再以RTD重建方向为准。
            //适配依据：扫描长度，扫描方向
            //同时根据原来的Start和length，更新扫描范围

            var pp = scan.Parent.Parent.PatientPosition;
            var tableDirection = scan.TableDirection;
            var start =  scan.ReconVolumeStartPosition;
            var end = scan.ReconVolumeEndPosition;
            var length =  (int)scan.ScanLength;

            var smallerValue = start > end ? end : start;
            var largerValue = start > end ? start : end;
            
            var tableInfo = SystemConfig.TableConfig.Table;

            if (tableDirection == TableDirection.In)            //增加床位限位判断逻辑，保证扫描可用长度
            {
                start = largerValue < tableInfo.MaxZ.Value?largerValue:tableInfo.MaxZ.Value;

                end = start - length;
                if(end < tableInfo.MinZ.Value)
                {
                    end = tableInfo.MinZ.Value;
                    start = end + length;
                }
            }
            else
            {
                start = smallerValue > tableInfo.MinZ.Value?smallerValue:tableInfo.MinZ.Value;
                end = start + length;
                if(end > tableInfo.MaxZ.Value)
                {
                    end = tableInfo.MaxZ.Value;
                    start = end - length;
                }
            }

            List<ParameterModel> scanParamList = new List<ParameterModel>();
           
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

            var pos = ScanReconCoordinateHelper.GetTopoReconParamByScanRange(pp, start, end);        //图像坐标系下中心点            

            //需要根据扫描长度自动校正定位像的矩阵大小。
            var fovLengthHor = recon.FOVLengthHorizontal;
            var pixelSpacing = (float)(recon.FOVLengthHorizontal / recon.ImageMatrixHorizontal);
            var fovLengthVer = length;
            var matrixVer = fovLengthVer / pixelSpacing;

            //根据扫描方向确定ImageOrder
            var imageOrder = CoordinateConverter.Instance.GetImageOrderByTableDirection(pp, scan.TableDirection);

            List<ParameterModel> result = new();
            
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X, Value = dir[0].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y, Value = dir[1].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z, Value = dir[2].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X, Value = dir[3].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y, Value = dir[4].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z, Value = dir[5].ToString() });

            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_LENGTH_VERTICAL, Value = ((int)fovLengthVer).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_VERTICAL, Value = ((int)matrixVer).ToString() });

            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_X, Value = ((int)pos[0]).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y, Value = ((int)pos[1]).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z, Value = ((int)pos[2]).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_X, Value = ((int)pos[0]).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Y, Value = ((int)pos[1]).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = ((int)pos[2]).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_IMAGE_ORDER, Value = imageOrder.ToString() });
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
            var imageOrder = recon.ImageOrder;

            if (recon.IsRTD)
            {
                imageOrder = CoordinateConverter.Instance.GetImageOrderByTableDirection(pp, scan.TableDirection);
            }

            var pos = ScanReconCoordinateHelper.GetTomoDefaultFirstLastCenterByScanRange(pp, imageOrder, start,end);

            List<ParameterModel> result = new List<ParameterModel>();
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X, Value = dir[0].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y, Value = dir[1].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z, Value = dir[2].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X, Value = dir[3].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y, Value = dir[4].ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z, Value = dir[5].ToString() });

            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_X, Value = ((int)pos[0]).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y, Value = ((int)pos[1]).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z, Value = ((int)pos[2]).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_X, Value = ((int)pos[3]).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Y, Value = ((int)pos[4]).ToString() });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = ((int)pos[5]).ToString() });

            result.Add(new ParameterModel { Name = ProtocolParameterNames.RECON_IMAGE_ORDER, Value = imageOrder.ToString() });

            return result;
        }
    }
}
