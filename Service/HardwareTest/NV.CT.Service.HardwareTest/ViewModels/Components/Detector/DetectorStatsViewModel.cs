using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using MiniExcelLibs;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Configurations;
using NV.CT.Service.HardwareTest.Attachments.Extensions;
using NV.CT.Service.HardwareTest.Attachments.Helpers;
using NV.CT.Service.HardwareTest.Attachments.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Messages;
using NV.CT.Service.HardwareTest.Models.Components.Detector;
using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.Services.Components.Detector;
using NV.CT.Service.HardwareTest.Services.Universal.PrintMessage.Abstractions;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.Share.Enums;
using NV.CT.Service.HardwareTest.Share.Utils;
using NV.CT.Service.HardwareTest.UserControls.Components.Detector;
using NV.CT.Service.HardwareTest.ViewModels.Foundations;
using NV.MPS.Configuration;
using RawDataHelperWrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NV.CT.Service.HardwareTest.ViewModels.Components.Detector
{
    public partial class DetectorStatsViewModel : NavigationViewModelBase, IModuleDirectory
    {
        private readonly ILogService logService;
        private readonly IMessagePrintService messagePrintService;       
        private readonly HardwareTestConfigOptions hardwareTestConfigService;

        public DetectorStatsViewModel(
            ILogService logService, 
            IMessagePrintService messagePrintService, 
            IOptions<HardwareTestConfigOptions> hardwareTestOptions)
        { 
            this.logService = logService;
            this.messagePrintService = messagePrintService;
            this.hardwareTestConfigService = hardwareTestOptions.Value;
            //Initialize
            InitializeProperties();
            InitializeModuleDirectory();
            InitializeDetectBoardSlots();
            InitializeDetectBoardSources();
            InitializeQueuedDataAcquisitionService();
            //Load XML Config
            LoadXMLConfig();
            //Message
            WeakReferenceMessenger.Default.Register<UpdateDetectorBoardSlotSourceMessage>(this, UpdateDetectBoardSlotSourceCallBack);
        }

        #region Initialize

        private void InitializeProperties() 
        {
            //暗场采集参数
            DarkFieldParameters = new();
            DarkFieldParameters.ExposureParameters.MAs = [0, 0, 0, 0, 0, 0, 0, 0];
            DarkFieldParameters.ExposureParameters.KVs = [0, 0, 0, 0, 0, 0, 0, 0];
            DarkFieldParameters.DetectorParameters.CurrentGain = Gain.Fix3pC;
            DarkFieldParameters.ExposureParameters.AutoDeleteNumber = 48;
            DarkFieldParameters.ExposureParameters.TotalFrames = 100;
            //亮场采集参数
            BrightFieldParameters = new();
            BrightFieldParameters.ExposureParameters.MAs = [90, 0, 0, 0, 0, 0, 0, 0];
            BrightFieldParameters.ExposureParameters.KVs = [120, 0, 0, 0, 0, 0, 0, 0];
            BrightFieldParameters.ExposureParameters.FrameTime = 10;
            BrightFieldParameters.ExposureParameters.ExposureTime = 5;
            BrightFieldParameters.DetectorParameters.CurrentGain = Gain.Fix24pC;
            BrightFieldParameters.ExposureParameters.AutoDeleteNumber = 48;
            BrightFieldParameters.ExposureParameters.TotalFrames = 100;
        }

        private void InitializeDetectBoardSlots()
        {
            DetectBoardSlots = new();
            DetectBoardSlots.AddRange(Enumerable.Range(1, 64).Select(i => new DetectBoardSlot()
            {
                DetectorModuleID = (uint)i
            })) ;
        }

        private void InitializeDetectBoardSources()
        {
            DetectBoardDtos = new();
        }

        private void InitializeQueuedDataAcquisitionService()
        {
            //采集任务列表
            var tasks = new DataAcquisitionTask[]
            {
                new("Dark Field Data Acquisition", DarkFieldParameters),
                new("Bright Field Data Acquisition", BrightFieldParameters)
            };
            //顺序执行队列
            QueuedDataAcquisitionService = new QueuedDataAcquisitionService(tasks);
            //注册过程信息事件
            QueuedDataAcquisitionService.OnTaskExecutionProcessMessageUpdated
                += QueuedDataAcquisitionService_OnTaskExecutionProcessMessageUpdated;
        }

        public void InitializeModuleDirectory()
        {
            FileUtils.EnsureDirectoryPath(ModuleDataDirectoryPath);
        }

        #endregion

        #region Properties

        /// <summary>
        /// 图像宽度
        /// </summary>
        public uint ImageWidth { get; init; } = (uint)SystemConfig.DetectorConfig.Detector.XChannelCount.Value;
        /// <summary>
        /// 图像高度
        /// </summary>
        public uint ImageHeight { get; init; } = (uint)SystemConfig.DetectorConfig.Detector.ZChannelCount.Value;
        /// <summary>
        /// 检出板数量
        /// </summary>
        public int DetectBoardTotalCount { get; init; } =
            SystemConfig.DetectorConfig.Detector.XModuleCount.Value * 
            SystemConfig.DetectorConfig.Detector.XSingleModuleChipCount.Value;

        /***  亮场&暗场数据采集参数  ***/

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanStartDataAcquisition))]
        [NotifyCanExecuteChangedFor(nameof(StartDataAcquisitionCommand))]
        private bool isDataAcquisitionStarted = false;
        public bool CanStartDataAcquisition => !IsDataAcquisitionStarted;
        public DataAcquisitionParameters DarkFieldParameters { get; set; } = null!;
        public DataAcquisitionParameters BrightFieldParameters { get; set; } = null!;
        public ConnectionStatus CurrentCTBoxConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
        public QueuedDataAcquisitionService QueuedDataAcquisitionService { get; set; } = null!;
        public bool IsNormalScanStopped { get; set; } = false;

        /***  探测器检出板更新记录参数  ***/
        public ObservableCollection<DetectBoardSlot> DetectBoardSlots { get; set; } = null!;
        public List<DetectBoardDto> DetectBoardDtos { get; set; } = null!;
        public string ModuleDataDirectoryPath => Path.Combine(hardwareTestConfigService.DataDirectoryPath, ComponentDefaults.DetectorComponent);
        public string XMLConfigFilePath => Path.Combine(ModuleDataDirectoryPath, "DetectBoardsUpdateHistory.xml");

        /***  Console  ***/
        [ObservableProperty]
        private string consoleMessage = string.Empty;

        #endregion

        #region Events

        #region Events Registration

        private void RegisterProxyEvents()
        {
            //缓存连接状态
            this.CurrentCTBoxConnectionStatus = DataAcquisitionProxy.Instance.CTBoxConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
            //Init 
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DetectorComponent}] Register DataAcquisitionProxy events.");
            DataAcquisitionProxy.Instance.DeviceConnectionChanged += DetectorComponent_CTBoxConnectionChanged;
            DataAcquisitionProxy.Instance.RealTimeStatusChanged += DetectorComponent_RealTimeStatusChanged;
            DataAcquisitionProxy.Instance.SystemStatusChanged += DetectorComponent_SystemStatusChanged;
            DataAcquisitionProxy.Instance.RawImageSaved += DetectorComponent_RawDataSaved;
        }

        private void UnRegisterProxyEvents()
        {
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DetectorComponent}] Un-register DataAcquisitionProxy events.");
            DataAcquisitionProxy.Instance.DeviceConnectionChanged -= DetectorComponent_CTBoxConnectionChanged;
            DataAcquisitionProxy.Instance.RealTimeStatusChanged -= DetectorComponent_RealTimeStatusChanged;
            DataAcquisitionProxy.Instance.SystemStatusChanged -= DetectorComponent_SystemStatusChanged;
            DataAcquisitionProxy.Instance.RawImageSaved -= DetectorComponent_RawDataSaved;
        }

        private void RegisterMessagePrinterEvents()
        {
            messagePrintService.OnConsoleMessageChanged += MessagePrintService_OnConsoleMessageChanged;
        }

        private void UnRegisterMessagePrinterEvents()
        {
            messagePrintService.OnConsoleMessageChanged -= MessagePrintService_OnConsoleMessageChanged;
        }

        #endregion

        #region CTBox Connection

        private void DetectorComponent_CTBoxConnectionChanged(object sender, ConnectionStatusArgs args)
        {
            //更新 
            CurrentCTBoxConnectionStatus = args.Connected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
            //信息 
            string message = $"[{ComponentDefaults.DetectorComponent}] Current CTBox connection status: [{Enum.GetName(CurrentCTBoxConnectionStatus)}].";
            //显示记录 
            messagePrintService.PrintLoggerInfo(message);
        }

        #endregion

        #region System Status

        private void DetectorComponent_SystemStatusChanged(object sender, SystemStatusArgs args)
        {
            messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DetectorComponent}] System status: [{Enum.GetName(args.Status)}].");

            if (!IsDataAcquisitionStarted) return;

            switch (args.Status)
            {
                case SystemStatus.NormalScanStopped: 
                    {
                        IsNormalScanStopped = true;
                        break;
                    }
                case SystemStatus.EmergencyStopped:
                    {
                        ResetDataAcquisition();
                        DialogService.Instance.ShowWarning("Emergency Stopped");
                        break;
                    }
                case SystemStatus.ErrorScanStopped:
                    {
                        ResetDataAcquisition();
                        var errorCode = args.GetErrorCodes().First();
                        messagePrintService.PrintLoggerError($"[{ComponentDefaults.DetectorComponent}] Exposure error: {errorCode.GetErrorCodeDescription()}.");
                        DialogService.Instance.ShowErrorCode(errorCode);
                        break;
                    }
            }
        }

        #endregion

        #region Realtime Status

        private void DetectorComponent_RealTimeStatusChanged(object sender, RealtimeEventArgs args)
        {
            //messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DetectorComponent}] Realtime status: [{Enum.GetName(args.Status)}].");
        }

        #endregion

        #region Rawdata Saved

        /// <summary>
        /// 生数据保存事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void DetectorComponent_RawDataSaved(object sender, RawImageSavedEventArgs args)
        {
            //采集结束时，发送采集结束信号
            if (args.IsFinished && IsNormalScanStopped) 
            {
                //打印
                messagePrintService.PrintToConsole($"[{nameof(DetectorStatsViewModel)}] Raw data has been collected.");
                //切数据
                Task.Run(() => 
                {
                    try
                    {
                        //获取当前时间戳
                        var currentTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        //获取源文件夹路径和探测器模组文件夹路径
                        var (targetSourceDirectory, subDirectories) = QueuedDataAcquisitionService.CurrentTaskAcquisitionType switch
                        {
                            AcquisitionType.DarkField => 
                                (Path.Combine(DetectorModuleConfig.DarkFieldSourceDataDirectory, currentTimeStamp), 
                                DetectorModuleConfig.GetAncillaryDataDirectories(DataType.DarkField)),
                            AcquisitionType.BrightField => 
                                (Path.Combine(DetectorModuleConfig.BrightFieldSourceDataDirectory, currentTimeStamp), 
                                DetectorModuleConfig.GetAncillaryDataDirectories(DataType.BrightField)),
                            _ => throw new NotImplementedException(nameof(QueuedDataAcquisitionService.CurrentTaskAcquisitionType))
                        };
                        //转存文件夹
                        DirectoryHelper.CopyDirectory(args.Directory, targetSourceDirectory);
                        //基于模组文件夹路径下，根据时间戳再生成一组路径】 
                        var subDirectoriesWithTimeStamp = subDirectories.Select(t => Path.Combine(t, currentTimeStamp));
                        //确保探测器模组文件夹存在 
                        DirectoryHelper.EnsureDirectories(subDirectoriesWithTimeStamp);
                        //拆分保存数据
                        var result = RawDataProcesserWrapper.SplitAndSaveRawDataListOfDetectorModule(subDirectoriesWithTimeStamp.ToList(), targetSourceDirectory);
                        //校验
                        if (!result.IsSuccessful) 
                        {
                            messagePrintService.PrintToConsole(
                                $"[{nameof(DetectorStatsViewModel)}] Failed to [SplitAndSaveRawDataListOfDetectorModule], " +
                                $"error codes: {result.ErrorCodeStr}.", PrintLevel.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        messagePrintService.PrintToConsole(
                            $"[{nameof(DetectorStatsViewModel)}] [{nameof(DetectorComponent_RawDataSaved)}] " +
                            $"Unhandled exception, please check log.", PrintLevel.Error);
                        logService.Error(ServiceCategory.HardwareTest, 
                            $"[{nameof(DetectorStatsViewModel)}] [{nameof(DetectorComponent_RawDataSaved)}] " +
                            $"Something wrong when [Resave and cut data to detector modules], [Stack]: {ex.ToString()}");
                    }
                });
                Task.Delay(5000).Wait();
                //开始下个任务
                QueuedDataAcquisitionService.ReleaseSignal();
            }
        }

        #endregion

        #endregion

        #region Data Acquisition

        [RelayCommand(CanExecute = nameof(CanStartDataAcquisition))]
        private async Task StartDataAcquisitionAsync()
        {
            //任务开始
            IsDataAcquisitionStarted = true;
            //开始采集任务
            await StartQueuedDataAcquisitionServiceAsync();
            //任务结束
            IsDataAcquisitionStarted = false;

        }

        [RelayCommand]
        private void StopDataAcquisition()
        {
            QueuedDataAcquisitionService.Stop();
            IsDataAcquisitionStarted = false;
        }

        private void ResetDataAcquisition() 
        {
            QueuedDataAcquisitionService.Stop();
            IsDataAcquisitionStarted = false;
            IsNormalScanStopped = false;
        }

        private async Task StartQueuedDataAcquisitionServiceAsync() 
        {
            //启动
            await Task.Run(async () => await QueuedDataAcquisitionService.StartAsync());
        }

        private void QueuedDataAcquisitionService_OnTaskExecutionProcessMessageUpdated(object? sender, string message)
        {
            messagePrintService.PrintToConsole(message);
        }

        #endregion

        #region Series Number

        /// <summary>
        /// 浏览序列号更新历史Command
        /// </summary>
        /// <param name="parameters"></param>
        [RelayCommand]
        private void BroswerSeriesNumberUpdateHistory(object[] parameters) 
        {
            //解析
            if (!int.TryParse(parameters[0].ToString(), out int detectorIndex) ||
                !int.TryParse(parameters[1].ToString(), out int boardIndex))
            {
                logService.Error(ServiceCategory.HardwareTest, "Failed to parse detector index or board index");
                return;
            }

        }

        /// <summary>
        /// 在新装机时载入所有检出板序列号
        /// </summary>
        [RelayCommand]
        private void LoadSeriesNumbersWhenNewInstallation() 
        {
            //文件选择
            var openFileDialog = new OpenFileDialog()
            {
                Title = "Please select a calibration json file.",
                Filter = "Excel(.xlsx)|*.xlsx",
                Multiselect = false,
                RestoreDirectory = true
            };
            /** 打开对话框 **/
            var dialogResult = openFileDialog.ShowDialog();
            /** 处理文件 **/
            if (dialogResult == DialogResult.OK)
            {
                try
                {
                    //获取序列号
                    var seriesNumbers = MiniExcel.Query<SeriesNumberExcelModel>(openFileDialog.FileName);
                    //校验
                    if (seriesNumbers.Count() < DetectBoardTotalCount || seriesNumbers.ElementAt(DetectBoardTotalCount - 1).ID != DetectBoardTotalCount)
                    {
                        //弹窗
                        DialogService.Instance.ShowError("Wrong xlsx file format.");
                        //记录
                        logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.CollimatorComponent}]" +
                            $"[{nameof(LoadSeriesNumbersWhenNewInstallation)}] Wrong xlsx file format, " +
                            $"count - {seriesNumbers.Count()}, 64th id - {seriesNumbers.ElementAt(DetectBoardTotalCount - 1).ID}.");
                    }
                    else
                    {
                        var matchedSeriesNumbers = seriesNumbers.Take(DetectBoardTotalCount).ToArray();
                        //更新槽位对应的板子
                        UpdateDetectBoardSlotSourceWhenNewInstallations(matchedSeriesNumbers);
                    }
                }
                catch (Exception ex)
                {
                    //弹窗
                    DialogService.Instance.ShowError("Failed to load selected file.");
                    //记录
                    logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.CollimatorComponent}]" +
                        $"[{nameof(LoadSeriesNumbersWhenNewInstallation)}] Open file dialog failed, [Stack]: {ex}.");
                }
            }
            else
            {
                logService.Warn(ServiceCategory.HardwareTest, $"[{ComponentDefaults.CollimatorComponent}]" +
                    $"[{nameof(LoadSeriesNumbersWhenNewInstallation)}] Open file dialog failed.");
            }
        }

        #endregion

        #region DetectBoard Update Control

        /// <summary>
        /// 更新探测器检出板槽位对应的源
        /// </summary>
        /// <param name="parameters"></param>
        [RelayCommand]
        private void UpdateDetectBoardSlotSource(object[] parameters) 
        {
            //打开配置窗口
            DialogHelper.ShowDialog<UpdateDetectorBoardSeriesNumberView>(parameters, 780, 155);
        }

        /// <summary>
        /// 查看探测器检出板槽位的更新历史
        /// </summary>
        /// <param name="detectorModuleID"></param>
        [RelayCommand]
        private void BroswerDetectBoardSlotSourceUpdateHistory(uint detectorModuleID) 
        {
            //根据ID获取所有匹配的板子记录
            var matchedDetectBoardSource = DetectBoardDtos.Where(t => t.DetectorModuleID == detectorModuleID).OrderBy(t => t.InstallTime);
            //组装参数
            var parameters = new object[] { detectorModuleID, matchedDetectBoardSource };
            //打开配置窗口
            DialogHelper.ShowDialog<BroswerDetectBoardUpdateHistoryView>(parameters, 780, 400);
        }

        /// <summary>
        /// 序列号更新窗口回调消息
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="message"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void UpdateDetectBoardSlotSourceCallBack(object recipient, UpdateDetectorBoardSlotSourceMessage message)
        {
            //获取新增的检出板信息
            var detectBoardSource = message.DetectBoardSource;
            //添加至记录列表
            DetectBoardDtos.Add(detectBoardSource);
            //匹配对应的槽位
            var detectBoardSlot = DetectBoardSlots.First(t => t.DetectorModuleID == detectBoardSource.DetectorModuleID);
            //替换板子
            detectBoardSlot.ExchangeBoard(detectBoardSource);
            //写配置文件
            DetectorModuleConfig.AddOrUpdateDetectBoardExchangeRecords(DetectBoardDtos.ToDetectBoardExchangeRecords());
            //备份辐照统计文件
            BackUpDataWhenDetectBoardUpdated(detectBoardSource.DetectorModuleID);
        }

        /// <summary>
        /// 当新安装时更新检出板槽对应的Source
        /// </summary>
        private void UpdateDetectBoardSlotSourceWhenNewInstallations(IEnumerable<SeriesNumberExcelModel> seriesNumberExcelModels) 
        {
            //校验
            if (seriesNumberExcelModels.Count() != DetectBoardTotalCount) 
            {
                throw new ArgumentException($"Not enough input series number, count - {seriesNumberExcelModels.Count()}");
            }
            
            uint tempID = 1;
            //遍历替换
            foreach (var seriesNumberExcelModel in seriesNumberExcelModels)
            {
                var detectBoardDto = new DetectBoardDto()
                {
                    DetectorModuleID = tempID,
                    InstallTime = DateTime.Now,
                    SeriesNumber = seriesNumberExcelModel.SeriesNumber,
                    Using = true
                };
                //添加至记录列表
                DetectBoardDtos.Add(detectBoardDto);
                //更换
                DetectBoardSlots[(int)tempID - 1].ExchangeBoard(detectBoardDto);
                //ID++
                tempID++;
            }
            //写配置文件
            DetectorModuleConfig.AddOrUpdateDetectBoardExchangeRecords(DetectBoardDtos.ToDetectBoardExchangeRecords());
            //备份辐照统计文件
            BackUpDataWhenNewInstallations();
        }

        /// <summary>
        /// 更新槽位对应的板子
        /// </summary>
        private void UpdateDetectBoardSlotSource() 
        {
            foreach (var detectBoardSource in DetectBoardDtos.Where(t => (t.Using == true))) 
            {
                var detectorModuleID = detectBoardSource.DetectorModuleID;
                var detectBoardSlot = DetectBoardSlots.First(b => b.DetectorModuleID == detectorModuleID);
                detectBoardSlot.InstalledDetectBoard = detectBoardSource;
            }
        }

        #endregion

        #region Irradiation Data BackUp

        /// <summary>
        /// 当检出板更新时，备份辐照统计数据
        /// </summary>
        private void BackUpDataWhenDetectBoardUpdated(uint detectorModuleID) 
        {
            try
            {
                //辐照统计数据文件路径
                string filePath = DetectorModuleConfig.IrradiationStatisticsLatestFilePath;
                //校验
                if (!File.Exists(filePath)) 
                {
                    logService.Error(ServiceCategory.HardwareTest, 
                        $"[{nameof(DetectorStatsViewModel)}] [{nameof(BackUpDataWhenDetectBoardUpdated)}] " +
                        $"Detector irradiation data file dosen't exist, please check :{filePath}");
                    return;
                }
                //新文件名
                string backupDataFileName = $"{Path.GetFileNameWithoutExtension(filePath)}_" +
                    $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_UpdatedModuleID_{detectorModuleID}.raw";

                Task.Run(() => 
                {
                    //备份文件
                    File.Copy(filePath, Path.Combine(DetectorModuleConfig.IrradiationStatisticsHistoryDirectory, backupDataFileName));
                    //载入探测器总辐照统计数据
                    var result = RawDataProcesserWrapper.LoadDetectorIrraditaionCountData(filePath);
                    //校验
                    if (!result.IsSuccessful) 
                    {
                        logService.Error(ServiceCategory.HardwareTest,
                            $"[{nameof(DetectorStatsViewModel)}] [{nameof(BackUpDataWhenDetectBoardUpdated)}] " + 
                            $"Failed to load detector irradiation data, error code: {result.ErrorCodeStr}");
                        return;
                    }
                    //清除对应ModuleID的数据
                    for (uint i = 0; i < ImageHeight; i++)
                    {
                        uint offset = (detectorModuleID - 1) * (ImageWidth/ (uint)DetectBoardTotalCount) + i * ImageWidth;

                        for (uint j = offset; j < offset + ImageWidth / (uint)DetectBoardTotalCount; j++)
                        {
                            result.Message[j] = 0;
                        }
                    }
                    //写入文件
                    var saveResult = RawDataProcesserWrapper.SavedDetectorIrraditaionCountData(filePath, result.Message);
                    //校验
                    if (!saveResult.IsSuccessful) 
                    {
                        logService.Error(ServiceCategory.HardwareTest,
                            $"[{nameof(DetectorStatsViewModel)}] [{nameof(BackUpDataWhenDetectBoardUpdated)}] " +
                            $"Failed to save module ID: {detectorModuleID} cleared detector irradiation data, error code: {result.ErrorCodeStr}");
                        return;
                    }
                    //记录
                    logService.Info(ServiceCategory.HardwareTest,
                       $"[{nameof(DetectorStatsViewModel)}] [{nameof(BackUpDataWhenDetectBoardUpdated)}] " +
                       $"Detector irradiation data has been backed up, the origin detector module ID: {detectorModuleID} data has been clear.");
                });          
            }
            catch (Exception ex)
            {
                logService.Error(ServiceCategory.HardwareTest, 
                    $"[{nameof(DetectorStatsViewModel)}] [{nameof(BackUpDataWhenDetectBoardUpdated)}] " +
                    $"Unhandled exception occured, [Stack]: {ex.ToString()}");
            }         
        }

        /// <summary>
        /// 当检出板更新时，备份辐照统计数据
        /// </summary>
        private void BackUpDataWhenNewInstallations()
        {
            try
            {
                //辐照统计数据文件路径
                string filePath = DetectorModuleConfig.IrradiationStatisticsLatestFilePath;
                //校验
                if (!File.Exists(filePath))
                {
                    logService.Error(ServiceCategory.HardwareTest,
                        $"[{nameof(DetectorStatsViewModel)}] [{nameof(BackUpDataWhenDetectBoardUpdated)}] " +
                        $"Detector irradiation data file dosen't exist, please check :{filePath}");
                    return;
                }
                //新文件名
                string backUpDataFileName = $"{Path.GetFileNameWithoutExtension(filePath)}_" +
                    $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_NewInstallation.raw";

                Task.Run(() =>
                {
                    //备份文件
                    File.Copy(filePath, Path.Combine(DetectorModuleConfig.IrradiationStatisticsHistoryDirectory, backUpDataFileName));
                    //载入探测器总辐照统计数据
                    var result = RawDataProcesserWrapper.LoadDetectorIrraditaionCountData(filePath);
                    //校验
                    if (!result.IsSuccessful)
                    {
                        logService.Error(ServiceCategory.HardwareTest,
                            $"[{nameof(DetectorStatsViewModel)}] [{nameof(BackUpDataWhenDetectBoardUpdated)}] " +
                            $"Failed to load detector irradiation data, error code: {result.ErrorCodeStr}");
                        return;
                    }
                    //清除对应ModuleID的数据
                    for (uint i = 0; i < ImageWidth * ImageHeight; i++)
                    {
                        result.Message[i] = 0;
                    }
                    //写入文件
                    var saveResult = RawDataProcesserWrapper.SavedDetectorIrraditaionCountData(filePath, result.Message);
                    //校验
                    if (!saveResult.IsSuccessful)
                    {
                        logService.Error(ServiceCategory.HardwareTest,
                            $"[{nameof(DetectorStatsViewModel)}] [{nameof(BackUpDataWhenDetectBoardUpdated)}] " +
                            $"Failed to save complete new detector irradiation data, error code: {result.ErrorCodeStr}");
                        return;
                    }
                    //记录
                    logService.Info(ServiceCategory.HardwareTest,
                       $"[{nameof(DetectorStatsViewModel)}] [{nameof(BackUpDataWhenDetectBoardUpdated)}] " +
                       $"Detector irradiation data has been backed up, the complete new file is cleared.");
                });
            }
            catch (Exception ex)
            {
                logService.Error(ServiceCategory.HardwareTest,
                    $"[{nameof(DetectorStatsViewModel)}] [{nameof(BackUpDataWhenDetectBoardUpdated)}] " +
                    $"Unhandled exception occured, [Stack]: {ex.ToString()}");
            }
        }

        #endregion

        #region XML File

        /// <summary>
        /// 载入XML配置文件
        /// </summary>
        private void LoadXMLConfig()
        {
            //获取检出板更换记录
            var detectBoardExchangeRecords = DetectorModuleConfig.DetectBoardsExchangeHistoryConfig.DetectBoardExchangeRecords;
            //更新DetectorBoardSources
            var detectBoardDtos = detectBoardExchangeRecords.ToDetectBoardDtos();
            //更新DetectorBoardSources
            DetectBoardDtos = new(detectBoardDtos);
            //更新槽位对应的板子
            UpdateDetectBoardSlotSource();
        }

        #endregion

        #region Message Print

        private void MessagePrintService_OnConsoleMessageChanged(object? sender, string message)
        {
            ConsoleMessage = message;
        }

        [RelayCommand]
        private void ClearConsoleMessage()
        {
            messagePrintService.Clear();
        }

        #endregion

        #region Navigation

        public override void BeforeNavigateToCurrentPage()
        {
            //记录
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DetectorComponent}] Enter [Detector Stats] testing page.");
            //注册Proxy事件 
            RegisterProxyEvents();
            //注册消息打印事件 
            RegisterMessagePrinterEvents();           
        }

        public override void BeforeNavigateToOtherPage()
        {
            //取消注册Proxy事件
            UnRegisterProxyEvents();
            //取消注册消息打印事件 
            UnRegisterMessagePrinterEvents();
            // 记录
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DetectorComponent}] Leave [Detector Stats] testing page.");
        }

        #endregion

    }
}
