using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Extensions;
using NV.CT.Service.AutoCali.Logic.Handlers.Scans;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic.Handlers
{
    public class AixalScanHandler : AbstractScanHandler
    {
        public AixalScanHandler(ILogService logService,
            IMessagePrintService messagePrintService,
            CancellationTokenSource cancellationTokenSource)
            : base(logService, messagePrintService, cancellationTokenSource)
        {
        }

        /// <summary>
        /// 曝光将要用到的射线源，All：代表全部
        /// </summary>
        public XRaySourceIndex XRaySourceIndex { get; set; } = XRaySourceIndex.All;

        protected override async Task<CommandResult> Scan()
        {
            var scanOption = this._ScanReconParam.ScanParameter.ScanOption;
            if (ScanOption.Axial != scanOption)
            {
                throw new ArgumentException($"The ScanOption is {scanOption}, Not expected ScanOption.Axial.");
            }

            uint totalFrames = 0;// ((GeneralArgProtocolViewModel)protocolVM).TotalFrames;
            bool collimatorSwitch = true;// (1 == ((GeneralArgProtocolViewModel)protocolVM).CollimatorSwitch);
            return await StartFreeScanAsync(totalFrames, collimatorSwitch);
        }

        /// <summary>
        /// 发起请求：开始扫描，然后（异步）等待状态
        /// </summary>
        /// <param name="totalFrames"></param>
        /// <param name="collimatorSwitch"></param>
        /// <returns></returns>
        private async Task<CommandResult> StartFreeScanAsync(uint totalFrames, bool collimatorSwitch = true)
        {
            this._logger.Debug(ServiceCategory.AutoCali, $"Beginning RequestStartScanAsync");

            //CleanBeforeRequestStartScan(taskViewModel);
            CommandResult commandResult = new () { Sender = nameof(StartFreeScanAsync) };

            string commandName = "StartScan";
            var scanReconParam = this._ScanReconParam;
            bool isDark = scanReconParam.ScanParameter.kV[0] == 0;
            ////3.发起请求：开始扫描
            //var freeScan = (isDark) ? (new FreeScanViewModel_Dark(this._logger, this._loggerUI))
            //    : (new FreeScanViewModel(this._logger, this._loggerUI));
            ////freeScan.RegisterProxyEvents();
            //#region make freeScan.XDataAcquisitionParameters

            //freeScan.XDataAcquisitionParameters.FuncMode = scanReconParam.ScanParameter.FunctionMode;//
            //freeScan.XDataAcquisitionParameters.XRawDataType = scanReconParam.ScanParameter.RawDataType;

            //freeScan.XDataAcquisitionParameters.StudyUID = scanReconParam.Study.StudyInstanceUID;
            //freeScan.XDataAcquisitionParameters.ScanUID = scanReconParam.ScanParameter.ScanUID;

            //freeScan.XDataAcquisitionParameters.ExposureParams.ScanMode = scanReconParam.ScanParameter.ScanMode;
            //freeScan.XDataAcquisitionParameters.ExposureParams.Voltage = scanReconParam.ScanParameter.kV[0];
            //freeScan.XDataAcquisitionParameters.ExposureParams.Current = (uint)(scanReconParam.ScanParameter.mA[0] / 1000.0f);

            //freeScan.XDataAcquisitionParameters.ExposureParams.TotalFrames = totalFrames;
            //freeScan.XDataAcquisitionParameters.ExposureParams.FramesPerCycle = scanReconParam.ScanParameter.FramesPerCycle;
            //freeScan.XDataAcquisitionParameters.ExposureParams.PostOffsetFrames = scanReconParam.ScanParameter.PostOffsetFrames;

            ////[]不再提供是否关心限束器
            ////freeScan.XDataAcquisitionParameters.ExposureParams.BowtieSwitch = bowtieSwitch;
            //freeScan.XDataAcquisitionParameters.ExposureParams.Bowtie = scanReconParam.ScanParameter.BowtieEnable;
            //freeScan.XDataAcquisitionParameters.ExposureParams.ExposureMode = scanReconParam.ScanParameter.ExposureMode;

            //freeScan.XDataAcquisitionParameters.ExposureParams.FrameTime = scanReconParam.ScanParameter.FrameTime / 1000.0f;
            //freeScan.XDataAcquisitionParameters.ExposureParams.ExposureTime = scanReconParam.ScanParameter.ExposureTime / 1000.0f;

            ////nvSync Delay control
            //freeScan.XDataAcquisitionParameters.DetectorParams.RDelay = scanReconParam.ScanParameter.RDelay / 1000.0f;
            //freeScan.XDataAcquisitionParameters.DetectorParams.TDelay = scanReconParam.ScanParameter.TDelay / 1000.0f;
            //freeScan.XDataAcquisitionParameters.DetectorParams.SpotDelay = scanReconParam.ScanParameter.SpotDelay / 1000.0f;
            //freeScan.XDataAcquisitionParameters.DetectorParams.CollimatorSpotDelay = scanReconParam.ScanParameter.CollimatorSpotDelay / 1000.0f;

            //freeScan.XDataAcquisitionParameters.DetectorParams.FrameTime = scanReconParam.ScanParameter.FrameTime / 1000.0f;
            //freeScan.XDataAcquisitionParameters.DetectorParams.ExposureTime = scanReconParam.ScanParameter.ExposureTime / 1000.0f;

            //var CollimitorZ = scanReconParam.ScanParameter.CollimatorZ;
            //freeScan.XDataAcquisitionParameters.ExposureParams.CollimatorOpenWidth = CollimitorZ;
            ////[]不再提供是否关心限束器
            ////freeScan.XDataAcquisitionParameters.ExposureParams.CollimatorSwitch = collimatorSwitch;//是否下发CollimatroZ参数
            ////开口模式，1:基于小锥角模式（256,242,128,64）；2：基于中心位置模式（288，128,64）
            //freeScan.XDataAcquisitionParameters.ExposureParams.CollimatorOpenMode = (288 == CollimitorZ) ? 2u : 1;

            //freeScan.XDataAcquisitionParameters.DetectorParams.CurrentGain = scanReconParam.ScanParameter.Gain;
            //freeScan.XDataAcquisitionParameters.DetectorParams.CurrentScatteringGain = FacadeProxy.Models.DataAcquisition.ScatteringDetectorGain.Fix16Pc;

            //var preOffsetFrames = scanReconParam.ScanParameter.PreOffsetFrames;
            //if (preOffsetFrames > 1)
            //{
            //    freeScan.XDataAcquisitionParameters.DetectorParams.PreOffsetEnable = 1;
            //    freeScan.XDataAcquisitionParameters.DetectorParams.PreOffsetAcqTotalFrame = (int)preOffsetFrames;
            //    freeScan.XDataAcquisitionParameters.DetectorParams.PreOffsetAcqStartVaildFrame = 3;//default
            //    this._logger.Debug(ServiceCategory.AutoCali, $"Debug set PreOffsetEnable:{freeScan.XDataAcquisitionParameters.DetectorParams.PreOffsetEnable}，" +
            //        $"PreOffsetAcqTotalFrame:{freeScan.XDataAcquisitionParameters.DetectorParams.PreOffsetAcqTotalFrame}");
            //}

            ////机架的起始位置
            //freeScan.XDataAcquisitionParameters.ExposureParams.GantryStartPosition = scanReconParam.ScanParameter.GantryStartPosition;
            //freeScan.XDataAcquisitionParameters.ExposureParams.GantrySpeed = (uint)scanReconParam.ScanParameter.GantrySpeed;
            //freeScan.XDataAcquisitionParameters.ExposureParams.GantryAccTime = scanReconParam.ScanParameter.GantryAccelerationTime;

            //#endregion

            var handler = (XRaySourceIndex.All == XRaySourceIndex)
               ? (new FreeScanHandler(this._logger, this._loggerUI))
               : (new SingleExposureFreeScanHandler(XRaySourceIndex, this._logger, this._loggerUI));
            var freeScan = handler.GetFreeScan(scanReconParam);

            //UpdateProgressInfo("Run [Simulator]");
            //Task.Run(freeScan.Simulator);
            //UpdateProgressInfo("await Delay(3000) for [StartDataAcquisition]");
            //await Task.Delay(3000);
            //UpdateTotalScanCount(scanReconParam);
            bool success = freeScan.StartDataAcquisition(scanReconParam);
            if (!success)
            {
                //taskViewModel.CaliTaskState = CaliTaskState.Error;
                commandResult.ErrorCodes = new();//todo
                return commandResult;
            }

            //服务端成功收到命令，并开始扫描，异步持续返回进度
            this._loggerUI.PrintLoggerInfo($"Sent the command \"{commandName}\" Successfully, and Async receive the service state...");
            //int waittingTimes = 0;
            //string taskName = taskViewModel.Name;

            //2.倒计时提示曝光延迟
            var exposureDelaySeconds = ConvertMicroToSecond(scanReconParam.ScanParameter.ExposureDelayTime);
            commandResult = await CountDownToRequestServiceAsync("Exposure", null, (int)exposureDelaySeconds);

            if (commandResult?.Success() != true)
            {
                while (true)
                {
                    if (this._cancellationTokenSource.IsCancellationRequested)
                    {
                        this._loggerUI.PrintLoggerInfo($"User cancelled the task.");
                        //taskViewModel.CaliTaskState = CaliTaskState.Canceled;

                        //用户主动取消了扫描，系统需要通知服务代理的终止扫描
                        //RequestAbortScan(taskViewModel);
                        freeScan.StopDataAcquisitionCommand.Execute(scanReconParam);
                        break;
                    }
                    await Task.Delay(1000);

                    //if (IsScanRecon_Failed())
                    //{
                    //    taskViewModel.CaliTaskState = CaliTaskState.Error;
                    //    break;
                    //}
                    ////扫描成功了
                    //else if (IsScanRecon_CompletedSuccessfully())
                    //{
                    //    taskViewModel.CaliTaskState = CaliTaskState.Success;
                    //    break;
                    //}

                    //LogServiceStateHeartBeat(commandName, ++waittingTimes);
                }

            }
            //freeScan.UnRegisterProxyEvents();

            this._logger.Debug(ServiceCategory.AutoCali, $"Ended RequestStartScanAsync");

            return commandResult;
        }
    }
}
