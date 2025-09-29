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
using NV.CT.UI.Exam.Extensions;
using NV.MPS.Environment;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces
{
    public class AdjustTomoScanReconLengthByTopoDoneHandler : IHostedService
    {        
        private readonly IProtocolHostService _protocolHostService;

        public AdjustTomoScanReconLengthByTopoDoneHandler(IProtocolHostService protocolHostService)
        {
            _protocolHostService = protocolHostService;
			_protocolHostService.PerformStatusChanged -= PerformStatusService_PerformStatusChanged;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tomoScan"></param>
        /// <param name="topoScan"></param>
        private void AdjustTomoScanByTopoRange(ScanModel tomoScan, ScanModel topoScan)
        {
            //修正断层扫描在定位像完成后的范围自适应逻辑。
            //1. 扫描开始位置定义：保证定位像Head方向范围与断层范围中头范围一致
            //2. 长度在协议定义下进行微调，满足具体扫描参数
            //3. 扫描开始位置根据扫描方向进行调整，保证扫描范围在定位像范围内。
            //4. 根据扫描长度扫描开始位置，确定扫描结束位置。
            //5. todo:根据床位限位，校正开始结束位置与长度
            Dictionary<BaseModel, List<ParameterModel>> resultDic = new Dictionary<BaseModel, List<ParameterModel>>();

            var topoSmallValue = topoScan.ReconVolumeEndPosition > topoScan.ReconVolumeStartPosition ? 
                topoScan.ReconVolumeStartPosition : topoScan.ReconVolumeEndPosition;
            var topoLargeValue = topoScan.ReconVolumeEndPosition < topoScan.ReconVolumeStartPosition ? 
                topoScan.ReconVolumeStartPosition : topoScan.ReconVolumeEndPosition;

            int tomoScanStart, tomoScanEnd;
            var tomoScanLength = ScanLengthHelper.GetCorrectedScanLength(tomoScan, (int)UnitConvert.Micron2Millimeter(tomoScan.ScanLength));
            if (tomoScan.PatientPosition is PatientPosition.HFS or PatientPosition.HFP 
                or PatientPosition.HFDL or PatientPosition.HFDR)    //头先进，进床为头->脚
            {
                if(tomoScan.TableDirection is TableDirection.In)    //头->脚
                {
                    tomoScanStart = topoLargeValue;
                    tomoScanEnd = tomoScanStart - tomoScanLength;
                }
                else
                {
                    tomoScanEnd = topoLargeValue;
                    tomoScanStart = tomoScanEnd - tomoScanLength;
                }
            }
            else             //脚先进，进床为脚->头
            {
                if (tomoScan.TableDirection is TableDirection.Out)    //头->脚
                {
                    tomoScanStart = topoSmallValue;
                    tomoScanEnd = tomoScanStart + tomoScanLength;
                }
                else
                {
                    tomoScanEnd = topoSmallValue;
                    tomoScanStart = tomoScanEnd + tomoScanLength;
                }

            }

            //设置的scan参数
            resultDic.Add(tomoScan, new List<ParameterModel>());
            if (tomoScan.ScanOption == ScanOption.NVTestBolusBase || tomoScan.ScanOption == ScanOption.NVTestBolus || tomoScan.ScanOption == ScanOption.TestBolus)
            {
                resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_LENGTH, Value = 0.ToString() });
                resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, Value = tomoScanStart.ToString() });
                resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = tomoScanStart.ToString() });                
            }
            else
            {
                resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_LENGTH, Value = tomoScanLength.ToString() });
                resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, Value = tomoScanStart.ToString() });
                resultDic[tomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = tomoScanEnd.ToString() });               
            }


            //重建参数单位mm
            var currentFor = topoScan.Parent.Parent;

            //遍历修改重建参数：
            foreach (var recon in tomoScan.Children)
            {
                resultDic.Add(recon, new List<ParameterModel>());

                var posResult = ScanReconCoordinateHelper.GetTomoDefaultFirstLastCenterByScanRange(currentFor.PatientPosition, recon.ImageOrder, tomoScanStart, tomoScanEnd);

                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_FIRST_X, Value = posResult[0].ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y, Value = posResult[1].ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z, Value = posResult[2].ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_LAST_X, Value = posResult[3].ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_LAST_Y, Value = posResult[4].ToString() });
                if (tomoScan.ScanOption == ScanOption.NVTestBolusBase || tomoScan.ScanOption == ScanOption.NVTestBolus || tomoScan.ScanOption == ScanOption.TestBolus)
                {
                    resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = posResult[2].ToString() });
                }
                else
                {
                    resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = posResult[5].ToString() });
                }

                var dirResult = ScanReconCoordinateHelper.GetDefaultTomoReconOrientation(currentFor.PatientPosition);
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X, Value = dirResult[0].ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y, Value = dirResult[1].ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z, Value = dirResult[2].ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X, Value = dirResult[3].ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y, Value = dirResult[4].ToString() });
                resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z, Value = dirResult[5].ToString() });
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