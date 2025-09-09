//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/3/27 15:10:38           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using NV.CT.DicomUtility.Graphic;
using NV.CT.FacadeProxy.Common.Enums;
using NV.MPS.Environment;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces
{
    public class AdjustTomoScanReconLengthByTopoDoneHandler : IHostedService
    {
        // private const double DefaultAxialLength = 42200;
       // private const double DefaultAxialLength = 0;    //将原先的默认值设置成0
        private readonly IProtocolHostService _protocolHostService;

        public AdjustTomoScanReconLengthByTopoDoneHandler(IProtocolHostService protocolHostService)
        {
            _protocolHostService = protocolHostService;
            _protocolHostService.PerformStatusChanged += PerformStatusService_PerformStatusChanged;
        }

        private void PerformStatusService_PerformStatusChanged(object? sender, CTS.EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
        {
            var scanModel = e.Data.Model as ScanModel;
            if (scanModel is null)
            {
                return;
            }

            if(scanModel.Status is not PerformStatus.Performed)
            {
                return;
            }

            if (scanModel.ScanOption is not ScanOption.Surview
                &&scanModel.ScanOption is not ScanOption.DualScout)
            {
                return;
            }

            //根据Topo扫描参数调整对应断层扫描参数。

            var forModel = scanModel.Parent.Parent;

            foreach (var measurement in forModel.Children)
            {
                if (measurement == scanModel.Parent)
                {
                    continue;                       //跳过当前scan
                }

                if (measurement.Status is PerformStatus.Performed)
                {
                    continue;                       //跳过完成Measurement
                }

                foreach (var scan in measurement.Children)
                {
                    if (scan.ScanOption is ScanOption.Surview or ScanOption.DualScout)
                    {
                        continue;                   //相同FOR下的定位像图像不同步。
                    }
                    AdjustTomoScanByTopoRange(scan, scanModel);
                }
            }
        }

        private void AdjustTomoScanByTopoRange(ScanModel tomoScan, ScanModel topoScan)
        {
            //todo: 扫描长度联动接口修改完毕后，直接使用扫描长度修改接口。
            Dictionary<BaseModel, List<ParameterModel>> resultDic = new Dictionary<BaseModel, List<ParameterModel>>();
            var scanStart = topoScan.ReconVolumeStartPosition;
            var scanLength = topoScan.ScanLength;
            var tomoRTDRecon = tomoScan.Children.Single(x => x.IsRTD);
            var topoRTDRecon = topoScan.Children.FirstOrDefault(x => x.IsRTD);
            var currentFor = topoScan.Parent.Parent;

            //校正扫描长度
            //todo: 轴扫默认长度与定位像长度无关，固定为47.2.以后大概率更改。
            var correctedLength = tomoScan.ScanOption is ScanOption.Axial ?
                (int)tomoScan.ScanLength :
                ScanLengthCorrectionHelper.GetCorrectedHelicalScanLength((int)scanLength, tomoRTDRecon.SliceThickness);

            var topoScanDirection = ScanLengthCorrectionHelper.GetTableDirection(currentFor.PatientPosition, topoRTDRecon.ImageOrder);

            var correctedScanEnd = topoScanDirection is TableDirection.In ? scanStart - correctedLength : scanStart + correctedLength;

            var tomoScanDirection = ScanLengthCorrectionHelper.GetTableDirection(topoScan.Parent.Parent.PatientPosition, tomoRTDRecon.ImageOrder);

            //设置的scan参数
            resultDic.Add(tomoScan, new List<ParameterModel>());
            resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_TABLE_DIRECTION, Value = tomoScanDirection.ToString() });

            //小剂量基底扫描跟小剂量测试扫描的起始点位置在同一个点上
            if (tomoScan.ScanOption == ScanOption.NVTestBolusBase || tomoScan.ScanOption == ScanOption.NVTestBolus || tomoScan.ScanOption == ScanOption.TestBolus)
            {
                resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_LENGTH, Value = 0.ToString() });
                if (tomoScanDirection == topoScanDirection)
                {
                    resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, Value = scanStart.ToString() });
                    resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = scanStart.ToString() });
                }
                else
                {
                    resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, Value = correctedScanEnd.ToString() });
                    resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = correctedScanEnd.ToString() });
                }
            }
            else
            {
                resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_LENGTH, Value = correctedLength.ToString() });
                if (tomoScanDirection == topoScanDirection)
                {
                    resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, Value = scanStart.ToString() });
                    resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = correctedScanEnd.ToString() });
                }
                else
                {
                    resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, Value = correctedScanEnd.ToString() });
                    resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = scanStart.ToString() });
                }
            }
            //重建参数单位mm
            var pos1 = UnitConvert.Micron2Millimeter((double)scanStart);
            var pos2 = UnitConvert.Micron2Millimeter((double)correctedScanEnd); 
            //遍历修改重建参数：
            foreach (var recon in tomoScan.Children)
            {
                resultDic.Add(recon, new List<ParameterModel>());
                //todo:待调整
                var posResult = ScanReconCoordinateHelper.GetTomoDefaultFirstLastCenterByScanRange(currentFor.PatientPosition, recon.ImageOrder, pos1, pos2);
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_FIRST_X, Value = ((int)UnitConvert.Millimeter2Micron(posResult[0])).ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y, Value = ((int)UnitConvert.Millimeter2Micron(posResult[1])).ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z, Value = ((int)UnitConvert.Millimeter2Micron(posResult[2])).ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_LAST_X, Value = ((int)UnitConvert.Millimeter2Micron(posResult[3])).ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_LAST_Y, Value = ((int)UnitConvert.Millimeter2Micron(posResult[4])).ToString() });

                var dirResult = ScanReconCoordinateHelper.GetDefaultTomoReconOrientation(currentFor.PatientPosition);
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X, Value = ((int)dirResult[0]).ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y, Value = ((int)dirResult[1]).ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z, Value = ((int)dirResult[2]).ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X, Value = ((int)dirResult[3]).ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y, Value = ((int)dirResult[4]).ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z, Value = ((int)dirResult[5]).ToString() });
                if (tomoScan.ScanOption == ScanOption.NVTestBolusBase || tomoScan.ScanOption == ScanOption.NVTestBolus || tomoScan.ScanOption == ScanOption.TestBolus)
                {
                    resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = ((int)UnitConvert.Millimeter2Micron(posResult[2])).ToString() });
                }
                else
                {
                    resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = ((int)UnitConvert.Millimeter2Micron(posResult[5])).ToString() });
                }
                //根据topo完成信息修改范围不涉及fov
                //var fov = 506.88;           //默认为最大fov
                //resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_LENGTH_HORIZONTAL, Value = fov.ToString() });
                //resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_LENGTH_VERTICAL, Value = fov.ToString() });

                //根据topo完成信息修改范围不涉及matrix
                //var matrixSize = recon.IsRTD ? 512 : 1024;                
                //resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_HORIZONTAL, Value = matrixSize.ToString() });
                //resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_VERTICAL, Value = matrixSize.ToString() });
            }
            _protocolHostService.SetParameters(resultDic);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}