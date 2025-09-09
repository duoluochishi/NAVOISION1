using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums.ScanEnums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Common.Models.Generic;
using NV.CT.FacadeProxy.Models.DataAcquisition;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Enums;
using NV.CT.Service.Enums.Integrations;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic
{
    public partial class FreeScanViewModel : ObservableObject
    {
        private readonly string Service_Module_Name = "FreeScan";
        protected readonly ILogService logService;
        private readonly IMessagePrintService loggerUI;

        public static readonly uint CONST_GeneralExposureChildNodesConfig;
        public static readonly uint CONST_StaticExposureChildNodesConfig;
        public static readonly uint CONST_DarkExposureChildNodesConfig;

        static FreeScanViewModel()
        {
            CONST_GeneralExposureChildNodesConfig = GetExposureChildNodesConfig();
            CONST_StaticExposureChildNodesConfig = GetExposureChildNodesConfig(false);
            CONST_DarkExposureChildNodesConfig = GetExposureChildNodesConfig(false, false);
        }

        public FreeScanViewModel(ILogService logService, IMessagePrintService messagePrintService)
        {
            //Get from DI 
            this.logService = logService;
            this.loggerUI = messagePrintService;

            //Initialize 
            this.InitializeProxy();
            this.InitializeProperties();
        }

        public void SetExposureChildNodesConfig(uint exposureChildNodesConfig)
        {
            this.XDataAcquisitionParameters.ExposureParams.ExposureRelatedChildNodesConfig = exposureChildNodesConfig;

        }
        #region Initialize
        private void InitializeProperties()
        {
            this.XDataAcquisitionParameters = new() { };
            var exposureParams = this.XDataAcquisitionParameters.ExposureParams;
            
            SetExposureChildNodesConfig(CONST_GeneralExposureChildNodesConfig);

            exposureParams.ExposureTriggerMode = ExposureTriggerMode.TimeTrigger;
            exposureParams.GantryDirection = 0;
        }

        /// <summary>
        /// 子节点曝光判断位，对应嵌入式文档《软件2.0架构下ctBox交互协议》--8.3.1部件屏蔽功能
        /// Bit0: ifBox
        /// Bit1: pduCtrl
        /// Bit2: mtCtrl(Gantry机架)
        /// Bit3: table
        /// Bit4~9: tubeIntf1 ~6
        /// Bit10: auxboard
        /// Bit11: extboard
        /// Bit12: reserved
        /// Bit13：workstation
        /// (默认)扫描，床（不使能），机架（使能），球管（使能），  对应值9205（0010 0011 1111 0101）
        /// 静态扫描，床（不使能），机架（不使能），球管（使能），  对应值9201（0010 0011 1111 0001）
        /// 暗场扫描，床（不使能），机架（不使能），球管（不使能），对应值8193（0010 0000 0000 0101）
        /// 不使能：床 + 机架，使能： 球管，对应值9213（0010 0011 1111 1101）
        /// 不使能 机架 + 床 + 球管，对应值9213（0010 0011 1111 1101）
        /// </summary>
        internal static uint GetExposureChildNodesConfig(bool enableGantry = true, bool enableTube = true)
        {
            //[0]ifBox + [2]mtctrl(gantry) + [3]table  + [4~9]tubeIntf1-6 + [13]workstation
            BitArray bitArray = new(14);
            bitArray.Set(0, true);//Bit0: ifBox
            bitArray.Set(1, false);

            bitArray.Set(2, enableGantry);//Bit2: mtCtrl(Gantry机架)
            bitArray.Set(3, false);//Bit3: table

            // Bit4~9: tubeIntf1 ~6
            bitArray.Set(4, enableTube);
            bitArray.Set(5, enableTube);
            bitArray.Set(6, enableTube);
            bitArray.Set(7, enableTube);
            bitArray.Set(8, enableTube);
            bitArray.Set(9, enableTube);

            bitArray.Set(10, false);
            bitArray.Set(11, false);
            bitArray.Set(12, false);

            bitArray.Set(13, true);//Bit13：workstation

            int value = bitArray.ToInt();
            uint uintValue = (uint)(value);
            return uintValue;
        }

        private void InitializeProxy()
        {
            try
            {
                //记录 
                logService.Info(ServiceCategory.AutoCali,
                    $"[{Service_Module_Name}] DataAcquisitionProxy has been initialized.");

            }
            catch (Exception ex)
            {
                logService.Error(ServiceCategory.AutoCali,
                    $"[{Service_Module_Name}] Something wrong when initialize [InitializeDataAcquisitionProxy], [Stack]: {ex.ToString()}.");
            }
        }

        #endregion

        public void RegisterProxyEvents()
        {
            UnRegisterProxyEvents();

            //缓存连接状态 **/
            this.CurrentCTBoxConnectionStatus = DataAcquisitionProxy.Instance.CTBoxConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
            logService.Info(ServiceCategory.AutoCali, $"[{Service_Module_Name}] Save current CTBox connection status: {Enum.GetName(this.CurrentCTBoxConnectionStatus)}.");
            //Init 
            logService.Info(ServiceCategory.AutoCali, $"[{Service_Module_Name}] Register DataAcquisitionProxy events.");
            DataAcquisitionProxy.Instance.DeviceConnectionChanged += DataAcquisition_CTBoxConnectionChanged;
            DataAcquisitionProxy.Instance.CycleStatusChanged += DataAcquisition_CycleStatusChanged;
            DataAcquisitionProxy.Instance.RealTimeStatusChanged += DataAcquisition_RealTimeStatusChanged;
            DataAcquisitionProxy.Instance.SystemStatusChanged += DataAcquisition_SystemStatusChanged;
            //DataAcquisitionProxy.Instance.EventDataReceived += DataAcquisition_EventDataReceived;
            DataAcquisitionProxy.Instance.RawImageSaved += DataAcquisition_RawDataSaved;
        }

        public void UnRegisterProxyEvents()
        {
            logService.Info(ServiceCategory.AutoCali, $"[{Service_Module_Name}] Un-register DataAcquisitionProxy events.");
            DataAcquisitionProxy.Instance.DeviceConnectionChanged -= DataAcquisition_CTBoxConnectionChanged;
            DataAcquisitionProxy.Instance.CycleStatusChanged -= DataAcquisition_CycleStatusChanged;
            DataAcquisitionProxy.Instance.RealTimeStatusChanged -= DataAcquisition_RealTimeStatusChanged;
            DataAcquisitionProxy.Instance.SystemStatusChanged -= DataAcquisition_SystemStatusChanged;
            //DataAcquisitionProxy.Instance.EventDataReceived -= DataAcquisition_EventDataReceived;
            DataAcquisitionProxy.Instance.RawImageSaved -= DataAcquisition_RawDataSaved;
        }

        #region CTBox Connection

        private void DataAcquisition_CTBoxConnectionChanged(object sender, ConnectionStatusArgs args)
        {
            //更新 
            this.CurrentCTBoxConnectionStatus = args.Connected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
            //信息 
            string message = $"[{Service_Module_Name}] Current CTBox connection status: [{Enum.GetName(CurrentCTBoxConnectionStatus)}].";
            //显示记录 
            loggerUI.PrintLoggerInfo(message);
        }

        #endregion

        private void DataAcquisition_CycleStatusChanged(object sender, CycleStatusArgs args)
        {
            // 获取DeviceSystem
            var deviceSystem = args.Device;
            //更新门状态 
            this.CurrentDoorStatus = args.Device.DoorClosed ? DoorStatus.Closed : DoorStatus.Open;
            ////更新射线源状态 
            //ComponentStatusHelper.UpdateXRaySourceStatusByCycle(this.XRaySources, deviceSystem);
            ////更新扫描床状态 
            //ComponentStatusHelper.UpdateTableStatusByCycle(this.XTableSource, deviceSystem);
            ////更新扫描架状态 
            //ComponentStatusHelper.UpdateGantryStatusByCycle(this.XGantrySource, deviceSystem);
            ////更新采集卡状态 
            //ComponentStatusHelper.UpdateAcquisitionCardStatusByCycle(this.AcquisitionCardSources, deviceSystem);
            ////更新探测器状态 
            //ComponentStatusHelper.UpdateDetectorStatusByCycle(this.DetectorSources, deviceSystem);
            ////存储温度数据 
            //this.SaveTemperatureDataByCycle(deviceSystem);
        }


        #region Realtime Status

        private void DataAcquisition_RealTimeStatusChanged(object sender, RealtimeEventArgs args)
        {
            loggerUI.PrintLoggerInfo($"[{Service_Module_Name}] Realtime status: [{Enum.GetName(args.Status)}].");
        }

        #endregion

        #region System Status

        private void DataAcquisition_SystemStatusChanged(object sender, SystemStatusArgs args)
        {
            loggerUI.PrintLoggerInfo($"[{Service_Module_Name}] System status: [{Enum.GetName(args.Status)}].");
        }

        #endregion

        #region Acquisition Parameters

        //采集参数集合 
        public DataAcquisitionParams XDataAcquisitionParameters { get; set; } = null!;
        ////运动控制参数集合
        //public MotionControlParameters XMotionControlParameters { get; set; } = null!;
        //采集状态 
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanStartDataAcquisition))]
        private DataAcquisitionStatus currentDataAcquisitionStatus = DataAcquisitionStatus.NormalStop;
        //可采集标志 
        public bool CanStartDataAcquisition => CurrentDataAcquisitionStatus == DataAcquisitionStatus.NormalStop;
        //当前CTBox连接状态 
        public ConnectionStatus CurrentCTBoxConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
        //当前门状态 
        public DoorStatus CurrentDoorStatus { get; set; } = DoorStatus.Open;
        ////当前探测器状态 
        //public DetectorStatus CurrentDetectorStatus { get; set; } = DetectorStatus.NotReady;

        //partial void OnCurrentDataAcquisitionStatusChanged(DataAcquisitionStatus oldValue, DataAcquisitionStatus newValue)
        //{
        //    DispatcherWrapper.Invoke(() =>
        //    {
        //        StartDataAcquisitionCommand.NotifyCanExecuteChanged();
        //    });
        //}

        #endregion


        #region RawData Saved

        /// <summary>
        /// 生数据保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void DataAcquisition_RawDataSaved(object sender, RawImageSavedEventArgs args)
        {
            await HandleRealtimeRawDataAcquisitionAsync(args);
        }

        ////数据存取通道 
        //private Channel<List<AbstractRawDataInfo>> AcquisitionRawDataChannel { get; set; } = null!;

        ///// <summary>
        ///// 初始化通道
        ///// </summary>
        //private void InitializeChannel()
        //{
        //    this.AcquisitionRawDataChannel = Channel.CreateUnbounded<List<AbstractRawDataInfo>>();
        //}

        ///// <summary>
        ///// 监听通道数据
        ///// </summary>
        //private void StartListenChannel()
        //{
        //    var reader = this.AcquisitionRawDataChannel.Reader;
        //    //读取 
        //    Task.Run(async () =>
        //    {
        //        //读取 
        //        //while (await reader.WaitToReadAsync())
        //        //{
        //        //    if (reader.TryRead(out var dataSet))
        //        //    {
        //        //        this.RawDataInfoSet.AddRange(dataSet);
        //        //        //刷新显示 
        //        //        DispatcherWrapper.Invoke(() =>
        //        //        {
        //        //            this.ImageViewer.ClearViewData();

        //        //            if (this.RawDataInfoSet.Count > 0)
        //        //            {
        //        //                this.CurrentShowIndex = (uint)this.RawDataInfoSet.Count - 1;
        //        //                this.DisplayRawDataInfo(this.RawDataInfoSet[(int)CurrentShowIndex]);
        //        //            }
        //        //        });
        //        //    }
        //        //}
        //    });
        //}

        /// <summary>
        /// 生数据的实时提取
        /// </summary>
        private async Task HandleRealtimeRawDataAcquisitionAsync(RawImageSavedEventArgs args)
        //private void HandleRealtimeRawDataAcquisitionAsync(RawImageSavedEventArgs args)
        {
            string message = string.Empty;

            //第一帧打印文件夹路径 
            if (args.FinishCount == 1)
            {
                //更新信息 
                message = $"[{Service_Module_Name}] Raw data folder: {args.Directory}";
                //显示记录 
                loggerUI.PrintLoggerInfo(message);
                //清空图像列表 
                //[ToDo]DispatcherWrapper.Invoke(() => { this.RawDataInfoSet.Clear(); });

                ////开启监听 
                //this.StartListenChannel();
            }
            //进度信息 
            message = $"[{Service_Module_Name}] " +
            //$"Data acquisition progress: {args.FinishCount} / {XDataAcquisitionParameters.ExposureParameters.SeriesLength}";
            $"Data acquisition progress: {args.FinishCount} / {args.TotalCount}";
            //显示记录 
            //messagePrintService.PrintLoggerInfo(message);

            //采集生数据 
            try
            {
                //若已完成 
                if (args.IsFinished)
                {
                    ////若完成时还有部分不足interval的张数，存储 
                    //if (args.FinishCount % dataAcquisitionConfigService.RawDataReadInterval != 0)
                    //{
                    //    //剩余张数 
                    //    int leftCount = args.FinishCount % dataAcquisitionConfigService.RawDataReadInterval;
                    //    //进度信息 
                    //    message = $"[{Service_Module_Name}] Hit the point to save the left {leftCount} raw data.";
                    //    //显示记录 
                    //    messagePrintService.PrintLoggerInfo(message);
                    //    //获取生数据列表 
                    //    List<AbstractRawDataInfo>? tempRawDataSet = await GetAcquisitionDataAsync(
                    //        args.Directory, leftCount, args.FinishCount, (int)this.XDataAcquisitionParameters.ExposureParameters.FramesPerCycle);
                    //    //数据判定 
                    //    if (tempRawDataSet is not null && tempRawDataSet.Count > 0)
                    //    {
                    //        //添加 
                    //        await AcquisitionRawDataChannel.Writer.WriteAsync(tempRawDataSet);
                    //    }
                    //}
                    //更新信息 
                    message = $"[{Service_Module_Name}] Data Acquisition has been finished. ";
                    //显示记录 
                    loggerUI.PrintLoggerInfo(message);
                    //更新状态 
                    this.CurrentDataAcquisitionStatus = DataAcquisitionStatus.NormalStop;

                    return;
                    //return new Task();
                }

                ////每隔interval个获取采集数据 
                //if (args.FinishCount % dataAcquisitionConfigService.RawDataReadInterval == 0)
                //{
                //    //进度信息 
                //    message = $"[{Service_Module_Name}] Hit the point to get data, finish count: {args.FinishCount}.";
                //    //显示记录 
                //    messagePrintService.PrintLoggerInfo(message);
                //    //获取生数据列表 
                //    List<AbstractRawDataInfo>? tempRawDataSet = await GetAcquisitionDataAsync(
                //        args.Directory, dataAcquisitionConfigService.RawDataReadInterval,
                //        args.FinishCount, (int)this.XDataAcquisitionParameters.ExposureParameters.FramesPerCycle);
                //    //数据判定 
                //    if (tempRawDataSet == null || tempRawDataSet.Count == 0)
                //    {
                //        //进度信息 
                //        message = $"[{Service_Module_Name}] Unable to get temp rawdata, finishCount:{args.FinishCount}."; return;
                //    }
                //    //添加 
                //    await AcquisitionRawDataChannel.Writer.WriteAsync(tempRawDataSet);
                //}
                await Task.Run(() => { });
            }
            catch (Exception ex)
            {
                //更新信息 
                message = $"[{Service_Module_Name}] " +
                    $"Something wrong when acquiring data, [Stack]: {ex.ToString()}.";
                //显示记录 
                loggerUI.PrintLoggerError(message);
            }
        }

        /////// <summary>
        /////// 获取采集数据（.head & .raw）
        /////// </summary>
        /////// <param name="folderPath"></param>
        /////// <param name="interval"></param>
        /////// <param name="finishCount"></param>
        /////// <param name="framesPerCycle"></param>
        /////// <returns></returns>
        ////private async Task<List<AbstractRawDataInfo>?> GetAcquisitionDataAsync(string folderPath, int interval, int finishCount, int framesPerCycle = 1080)
        ////{
        ////    string message = string.Empty;
        ////    //前置信息 
        ////    const int imageWidth = 10240;
        ////    const int imageHeight = 288;
        ////    int imageSize = imageWidth * imageHeight * sizeof(ushort);
        ////    int imageHeadSize = Marshal.SizeOf(typeof(RawDataHead));
        ////    //路径校验 
        ////    if (!Directory.Exists(folderPath))
        ////    {
        ////        //更新信息 
        ////        message = $"[{Service_Module_Name}] The acquistion data folder path does not exist, path: [{folderPath}].";
        ////        //显示记录 
        ////        messagePrintService.PrintLoggerInfo(message);

        ////        return null;
        ////    }
        ////    //获取路径下的RawData文件(.raw和.head) 
        ////    string[] acquiringDataFileNames = Directory.GetFiles(folderPath, "*.raw");
        ////    string[] acquiringDataHeadFileNames = Directory.GetFiles(folderPath, "*.head");
        ////    //根据帧计算对应的数据offset和文件Index 
        ////    int fileIndex = (finishCount - 1) / framesPerCycle;
        ////    long dataOffset = (finishCount - interval) - (fileIndex * framesPerCycle);
        ////    //更新信息 
        ////    message = $"[{Service_Module_Name}] Acquiring head & data, file index: {fileIndex}, data offset: {dataOffset}.";
        ////    //显示记录 
        ////    messagePrintService.PrintLoggerInfo(message);
        ////    //有效数据文件校验 
        ////    if (acquiringDataFileNames.Length == 0 || acquiringDataFileNames.Length <= fileIndex)
        ////    {
        ////        //更新信息 
        ////        message = $"[{Service_Module_Name}] The acquistion data folder path does not have valid raw data, " +
        ////            $"current file index: {fileIndex}, current data offset: {dataOffset}.";
        ////        //显示记录 
        ////        messagePrintService.PrintLoggerInfo(message);

        ////        return null;
        ////    }
        ////    //有效数据文件头校验 
        ////    if (acquiringDataHeadFileNames.Length == 0 || acquiringDataHeadFileNames.Length <= fileIndex)
        ////    {
        ////        //更新信息 
        ////        message = $"[{Service_Module_Name}] The acquistion data folder path does not have valid head file, " +
        ////            $"current file index: {fileIndex}, current data offset: {dataOffset}.";
        ////        //显示记录 
        ////        messagePrintService.PrintLoggerInfo(message);

        ////        return null;
        ////    }
        ////    //获取对应的文件路径 
        ////    string dataFilePath = Path.Combine(folderPath, acquiringDataFileNames[fileIndex]);
        ////    string dataHeadFilePath = Path.Combine(folderPath, acquiringDataHeadFileNames[fileIndex]);
        ////    //初始化多帧数据buffer 
        ////    byte[] bufferForDataFrames = new byte[imageSize * interval];
        ////    byte[] bufferForHeadFrames = new byte[imageHeadSize * interval];
        ////    //初始化RawDataInfo集合 
        ////    List<AbstractRawDataInfo> rawDataSet = new List<AbstractRawDataInfo>();
        ////    //初始化单帧数据tempData和单帧数据头tempHead * */
        ////    byte[] tempHead = new byte[imageHeadSize];
        ////    //读取 
        ////    try
        ////    {
        ////        //FileStream 开启FileShare 
        ////        using FileStream dataFileStream = new FileStream(dataFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        ////        using FileStream headFileStream = new FileStream(dataHeadFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        ////        //移动读位置 
        ////        dataFileStream.Seek(dataOffset * imageSize, SeekOrigin.Begin);
        ////        headFileStream.Seek(dataOffset * imageHeadSize, SeekOrigin.Begin);
        ////        //更新信息 
        ////        message = $"[{Service_Module_Name}] " +
        ////            $"Acquring data offset: {dataOffset * imageSize}, fileStreamLength: {dataFileStream.Length}," +
        ////            $"Acquring head offset: {dataOffset * imageHeadSize}, headStreamLength: {headFileStream.Length}";
        ////        //显示记录 
        ////        messagePrintService.PrintLoggerInfo(message);
        ////        //读取imageSize * interval长度的数据 
        ////        int readDataLength = await dataFileStream.ReadAsync(bufferForDataFrames, 0, bufferForDataFrames.Length);
        ////        int readHeadLength = await headFileStream.ReadAsync(bufferForHeadFrames, 0, bufferForHeadFrames.Length);
        ////        //遍历写入 
        ////        for (int i = 0; i < interval; i++)
        ////        {
        ////            //生成 UshortRawDataInfo
        ////            UshortRawDataInfo ushortRawDataInfo = new UshortRawDataInfo(imageWidth, imageHeight);
        ////            //获取单帧头数据 
        ////            Buffer.BlockCopy(bufferForHeadFrames, i * imageHeadSize, tempHead, 0, imageHeadSize);
        ////            RawDataHead imageHead = ByteUtils.BytesToStruct<RawDataHead>(tempHead)!.Value;
        ////            //获取单帧数据 
        ////            Buffer.BlockCopy(bufferForDataFrames, i * imageSize, ushortRawDataInfo.Data, 0, imageSize);
        ////            //更新 UshortRawDataInfo
        ////            ushortRawDataInfo.Width = imageHead.ImageSizeX;
        ////            ushortRawDataInfo.Height = imageHead.ImageSizeY;
        ////            ushortRawDataInfo.SupportInfo.SourceID = imageHead.SourceId;
        ////            ushortRawDataInfo.SupportInfo.FrameSeriesNumber = imageHead.FrameSeriesNumber;
        ////            ushortRawDataInfo.SupportInfo.GantryRotateAngle = imageHead.GantryRotateAngle;
        ////            ushortRawDataInfo.SupportInfo.TablePosition = imageHead.TablePosition;
        ////            //添加 
        ////            rawDataSet.Add(ushortRawDataInfo);
        ////        }
        ////        //更新信息 
        ////        message = $"[{Service_Module_Name}] From No.{finishCount - interval + 1} to No.{finishCount} raw data with head has been loaded.";
        ////        //显示记录 
        ////        messagePrintService.PrintLoggerInfo(message);
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        //更新信息 
        ////        message = $"[{Service_Module_Name}] Something wrong when extracting raw data with head , [Stack]: {ex.ToString()}";
        ////        //显示记录 
        ////        messagePrintService.PrintLoggerError(message);
        ////    }

        ////    return rawDataSet;
        ////}

        #endregion

        #region Data Acquisition Control

        private static string ErrorCode_DoorNotClosed = "MCS007000004";
        /// <summary>
        /// CTBox的连接状态为未连接
        /// </summary>
        private static string ErrorCode_Device_Disconnected = "MRS005001002";

        /// <summary>
        /// 开始数据采集
        /// </summary>
        public bool StartDataAcquisition(object commandParam)
        {
            bool result = false;
            //CTBox连接状态校验
            if (CurrentCTBoxConnectionStatus != ConnectionStatus.Connected)
            {
                loggerUI.PrintLoggerError($"CTBox is now disconnected, please check CTBox status!");
                DialogService.Instance.ShowErrorCode(ErrorCode_Device_Disconnected);

                return result;
            }

            //门状态校验
            if (!DataAcquisitionProxy.Instance.DeviceSystem.DoorClosed)
            {
                loggerUI.PrintLoggerError($"Door is not closed, please close the door!");
                DialogService.Instance.ShowErrorCode(ErrorCode_DoorNotClosed);

                return result;
            }

            //由外界传入 ScanReconParam 对象，单纯由 DataAcquisitionParameters.ToScanReconParam 不够满足需求，比如校准，需要特定ReconParam
            if (commandParam is ScanReconParam)
            {
                var scanReconParam = commandParam as ScanReconParam;
                //使用自由协议，如果没有调用UpdateScanReconParam替换DataAcquisitionProxy的ScanReconParam，Proxy内部已自动设置为FreeProtocol
                scanReconParam.ScanParameter.ProtocolType = FacadeProxy.Common.Enums.ScanEnums.ProtocolType.FreeProtocol;
                DataAcquisitionProxy.Instance.UpdateScanReconParam(scanReconParam);
            }

            ////初始化Channel
            //this.InitializeChannel();
            //启动数据采集
            result = this.StartDataAcquisitionCommon();
            //切换状态
            if (result)
            {
                this.CurrentDataAcquisitionStatus = DataAcquisitionStatus.AcquiringData;
            }
            return result;
        }

        /// <summary>
        /// 停止数据采集
        /// </summary>
        [RelayCommand]
        private void StopDataAcquisition()
        {
            //更新状态
            this.CurrentDataAcquisitionStatus = DataAcquisitionStatus.NormalStop;
            //停止
            this.StopDataAcquisitionCommon();
        }

        /// <summary>
        /// 开始采集Common
        /// </summary>
        /// <returns></returns>
        private bool StartDataAcquisitionCommon()
        {
            //开始配置
            loggerUI.PrintLoggerInfo($"[{Service_Module_Name}] Prepare to configure data acquisition.");
            //参数转换 
            //var proxyParameters = this.XDataAcquisitionParameters.ToProxyParam();
            //var proxyParameters = new DataAcquisitionParams();
            //配置参数 
            var configureResponse = DataAcquisitionProxy.Instance.ConfigureDataAcquisition(XDataAcquisitionParameters);
            //校验 
            if (!configureResponse.Status)
            {
                string errorInfo = HandleScanError(configureResponse);
                loggerUI.PrintLoggerError($"[{Service_Module_Name}] Failed to configure data acquisition.{errorInfo}.");

                return false;
            }
            //配置结束
            loggerUI.PrintLoggerInfo($"[{Service_Module_Name}] Data acquisition has been configured.");
            //开始采集
            loggerUI.PrintLoggerInfo($"[{Service_Module_Name}] Prepare to start data acquisition.");
            //启动采集
            var startResponse = DataAcquisitionProxy.Instance.StartDataAcquisition(XDataAcquisitionParameters);
            //校验 
            if (!startResponse.Status)
            {
                string errorInfo = HandleScanError(startResponse);
                loggerUI.PrintLoggerError($"[{Service_Module_Name}] Failed to start data acquisition.{errorInfo}.");

                return false;
            }
            //采集结束
            loggerUI.PrintLoggerInfo($"[{Service_Module_Name}] Data acquisition has been started.");

            return true;
        }

        private string HandleScanError(GenericResponse<bool> response)
        {
            var errorCode = response.ErrorCode;
            string errorInfo = $"[ErrorCode] {errorCode}.";
            if (!string.IsNullOrEmpty(errorCode))
            {
                DialogService.Instance.ShowErrorCode(errorCode);//[ToDo]优化成发生消息，解耦消息与响应
            }
            else
            {
                string errorMsg = response.Message;
                errorInfo = $"[ErrorMessage] {errorMsg}.";
                DialogService.Instance.ShowError(errorMsg);
            }
            loggerUI.PrintLoggerError($"[{Service_Module_Name}] Failed to configure data acquisition with {errorInfo}.");

            return errorInfo;
        }

        /// <summary>
        /// 停止采集Common
        /// </summary>
        private bool StopDataAcquisitionCommon()
        {
            //停止采集
            loggerUI.PrintLoggerInfo($"[{Service_Module_Name}] Prepare to stop data acquisition.");
            //停止开始
            var stopResponse = DataAcquisitionProxy.Instance.StopDataAcquisition();
            //校验 
            if (!stopResponse.Status)
            {
                loggerUI.PrintLoggerError($"[{Service_Module_Name}] Failed to stop data acquisition.");

                return false;
            }
            //停止结束
            loggerUI.PrintLoggerInfo($"[{Service_Module_Name}] Data acquisition has been stopped.");

            return true;
        }

        #endregion
    }
    public static class BitArrayExtensions
    {
        public static int ToInt(this BitArray bitArray)
        {
            int number = 0;
            for (int i = 0; i < bitArray.Count; i++)
            {
                if (bitArray.Get(i))
                {
                    number |= 1 << i;
                }
            }
            return number;
        }
    }
}


