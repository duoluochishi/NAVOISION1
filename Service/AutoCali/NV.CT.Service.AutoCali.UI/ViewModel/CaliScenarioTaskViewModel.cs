using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry.Helic;
using NV.CT.Alg.ScanReconCalculation.Scan.Table;
using NV.CT.Alg.ScanReconCalculation.Scan.Table.Axial;
using NV.CT.Alg.ScanReconCalculation.Scan.Table.Helic;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Helpers;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Extensions;
using NV.CT.FacadeProxy.Models.AutoCalibration;
using NV.CT.FacadeProxy.Models.DataAcquisition;
using NV.CT.FacadeProxy.Models.MotionControl.Gantry;
using NV.CT.Service.AutoCali.DAL;
using NV.CT.Service.AutoCali.Logic.Handlers;
using NV.CT.Service.AutoCali.Logic.Handlers.Scans;
using NV.CT.Service.AutoCali.Logic.Messages;
using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.AutoCali.UI.Logic;
using NV.CT.Service.AutoCali.UI.Resources;
using NV.CT.Service.AutoCali.UI.ViewModel;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Helper;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.Common.Utils;
using NV.CT.Service.Enums;
using NV.CT.Service.UI.Util;
using NV.CT.Service.Universal.PrintMessage;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using NV.MPS.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using RawImageInfo = NV.CT.Service.AutoCali.UI.ViewModel.RawImageChangedEventArgs;

namespace NV.CT.Service.AutoCali.Logic
{
    /// <summary>
    /// 校准场景的查看功能的视图模型，供View绑定使用
    /// </summary>
    public class CaliScenarioTaskViewModel : CaliTaskViewModel<CalibrationScenario>
    {
        /// <summary>
        /// 外发事件--校准任务已启动
        /// </summary>
        public event EventHandler CaliTaskStarted;

        /// <summary>
        /// 外发事件--校准任务已结束（正常完成/用户取消/错误终止）
        /// </summary>
        public event EventHandler CaliTaskEnded;

        /// <summary>
        /// 是否可以进入下一步的阻塞信号，比如，扫描完成，可以查看确认扫描的生数据了
        /// </summary>
        private AutoResetEvent goNextEvent = new AutoResetEvent(false);

        private ILogService _logger;
        private IMessagePrintService _uiLogger;
        public string MessengerToken { get; set; }

        //static CaliScenarioTaskViewModel() {
        //}

        public CaliScenarioTaskViewModel(CalibrationScenario? caliScenario) : base(caliScenario)
        {
            try
            {
                _logger = LogService.Instance;
                InitMessagePrintService();

                MessengerToken = $"{DateTime.Now.ToString("yyyyMMdd.HHmmss.fff")}.{this.GetHashCode()}";

                if (caliScenario != null)
                {
                    CaliItemTaskViewModels = new ObservableCollection<CaliItemTaskViewModel>();
                    foreach (var caliItemName in caliScenario.CalibrationItemReferenceGroup)
                    {
                        var caliItem = CaliItemServiceImpl.Instance.CacheCaliItems.FirstOrDefault(x => x.Name == caliItemName);
                        if (caliItem == null)
                        {
                            Console.WriteLine($"校准场景：\"{caliScenario.Name}\"，配置的校准项目：\"{caliItemName}\" 未找到！");
                            continue;
                        }

                        Console.WriteLine($"校准场景：\"{caliScenario.Name}\"，配置的校准项目：\"{caliItemName}\" 已找到！");
                        CaliItemTaskViewModels.Add(new CaliItemTaskViewModel(caliItem));
                    }
                    SubTaskViewModels = new ObservableCollection<ICaliTaskViewModel>(CaliItemTaskViewModels);
                }
                InitCommand();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region 校准项目任务

        private CaliItemTaskViewModel? mSelectedCaliItemTaskViewModel;

        public CaliItemTaskViewModel? SelectedCaliItemTaskViewModel
        {
            get => mSelectedCaliItemTaskViewModel;
            set
            {
                if (value == mSelectedCaliItemTaskViewModel)
                {
                    return;
                }

                mSelectedCaliItemTaskViewModel = value;
                if (null == mSelectedCaliItemTaskViewModel)
                {
                    return;
                }

                OnPropertyChanged("SelectedCaliItemTaskViewModel");
                UpdateArgProtocolDataView();
            }
        }

        /// <summary>
        /// 是否自动确认扫描结果和校准结果
        /// </summary>
        public bool IsAutoConfirmResult { get; set; }

        /// <summary>
        /// 散射探测器的增益模式，默认16pc
        /// </summary>
        public ScatteringDetectorGain XScatteringDetectorGain { get; set; }

        /// <summary>
        /// 是否内部调试模式，
        /// 可以手动指定模拟环境下原始生数路径（T03的单帧文件，D:\TestData）
        /// 手动指定扫描后的生数据路径（S03的多帧文件，F:\AppData\DataMRS\ServiceData)
        /// </summary>
        public bool IsDebugMode { get; set; }

        /// <summary>
        /// 是否自动确认结果，条件就是 非调试模式（需要手动指定目录）& 自动确认结果
        /// </summary>
        public bool IsAutoNext { get => !IsDebugMode && IsAutoConfirmResult; }

        public ObservableCollection<CaliItemTaskViewModel>? CaliItemTaskViewModels { get; set; }

        #endregion 校准项目任务

        /// <summary>
        /// 参数协议的数据表格视图，切换校准项目会导致表格列名和行数据发生变化
        /// 比如，校准项目A，数据【名称：a】，【扫描速度】
        /// </summary>
        public DataView ArgProtocolDataView { get; private set; }

        protected void UpdateArgProtocolDataView()
        {
            CreateArgProtocolDataTable();
            ArgProtocolDataView = _ArgProtocolDataTable.AsDataView();
            OnPropertyChanged("ArgProtocolDataView");
        }

        private void CreateArgProtocolDataTable()
        {
            if (null == _ArgProtocolDataTable)
            {
                _ArgProtocolDataTable = new DataTable();
            }
            else
            {
                _ArgProtocolDataTable.Columns.Clear();
                _ArgProtocolDataTable.Clear();
            }

            bool hasHeader = false;
            if (null == SelectedCaliItemTaskViewModel || null == SelectedCaliItemTaskViewModel.SubTaskViewModels)
            {
                return;
            }

            int columnCount = 0;
            foreach (var caliProtocolTaskViewModel in SelectedCaliItemTaskViewModel.SubTaskViewModels)
            {
                foreach (var stepTaskViewModel in caliProtocolTaskViewModel.SubTaskViewModels)
                {
                    var stepDto = ((CaliScanTaskViewModel)stepTaskViewModel).Inner;
                    if (!hasHeader)
                    {
                        _ArgProtocolDataTable.Columns.Add("运行状态");////参数协议名称--表头
                        _ArgProtocolDataTable.Columns.Add("Name");////参数协议名称--表头
                        foreach (var argItem in stepDto.Parameters)
                        {
                            if (!_ArgProtocolDataTable.Columns.Contains(argItem.Name))
                            {
                                _ArgProtocolDataTable.Columns.Add(argItem.Name);
                            }
                        }

                        hasHeader = true;
                        columnCount = _ArgProtocolDataTable.Columns.Count;
                    }

                    var row = new List<string>();
                    var dataRow = _ArgProtocolDataTable.NewRow();
                    int index = 0;
                    dataRow[index++] = stepTaskViewModel.IsCompleted;//运行状态
                    dataRow[index++] = stepDto.Name;//参数协议名称
                    foreach (var argItem in stepDto.Parameters)
                    {
                        // 避免数据对象字段超过显示表格的列定义宽度
                        if (index >= columnCount)
                        {
                            Console.WriteLine($"[ERR]ArgItem[{index}]:{argItem.Name} can not add to dataRaw, because the column index is out of range({columnCount}).");
                            continue;
                        }

                        dataRow[index++] = argItem.Value;
                    }
                    _ArgProtocolDataTable.Rows.Add(dataRow);
                }
            }
        }

        private string _TaskOutput = string.Empty;

        public string TaskOutput
        {
            get => _TaskOutput;
            set
            {
                if (value == _TaskOutput)
                {
                    return;
                }

                _TaskOutput = value;
                OnPropertyChanged("TaskOutput");
            }
        }

        public void RunScenarioTask()
        {
            _cancellationTokenSource = new CancellationTokenSource();//是否取消任务运行

            InitTaskState(this.SubTaskViewModels);

            RunScenarioByTask();
        }

        private void CancelScenarioTask(object sender, EventArgs eventArgs)
        {
            _cancellationTokenSource?.Cancel();
        }

        #region private methods

        private static string folder1 = @"F:\AppData\DataMRS\ServiceData\2024_0418_130240_SourceXZ\130240_inDynamic_AfterDelete";
        private static string folder2 = @"F:\AppData\DataMRS\ServiceData\2024_0419_154821_WaterValue-154821_20_Single_AfterDelete";
        string curFolder = folder1;
        private void RunScenarioByTask()
        {
            CaliScenarioTaskViewModel caliScenarioTaskViewModel = this;

            //有效性检查：是否包括可执行的校准协议
            if (!CheckAnyExecutableCalibrationProtocol(caliScenarioTaskViewModel))
            {
                return;
            }

            //有效性检查：用户确认扫描注意事项
            if (!ConfirmScanningAttention())
            {
                return;
            }

            StartCommandEnabled = false;
            ResetTaskStatus(caliScenarioTaskViewModel, CaliTaskState.WaitingToRun);

            caliScenarioTaskViewModel.CaliTaskStarted?.Invoke(this, null);

            caliScenarioTaskViewModel.CaliTaskState = CaliTaskState.Running;

            CalibrationScenario caliScenario = caliScenarioTaskViewModel.Inner;
            string msg = string.Format(Calibration_Lang.Calibration_StartRunningScenario, caliScenario.Name);
            PrintMessage(msg);

            Task.Run(async () =>
            {
                try
                {
                    //curFolder = (curFolder == folder2) ? folder1 : folder2;
                    //this.LoadRawImages(curFolder);
                    //return;

                    for (int caliItemIndex = 0; caliItemIndex < this.CaliItemTaskViewModels.Count; caliItemIndex++)
                    {
                        try
                        {
                            var curCaliItem = this.CaliItemTaskViewModels[caliItemIndex];
                            if (!curCaliItem.IsChecked) continue;

                            //Start running the calibration project
                            this.SelectedCaliItemTaskViewModel = curCaliItem;
                            await RunTaskForCaliItem(this.SelectedCaliItemTaskViewModel);
                        }
                        catch (UserRepeatException ex)
                        {
                            int viewIndex = caliItemIndex + 1;
                            PrintMessage($"Redo the project: \"{SelectedCaliItemTaskViewModel.Name}\".");
                            ResetTaskState(caliItemIndex);
                            --caliItemIndex;
                        }
                    }

                    OnCaliScenarioTaskFinished(caliScenarioTaskViewModel);

                    ResetIsCheckedWhen(caliScenarioTaskViewModel.SubTaskViewModels, CaliTaskState.Success, false);
                    ResetTaskStatusWhen(caliScenarioTaskViewModel, CaliTaskState.WaitingToRun, CaliTaskState.Created);
                }
                catch (Exception ex)
                {
                    //不再监听 扫描采集 或者 校准 事件
                    OnUnRegiterEvent(this);
                    OnCaliScenarioTaskCanceled(caliScenarioTaskViewModel, ex);

                    ResetTaskStatusWhen(caliScenarioTaskViewModel, CaliTaskState.WaitingToRun, CaliTaskState.Created);

                    //todo:区分用户取消和服务返回错误,
                    //考虑更好的方式返回,而不是用userAbortException,否则无法区分用户abort和服务返回错误
                    if (ex is UserAbortException)
                    {
                        ResetTaskStatusWhen(caliScenarioTaskViewModel, CaliTaskState.Running, CaliTaskState.Canceled);
                    }
                    else if (ex is SystemAbortException)
                    {
                        ResetTaskStatusWhen(caliScenarioTaskViewModel, CaliTaskState.Running, CaliTaskState.Error);
                    }
                    else
                    {
                        ResetTaskStatusWhen(caliScenarioTaskViewModel, CaliTaskState.Running, CaliTaskState.Error);
                        PrintMessage($"[Error] {ex.Message}", PrintLevel.Error);
                        logWrapper.Error($"An error occurred while executing [RunScenarioByTask], details:{ex}");
                    }
                }
                finally
                {
                    StartCommandEnabled = true;
                    NextCommandEnabled = false;
                    _offlineReconHandler?.UnregisterOfflineMachineProxy();
                }
            });

            Console.WriteLine($"create task to start progressInfo");
        }

        private OfflineReconHandler _offlineReconHandler;
        private OfflineCalibrationHandler _offlineCalibrationHandler;

        private bool ConfirmScanningAttention()
        {
            string title = Calibration_Lang.Calibration_Scanning_Attention;
            string content = Calibration_Lang.Calibration_Scanning_Attention_Content;
            return DialogService.Instance.ShowConfirm(content, title);
        }

        /// <summary>
        /// 检查是否包括任意可执行的的校准协议
        /// </summary>
        /// <param name="caliScenarioTaskViewModel"></param>
        /// <returns></returns>
        private bool CheckAnyExecutableCalibrationProtocol(CaliScenarioTaskViewModel caliScenarioTaskViewModel)
        {
            int count = 0;
            for (int caliItemIndex = 0; caliItemIndex < caliScenarioTaskViewModel?.CaliItemTaskViewModels?.Count; caliItemIndex++)
            {
                var caliProject = this.CaliItemTaskViewModels[caliItemIndex];
                if (!caliProject.IsChecked)
                {
                    continue;
                }

                foreach (var task in caliProject?.SubTaskViewModels)
                {
                    if (!task.IsChecked)
                    {
                        continue;
                    }
                    foreach (var subTask in task?.SubTaskViewModels)
                    {
                        if (!subTask.IsChecked)
                        {
                            continue;
                        }
                        count++;
                    }
                }
            }
            if (count == 0)
            {
                //未找到可执行的校准协议，请勾选一个或者多个校准协议
                string text1 = StringResources.NoCalibrationProtocolChecked;
                string text2 = StringResources.PleaseCheckOneOrMoreCalibrationProtocols;
                DialogService.Instance.ShowWarning($"{text1}\n{text2}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 重新开始（从0，或者某个项目开始）
        /// </summary>
        /// <param name="startCaliItemIndex"></param>
        private void ResetTaskState(int startCaliItemIndex = 0)
        {
            for (int caliItemIndex = startCaliItemIndex; caliItemIndex < this.CaliItemTaskViewModels.Count; caliItemIndex++)
            {
                var caliItemTaskViewModel = this.CaliItemTaskViewModels[caliItemIndex];

                ResetTaskStatus(caliItemTaskViewModel);
            }
        }

        private void ResetTaskStatus(ICaliTaskViewModel taskViewModel, CaliTaskState taskStatus = CaliTaskState.Created)
        {
            if (taskViewModel == null)
            {
                return;
            }

            taskViewModel.IsCompleted = false;
            taskViewModel.CaliTaskState = taskStatus;

            if (taskViewModel.SubTaskViewModels == null)
            {
                return;
            }

            foreach (ICaliTaskViewModel childTaskVM in taskViewModel.SubTaskViewModels)
            {
                ResetTaskStatus(childTaskVM, taskStatus);//递归操作子项
            }
        }

        private void ResetTaskStatusWhen(ICaliTaskViewModel taskViewModel, CaliTaskState whenTaskStatus,
            CaliTaskState toNewTaskStatus = CaliTaskState.Created)
        {
            if (taskViewModel == null)
            {
                return;
            }

            if (taskViewModel.SubTaskViewModels != null)
            {
                foreach (ICaliTaskViewModel childTaskVM in taskViewModel.SubTaskViewModels)
                {
                    ResetTaskStatusWhen(childTaskVM, whenTaskStatus, toNewTaskStatus);//递归操作子项
                }
            }

            taskViewModel.IsCompleted = false;
            if (taskViewModel.CaliTaskState == whenTaskStatus)
            {
                this._logger.Debug(ServiceCategory.AutoCali, $"{taskViewModel.GetType().Name} '{taskViewModel.Name}' TaskStatus: '{taskViewModel.CaliTaskState}' -> {toNewTaskStatus}");
                taskViewModel.CaliTaskState = toNewTaskStatus;
            }
        }

        private void ResetIsCheckedWhen(Collection<ICaliTaskViewModel> taskViewModelList, CaliTaskState whenTaskStatus, bool toNewIsChecked)
        {
            if (taskViewModelList == null)
            {
                return;
            }

            foreach (var taskViewModel in taskViewModelList)
            {
                ResetIsCheckedWhen(taskViewModel, whenTaskStatus, toNewIsChecked);
            }
        }
        private void ResetIsCheckedWhen(ICaliTaskViewModel taskViewModel, CaliTaskState whenTaskStatus, bool toNewIsChecked)
        {
            if (taskViewModel == null)
            {
                return;
            }

            if (taskViewModel.SubTaskViewModels != null)
            {
                foreach (ICaliTaskViewModel childTaskVM in taskViewModel.SubTaskViewModels)
                {
                    ResetIsCheckedWhen(childTaskVM, whenTaskStatus, toNewIsChecked);//递归操作子项
                }
            }

            if (taskViewModel.CaliTaskState == whenTaskStatus)
            {
                this._logger.Debug(ServiceCategory.AutoCali, $"[{nameof(ResetIsCheckedWhen)}] {taskViewModel.GetType().Name} '{taskViewModel.Name}' TaskStatus='{taskViewModel.CaliTaskState}', IsChecked: {taskViewModel.IsChecked} -> {toNewIsChecked}");
                taskViewModel.IsChecked = toNewIsChecked;
            }
        }

        #region dynamic generate the StudyUID and ScanUID

        private static readonly int MaxLengthOfCaliItemName = 30;

        private string GenerateStudyUid(string caliItemName, string protocolName)
        {
            string usedCaliItemName = (caliItemName.Length <= MaxLengthOfCaliItemName) ? caliItemName :
                caliItemName.Substring(0, MaxLengthOfCaliItemName);
            return UidUtil.GenerateUid_16() + "_" + usedCaliItemName + "_" + protocolName; ;
        }

        #endregion dynamic generate the StudyUID and ScanUID

        private string CurrentScanUID = string.Empty;//当前扫描Uid，用来区分并过滤内置的PreOffset扫描

        /// <summary>
        /// 更新扫描参数中的ScanUID，并且更新当前对象
        /// </summary>
        /// <param name="scanParam"></param>
        /// <param name="scanUID"></param>
        private void UpdateScanUID(ref ScanParam scanParam)
        {
            //扫描前更新ScanUID，避免前面其它处理耽误太长时间了，导致ScanUID不准确
            string scanUid = UidUtil.GenerateScanUid_16(scanParam.RawDataType);
            scanParam.ScanUID = scanUid;
            UpdateCurrentScanUID(scanUid);
        }

        private void UpdateCurrentScanUID(string scanUID)
        {
            CurrentScanUID = scanUID;
            logWrapper.Debug($"Set CurrentScanUID '{CurrentScanUID}'");
        }

        private void ResetForRetry(ref bool isRetryEnable)
        {
            isRetryEnable = false;

            //避免被取消一次后，取消令牌一直是已取消的状态
            if (this._cancellationTokenSource?.IsCancellationRequested == true)
            {
                this._cancellationTokenSource = new CancellationTokenSource();
            }
        }
        private async Task RunTaskForCaliItem(CaliItemTaskViewModel caliItemTaskViewModel)
        {
            caliItemTaskViewModel.CaliTaskState = CaliTaskState.Running;
            var caliItem = caliItemTaskViewModel.Inner;
            string caliItemName = caliItem.Name;

            string msg = string.Format(Calibration_Lang.Calibration_StartRunningTheCalibrationProject, caliItem.Name);
            PrintMessage(msg);

            //用户确认是否完成注意事项了
            if (!TryConfirmToRunTaskForCaliItem(caliItemTaskViewModel.Inner))
            {
                throw new UserAbortException();//取消剩余未执行的任务
            }

            foreach (var caliProtocolTaskViewModel in SelectedCaliItemTaskViewModel.SubTaskViewModels)
            {
                //多个扫描参数依次请求扫描采集
                var scanTaskViewModelList = caliProtocolTaskViewModel.SubTaskViewModels;
                int scanCount = scanTaskViewModelList.Count;
                if (!caliProtocolTaskViewModel.IsChecked || 0 == scanCount)
                {
                    caliProtocolTaskViewModel.IsCompleted = true;
                    continue;
                }

                //协议任务更新为运行状态
                caliProtocolTaskViewModel.CaliTaskState = CaliTaskState.Running;

                string protocolName = caliProtocolTaskViewModel.Name;
                string protocolNameWithProject = $"{caliItemName}-{protocolName}";
                string studyInstanceUID = GenerateStudyUid(caliItemName, protocolName);
                var scanReconParamList = new List<ScanReconParam>();
                ScanReconParam scanReconParam = null;
                int lapNo = 0;

                #region 依次扫描

                GeneralArgProtocolViewModel protocolVM = null;
                Handler stepDto = null;//
                string taskName = string.Empty;

                bool isRetryEnable = false;
                for (int scanIndex = 0; scanIndex < scanCount; scanIndex++)
                {
                    CommandResult commandResult = null;
                    try
                    {
                        taskName = $"Scan round '{scanIndex + 1}/{scanCount}' for {protocolNameWithProject}";
                        PrintMessage($"Prepare parameters of {taskName}");

                        ResetForRetry(ref isRetryEnable);

                        #region 转换成扫描参数

                        CaliScanTaskViewModel scanTaskVM = scanTaskViewModelList[scanIndex] as CaliScanTaskViewModel;
                        scanTaskVM.CaliTaskState = CaliTaskState.Running;

                        protocolVM = ProtocolVM_Factory.GetProtocolViewModel(caliItem) as GeneralArgProtocolViewModel;
                        stepDto = scanTaskVM.Inner;
                        protocolVM.SetValueFrom(stepDto);

                        scanTaskVM.Name = taskName;
                        PrintMessage(taskName);

                        scanReconParam = protocolVM.PrepareScanParam();
                        if (null == scanReconParam)
                        {
                            msg = Calibration_Lang.Calibration_CancelRunningTheCalibrationScenario;
                            msg += string.Format(Calibration_Lang.Calibration_Comma_FailedToPrepareScanParam, scanIndex, stepDto.ID);
                            PrintMessage(msg);

                            throw new SystemAbortException();//取消剩余未执行的任务
                        }

                        scanReconParam.Study.StudyInstanceUID = studyInstanceUID;//同一组校准的多个扫描统一studyInstanceUID
                        scanReconParamList.Add(scanReconParam);
                        _lastScanReconParam = scanReconParam;

                        #endregion 转换成扫描参数

                        ScanParam scanParam = scanReconParam.ScanParameter;
                        var scanUID = scanParam.ScanUID;
                        var funcMode = scanParam.FunctionMode;
                        var rawDataType = protocolVM.RawDataType;

                        TryChangePostOffsetFrames(scanParam);

                        //水值校准，涉及到重建，因此动态计算部分参数：重建删图长度
                        if (funcMode == FunctionMode.Cali_WaterValue)
                        {
                            TryModifyReconDeleteLength(scanReconParam);
                        }

                        //todo:DebugMode下修改扫描参数，并跳过扫描
                        if (TryChangeScanParamsWhenDebugMode(scanTaskVM, scanReconParam))
                        {
                            continue;//跳过扫描，已经手动指定了（已存在的）生数据路径
                        }

                        //禁用“下一步”命令
                        string step = $"Check Device whether Enable for {taskName}";
                        PrintMessage(step);
                        DisableNextCommand(step);

                        //检查磁盘空间
                        if (!DiskspaceService.Instance.ValidateRawDataDiskFreeSapce())
                        {
                            string diskErrorCode = "MCS007000003";
                            DialogService.Instance.ShowErrorCode(diskErrorCode);
                            throw new SystemAbortException();//取消剩余未执行的任务
                        }


                        //注册扫描采集事件
                        OnRegriterAcqEvent(this);

                        //1.检测设备是否可扫描
                        await CheckScanIsEnabledAsync(scanTaskVM, _cancellationTokenSource);
                        if (scanTaskVM.CaliTaskState == CaliTaskState.Canceled)
                        {
                            throw new UserAbortException();//取消剩余未执行的任务
                        }

                        #region 扫描开始前，（可配置的）准备工作：PreOffsetBySoftware / 机架归零 / 床自动移动到探测器边缘

                        //3.发起请求：开始扫描

                        //可配置的预处理
                        await TryPreProcess(scanReconParam, scanTaskVM, caliItemTaskViewModel, protocolVM);
                        //处理完成PreProcessHandlers后，禁用下一步按钮
                        DisableNextCommand("AfterPreProcess");

                        string calibrationItemName = caliItemTaskViewModel.Inner.Name;
                        bool isDetectorOffset = string.Equals(calibrationItemName, "DetectorOffset", StringComparison.OrdinalIgnoreCase);
                        #region 3.2. 扫描开始前，机架归零

                        //非 探测器偏移校准 校准项，才有可能需要机架归零
                        if (!isDetectorOffset)
                        {
                            //3.2. 正式扫描前准备--（可选）机架归0
                            await TryMoveGantryToInit(protocolVM.MoveGantryToInit, scanParam);
                        }
                        else
                        {
                            commandResult = await TryMoveGantryToTarget(scanParam.GantryStartPosition/*, scanParam*/, true);
                            if (!commandResult.Success())
                            {
                                throw new SystemAbortException();
                            }
                        }

                        #endregion 3.2. 扫描开始前，机架归零

                        //3.3. 配置床的扫描范围在安全范围
                        SetTablePositionFromDevice(scanParam);

                        #region 3.4. 扫描开始前，床自动移动到探测器边缘

                        //1.1 扫描前准备--（可选）床自动叠加 激光灯等位线与探测器边缘的距离
                        if (protocolVM.MoveTableAfterAddFixDistance)
                        {
                            string operationName = "水平移床";
                            string messageToConfirmMoveTable = $"系统将自动 {operationName} 固定距离进入曝光区域。\n如果不需要，请忽略此操作。";
                            if (DialogService.Instance.ShowConfirm(messageToConfirmMoveTable, operationName))
                            {
                                await RequestMoveTableAfterAddFixDistance(scanTaskVM, this._cancellationTokenSource);
                                if (scanTaskVM.CaliTaskState == CaliTaskState.Canceled)
                                {
                                    throw new UserAbortException();//取消剩余未执行的任务
                                }

                                //移床后，重新从Devic获取当前床码值，并且设置到ScanReconParam中
                                SetTablePositionFromDevice(scanParam);
                            }
                        }

                        #endregion 3.4. 扫描开始前，床自动移动到探测器边缘

                        //1.3 重新计算床的运动参数，以及机架的运动参数
                        if (isDetectorOffset)
                        {
                            CalcGantryMotionParamForDetectorOffset(scanParam);
                        }
                        else
                        {
                            CalcGantryMotionParam(scanParam);
                        }

                        CalcTableMotionParam(scanParam);

                        #endregion 扫描开始前，（可配置的）准备工作：PreOffsetBySoftware / 机架归零 / 床自动移动到探测器边缘

                        //扫描前更新ScanUID，避免前面其它处理耽误太长时间了，导致ScanUID不准确
                        UpdateScanUID(ref scanParam);

                        //准备启动新的扫描后，清空原有的生数据图像
                        PrintMessage("Try to Clear the loaded images from UI.");
                        TryClearRawImage();

                        scanCommandResult = null;
                        //3.3. 开始本次真正的扫描
                        if (scanReconParam.ScanParameter.ScanOption == ScanOption.Axial)
                        {
                            await StartFreeScanAsync(scanTaskVM, scanReconParam, protocolVM);
                        }
                        else
                        {
                            await RequestStartScanAsync(scanTaskVM, scanReconParam);
                        }

                        //任务发生错误，无论自动还是手动，都需要停下来，并交互是否“Redo” 或者 “StopAll”
                        if (scanTaskVM.CaliTaskState == CaliTaskState.Error)
                        {
                            isRetryEnable = true;
                        }
                        else
                        {
                            //非自动确认结果模式下，需要用户查看结果，并手动下一步
                            if (IsAutoConfirmResult && !isRetryEnable)
                            {
                                PrintMessage($"System Auto Next to Skip the raw images loading.");
                            }
                            else
                            {
                                if (scanTaskVM.CaliTaskState == CaliTaskState.Success)
                                {
                                    //加载图片
                                    TryLoadRawImage(scanParam.RawDataDirectory);
                                }
                                else if (scanTaskVM.CaliTaskState == CaliTaskState.Canceled)
                                {
                                    throw new UserAbortException();//取消剩余未执行的任务
                                }

                                DisableNextCommand("PendingUserViewImage");
                                PrintMessage("Auto Wait 5s for User View Image");
                                await Task.Delay(TimeSpan.FromSeconds(5));

                                //string userNextGuid = "Next after view the image";
                                //await PendingUserNext(userNextGuid, () => { return Task.CompletedTask; });
                            }
                        }

                        #region Confirm to Redo or Next
                        //todo,修改为UI操作指令
                        if (IsAutoConfirmResult && !isRetryEnable)
                        {
                            PrintMessage($"System Auto Next to Skip View and Confirm the image.");
                        }
                        else
                        {
                            isRetryEnable = true;
                        }
                        #endregion Confirm to Redo or Next

                        #region PostProcess

                        //可配置的预处理
                        await TryPostProcess(scanReconParam, scanTaskVM, caliItemTaskViewModel, protocolVM);
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        //todo:区分用户取消和服务返回错误,
                        //考虑更好的方式返回,而不是用userAbortException,否则无法区分用户abort和服务返回错误
                        //[todo]暂不支持用户stop后再重做的，后续可以新增“全部停止”区别单步停止和全部停止（即当前的UserAbort）
                        if (!IsAutoConfirmResult && ex is /*UserAbortException or*/ SystemAbortException)
                        {
                            isRetryEnable = true;
                        }
                        else
                        {
                            throw ex;
                        }
                    }

                    logWrapper.Debug($"After {taskName}, Check Retry whether Enable '{isRetryEnable}'");
                    //Just break the loop to Next
                    if (!isRetryEnable)
                    {
                        logWrapper.Debug($"System auto Next");
                        continue;
                    }

                    #region 界面流程停一下，等待用户交互：下一步，或者 重做

                    string userNextGuid = CONST_Next_Or_Redo;
                    ResetForRetry(ref isRetryEnable);

                    commandResult = await PendingUserNext(userNextGuid, () => { return Task.CompletedTask; });
                    if (commandResult.Status == CommandStatus.Cancelled)
                    {
                        PrintMessage($"User cancelled when pending 'Next' for {taskName}");
                        throw new UserAbortException();//取消剩余未执行的任务
                    }
                    else
                    {
                        string sender = commandResult.Sender;
                        bool isConfirmedToRedo = string.Equals(sender, CONST_Sender_UserRedo, StringComparison.OrdinalIgnoreCase);
                        if (isConfirmedToRedo)
                        {
                            PrintMessage($"Selected to 'Redo' After {taskName}");
                            RevertToRedoScan(taskName, ref scanIndex, scanReconParamList, scanReconParam);
                            //continue;
                        }
                        else
                        {
                            PrintMessage($"Selected to 'Next' After {taskName}");
                            //break;//不要跳出去，
                        }
                    }

                    #endregion 界面流程停一下，等待用户交互：下一步，或者 重做
                }

                //不再监听扫描采集事件（包括硬件错误事件）
                OnUnRegiterAcqEvent(this);

                #endregion 依次扫描

                #region 合并多次扫描后，请求校准算法执行

                taskName = $"Computing for {protocolNameWithProject}";
                int repeatMaxNum = 5;
                int currentExcuteTimes = 0;
                while (currentExcuteTimes++ < repeatMaxNum)
                {
                    CommandResult commandResult;
                    try
                    {
                        ResetForRetry(ref isRetryEnable);

                        string repeatInfo = (currentExcuteTimes > 1) ? ($"{currentExcuteTimes} times") : string.Empty;
                        PrintMessage($"Ready to Calibration Computing {repeatInfo}");
                        //进入校准计算过程，在完成之前，禁止“下一步”
                        DisableNextCommand("Calibration Computing");

                        //[TODO]重构，并且等待后续算法提供可调用的dll后（目前是matlab的exe，无法直接调用）
                        if (caliItemTaskViewModel.Inner.Name == "DetectorOffset_Old")
                        {
                            string rawDataDir = _lastScanReconParam.ScanParameter.RawDataDirectory;
                            new DetectofOffsetCalibrationHandler(rawDataDir).Execute();

                            //等待用户手动“下一步”
                            ResumeNextCommand("After DetectofOffsetCalibrationHandler");
                            //await PendingOnManualNext(caliItemTaskViewModel, _cancellationTokenSource);
                        }
                        else if (protocolVM.OfflineReconProxyEnable)
                        {
                            //先释放可能未释放的监听事件，避免泄露
                            _offlineReconHandler?.UnregisterOfflineMachineProxy();
                            _offlineReconHandler = null;

                            //创建新的处理类
                            _offlineReconHandler = new OfflineReconHandler(this._logger, this._uiLogger, this._cancellationTokenSource);

                            //
                            var UpdatePropertyForDicom = (() =>
                            {
                                string calibrationProtocolName = caliProtocolTaskViewModel.Name;//校准协议名称

                                string rawDataDir = _lastScanReconParam.ScanParameter.RawDataDirectory;
                                string firstHeadPath = Directory.GetFiles(rawDataDir, "*.head").FirstOrDefault();
                                DateTime rawCreationTime = System.IO.File.GetCreationTime(firstHeadPath);

                                _lastScanReconParam.Study.StudyID = studyInstanceUID;
                                _lastScanReconParam.Study.StudyDate = rawCreationTime;
                                _lastScanReconParam.Study.StudyTime = rawCreationTime;
                                _lastScanReconParam.Study.StudyDescription = $"{calibrationProtocolName},@{rawCreationTime}";

                                _lastScanReconParam.ReconSeriesParams[0].AcquisitionDate = rawCreationTime;
                                _lastScanReconParam.ReconSeriesParams[0].SeriesDescription = $"{calibrationProtocolName},@{DateTime.Now}";

                                _lastScanReconParam.Patient.Name = caliItemName;//校准项名称
                                _lastScanReconParam.Patient.ID = _lastScanReconParam.ScanParameter.ScanUID;
                                _lastScanReconParam.Patient.BirthDate = rawCreationTime;//校准生数据日期
                                _lastScanReconParam.Patient.BirthTime = rawCreationTime;//校准生数据时间
                                _lastScanReconParam.Patient.Sex = PatientSex.O;
                            });

                            //开始离线重建任务
                            commandResult = await _offlineReconHandler.StartTask(_lastScanReconParam);//异步等待

                            //更新任务状态
                            var caliTaskState = TotCaliTaskState(commandResult.Status);
                            caliProtocolTaskViewModel.CaliTaskState = caliTaskState;
                            caliItemTaskViewModel.CaliTaskState = caliTaskState;

                            if (!commandResult.Success())
                            {
                                string firstErrorCode = commandResult.ErrorCodes?.Codes?.FirstOrDefault();

                                if (!string.IsNullOrEmpty(firstErrorCode))
                                {
                                    if (firstErrorCode == ErrorCodeDefines.ERROR_CODE_USER_CANCELED)
                                    {
                                        throw new UserAbortException();//取消剩余未执行的任务
                                    }
                                    else
                                    {
                                        DialogService.Instance.ShowErrorCode(firstErrorCode);
                                    }
                                }
                                else
                                {
                                    string errorMsg = commandResult.Description;
                                    DialogService.Instance.ShowError(errorMsg);
                                }

                                throw new SystemAbortException();//取消剩余未执行的任务
                            }
                        }
                        else if (scanCount > 0)
                        {
                            if (caliItemTaskViewModel.Inner.Name == "DLT")
                            {
                                string title = "Clear the images";
                                string message = "Clear the images to avoid no enough memory resources for Computing.";
                                DialogService.Instance.ShowWarning(message, title);
                                ClearRawImages();//DLT的原始数据太大了，所以在计算之前需要从界面上清空图像，避免在同一个机器上计算时内存不够
                            }

                            //[ToDo]有配置开关控制请求离线机校准 或者 主控机校准
                            if (ComputingMachineType.MasterMachine == caliItem.ComputingMachineType)
                            {
                                PrintMessage($"Calibration Computing on the Master Machine");
                                await Calibration(protocolNameWithProject, caliProtocolTaskViewModel, scanReconParamList);
                            }
                            else
                            {
                                PrintMessage($"Calibration Computing on the Recon Machine");
                                await RequestOfflineCalibration(caliItemName, caliItemTaskViewModel,
                                    (CaliProtocolTaskViewModel)caliProtocolTaskViewModel, studyInstanceUID,
                                    scanReconParamList);
                            }
                        }

                        //校准计算结束了（成功后者失败）
                        if (IsAutoConfirmResult)//自动确认模式下，不支持重做
                        {
                            //默认支持重做
                            isRetryEnable = false;
                        }
                        else
                        {
                            var currentTaskStatus = caliProtocolTaskViewModel.CaliTaskState;
                            PrintMessage($"Protocol Task Status:{currentTaskStatus}");
                            if (currentTaskStatus == CaliTaskState.Success)
                            {
                                msg = "Calibration Computing Finished‌.";
                                PrintMessage(msg);
                                DialogService.Instance.ShowInfo(msg);

                                isRetryEnable = false;
                                logWrapper.Debug($"Not need to retry fro {taskName} due to {msg}");
                            }
                            else
                            {
                                isRetryEnable = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //todo:区分用户取消和服务返回错误,
                        //考虑更好的方式返回,而不是用userAbortException,否则无法区分用户abort和服务返回错误
                        //[todo]暂不支持用户stop后再重做的，后续可以新增“全部停止”区别单步停止和全部停止（即当前的UserAbort）
                        if (!IsAutoConfirmResult && ex is /*UserAbortException or*/ SystemAbortException)
                        {
                            isRetryEnable = true;
                        }
                        else
                        {
                            throw ex;
                        }
                    }

                    logWrapper.Debug($"After Computed for {taskName}, Checked isRetryEnable '{isRetryEnable}'");
                    //Just break the loop to Next
                    if (!isRetryEnable)
                    {
                        logWrapper.Debug($"System Auto Next due to not allow retry");
                        break;
                    }

                    //界面流程停一下，等待用户交互：下一步，或者 重做
                    ResetForRetry(ref isRetryEnable);

                    string userNextGuid = CONST_Next_Or_Redo;
                    commandResult = await PendingUserNext(userNextGuid, () => { return Task.CompletedTask; });
                    if (commandResult.Status == CommandStatus.Cancelled)
                    {
                        PrintMessage($"User cancelled when pending 'Next' for {taskName}");
                        throw new UserAbortException();//取消剩余未执行的任务
                    }
                    else
                    {
                        string sender = commandResult.Sender;
                        bool isConfirmedToRedo = string.Equals(sender, CONST_Sender_UserRedo, StringComparison.OrdinalIgnoreCase);
                        if (isConfirmedToRedo)
                        {
                            PrintMessage($"Selected to 'Redo' Computing");
                            continue;//回到当前扫描协议，重新开始
                        }
                        else
                        {
                            PrintMessage($"Selected to 'Next'");
                            break;
                        }
                    }
                }

                #endregion 合并多次扫描后，请求校准算法执行

                //caliProtocolTaskViewModel.CaliTaskState = CaliTaskState.Success;
            }
            caliItemTaskViewModel.IsCompleted = true;
            caliItemTaskViewModel.CaliTaskState = CaliTaskState.Success;
        }


        //private static readonly string CONST_Select_Ok_To_Redo_Or_Cancel_To_Next = @"Select 'OK' to 'Redo' if needed, or select 'Cancel' to 'Next'.";

        //private static readonly string CONST_Comfirm_Message_Do_You_Want_To_Redo = "Do you want to Redo?";
        //private static readonly string CONST_Comfirm_Message_Redo_Or_Next = $"{CONST_Comfirm_Message_Do_You_Want_To_Redo}\n{CONST_Select_Ok_To_Redo_Or_Cancel_To_Next}";
        //private static readonly string CONST_Next_And_Select = $"'Next', and {CONST_Select_Ok_To_Redo_Or_Cancel_To_Next}";
        private static readonly string CONST_Next_Or_Redo = $"'Next' or 'Redo' if needed";
        private static readonly string CONST_Sender_UserRedo = "UserRedo";

        private void RevertToRedoScan(string taskName, ref int scanIndex, List<ScanReconParam> scanReconParamList, ScanReconParam scanReconParam)
        {
            var oldScanIndex = scanIndex;
            --scanIndex;//准备再次执行
            var newScanIndex = scanIndex;

            scanReconParamList.Remove(scanReconParam);//丢弃本次扫描的
            this._logger.Debug(ServiceCategory.AutoCali, $"Reverted the flow to Redo Scan by Reset scanIndex from {oldScanIndex} to {newScanIndex}");

            //因为重做，作废当前生数据文件夹
            string currentRawDataDir = scanReconParam.ScanParameter.RawDataDirectory;
            if (Directory.Exists(currentRawDataDir))
            {
                string logMsg = $"Discarding the directory '{currentRawDataDir}' when 'Redo'";
                try
                {
                    if (scanReconParam.ScanParameter.FunctionMode == FunctionMode.Cali_AfterGlow)
                    {
                        logMsg += $", and Delete '{currentRawDataDir}' when FunctionMode.Cali_AfterGlow";
                        logWrapper.Debug(logMsg);
                        Directory.Delete(currentRawDataDir, true);//必须用true，否则抛异常“目录不为空”
                    }
                    else
                    {
                        string discardRawDataDir = currentRawDataDir + "_Discard";
                        logMsg += $", and Move to '{discardRawDataDir}'";
                        logWrapper.Debug(logMsg);
                        Directory.Move(currentRawDataDir, discardRawDataDir);
                    }
                }
                catch (Exception ex)
                {
                    logWrapper.Error($"Failed to Discard {logMsg}, with the exception:{ex}");
                }
            }
        }

        /// <summary>
        /// 请求执行离线校准
        /// </summary>
        /// <param name="caliItemName"></param>
        /// <param name="caliItemTaskViewModel"></param>
        /// <param name="caliProtocolTaskViewModel"></param>
        /// <param name="studyID"></param>
        /// <param name="scanReconParamList"></param>
        /// <returns></returns>
        /// <exception cref="SystemAbortException"></exception>
        private async Task RequestOfflineCalibration(string caliItemName,
            CaliItemTaskViewModel caliItemTaskViewModel,
            CaliProtocolTaskViewModel caliProtocolTaskViewModel,
            string studyID,
            List<ScanReconParam> scanReconParamList)
        {
            //先释放可能未释放的监听事件，避免泄露
            _offlineCalibrationHandler?.UnregisterOfflineCalibrationProxy();
            _offlineCalibrationHandler = null;

            //创建新的处理类
            _offlineCalibrationHandler = new(this._logger, this._uiLogger, this._cancellationTokenSource);

            string calibrationProtocolName = caliProtocolTaskViewModel.Name;//校准协议名称
            UpdatePropertyForDicom(ref _lastScanReconParam, caliItemName, studyID, calibrationProtocolName);

            //开始离线重建任务
            CommandResult commandResult = await _offlineCalibrationHandler.ExecuteTask(scanReconParamList);//异步等待

            //更新任务状态
            var caliTaskState = TotCaliTaskState(commandResult.Status);
            caliProtocolTaskViewModel.CaliTaskState = caliTaskState;
            caliItemTaskViewModel.CaliTaskState = caliTaskState;

            var commandStatus = commandResult.Status;
            if (CommandStatus.Cancelled == commandStatus)
            {
                throw new UserAbortException();//取消剩余未执行的任务
            }
            else if (CommandStatus.Failure == commandStatus)
            {
                string firstErrorCode = commandResult.ErrorCodes?.Codes?.FirstOrDefault();

                if (!string.IsNullOrEmpty(firstErrorCode))
                {
                    if (firstErrorCode == ErrorCodeDefines.ERROR_CODE_USER_CANCELED)
                    {
                        throw new UserAbortException();//取消剩余未执行的任务
                    }
                    else
                    {
                        DialogService.Instance.ShowErrorCode(firstErrorCode);
                    }
                }
                else
                {
                    string errorMsg = commandResult.Description;
                    DialogService.Instance.ShowError(errorMsg);
                }

                throw new SystemAbortException();//取消剩余未执行的任务
            }
        }

        private void UpdatePropertyForDicom(ref ScanReconParam scanReconParam, string patientName, string studyID, string protocolName)
        {
            //string calibrationProtocolName = caliProtocolTaskViewModel.Name;//校准协议名称

            string rawDataDir = scanReconParam.ScanParameter.RawDataDirectory;
            string firstHeadPath = Directory.GetFiles(rawDataDir, "*.head").FirstOrDefault();
            DateTime rawCreationTime = File.GetCreationTime(firstHeadPath);

            scanReconParam.Study.StudyID = studyID;
            scanReconParam.Study.StudyDate = rawCreationTime;
            scanReconParam.Study.StudyTime = rawCreationTime;
            scanReconParam.Study.StudyDescription = $"{protocolName},@{rawCreationTime}";

            scanReconParam.ReconSeriesParams[0].AcquisitionDate = rawCreationTime;
            scanReconParam.ReconSeriesParams[0].SeriesDescription = $"{protocolName},@{DateTime.Now}";

            scanReconParam.Patient.Name = patientName;//校准项名称
            scanReconParam.Patient.ID = scanReconParam.ScanParameter.ScanUID;
            scanReconParam.Patient.BirthDate = rawCreationTime;//校准生数据日期
            scanReconParam.Patient.BirthTime = rawCreationTime;//校准生数据时间
            scanReconParam.Patient.Sex = PatientSex.O;
        }

        /// <summary>
        /// 动态计算部分参数：重建删图长度
        /// </summary>
        /// <param name="scanReconParam"></param>
        /// <returns></returns>
        private bool TryModifyReconDeleteLength(ScanReconParam scanReconParam)
        {
            var firstScanParam = scanReconParam.ScanParameter;
            var tableControlOutput = GetTableControlOutput(firstScanParam);
            if (tableControlOutput == null)
            {
                return false;
            }

            firstScanParam.SmallAngleDeleteLength = (uint)tableControlOutput.SmallAngleDeleteLength;
            firstScanParam.LargeAngleDeleteLength = (uint)tableControlOutput.LargeAngleDeleteLength;

            //new WaterValueReconHandler().ModifyReconPositionParam(this.logger, scanReconParam);
            return true;
        }

        private TaskToken _taskToken;

        private async Task TryMoveGantryToInit(bool isMoveGantryToInitFromConfig, ScanParam scanParam)
        {
            bool isMoveGantryToInit = isMoveGantryToInitFromConfig || HasEnoughGantryAngle(scanParam);
            //3.2. 正式扫描前准备--（可选）机架归0
            if (isMoveGantryToInit)
            {
                string taskName = "机架复位";
                string messageToConfirmMoveTable = $"系统即将执行 {taskName}。\n如果不需要，请忽略此操作。";
                this._uiLogger.PrintLoggerWarn(messageToConfirmMoveTable);

                bool isConfirmed;
                if (IsAutoNext)
                {
                    isConfirmed = IsAutoNext;
                    this._uiLogger.PrintLoggerWarn($"系统自动确认执行 {taskName}");
                }
                else
                {
                    isConfirmed = (DialogService.Instance.ShowConfirm(messageToConfirmMoveTable, taskName));
                    string userChooseInfo = isConfirmed ? "已确认" : "已取消";
                    this._uiLogger.PrintLoggerWarn($"用户 {userChooseInfo} 执行 {taskName}");
                }

                if (isConfirmed)
                {
                    await MoveGantryBackToInitAsync(this._cancellationTokenSource);
                }
            }
        }

        /// <summary>
        /// 机架位置微小误差视为相同位置，避免机械抖动
        /// </summary>
        private static readonly uint SameGantryPositionDeltaAsSame = 50;
        private bool IsSameGantryPosition(uint pos1, uint pos2)
        {
            var abs = Math.Abs(pos2 - pos1);
            return (abs < SameGantryPositionDeltaAsSame);
        }
        private async Task<CommandResult> TryMoveGantryToTarget(uint targetPosition/*, ScanParam scanParam*/, bool isAutoConfirm = true)
        {
            CommandResult commandResult = CommandResult.DefaultSuccessResult;

            var currentPosition = DeviceSystem.Instance.Gantry.Position;
            bool isMoveGantry = !IsSameGantryPosition(currentPosition, targetPosition);
            //3.2. 正式扫描前准备--（可选）机架归0
            if (!isMoveGantry)
            {
                return commandResult;
            }

            string taskName = $"机架运动到{ToGantryPositionForView(targetPosition)}";
            string messageToConfirmMoveTable = $"系统即将执行 {taskName}。\n如果不需要，请忽略此操作。";
            this._uiLogger.PrintLoggerWarn(messageToConfirmMoveTable);

            bool isConfirmed;
            if (isAutoConfirm)
            {
                isConfirmed = isAutoConfirm;
                this._uiLogger.PrintLoggerWarn($"系统自动确认执行 {taskName}");
            }
            else
            {
                isConfirmed = (DialogService.Instance.ShowConfirm(messageToConfirmMoveTable, taskName));
                string userChooseInfo = isConfirmed ? "已确认" : "已取消";
                this._uiLogger.PrintLoggerWarn($"用户 {userChooseInfo} 执行 {taskName}");
            }

            if (isConfirmed)
            {
                commandResult = await MoveGantryToTargetAsync(this._cancellationTokenSource, targetPosition);
            }

            return commandResult;
        }

        /// <summary>
        /// 是否有足够的机架角度
        /// </summary>
        /// <param name="scanParam"></param>
        /// <returns></returns>
        private bool HasEnoughGantryAngle(ScanParam scanParam)
        {
            logWrapper.Debug($"Checking the gantry angle whether enough ...");
            uint currentGantryPosition = GetGantryPositionFromDevice(); //最小60度,*100=6000

            var framesPerCycle = scanParam.FramesPerCycle;
            var cycles = scanParam.TotalFrames * 1.0 / framesPerCycle;

            ////机架的参数
            //var gantryTime_Second = ConvertMicroToSecond(framesPerCycle * scanParam.FrameTime);
            //var gantrySpeed = 15 * 100.0 / gantryTime_Second;//0.01度/秒，所以 度数*100
            //机架速度
            //scanParam.GantrySpeed = (uint)gantrySpeed;//=277*100,机架速度
            //scanParam.GantryAccelerationTime = 500 * 1000;//机架加速度需要设置，默认500ms

            //var currentAngle = currentGantryPosition;
            var gantryMotionLength = CalcGantryMotionLength(cycles);
            var aboutGangryEndPosition = currentGantryPosition + gantryMotionLength;
            bool hasEnoughGantryAngle = (aboutGangryEndPosition > GantryPositionMax);
            logWrapper.Debug($"Checked the gantry angle whether enough, [Output] {HasEnoughGantryAngle}. By [Input] currentGantryPosition:{currentGantryPosition}, gantryMotionLength:{gantryMotionLength}, aboutGangryEndPosition:{aboutGangryEndPosition}.");
            return hasEnoughGantryAngle;
        }

        /// <summary>
        /// 获取机架的当前角度位置，单位：0.01度
        /// </summary>
        /// <returns></returns>
        private uint GetGantryPosition()
        {
            return DeviceSystem.Instance.Gantry.Position;
        }

        private static readonly int GantryPositionCoef = 100;
        private static readonly int GantryPositionMax = 524 * GantryPositionCoef;
        private async Task TryMoveGantryToInitWhenNotEnough(ScanParam scanParam)
        {
            string method = nameof(TryMoveGantryToInitWhenNotEnough);

            var currentPotion = DeviceSystem.Instance.Gantry.Position;
            var availableAngle = GantryPositionMax - currentPotion;

            float cycle = scanParam.TotalFrames * 1.0f / scanParam.FramesPerCycle;
            var gantryMotionLength = (cycle * 15 + 10) * GantryPositionCoef;//每圈15度，前后加速减少缓冲距离10度

            this._logger.Debug(ServiceCategory.AutoCali, $"[{method}] availableAngle:{availableAngle}, gantryMotionLength:{gantryMotionLength}, cycle:{cycle}");
            if (availableAngle > gantryMotionLength)
            {
                this._uiLogger.PrintLoggerInfo($"No Need to Move Gantry due to Available Angle Enough");
                return;
            }

            //3.2. 正式扫描前准备--（可选）机架归0
            string taskName = "机架复位";
            string messageToConfirmMoveTable = $"系统将执行 {taskName}。\n如果不需要，请忽略此操作。";
            if (DialogService.Instance.ShowConfirm(messageToConfirmMoveTable, taskName))
            {
                await MoveGantryBackToInitAsync(this._cancellationTokenSource);
            }
        }

        private void TaskStatusAsRunningWhenMiddleSuccess(ICaliTaskViewModel taskViewModel)
        {
            switch (taskViewModel.CaliTaskState)
            {
                case CaliTaskState.Success:
                    taskViewModel.CaliTaskState = CaliTaskState.Running;
                    break;

                case CaliTaskState.Error:
                    throw new SystemAbortException();
                case CaliTaskState.Canceled:
                    throw new UserAbortException();
            }
        }

        private async Task<CommandResult> PendingUserNext(string operation, Func<Task> funcAsyncWhenNext)
        {
            string sender = $"{nameof(PendingUserNext)}_{operation}";
            string messageHeader = $"[{nameof(PendingUserNext)}]";

            _taskToken = null;
            this._logger.Debug(ServiceCategory.AutoCali, $"{messageHeader} Set _taskToken=null for '{operation}'");

            this._logger.Debug(ServiceCategory.AutoCali, $"{messageHeader} Call ResumeNextCommand for '{operation}'");
            ResumeNextCommand(sender);

            this._uiLogger.PrintLoggerInfo($"Pending to {operation}");
            _taskToken = new TaskToken(this._logger, this._uiLogger, operation, this._cancellationTokenSource);

            CommandResult commandResult = await _taskToken.Take();

            this._logger.Debug(ServiceCategory.AutoCali, $"{messageHeader} Call DisableNextCommand for '{operation}'");
            DisableNextCommand($"{nameof(PendingUserNext)}");

            this._logger.Debug(ServiceCategory.AutoCali, $"{messageHeader} Await Invoke the input func (async) for '{operation}'");
            if (funcAsyncWhenNext != null)
            {
                await funcAsyncWhenNext.Invoke();
            }

            this._logger.Debug(ServiceCategory.AutoCali, $"{messageHeader} Call ResumeNextCommand for '{operation}'");
            ResumeNextCommand(sender);
            //PendingUserNextAndHold(operation, funcAsyncWhenNext);
            if (commandResult.Status == CommandStatus.Cancelled)
            {
                throw new UserAbortException();
            }

            return commandResult;
        }

        //private async Task<CommandResult> PendingUserNextAndHold(string operation, Func<Task> funcAsyncWhenNext)
        //{
        //    string sender = $"{nameof(PendingUserNextAndHold)}_{operation}";
        //    string messageHeader = $"[{nameof(PendingUserNextAndHold)}]";

        //    _taskToken = null;
        //    _goNextTaskCompletionSource = null;
        //    this._logger.Debug(ServiceCategory.AutoCali, $"{messageHeader} Set _taskToken=null for '{operation}'");

        //    this._logger.Debug(ServiceCategory.AutoCali, $"{messageHeader} Call ResumeNextCommand for '{operation}'");
        //    ResumeNextCommand(sender);

        //    this._uiLogger.PrintLoggerInfo($"Pending to {operation}");
        //    //不再需要TaskToken，改用_goNextTaskCompletionSource
        //    //_taskToken = new TaskToken(this._logger, this._uiLogger, operation, this._cancellationTokenSource);
        //    //await _taskToken.Take();

        //    _goNextTaskCompletionSource = new();

        //    this._logger.Debug(ServiceCategory.AutoCali, $"{messageHeader} Call DisableNextCommand for '{operation}'");
        //    DisableNextCommand(sender);

        //    this._logger.Debug(ServiceCategory.AutoCali, $"{messageHeader} Await Invoke the input func (async) for '{operation}'");
        //    if (funcAsyncWhenNext != null)
        //    {
        //        await funcAsyncWhenNext.Invoke();
        //    }

        //    this._logger.Debug(ServiceCategory.AutoCali, $"{messageHeader} Call ResumeNextCommand for '{operation}'");
        //    ResumeNextCommand(sender);

        //    return await Hold(sender);
        //}

        private TaskCompletionSource<CommandResult> _goNextTaskCompletionSource;
        //private async Task<CommandResult> Hold(object sender)
        //{
        //    //goNextEvent.WaitOne();
        //    this._logger.Debug(ServiceCategory.AutoCali, $"[{nameof(Hold)}] Hold by the sender '{sender}'");
        //    return await _goNextTaskCompletionSource.Task;
        //}
        /// <summary>
        /// 等待用户主动点击"下一步"
        /// </summary>
        /// <param name="taskViewModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        //private async Task<CommandResult> PendingOnManualNext2(CancellationTokenSource cancellationToken)
        //{
        //    CommandResult commandResult = CommandResult.DefaultSuccessResult;
        //    PrintMessage($"You can go \"Next\"，or \"Stop\"");
        //    while (!goNextEvent.WaitOne(1000))
        //    {
        //        if (cancellationToken.IsCancellationRequested)
        //        {
        //            //用户主动取消了任务
        //            PrintMessage($"The user cancelled the task");
        //            //taskViewModel.CaliTaskState = CaliTaskState.Canceled;
        //            commandResult.Status = CommandStatus.Cancelled;
        //            break;
        //        }
        //    }

        //    return commandResult;
        //}

        private async Task TryPreProcess(ScanReconParam scanReconParam,
            CaliScanTaskViewModel scanTaskVM,
            CaliItemTaskViewModel parentCaliTaskViewModel,
            GeneralArgProtocolViewModel protocolVM)
        {
            var preProcessHandlers = scanTaskVM.Inner?.PreHandlers;
            if (preProcessHandlers?.Count < 1)
            {
                this._uiLogger.PrintLoggerInfo($"No PreProcess Configured.");
                return;
            }

            foreach (var preProcessHandlerConfig in preProcessHandlers)
            {
                string handlerType = preProcessHandlerConfig.Type;
                if (handlerType == nameof(AutoAirBowtieCalibrationHandler))
                {
                    string calibrationProjectName = "AirBowtie";

                    var tempScanParam = scanReconParam.ScanParameter;

                    //从Devic获取当前床码值，并且设置到ScanReconParam中
                    SetTablePositionFromDevice(tempScanParam);

                    var backupPartParam = new
                    {
                        tempScanParam.TableStartPosition,
                        tempScanParam.TableEndPosition,

                        tempScanParam.FunctionMode,
                        tempScanParam.RawDataType,
                        tempScanParam.ScanUID,

                        tempScanParam.TotalFrames,
                    };

                    #region //1.退床

                    var targetTableStartPosition = 30 * DistanceUmToMmCeof;
                    string taskName = $"Move Table to {FormatTablePosition(targetTableStartPosition)} for {calibrationProjectName}";
                    string userNextGuid = $"Next to {taskName}";
                    this._uiLogger.PrintLoggerInfo(taskName);
                    var autoMoveTableHandler = new AutoMoveTableHandler(this._logger, this._uiLogger, this._cancellationTokenSource);

                    var commandResult = await autoMoveTableHandler.Run(targetTableStartPosition);

                    if (!commandResult.Success())
                    {
                        scanTaskVM.CaliTaskState = CaliTaskState.Error;
                    }
                    TaskStatusAsRunningWhenMiddleSuccess(scanTaskVM);
                    #endregion //1.退床

                    #region 退机架回退，保证扫描具有足够可用角度
                    //扫描前准备--（可选）机架归0
                    taskName = $"Move Gantry for {calibrationProjectName}";
                    userNextGuid = $"Next to {taskName}";
                    await PendingUserNext(userNextGuid, async () =>
                    {
                        await TryMoveGantryToInitWhenNotEnough(tempScanParam);
                    });
                    #endregion 退机架回退，保证扫描具有足够可用角度

                    #region //2.扫描

                    //重新计算床的运动参数，以及机架的运动参数
                    SetTablePositionFromDevice(tempScanParam);
                    CalcGantryMotionParam(tempScanParam);

                    tempScanParam.FunctionMode = FunctionMode.Cali_DynamicGain_AirBowtie;
                    tempScanParam.RawDataType = RawDataType.Bowtie;
                    UpdateScanUID(ref tempScanParam);

                    tempScanParam.TotalFrames = tempScanParam.FramesPerCycle;

                    uint totalFrames = tempScanParam.TotalFrames;
                    bool bowtieSwitch = protocolVM.BowtieSwitch;
                    bool collimatorSwitch = (1 == protocolVM.CollimatorSwitch);

                    taskName = $"Scan for {calibrationProjectName}";
                    userNextGuid = $"Next to {taskName}";
                    await PendingUserNext(userNextGuid, async () =>
                    {
                        this._uiLogger.PrintLoggerInfo($"Start to {taskName}");

                        //准备启动新的扫描后，清空原有的生数据图像
                        TryClearRawImage();

                        //下发扫描命令
                        await StartFreeScanAsync(scanTaskVM, scanReconParam, protocolVM);

                        this._uiLogger.PrintLoggerInfo($"Completed to {taskName}");
                    });
                    TaskStatusAsRunningWhenMiddleSuccess(scanTaskVM);
                    //加载图片
                    TryLoadRawImage(tempScanParam.RawDataDirectory);

                    #endregion //2.扫描

                    //3.校准
                    taskName = $"Calibration for {calibrationProjectName}";
                    userNextGuid = $"Next to {taskName}";
                    await PendingUserNext(userNextGuid, async () =>
                    {
                        this._uiLogger.PrintLoggerInfo($"Start to {taskName}");
                        await Calibration(calibrationProjectName, parentCaliTaskViewModel, new List<ScanReconParam>() { scanReconParam });

                        this._uiLogger.PrintLoggerInfo($"Completed to {taskName}");
                    });
                    TaskStatusAsRunningWhenMiddleSuccess(scanTaskVM);

                    //4.恢复床位
                    taskName = $"Revert Table Position After {calibrationProjectName}";
                    userNextGuid = $"Next to {taskName}";
                    await PendingUserNext(userNextGuid, async () =>
                    {
                        this._uiLogger.PrintLoggerInfo($"Start to {taskName}");
                        commandResult = await autoMoveTableHandler.Run(backupPartParam.TableStartPosition);
                    });
                    if (!commandResult.Success())
                    {
                        scanTaskVM.CaliTaskState = CaliTaskState.Error;
                    }
                    TaskStatusAsRunningWhenMiddleSuccess(scanTaskVM);

                    #region //5.恢复参数
                    this._logger.Info(ServiceCategory.AutoCali, $"#{calibrationProjectName}, Revet Scan Parameter:{backupPartParam}");
                    scanReconParam.ScanParameter.TableStartPosition = backupPartParam.TableStartPosition;
                    scanReconParam.ScanParameter.TableEndPosition = backupPartParam.TableEndPosition;
                    scanReconParam.ScanParameter.FunctionMode = backupPartParam.FunctionMode;
                    scanReconParam.ScanParameter.RawDataType = backupPartParam.RawDataType;
                    scanReconParam.ScanParameter.ScanUID = backupPartParam.ScanUID;
                    scanReconParam.ScanParameter.TotalFrames = backupPartParam.TotalFrames;
                    #endregion //5.恢复参数
                }
                else if (handlerType == nameof(AttentionHandler))
                {
                    RegisterAttentionEvents();

                    var handler = new AttentionHandler(_logger, _uiLogger, _cancellationTokenSource);
                    handler.MessengerToken = this.MessengerToken;
                    handler.Execute(preProcessHandlerConfig);
                }
            }
        }

        private async Task TryPostProcess(ScanReconParam scanReconParam,
            CaliScanTaskViewModel scanTaskVM,
            CaliItemTaskViewModel parentCaliTaskViewModel,
            GeneralArgProtocolViewModel protocolVM)
        {
            var processHandlers = scanTaskVM.Inner?.PostHandlers;
            if (processHandlers?.Count < 1)
            {
                this._logger.Info(ServiceCategory.AutoCali, $"No PostProcess Configured.");
                return;
            }

            foreach (var processHandlerConfig in processHandlers)
            {
                string handlerType = processHandlerConfig.Type;
                if (handlerType == nameof(ValidatePostOffsetHandler))
                {
                    var handler = new ValidatePostOffsetHandler(_logger, _uiLogger, scanReconParam.ScanParameter.RawDataDirectory);
                    var commandResult = handler.ExecuteAsync();
                    if (!string.IsNullOrEmpty(handler.ErrorInfo))
                    {
                        string msg = $"终止流程!\n{handler.ErrorInfo}";
                        DialogService.Instance.ShowError(msg);
                        this._logger.Error(ServiceCategory.AutoCali, msg);
                        throw new SystemAbortException();
                    }
                }
            }
        }

        #region Events

        private void RegisterAttentionEvents()
        {
            WeakReferenceMessenger.Default.Unregister<AttentionMessage, string>(this, MessengerToken);
            try
            {
                WeakReferenceMessenger.Default.Register<AttentionMessage, string>(this, MessengerToken, OnAttentionAsync);
            }
            catch (Exception ex)
            {
                _logger.Debug(ServiceCategory.AutoCali, $"[{nameof(RegisterAttentionEvents)}] Failed to Register for AttentionMessage, [Exception] {ex}");
            }
        }

        private async void OnAttentionAsync(object sender, AttentionMessage attentionMessage)
        {
            string userNextGuid = "确认完成 注意事项 后，请按‘下一步’";
            await PendingUserNext(userNextGuid, async () => { });
        }
        #endregion Events

        private void TryLoadRawImage(string rawDataDirectory)
        {
            //加载图片
            if (!IsAutoConfirmResult)
            {
                PrintMessage($"Loading the raw images from {rawDataDirectory}");
                this.LoadRawImages(rawDataDirectory);
            }
            else
            {
                PrintMessage($"Don't load the raw images due to the config 'AutoConfirmResult'");
            }
        }

        private async Task Calibration(string protocolName, ICaliTaskViewModel caliTaskViewModel, List<ScanReconParam> scanReconParamList)
        {
            //监听校准事件
            CaliService.Instance.RegisterCalibrationEvent(this);

            await CalibrationCore(protocolName, caliTaskViewModel, scanReconParamList);
            await SubmitCalibrationResult(protocolName, caliTaskViewModel);

            //不再监听校准事件
            CaliService.Instance.UnRegisterCalibrationEvent(this);
        }

        private async Task CalibrationCore(string protocolName, ICaliTaskViewModel caliTaskViewModel, List<ScanReconParam> scanReconParamList)
        {
            string taskName = $"Computing for {protocolName}";
            PrintMessage($"Request to {taskName} ...");

            //初始化状态变量
            DisableNextCommand(nameof(CalibrationCore));
            currentProgressInfoFormatter = CalibrateProgressInfo_DetailFormatter;
            isServiceStateFinished = false;
            lastAcqReconStatusArgs = null;
            _lastCalcStatusInfo = null;

            //1.倒计时请求开始校准，避免直接进入请求，在模拟器环境下，偶发PostOffset文件读取失败
            //（怀疑扫描刚结束文件还没有被释放就因为开始校准进入Pipeline读取文件时发现文件被占用，无法打开）
            await CountDownToRequestServiceAsync(_cancellationTokenSource, "Computing", () =>
            {
                caliTaskViewModel.CaliTaskState = CaliTaskState.Canceled;

                throw new UserAbortException();//取消剩余未执行的任务
            });

            //2发起请求：开始生成校准表
            await RequestStartCalibrateAsync(caliTaskViewModel, scanReconParamList);
            if (caliTaskViewModel.CaliTaskState == CaliTaskState.Canceled)
            {
                throw new UserAbortException();//取消剩余未执行的任务
            }
            else if (caliTaskViewModel.CaliTaskState == CaliTaskState.Error)
            {
                string errorCode = _lastCalcStatusInfo!.ErrorCode;
                var msg = $"Failed to {taskName}, ErrorCode:{errorCode}";
                PrintMessage(msg, PrintLevel.Error);
                DialogService.Instance.ShowErrorCode(errorCode);

                throw new SystemAbortException();//取消剩余未执行的任务
            }
        }

        private async Task SubmitCalibrationResult(string protocolName, ICaliTaskViewModel caliTaskViewModel)
        {
            string taskName = $"Submit Result for {protocolName}";
            PrintMessage($"Request to {taskName} ...");

            //初始化状态变量
            DisableNextCommand(nameof(SubmitCalibrationResult));
            currentProgressInfoFormatter = ApplyCalibrationTemplateProgressInfo_DetailFormatter;
            isServiceStateFinished = false;
            lastAcqReconStatusArgs = null;
            _lastCalcStatusInfo = null;

            //2发起请求：开始生成校准表
            await RequestSubmitCalibrationResultAsync(caliTaskViewModel);
            if (caliTaskViewModel.CaliTaskState == CaliTaskState.Canceled)
            {
                throw new UserAbortException();//取消剩余未执行的任务
            }
            else if (caliTaskViewModel.CaliTaskState == CaliTaskState.Error)
            {
                //弹窗报告错误
                string errorCode = _lastCalcStatusInfo!.ErrorCode;
                var msg = $"Failed to {taskName}, ErrorCode:{errorCode}";
                PrintMessage(msg, PrintLevel.Error);
                DialogService.Instance.ShowErrorCode(errorCode);

                throw new SystemAbortException();//取消剩余未执行的任务
            }
        }

        private void TryChangePostOffsetFrames(ScanParam scanParam)
        {
            //if (scanParam.PostOffsetFrames < 1)
            {
                var original = scanParam.PostOffsetFrames;
                OffsetHelper.ChangePostOffsetFrames(scanParam);
                logWrapper.Info($"Changed for PostOffsetFrames by OffsetCalculator, from '{original} to '{scanParam.PostOffsetFrames}'");

                scanParam.PostOffsetFrames += 2;//额外增加2张，避免计算库动态计算的小于实际需要的
                logWrapper.Info($"Changed PostOffsetFrames by manually, from '{original} to '{scanParam.PostOffsetFrames}'");
            }
            //else
            //{
            //    logWrapper.Debug($"No changes for PostOffsetFrames '{scanParam.PostOffsetFrames}'");
            //}
        }

        private CaliTaskState TotCaliTaskState(CommandStatus commandStatus)
        {
            var caliTaskStatus = commandStatus switch
            {
                CommandStatus.Cancelled => CaliTaskState.Canceled,
                CommandStatus.Success => CaliTaskState.Success,
                CommandStatus.Failure => CaliTaskState.Error,
            };

            PrintMessage($"CommandStatus To CaliTaskStatus: {commandStatus} -> {caliTaskStatus}.");
            return caliTaskStatus;
        }

        #region move gantry

        private async Task MoveGantryBackToInitAsync(CancellationTokenSource cancellationTokenSource)
        {
            PrintMessage($"Request moving the gantry back to the initial position ...", PrintLevel.Warn);

            GantryParams gantryParams = new() { MoveMode = GantryMoveMode.BackToStart, Velocity = CONST_GantryVelocityDefault };
            var gantryResponse = MotionControlProxy.Instance.StartMoveGantry(gantryParams);
            //校验
            if (!gantryResponse.Status)
            {
                PrintMessage($"Failed to move the gantry back to the initial position.", PrintLevel.Warn);
                //可忽略机架不转
                //return false;
            }
            else
            {
                PrintMessage("Async wait Gantry Moving Back to Init");
                await GantryArrived(cancellationTokenSource, CONST_GantryInitPosition);
                PrintMessage($"Success to move the gantry back to the initial position.");
            }
        }

        private async Task<CommandResult> MoveGantryToTargetAsync(CancellationTokenSource cancellationTokenSource, uint targetPosition = CONST_GantryInitPosition)
        {
            CommandResult commandResult = CommandResult.DefaultSuccessResult;

            var currentPositionOfView = DeviceSystem.Instance.Gantry.Position / 100.0;
            var targetPositionOfView = targetPosition / 100.0;
            string bodyMsg = $"the gantry from '{currentPositionOfView}' to '{targetPositionOfView}'";
            PrintMessage($"Request moving {bodyMsg} ...", PrintLevel.Warn);

            GantryParams gantryParams = CreateGantryParams(targetPosition);
            var gantryResponse = MotionControlProxy.Instance.StartMoveGantry(gantryParams);
            //校验
            if (!gantryResponse.Status)
            {
                PrintMessage($"Failed to move {bodyMsg}.", PrintLevel.Warn);
                //可忽略机架不转
                //return false;
                commandResult.Status = CommandStatus.Failure;
            }
            else
            {
                PrintMessage($"Pending to move {bodyMsg}.");
                commandResult = await GantryArrived(cancellationTokenSource, targetPosition);
                PrintMessage($"Success to move {bodyMsg}.");
            }

            return commandResult;
        }

        /// <summary>
        /// 移动机架
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        private async Task MoveGantryAsync(CancellationTokenSource cancellationTokenSource, uint targetPosition)
        {
            string messagePart = $"the gantry to the position({targetPosition})";
            PrintMessage($"Request moving {messagePart}...", PrintLevel.Info);

            var gantryParams = new GantryParams()
            {
                MoveMode = GantryMoveMode.PositionMode,
                TargetPosition = targetPosition
            };
            var gantryResponse = MotionControlProxy.Instance.StartMoveGantry(gantryParams);
            //校验
            if (!gantryResponse.Status)
            {
                PrintMessage($"Failed to move {messagePart}.", PrintLevel.Info);
                //可忽略机架不转
                //return false;
            }
            else
            {
                PrintMessage($"Await gantry moving {messagePart}.");
                await GantryArrived(cancellationTokenSource, targetPosition);
                PrintMessage($"Success to move {messagePart}.");
            }
        }

        private GantryParams CreateGantryParams(uint targetPosition = CONST_GantryInitPosition)
        {
            return new()
            {
                MoveMode = (targetPosition == CONST_GantryInitPosition) ? GantryMoveMode.BackToStart : GantryMoveMode.PositionMode,
                TargetPosition = targetPosition,
                Velocity = CONST_GantryVelocityDefault// 600u
            };
        }

        /// <summary>
        /// 机架旋转到位的超时时间（ms）
        /// </summary>
        private static readonly uint CONST_GantryMoveTimeOut = 120 * 1000;

        /// <summary>
        /// 机架归零的极限位置6000(0.01)
        /// </summary>
        private const uint CONST_GantryInitPosition = 60 * 100;
        /// <summary>
        /// 0.01度/每秒
        /// </summary>
        private const uint CONST_GantryVelocityDefault = 600;
        /// <summary>


        private async Task<bool> StopMoveGantryAsync()
        {
            PrintMessage($"Command to Stop Move Gantry");
            var response = MotionControlProxy.Instance.StopMoveGantry();
            if (!response.Status)
            {
                PrintMessage($"Failed to Command to Stop Move Gantry");
            }

            //等待机械真正的停止下来，上面只是发出请求
            await Task.Delay(TimeSpan.FromSeconds(3));

            return response.Status;
        }
        private double ToGantryPositionForView(uint gantryPosition)
        {
            return gantryPosition / 100.0;//0.01度 转换为人类视角‘度’
        }

        private async Task<CommandResult> GantryArrived(CancellationTokenSource cancellationTokenSource, uint targetPosition)
        {
            CommandResult commandResult = new();

            var currentPosition = DeviceSystem.Instance.Gantry.Position;
            bool isToBigger = (targetPosition > currentPosition);
            logWrapper.Debug($"[{nameof(GantryArrived)}] isToBigger '{isToBigger}'");
            bool isArrived = false;

            int timeSpent = 0;
            uint noChangedCount = 0;
            uint noChangedMaxCount = 5;

            uint lastestChangedPosition = currentPosition;
            bool gantryResponse = false;

            var currentPositionForView = ToGantryPositionForView(currentPosition);
            var targetPositionForView = ToGantryPositionForView(targetPosition);
            while (timeSpent < CONST_GantryMoveTimeOut)
            {
                await Task.Delay(IntervalTimeForRefreshStatus);
                timeSpent += IntervalTimeForRefreshStatus;

                if (cancellationTokenSource.IsCancellationRequested)
                {
                    PrintMessage($"User cancelled moving Gantry to the target position '{currentPositionForView}'.");
                    commandResult.Status = CommandStatus.Cancelled;
                    break;
                }

                currentPosition = DeviceSystem.Instance.Gantry.Position;
                if (currentPosition != lastestChangedPosition)
                {
                    lastestChangedPosition = currentPosition;
                    noChangedCount = 0;//清零
                }
                else
                {
                    noChangedCount++;//开始计数，未发生变化的次数，累积一定次数后，可以判断不动了
                    logWrapper.Debug($"[{nameof(GantryArrived)}] no changed, and count '{noChangedCount}'");
                }

                PrintMessage($"Gantry arrived to the position '{ToGantryPositionForView(lastestChangedPosition)}', the target '{targetPositionForView}'");
                //回退（逆向）运动，latest - target：100,50,1,0，-1，判断标准：小于
                //正向运动，latest - target：-100,-50,-1,0，1

                isArrived = isToBigger
                    ? (lastestChangedPosition >= targetPosition)
                    : (lastestChangedPosition <= targetPosition);
                if (isArrived)
                {
                    PrintMessage($"Gantry arrived to the target position '{targetPositionForView}'");
                    break;
                }
                else if (noChangedCount >= noChangedMaxCount)
                {
                    PrintMessage($"Gantry position has not changed anymore for {noChangedMaxCount} times, so stop move.");
                    break;
                }
            }

            //超时还没有获得到位信号，需要主动下发停止运动命令；
            //否则，硬件系统在位置模式下，到位后自动停止
            if (timeSpent > CONST_GantryMoveTimeOut)
            {
                PrintMessage($"Timeout({CONST_GantryMoveTimeOut}s) to move gantry to the target position '{targetPositionForView}', so stop move.");

                commandResult.Status = CommandStatus.Failure;
            }

            PrintMessage($"Stoping to move gantry.");
            gantryResponse = await StopMoveGantryAsync();
            this._logger.Debug(ServiceCategory.AutoCali, $"End to StopMoveGantry, [Output] {gantryResponse}");

            if (!commandResult.Success())
            {
                var result = IsSameGantryPosition(lastestChangedPosition, targetPosition);// true;
                if (result)
                {
                    commandResult.Status = CommandStatus.Failure;

                    PrintMessage($"Gantry not arrived to the target position '{targetPositionForView}'.", PrintLevel.Error);
                }
            }

            return commandResult;
        }

        #endregion move gantry

        #region move table

        /// <summary>
        /// 距离转换系数：底层单位：微米um，上层UI显示为毫米mm，
        /// </summary>
        public static int DistanceUmToMmCeof = 1000;
        /// <summary>
        /// 用于示意对准模体精准位置的激光灯与探测器的距离
        /// 单位：微米um，默认273mm * 1000
        /// </summary>
        public static uint DistanceBetweenLaserLightAndDetector = (uint)SystemConfig.LaserConfig.Laser.Offset.Value;
        //(uint)(273 * DistanceUmToMmCeof);

        /// <summary>
        /// 使用激光灯对准实现精准摆模，需要系统内部自动移床一定距离D（激光灯对准线与探测器边缘的距离，单位mm）
        /// 距离D在每台设备上是固定的
        /// </summary>
        /// <param name="functionMode"></param>
        private async Task RequestMoveTableAfterAddFixDistance(ICaliTaskViewModel taskViewModel, CancellationTokenSource cancellationTokenSource)
        {
            //Micron2Millimeter()
            logWrapper.Debug($"[RequestMoveTableAfterAddFixDistance] Entering ...");

            //自动移床一段距离到探测器边缘
            uint currentTableHorizontalPositionFromDevice = (uint)Math.Abs(GetTableHorizontalPositionFromDevice());
            uint targetTableHorizontalPosition = currentTableHorizontalPositionFromDevice + DistanceBetweenLaserLightAndDetector;

            uint currentTableVerticalPosition = (uint)Math.Abs(GetTableVerticalPositionFromDevice());
            uint tableTargetVerticalPosition = currentTableHorizontalPositionFromDevice;

            PrintMessage($"Add the fix distance({DistanceBetweenLaserLightAndDetector}) " +
                $"between the laser line and the detector...");
            //PrintMessage($"Request to move the table position, horizontal: {tableHorizontalPosFromDevice} -> {tableTargetHorizontalPosition} " +
            //    $"vertical: {tableTargetVerticalPosition} -> {tableTargetVerticalPosition}");
            PrintMessage($"Request to move the table position, horizontal: {currentTableHorizontalPositionFromDevice} -> {targetTableHorizontalPosition} ");

            await AutoMoveTable(
                currentTableHorizontalPositionFromDevice, currentTableVerticalPosition,
                targetTableHorizontalPosition, tableTargetVerticalPosition, taskViewModel, cancellationTokenSource);

            //PrintMessage($"After moved table.");
            //uint newTableHorizontalPosFromDevice = (uint)Math.Abs(GetTableHorizontalPositionFromDevice());
            //uint newTableTargetVerticalPosition = (uint)Math.Abs(GetTableVerticalPositionFromDevice());

            logWrapper.Debug($"[RequestMoveTableAfterAddFixDistance] Exiting ...");
        }

        /// <summary>
        /// 自动移床到探测器边缘
        /// </summary>
        private async Task<bool> AutoMoveTable(
            uint currentTableHorizontalPosition, uint currentTableVerticalPosition,
            uint targetTableHorizontalPosition, uint targetTableVerticalPosition,
            ICaliTaskViewModel taskViewModel,
            CancellationTokenSource cancellationTokenSource)
        {

            bool result = false;
            //PrintMessage.PrintLoggerWarn($"Command to move the table inward by the distance({DistanceBetweenLaserLightAndDetector})...");
            //DataAcquisitionProxy.Instance.Initialize(new());
            //var gantryResponse = DataAcquisitionProxy.Instance.StartHorizontalMove(new TableBaseCategory() { HorizontalTargetPosition = targetPosition });
            //校验
            //if (!gantryResponse)

            var horizontalPosition = (int)((-1) * targetTableHorizontalPosition);
            var verticalPosition = (int)targetTableVerticalPosition;
            PrintMessage($"Auto move the table position, horizontal: {currentTableHorizontalPosition} -> {targetTableHorizontalPosition} ");

            isArrived = false;
            var commandResult = MotionControlProxy.Instance.StartMoveTable(new()
            {
                Direction = FacadeProxy.Models.MotionControl.Table.TableMoveDirection.Horizontal,
                HorizontalPosition = (uint)Math.Abs(horizontalPosition)
            });
            //DeviceSystem.Instance.Table.Arrived -= Table_Arrived;
            //DeviceSystem.Instance.Table.Arrived += Table_Arrived;
            await Task.Delay(5000);

            int positionArrivedCount = 0;
            //if (commandResult.status)
            {
                //PrintMessage($"Please Press the move button", PrintLevel.Warn);
                while (true)
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        PrintMessage($"User cancelled to move the table", PrintLevel.Warn);
                        taskViewModel.CaliTaskState = CaliTaskState.Canceled;
                        break;
                    }

                    await Task.Delay(1000);
                    //if (isArrived)
                    //{
                    //    break;
                    //}

                    var currentHorizontalPosition = DeviceSystem.Instance.Table.HorizontalPosition;
                    //var currentVerticalPosition = DeviceSystem.Instance.Table.VerticalPosition;
                    //PrintMessage($"Please Press the move button, now the table horizontal position({currentHorizontalPosition}) and vertical position({currentVerticalPosition})", PrintLevel.Warn);
                    //PrintMessage($"Please Press the move button, " +
                    //    $"Current value of the table horizontal position({currentHorizontalPosition})," +
                    //    $"target({targetTableHorizontalPosition})", PrintLevel.Warn);

                    if (Math.Abs(currentHorizontalPosition) >= targetTableHorizontalPosition - 500)
                    {
                        //if (positionArrivedCount++ < 5) continue;
                        PrintMessage($"Table arrived the target horizontal position({targetTableHorizontalPosition})");
                        //PrintMessage($"Please Release the move button", PrintLevel.Warn);
                        break;
                    }
                }
                result = true;
            }
            //else
            //{
            //    PrintMessage($"Failed to move the table", PrintLevel.Warn);
            //    //可忽略机架不转
            //    result = false;
            //}

            DeviceSystem.Instance.Table.Arrived -= Table_Arrived;
            return result;
        }

        private bool isArrived = false;

        private void Table_Arrived(object? sender, EventArgs e)
        {
            isArrived = true;
            PrintMessage($"Received signal that the table arrived.", PrintLevel.Warn);
            PrintMessage($"Please Release the move button", PrintLevel.Warn);
        }

        #endregion move table

        #region ScanPosition control

        private int GetTableHorizontalPositionFromDevice()
        {
            int horizontalPositionFromDevice = DeviceSystem.Instance.Table.HorizontalPosition;
            PrintMessage($"Got the table horizontal position({horizontalPositionFromDevice}) from device.");

            return horizontalPositionFromDevice;
        }

        private int GetTableVerticalPositionFromDevice()
        {
            int verticalPositionFromDevice = (int)DeviceSystem.Instance.Table.VerticalPosition;
            PrintMessage($"Got the table vertical position({verticalPositionFromDevice}) from device.");

            return verticalPositionFromDevice;
        }

        private uint GetGantryPositionFromDevice()
        {
            //获取当前床码值，通常是负值（负值坐标系，，绝对值越大代表离床的起始点越远）
            //比如， -3005（单位：0.1mm）；极限手动移床后可能为0，但不安全
            uint realtimeGantryPosition = DeviceSystem.Instance.Gantry.Position;//最小60度,*100=6000
            PrintMessage($"Got the gantry position({realtimeGantryPosition}) from device.");

            return realtimeGantryPosition;
        }

        /// <summary>
        /// 床的最小位置误差 30000 (单位：微米um）
        /// </summary>
        public static readonly int Table_Horizontal_Position_Min_Offset = 30000;

        /// <summary>
        /// 获取当前床的位置，并赋值给扫描参数中
        /// </summary>
        /// <param name="scanReconParam"></param>
        private void SetTablePositionFromDevice(ScanParam scanParam)
        {
            var scanLength = CalcScanLength(scanParam);

            this._logger.Debug(ServiceCategory.AutoCali, $"Entering {nameof(SetTablePositionFromDevice)}, the original ScanParam, " +
                $"ScanLength:{FormatTablePosition(scanLength)}," +
                $"TableStartPosition:{FormatTablePosition(scanParam.TableStartPosition)}," +
                $"TableEndPosition:{FormatTablePosition(scanParam.TableEndPosition)}," +
                $"ExposureStartPosition:{FormatTablePosition(scanParam.ExposureStartPosition)}," +
                $"ExposureEndPosition:{FormatTablePosition(scanParam.ExposureEndPosition)}");

            //获取当前床码值，通常是负值（负值坐标系，，绝对值越大代表离床的起始点越远）
            //比如， -3005（单位：0.1mm）；极限手动移床后可能为0，但不安全
            int tableHorizontalPosFromDevice = GetTableHorizontalPositionFromDevice();
            if (Math.Abs(tableHorizontalPosFromDevice) >= Table_Horizontal_Position_Min_Offset)
            {
                scanParam.TableStartPosition = tableHorizontalPosFromDevice;
            }
            else
            {
                scanParam.TableStartPosition = (-1) * Table_Horizontal_Position_Min_Offset;
                PrintMessage($"Safety Set to ScanParam.TableStartPosition:{FormatTablePosition(scanParam.TableStartPosition)} by the safety position.");
            }

            scanParam.TableEndPosition = (-1) * (Math.Abs(scanParam.TableStartPosition) + scanLength);

            scanParam.ExposureStartPosition = scanParam.TableStartPosition;
            scanParam.ExposureEndPosition = scanParam.TableEndPosition;

            this._logger.Debug(ServiceCategory.AutoCali, $"Leaving {nameof(SetTablePositionFromDevice)}, the latest ScanParam," +
                $"ScanLength:{FormatTablePosition(scanLength)}," +
                $"TableStartPosition:{FormatTablePosition(scanParam.TableStartPosition)}," +
                $"TableEndPosition:{FormatTablePosition(scanParam.TableEndPosition)}," +
                $"ExposureStartPosition:{FormatTablePosition(scanParam.ExposureStartPosition)}," +
                $"ExposureEndPosition:{FormatTablePosition(scanParam.ExposureEndPosition)}");
        }

        private int CalcScanLength(ScanParam scanParam)
        {
            var exposureStartPosition = Math.Abs(scanParam.ExposureStartPosition);// = 50 * 10;//0.1mm,500
            var exposureEndPosition = Math.Abs(scanParam.ExposureEndPosition);// = scanParam.ExposureStartPosition + 100 * 10;//0.1mm,1500
            var scanLength = Math.Abs(exposureEndPosition - exposureStartPosition);
            return scanLength;
        }

        public static readonly uint Max_GantryEndPosition = 540 * 100;
        public static readonly uint GantryMoveAccLength = 10 * 100;//加速距离10度 * 100（单位 0.01度）

        /// <summary>
        /// 扫描一个周期，对应机架旋转的角度默认1500（单位：0.01度）
        /// </summary>
        private uint GantryLengthPerCycle = 15 * 100;

        private uint CalcGantryMotionLength(double cycles)
        {
            //一圈15度（1500）,多圈
            var gantryMoveLength = cycles * GantryLengthPerCycle;
            var validGantryMoveLength = Math.Min(gantryMoveLength, Max_GantryEndPosition) + GantryMoveAccLength;
            return (uint)validGantryMoveLength;
        }

        /// <summary>
        /// 计算机架的运动参数
        /// </summary>
        /// <param name="scanParam"></param>
        private void CalcGantryMotionParam(ScanParam scanParam)
        {
            logWrapper.Debug($"[CalcGantryMotionParam] The input [ScanParam] {JsonConvert.SerializeObject(scanParam, Formatting.Indented)}");

            var framesPerCycle = scanParam.FramesPerCycle;
            var cycles = scanParam.TotalFrames * 1.0 / framesPerCycle;

            //机架的参数
            var gantryTime_Second = ConvertMicroToSecond(framesPerCycle * scanParam.FrameTime);
            var gantrySpeed = GantryLengthPerCycle * 1.0 / gantryTime_Second;//0.01度/秒，所以 度数*100
            scanParam.GantrySpeed = (uint)gantrySpeed;//=277
            scanParam.GantryAccelerationTime = 500 * 1000;//机架加速度需要设置，默认500ms

            uint realtimeGantryPosition = GetGantryPositionFromDevice(); //最小60度,*100=6000

            scanParam.GantryStartPosition = realtimeGantryPosition;// min:6000
            scanParam.GantryDirection = GantryDirection.Clockwise;

            var gantryMotionLength = CalcGantryMotionLength(cycles);
            scanParam.GantryEndPosition = scanParam.GantryStartPosition + gantryMotionLength;

            var gantryResult = new
            {
                gantryMotionLength,
                gantryTime_Second,
                gantrySpeed,
                scanParam.GantryStartPosition,
                scanParam.GantryEndPosition,
                scanParam.GantryDirection
            };
            var gantryInput = new
            {
                scanParam.TotalFrames,
                scanParam.FramesPerCycle,
                scanParam.FrameTime,
                realtimeGantryPosition
            };
            logWrapper.Debug($"[Calc Gantry] the input: {JsonConvert.SerializeObject(gantryInput)}");
            logWrapper.Debug($"[Calc Gantry] the output: {JsonConvert.SerializeObject(gantryResult)}");
        }

        private void CalcGantryMotionParamForDetectorOffset(ScanParam scanParam)
        {
            logWrapper.Debug($"[CalcGantryMotionParamForDetectorOffset] The input [ScanParam] {JsonConvert.SerializeObject(scanParam, Formatting.Indented)}");

            var framesPerCycle = scanParam.FramesPerCycle;
            var cycles = scanParam.TotalFrames * 1.0 / framesPerCycle;

            //机架的参数
            var gantryTime_Second = ConvertMicroToSecond(framesPerCycle * scanParam.FrameTime);

            var gantryMotionLength = scanParam.GantryEndPosition - scanParam.GantryStartPosition;// CalcGantryMotionLength(cycles);
            var gantrySpeed = gantryMotionLength * 1.0 / gantryTime_Second;//0.01度/秒
            scanParam.GantrySpeed = (uint)gantrySpeed;//=277
            //scanParam.GantryAccelerationTime = 500 * 1000;//机架加速度需要设置，默认500ms

            uint realtimeGantryPosition = GetGantryPositionFromDevice(); //最小60度,*100=6000

            scanParam.GantryStartPosition = realtimeGantryPosition;// min:6000
            scanParam.GantryDirection = GantryDirection.Clockwise;

            //var gantryMotionLength = gantryLength;// CalcGantryMotionLength(cycles);
            scanParam.GantryEndPosition = scanParam.GantryStartPosition + gantryMotionLength;

            var gantryResult = new
            {
                gantryMotionLength,
                gantryTime_Second,
                gantrySpeed,
                scanParam.GantryStartPosition,
                scanParam.GantryEndPosition,
                scanParam.GantryDirection
            };
            var gantryInput = new
            {
                scanParam.TotalFrames,
                scanParam.FramesPerCycle,
                scanParam.FrameTime,
                //realtimeGantryPosition
                scanParam.GantryStartPosition,
            };
            logWrapper.Debug($"[Calc Gantry] the input: {JsonConvert.SerializeObject(gantryInput)}");
            logWrapper.Debug($"[Calc Gantry] the output: {JsonConvert.SerializeObject(gantryResult)}");
        }

        private void CalcTableMotionParam(ScanParam scanParam)
        {
            //床的参数
            //logWrapper.Debug($"Calculated the gantry motion length({gantryMotionLength}).");

            var absExposureStartPosition = Math.Abs(scanParam.ExposureStartPosition);// = 50 * 10;//0.1mm,500
            var oldAbsExposureEndPosition = Math.Abs(scanParam.ExposureEndPosition);// = scanParam.ExposureStartPosition + 100 * 10;//0.1mm,1500
            var absScanLength = oldAbsExposureEndPosition - absExposureStartPosition; //0.1mm,FixTestData=1000

            //scanParam.TotalFrames = 2700;
            //var scanMilliseconds = scanParam.TotalFrames * 1.0 * 10 / 1000;//ms
            //scanParam.TableSpeed = (uint)tableSpeed;
            //double scanMilliseconds = scanParam.TotalFrames * 1.0 * 10 / 1000;//unit: s

            //床的开始位置(0.1mm)
            var tableMotionBuffer = GetTableMotionBuffer();// 20 * 10;//unit: 0.1mm, 床 加速或者减速的缓冲距离

            var newTableStartPosition = absExposureStartPosition - tableMotionBuffer;
            logWrapper.Debug($"Changed Table Start Pos: {absExposureStartPosition} -> {newTableStartPosition}");
            scanParam.TableStartPosition = newTableStartPosition;// scanParam.ExposureStartPosition - 10 * 10;//um

            //床的移动速度(0.1mm/s)
            double exposureScanTime_Second = ConvertMicroToSecond(scanParam.TotalFrames * scanParam.FrameTime);
            double tableSpeed_Second = absScanLength / exposureScanTime_Second;//um/s,  FixTestData=3700
            //扫描长度100mm，每圈540张，5圈，共2700张，对应床速3703um/s
            scanParam.TableSpeed = (uint)tableSpeed_Second;//um/s

            #region Calc by Standard Interface(LiYong supported)

            var cycleTimeByCalculated = CalcCycleTime(scanParam.FrameTime, scanParam.FramesPerCycle, SourceCountUsed);
            var tableSpeedByCalculated = CalcTableSpeed(cycleTimeByCalculated, scanParam.CollimatorZ, scanParam.Pitch);
            //扫描长度100mm，每圈540张，pitch=0.5，对应床速3697um/s
            var tableSpeedByCalculated_Second = tableSpeedByCalculated * 1000/*ms*/ * 1000/*s*/;//um/us -> um/s
            var newTableSpeed = (uint)tableSpeedByCalculated_Second;//um/s
            scanParam.TableSpeed = newTableSpeed;

            logWrapper.Debug($"Calculated the Table Speed by Standard Interface, [Output] TableSpeed:{newTableSpeed}, " +
                $"[Input] FrameTime:{scanParam.FrameTime}, FramesPerCycle:{scanParam.FramesPerCycle}" +
                $", SourceCount:{SourceCountUsed}, CollimatorZ:{scanParam.CollimatorZ}, Pitch:{scanParam.Pitch}");

            var scanTimeByCalculated = absScanLength * 1.0 / tableSpeedByCalculated;
            var cyclesByCalculated = scanTimeByCalculated / cycleTimeByCalculated;
            var framesByCalculated = (uint)(cyclesByCalculated * scanParam.FramesPerCycle);
            //logWrapper.Debug($"Calculated the Frames by Standard Interface, [Output] {framesByCalculated}, " +
            //    $"[Input] scanTimeByCalculated:{scanTimeByCalculated}, cyclesByCalculated:{cyclesByCalculated}");

            //var newTotalFrames = (uint)framesByCalculated;//
            //logWrapper.Debug($"Change ScanParam, TableSpeed:{scanParam.TableSpeed} -> {newTableSpeed}," +
            //    $"TotalFrames:{scanParam.TotalFrames} -> {newTotalFrames}");
            //scanParam.TotalFrames = newTotalFrames;

            //冗余20%，避免床速慢误差累积，比如，扫描2700张，下参3240
            //同时，下游ctbox有逻辑判断：扫描长度（100mm），或者总张数，任一满足即停止扫描
            //newTotalFrames = (uint)(newTotalFrames * 1.2);
            //logWrapper.Debug($"Change ScanParam, TableSpeed:{scanParam.TableSpeed} -> {newTableSpeed}," +
            //    $"TotalFrames:{scanParam.TotalFrames} -> {newTotalFrames}");
            //scanParam.TotalFrames = newTotalFrames;

            #endregion

            //床的结束位置(0.1mm)
            double postOffsetScanTime_Second = ConvertMicroToSecond(scanParam.PostOffsetFrames * scanParam.FrameTime);
            double postOffsetTableLength = tableSpeed_Second * postOffsetScanTime_Second;

            logWrapper.Debug($"postOffsetTableLength {postOffsetTableLength}");

            var tableExposureBuffer = GetTableMotionBuffer();//床 匀速曝光（含PostOffset）阶段由于速度误差累积，需要预留一段距离
            // 床 匀速曝光（含PostOffset）阶段由于速度误差累积，改由张数冗余来优先满足扫描长度
            //var tableExposureBuffer = 0;
            double newAbsExposureEndPosition = oldAbsExposureEndPosition + postOffsetTableLength + tableExposureBuffer;

            logWrapper.Debug($"newAbsExposureEndPosition {newAbsExposureEndPosition}" +
                $"= oldAbsExposureEndPosition {oldAbsExposureEndPosition}+ postOffsetTableLength{postOffsetTableLength}" +
                $" + tableExposureBuffer {tableExposureBuffer}");

            scanParam.ExposureEndPosition = (-1) * (int)(newAbsExposureEndPosition);
            scanParam.TableEndPosition = (int)(newAbsExposureEndPosition + tableMotionBuffer);// scanParam.ExposureEndPosition + 10 * 10;//0.1mm
            scanParam.TableDirection = TableDirection.In;

            logWrapper.Debug($"TableEndPosition {scanParam.TableEndPosition}= (int)(newAbsExposureEndPosition {newAbsExposureEndPosition}" +
                $" + tableMotionBuffer {tableMotionBuffer})");

            var tableInput = new
            {
                scanParam.ExposureStartPosition,
                scanParam.ExposureEndPosition,
                scanParam.FrameTime,
            };
            var tableResult = new
            {
                absScanLength,
                exposureScanTime_Second,
                tableSpeed_Second,
                postOffsetScanTime_Second,
                postOffsetTableLength,

                scanParam.ExposureStartPosition,
                scanParam.ExposureEndPosition,

                scanParam.TableStartPosition,
                scanParam.TableEndPosition,
                scanParam.TableSpeed,
            };
            logWrapper.Debug($"[Calc Table] the input: {JsonConvert.SerializeObject(tableInput)}");
            logWrapper.Debug($"[Calc Table] the output: {JsonConvert.SerializeObject(tableResult)}");

            logWrapper.Debug($"[CalcMotionParam] The output [ScanParam] {JsonConvert.SerializeObject(scanParam, Formatting.Indented)}");
        }

        /// <summary>
        /// 扫描使用源个数
        /// </summary>
        private static readonly uint SourceCountUsed = 1;
        private static readonly uint SliceWidth = 165;//um:  探测器长度47.52mm / 288 * 1000um
        private static readonly uint FullCollimatedSliceWidth = (uint)(47.2 * 1000);//um: 0.165mm * 1000 * 288

        /// <summary>
        /// 螺距系数100，沟通时说pitch=0.5，传参为50
        /// </summary>
        private static readonly uint PitchRatio = 100;
        private double CalcCycleTime(uint frameTime, uint framesPerCycle, uint sourceCount)
        {
            return (frameTime * framesPerCycle) * 1.0 / sourceCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frameTime"></param>
        /// <param name="framesPerCycle"></param>
        /// <param name="sourceCount"></param>
        /// <param name="collimatorOpenedColumns">探测器开口排数，288,242,256,144,128,0</param>
        /// <param name="pitchBasePercentage">基于百分制的螺距，比如50，对应螺距0.5</param>
        /// <returns></returns>
        private double CalcTableSpeed(double cycleTime, uint collimatorOpenedColumns, double pitchBasePercentage)
        {
            uint collimatedSliceWidth = collimatorOpenedColumns * SliceWidth;
            double validSliceWidth = collimatedSliceWidth * (pitchBasePercentage / PitchRatio);
            return validSliceWidth / cycleTime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frameTime"></param>
        /// <param name="framesPerCycle"></param>
        /// <param name="sourceCount"></param>
        /// <param name="collimatorOpenedColumns">探测器开口排数，288,242,256,144,128,0</param>
        /// <param name="pitch"></param>
        /// <returns></returns>
        private double CalcTableSpeed(uint frameTime, uint framesPerCycle, uint sourceCount, uint collimatorOpenedColumns, double pitch)
        {
            double cycleTime = CalcCycleTime(frameTime, framesPerCycle, sourceCount);
            return CalcTableSpeed(cycleTime, collimatorOpenedColumns, pitch);
        }

        private int GetTableMotionBuffer()
        {
            return 20 * DistanceUmToMmCeof;//um
        }
        /// <summary>
        /// 微秒 转换成 秒
        /// </summary>
        /// <param name="microSecond"></param>
        /// <returns></returns>
        private double ConvertMicroToSecond(uint microSecond)
        {
            return microSecond / 1000.0 / 1000;
        }

        /// <summary>
        /// 根据统一接口，计算床的控制参数，比如床速，SmallAngleDeleteLength
        /// </summary>
        /// <param name="scanParam"></param>
        /// <returns></returns>
        private TableControlOutput GetTableControlOutput(ScanParam scanParam)
        {
            scanParam.CollimatorSliceWidth = CommonCalcuteHelper.GetCollimatorSliceWidth(scanParam.CollimatorZ);
            scanParam.TableAcceleration = CONST_TableAcceleration_Default;//mm/s

            var tableControlInput = ToTableControlInput(scanParam);
            logWrapper.Debug($"[{nameof(GetTableControlOutput)}][TableControlInput] {JsonConvert.SerializeObject(tableControlInput, Formatting.Indented)}");

            IScanTableCalculator scanTableCalculator = (scanParam.ScanOption == ScanOption.Axial)
                ? (new AxialTableCalculator())
                : (new HelicTableCalculator());
            logWrapper.Debug($"[{nameof(GetTableControlOutput)}] Used IScanTableCalculator({scanTableCalculator.GetType().Name})");

            var tableControlOutput = scanTableCalculator.CalculateTableControlInfo(tableControlInput);
            logWrapper.Debug($"[{nameof(GetTableControlOutput)}][TableControlOutput] {JsonConvert.SerializeObject(tableControlOutput, Formatting.Indented)}");

            return tableControlOutput;
        }
        private void SetMotionPositionByAlgCalc(ScanParam scanParam)
        {
            scanParam.CollimatorSliceWidth = (uint)(scanParam.CollimatorZ * 0.165 * 1000);//1 slice = 0.165mm, 看准直器开度CollimatorOpenWidth
            scanParam.TableAcceleration = CONST_TableAcceleration_Default;//mm/s

            var tableControlInput = ToTableControlInput(scanParam);
            logWrapper.Debug($"[TableControlInput] {JsonConvert.SerializeObject(tableControlInput, Formatting.Indented)}");

            var tableControlOutput = (new HelicTableCalculator()).CalculateTableControlInfo(tableControlInput);
            logWrapper.Debug($"[TableControlOutput] {JsonConvert.SerializeObject(tableControlOutput, Formatting.Indented)}");

            var gantryControlInput = ToGantryControlInput(scanParam, tableControlOutput);
            logWrapper.Debug($"[GantryControlInput] {JsonConvert.SerializeObject(gantryControlInput, Formatting.Indented)}");

            var gantryControlOutput = (new HelicGantryCalculator()).GetGantryControlOutput(gantryControlInput);
            logWrapper.Debug($"[GantryControlOutput] {JsonConvert.SerializeObject(gantryControlOutput, Formatting.Indented)}");

            ///////////////////////////////////
            //获取当前床码值，通常是负值（负值坐标系，，绝对值越大代表离床的起始点越远）
            //比如， -3005（单位：0.1mm）；极限手动移床后可能为0，但不安全
            scanParam.GantryStartPosition = (uint)gantryControlOutput.GantryStartPos;// 6010;// realtimeGantryPosition;
            scanParam.GantryEndPosition = (uint)gantryControlOutput.GantryEndPos;// 500 * 100;
            scanParam.GantryDirection = GantryDirection.Clockwise;
            var framesPerCycle = scanParam.FramesPerCycle;
            //var gantryTime = 540 /*frames*/ * 10/*ms*/ / 1000;
            var gantryTime = 1.0 * framesPerCycle * (scanParam.FrameTime / 1000) / 1000;//us -> ms -> s, so "x/1000/1000"
            var gantrySpeed = gantryControlOutput.GantrySpeed;// 15 * 100.0 / gantryTime;//0.01度/秒，所以 度数*100
            scanParam.GantrySpeed = (uint)gantryControlOutput.GantrySpeed;// (int)gantrySpeed;

            logWrapper.Debug($"[framesPerCycle]{framesPerCycle}, [FrameTime]{scanParam.FrameTime}," +
                $" -> [gantryTime]{gantryTime},[gantrySpeed-manualCalc]{gantrySpeed}, [GantrySpeed-ByAlgCalculator]{scanParam.GantrySpeed}");

            scanParam.ExposureStartPosition = (int)tableControlOutput.ReconVolumeBeginPos;// 50 * 10;//0.1mm
            scanParam.ExposureEndPosition = (int)tableControlOutput.ReconVolumeEndPos;// scanParam.ExposureStartPosition + 100 * 10;//0.1mm

            scanParam.TableStartPosition = (int)tableControlOutput.TableBeginPos;// scanParam.ExposureStartPosition - 10 * 10;//0.1mm
            scanParam.TableEndPosition = (int)tableControlOutput.TableEndPos;// scanParam.ExposureEndPosition + 10 * 10;//0.1mm
            scanParam.TableDirection = TableDirection.In;

            // scanParam.TotalFrames = 2700;
            var tableTime = scanParam.TotalFrames * 1.0 * 10 / 1000;//ms
            var scanLength = scanParam.ExposureEndPosition - scanParam.ExposureStartPosition;
            var tableSpeed = scanLength * 1.0 / tableTime;// * 10;//0.1mm,所以*100
            scanParam.TableSpeed = (uint)tableControlOutput.TableSpeed;// (uint)tableSpeed;

            logWrapper.Debug($"[TotalFrames]{scanParam.TotalFrames}, [FrameTime]{scanParam.FrameTime}," +
                $" -> [tableTime]{tableTime},[tableSpeed-manualCalc]{tableSpeed}, [TableSpeed-ByAlgCalculator]{scanParam.TableSpeed}");

            logWrapper.Debug($"[ScanParameter] {JsonConvert.SerializeObject(scanParam, Formatting.Indented)}");
        }

        /// <summary>
        /// 当前设备的射线源 最大个数
        /// </summary>
        public static readonly int CONST_SourceCount = 24;

        /// <summary>
        /// 床加速度，默认值270mm/s
        /// </summary>
        public static readonly uint CONST_TableAcceleration_Default = 270;//mm/s

        private TableControlInput ToTableControlInput(ScanParam scan)
        {
            TableControlInput tableControlInput = new TableControlInput();
            tableControlInput.ScanMode = scan.ScanMode;
            tableControlInput.ScanOption = scan.ScanOption;
            tableControlInput.Pitch = scan.Pitch / 100.0;
            tableControlInput.FramesPerCycle = (int)scan.FramesPerCycle;
            tableControlInput.ExposureTime = scan.ExposureTime;
            tableControlInput.ExpSourceCount = (int)scan.ExposureMode;
            tableControlInput.FrameTime = scan.FrameTime;

            tableControlInput.CollimatorZ = (int)scan.CollimatorZ;
            tableControlInput.CollimatedSliceWidth = scan.CollimatorSliceWidth;//微米um

            //todo:以后要从配置文件里读出来
            //tableControlInput.FullSliceWidth = 475.2;
            tableControlInput.PreDeleteRatio = CommonCalcuteHelper.CONST_PreDeleteRatio;
            tableControlInput.ObjectFov = CommonCalcuteHelper.GetObjectFov(scan.BodyPart);

            tableControlInput.PreIgnoredFrames = (int)scan.AutoDeleteNum;
            tableControlInput.ReconVolumeBeginPos = scan.ReconVolumeStartPosition;// / 100.0;
            tableControlInput.ReconVolumeEndPos = scan.ReconVolumeEndPosition;// / 100.0;
            tableControlInput.TableAcc = scan.TableAcceleration / 100.0;
            tableControlInput.TableFeed = scan.TableFeed / 100.0;
            tableControlInput.TableDirection = scan.TableDirection;
            tableControlInput.TotalSourceCount = CONST_SourceCount;

            return tableControlInput;
        }

        /// <summary>
        /// 根据扫描参数跟床参数获取机架参数列表
        /// </summary>
        /// <param name="scan">扫描参数</param>
        /// <param name="tableControlOutput">床参数</param>
        /// <returns>机架参数列表</returns>
        private GantryControlInput ToGantryControlInput(ScanParam scan, TableControlOutput tableControlOutput)
        {
            GantryControlInput gantryControlInput = new GantryControlInput();
            gantryControlInput.NumOfScan = tableControlOutput.NumOfScan;
            gantryControlInput.FramesPerCycle = scan.FramesPerCycle;

            var GetCurrentGantryPosition = () =>
            {
                return DeviceSystem.Instance.Gantry.Position;//0.01度，即60度=6000
            };

            gantryControlInput.CurrentGantryPos = GetCurrentGantryPosition();// _tablePositionService.CurrentGantryPosition.Position;
            gantryControlInput.DataBeginPos = tableControlOutput.DataBeginPos;
            gantryControlInput.DataEndPos = tableControlOutput.DataEndPos;
            gantryControlInput.ExpSourceCount = (int)scan.ExposureMode;
            gantryControlInput.FramesPerCycle = scan.FramesPerCycle;
            gantryControlInput.FrameTime = scan.FrameTime;
            gantryControlInput.GantryAcc = scan.GantryAcceleration;
            gantryControlInput.TableSpeed = tableControlOutput.TableSpeed;
            gantryControlInput.TableFeed = scan.TableFeed / 100.0;
            gantryControlInput.TableAcc = scan.TableAcceleration / 100.0;
            //List<double> hc = new List<double>();
            //List<double> ot = new List<double>();
            ////foreach (var tube in _heatCapacityService.Current)
            ////{
            ////    hc.Add(tube.RaySource.HeatCapacity);
            ////    ot.Add(tube.RaySource.OilTemperature);
            ////}
            //gantryControlInput.HeatCaps = hc.ToArray();
            //gantryControlInput.OilTem = ot.ToArray();

            gantryControlInput.NumOfScan = tableControlOutput.NumOfScan;
            gantryControlInput.PreIgnoredN = (int)scan.AutoDeleteNum;
            gantryControlInput.ScanMode = scan.ScanMode;
            gantryControlInput.ScanOption = scan.ScanOption;

            gantryControlInput.TotalSourceCount = CONST_SourceCount;// _heatCapacityService.Current.Count;
            gantryControlInput.TubePositions = scan.TubePositions;
            logWrapper.Debug($"[GantryControlInput] {JsonConvert.SerializeObject(gantryControlInput, Formatting.Indented)}");

            return gantryControlInput;
        }

        /// <summary>
        /// 格式化床码值，比如-303.12mm
        /// 负数，单位：毫秒mm
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string FormatTablePosition(int value)
        {
            return $"{(-1) * Math.Abs(value) * 1.0f / DistanceUmToMmCeof} mm";
        }

        #endregion ScanPosition control

        public static readonly string Common_StudyInstanceUID = "StudyInstanceUID";
        public static readonly string Common_ScanUID = "ScanUID";
        public static readonly string Common_RawDataDirectory = "RawDataDirectory";
        public static readonly string Common_T03_RawDataDirectory = "T03_RawDataDirectory";

        private bool TryChangeScanParamsWhenDebugMode(ICaliTaskViewModel scanTaskVM, ScanReconParam scanReconParam)
        {
            if (!IsDebugMode)
                return false;

            var taskParams = scanTaskVM.TaskParams;
            if (taskParams == null || taskParams.Count < 1)
            {
                logWrapper.Debug($"Skip DebugMode and use ProductMode to Scan, due taskParams is empty when DebugMode");
                return false;
            }

            object newStudyInstanceUID = null;
            object newScanUID = null;
            object newRawDataDirectory = null;
            if (!taskParams.TryGetValue(Common_StudyInstanceUID, out newStudyInstanceUID) || string.IsNullOrEmpty(newStudyInstanceUID?.ToString())
                || !taskParams.TryGetValue(Common_ScanUID, out newScanUID) || string.IsNullOrEmpty(newScanUID?.ToString())
                || !taskParams.TryGetValue(Common_RawDataDirectory, out newRawDataDirectory) || string.IsNullOrEmpty(newRawDataDirectory?.ToString()))
            {
                logWrapper.Debug($"Skip DebugMode and use ProductMode to Scan, due taskParams hasn't valid item when DebugMode, " +
                        $"detail:[Common_StudyInstanceUID]={newStudyInstanceUID}, [Common_ScanUID]={newScanUID}, [Common_RawDataDirectory]={newRawDataDirectory}");
                return false;
            }

            logWrapper.Debug($"In DebugMode, manually change [scanReconParam.Study.StudyInstanceUID]=" +
                $"{scanReconParam.Study.StudyInstanceUID} --> {newStudyInstanceUID}");
            scanReconParam.Study.StudyInstanceUID = newStudyInstanceUID?.ToString();

            logWrapper.Debug($"In DebugMode, manually change [scanReconParam.ScanParameter.ScanUID]=" +
                $"{scanReconParam.ScanParameter.ScanUID} --> {newScanUID}");
            scanReconParam.ScanParameter.ScanUID = newScanUID?.ToString();

            logWrapper.Debug($"In DebugMode, manually change [scanReconParam.ScanParameter.RawDataDirectory]=" +
                $"{scanReconParam.ScanParameter.RawDataDirectory} --> {newRawDataDirectory}");
            scanReconParam.ScanParameter.RawDataDirectory = newRawDataDirectory?.ToString();

            return true;
        }

        private string FormatRequestError(AlgorithmCalculateStatusInfo calcStatusInfo)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"ErrorCode:{calcStatusInfo.ErrorCode}");
            sb.AppendLine($"ErrorInfo:{calcStatusInfo.ErrorInfo}");
            return sb.ToString();
        }

        private async Task<CommandResult> TryWaitServiceStateResultAsync(Func<CommandResult> funcIsFinished, string taskName, CommandResult commandResult, ICaliTaskViewModel caliTaskViewModel)
        {
            if (CommandStatus.Success != commandResult.Status)
            {
                caliTaskViewModel.IsCompleted = true;
                caliTaskViewModel.CaliTaskState = CaliTaskState.Error;

                PrintMessage($"recv service command result, Failed!!! ");
                PrintMessage(FormatCommandResultInfo(commandResult));
                PrintMessage($"cancelled the calibration scenario task.");
                return commandResult;
            }
            else
            {
                PrintMessage($"Send the service command successfully. ");
                //由（外部）设备实时状态事件通知，切换设备可用状态isAcqFinished，继续下一个任务或者收到取消任务的通知
                commandResult = await WaitServiceStateResultAsync(funcIsFinished, taskName);

                //更新TaskStatus
                UpdateTaskStatus(ref caliTaskViewModel, commandResult);

                if (!commandResult.Success())
                {
                    PrintMessage($"Execute the service command {commandResult.Status}. ");
                    return commandResult;
                }

                PrintMessage($"Execute the service command successfully. ");
                return commandResult;
            }
        }

        private void UpdateTaskStatus(ref ICaliTaskViewModel taskViewModel, CommandResult commandResult)
        {
            taskViewModel.IsCompleted = commandResult.Success();
            taskViewModel.CaliTaskState = commandResult.Status switch
            {
                CommandStatus.Cancelled => CaliTaskState.Canceled,
                CommandStatus.Failure => CaliTaskState.Error,
                _ => CaliTaskState.Success,
            };
        }

        /// <summary>
        /// 等待服务完成（比如，扫描可用了，收到acq结束信号，收到校准结束信号）或中途被用户主动取消
        /// 当服务状态标记为完成时，同时等待中途是否被用户主动取消；
        /// 当服务状态标记为完成时（由外界事件更新的），不再等待是否被用户主动取消；
        /// </summary>
        /// <param name="waittingFlag">是否等待服务完成的标识，由外界更新是否完成。需要使用ref，否则外界更新无法影响这里</param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private async Task<CommandResult> WaitServiceStateResultAsync(Func<CommandResult> funcIsFinished, string serviceName)
        {
            PrintMessage($"{serviceName} | WaitServiceStateResult, begin...");
            bool isCompleted = true;
            CommandResult commandResult = CommandResult.DefaultSuccessResult;
            int waittingTimes = 0;
            while (!commandResult.Success())
            {
                commandResult = funcIsFinished();

                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    PrintMessage($"cancelled to wait the state result of {serviceName}!!!");
                    isCompleted = false;
                    commandResult.Status = CommandStatus.Cancelled;
                    break;
                }

                if (++waittingTimes % 3 == 0)
                {
                    PrintMessage($"waitting the state result of {serviceName}, times: {waittingTimes / 3}");
                }

                await Task.Delay(1000);
            }

            PrintMessage($"{serviceName} | WaitServiceStateResult, end");
            //return isCompleted;
            return commandResult;
        }

        #region 校准服务

        private static readonly int IntervalTimeForRefreshStatus = 200;//ms,间隔一定时间刷新状态
        private static readonly int IntervalTimeForLogServiceStateHeartBeat = 5000;//ms,间隔一定时间轮询，记录服务状态（异步，持续的）
        private static readonly string CommandName_StartCalibrate = "StartCalibration";

        private async Task RequestStartCalibrateAsync(ICaliTaskViewModel taskViewModel, List<ScanReconParam> scanReconParamList)
        {
            logWrapper.Debug($"Beginning RequestStartCalibrateAsync");

            taskViewModel.IsCompleted = false;
            taskViewModel.CaliTaskState = CaliTaskState.Running;

            var commandResult = AutoCalibrationProxy.Instance.StartCalibration(scanReconParamList);

            //3.发起请求：开始xx（扫描/校准/...）
            if (CommandStatus.Success != commandResult.Status)
            {
                taskViewModel.IsCompleted = true;
                taskViewModel.CaliTaskState = CaliTaskState.Error;

                PrintMessage($"Recevied service command result, Failed!!! ");
                PrintMessage(FormatCommandResultInfo(commandResult));
                PrintMessage($"Cancelled the calibration scenario task.");

                logWrapper.Debug($"Ended RequestStartCalibrateAsync");
                return;
            }

            //服务端成功收到命令，并开始执行服务，异步持续返回进度
            PrintMessage($"Request '{CommandName_StartCalibrate}' Successfully, and Async receive the result ...");
            int waittingTimes = 0;
            string taskName = taskViewModel.Name;
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    PrintMessage($"User cancelled the task.");
                    taskViewModel.CaliTaskState = CaliTaskState.Canceled;

                    //用户主动取消了执行，系统需要通知服务代理的终止校准运行
                    RequestAbortCalibrate(taskViewModel);
                    break;
                }

                if (null != _lastCalcStatusInfo)
                {
                    //服务成功了
                    if (_lastCalcStatusInfo.RequestStatus == RequestStatus.Done)
                    {
                        taskViewModel.CaliTaskState = CaliTaskState.Success;
                        PrintMessage($"Computing the calibration result successfully.");
                        break;
                    }
                    else if (_lastCalcStatusInfo.RequestStatus == RequestStatus.Failed)
                    {
                        taskViewModel.CaliTaskState = CaliTaskState.Error;
                        PrintMessage($"Failed to computing the calibration result!");//todo：应该记录 校准【xx】失败了
                        break;
                    }
                }

                TryLogServiceStateHeartBeat(CommandName_StartCalibrate, ++waittingTimes);
                await Task.Delay(1000);
            }

            logWrapper.Debug($"Ended RequestStartCalibrateAsync");
        }

        private void RequestAbortCalibrate(ICaliTaskViewModel taskViewModel)
        {
            taskViewModel.CaliTaskState = CaliTaskState.Canceled;

            var commandResult = AutoCalibrationProxy.Instance.AbortCalibration(new AbortOperation(AbortCause.UserAbort, false));
            if (CommandStatus.Success != commandResult?.Status)
            {
                PrintMessage($"recv service command result, Failed!!! ");
                PrintMessage(FormatCommandResultInfo(commandResult!));
            }
            else
            {
                PrintMessage($"recv service command result, Success. ");
            }
        }

        private async Task RequestSubmitCalibrationResultAsync(ICaliTaskViewModel taskViewModel)
        {
            logWrapper.Debug($"Beginning RequestSubmitCalibrationResultAsync");

            taskViewModel.IsCompleted = false;
            taskViewModel.CaliTaskState = CaliTaskState.Running;

            var commandResult = AutoCalibrationProxy.Instance.ApplyCalibrationResult();

            //3.发起请求：开始xx（扫描/校准/...）
            if (CommandStatus.Success != commandResult.Status)
            {
                taskViewModel.IsCompleted = true;
                taskViewModel.CaliTaskState = CaliTaskState.Error;

                PrintMessage($"Send the command 'ApplyCalibrationResult', Failed!");
                PrintMessage(FormatCommandResultInfo(commandResult));
                PrintMessage($"Cancelled the calibration scenario task.");

                logWrapper.Debug($"Ended RequestSubmitCalibrationResultAsync");
                return;
            }

            //服务端成功收到命令，并开始执行服务，异步持续返回进度
            PrintMessage($"Request 'ApplyCalibrationResult' Successfully, and Async receive the service result ...");
            int waittingTimes = 0;
            string taskName = taskViewModel.Name;
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    PrintMessage($"User cancelled the task.");
                    taskViewModel.CaliTaskState = CaliTaskState.Canceled;

                    ////用户主动取消了执行，系统需要通知服务代理的终止校准运行
                    //RequestAbortCalibrate(taskViewModel);
                    break;
                }

                if (null != _lastCalcStatusInfo)
                {
                    //服务成功了
                    if (_lastCalcStatusInfo.RequestStatus == RequestStatus.Done)
                    {
                        taskViewModel.CaliTaskState = CaliTaskState.Success;
                        PrintMessage($"Successfully to submit the calibration result.");
                        break;
                    }
                    else if (_lastCalcStatusInfo.RequestStatus == RequestStatus.Failed)
                    {
                        taskViewModel.CaliTaskState = CaliTaskState.Error;
                        PrintMessage($"Failed to submit the calibration result!");//todo：应该记录 校准【xx】失败了
                        break;
                    }
                }

                TryLogServiceStateHeartBeat(CommandName_StartCalibrate, ++waittingTimes);
                await Task.Delay(1000);
            }

            logWrapper.Debug($"Ended RequestSubmitCalibrationResultAsync");
        }

        private async Task RequestServiceAsync(ICaliTaskViewModel taskViewModel, string commandName, Func<CommandResult> serviceFunc
            /*List<ScanReconParam> scanReconParamList*/)
        {
            logWrapper.Debug($"Beginning RequestStartCalibrateAsync");

            taskViewModel.IsCompleted = false;
            taskViewModel.CaliTaskState = CaliTaskState.Running;

            //var commandResult = AutoCalibrationProxy.Instance.StartCalibration(scanReconParamList);
            var commandResult = serviceFunc();

            //3.发起请求：开始xx（扫描/校准/...）
            if (CommandStatus.Success != commandResult.Status)
            {
                taskViewModel.IsCompleted = true;
                taskViewModel.CaliTaskState = CaliTaskState.Error;

                PrintMessage($"Recevied service command result, Failed!!! ");
                PrintMessage(FormatCommandResultInfo(commandResult));
                PrintMessage($"Cancelled the calibration scenario task.");

                logWrapper.Debug($"Ended RequestStartCalibrateAsync");
                return;
            }

            //服务端成功收到命令，并开始执行服务，异步持续返回进度
            PrintMessage($"Request '{commandName}' Successfully, and Async receive the service result ...");
            int waittingTimes = 0;
            string taskName = taskViewModel.Name;
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    PrintMessage($"User cancelled the task.");
                    taskViewModel.CaliTaskState = CaliTaskState.Canceled;

                    //用户主动取消了执行，系统需要通知服务代理的终止校准运行
                    RequestAbortCalibrate(taskViewModel);
                    break;
                }

                if (null != _lastCalcStatusInfo)
                {
                    //服务成功了
                    if (_lastCalcStatusInfo.RequestStatus == RequestStatus.Done)
                    {
                        taskViewModel.CaliTaskState = CaliTaskState.Success;
                        PrintMessage($"Successfully to generate the calibration result.");
                        break;
                    }
                    else if (_lastCalcStatusInfo.RequestStatus == RequestStatus.Failed)
                    {
                        taskViewModel.CaliTaskState = CaliTaskState.Error;
                        PrintMessage($"Failed to generate the calibration result!");//todo：应该记录 校准【xx】失败了
                        break;
                    }
                }

                TryLogServiceStateHeartBeat(CommandName_StartCalibrate, ++waittingTimes);
                await Task.Delay(1000);
            }

            logWrapper.Debug($"Ended RequestStartCalibrateAsync");
        }

        #endregion 校准服务

        #region 扫描服务

        /// <summary>
        /// 发起请求：开始扫描，然后（异步）等待状态
        /// </summary>
        /// <param name="taskViewModel"></param>
        /// <param name="scanReconParam"></param>
        /// <returns></returns>
        private async Task RequestStartScanAsync(ICaliTaskViewModel taskViewModel, ScanReconParam scanReconParam)
        {
            logWrapper.Debug($"Beginning RequestStartScanAsync");

            CleanBeforeRequestStartScan(taskViewModel);

            UpdateTotalScanCount(scanReconParam);

            var commandResult = AcqReconProxy.Instance.StartScan(new List<ScanReconParam> { scanReconParam });

            string commandName = "StartScan";
            //3.发起请求：开始扫描
            if (CommandStatus.Success != commandResult.Status)
            {
                taskViewModel.IsCompleted = true;
                taskViewModel.CaliTaskState = CaliTaskState.Error;

                PrintMessage($"Failed to receive the response of service command:{commandName}! ");
                PrintMessage(FormatCommandResultInfo(commandResult));

                var Service_Module_Name = "StandardScan";
                var errorCode = commandResult.ErrorCodes?.Codes?.FirstOrDefault();
                PrintMessage($"[{Service_Module_Name}] Failed to start data acquisition with [ErrorCode] {errorCode}.", PrintLevel.Error);
                DialogService.Instance.ShowErrorCode(errorCode);

                PrintMessage($"Cancelled the calibration scenario task.");

                logWrapper.Debug($"Ended RequestStartScanAsync");
                return;
            }

            //服务端成功收到命令，并开始扫描，异步持续返回进度
            PrintMessage($"Request '{commandName}' Successfully, and Async receive the service result ...");
            int waittingTimes = 0;
            string taskName = taskViewModel.Name;

            var exposureDelaySecond = ConvertMicroToSecond(scanReconParam.ScanParameter.ExposureDelayTime);
            CountDownToRequestServiceAsync(_cancellationTokenSource, "Exposure", null, (int)exposureDelaySecond);
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    PrintMessage($"User cancelled the task({commandName})");
                    taskViewModel.CaliTaskState = CaliTaskState.Canceled;

                    //用户主动取消了扫描，系统需要通知服务代理的终止扫描
                    RequestAbortScan(taskViewModel);
                    break;
                }

                if (IsScanRecon_Failed())
                {
                    PrintMessage($"Failed to scan");
                    taskViewModel.CaliTaskState = CaliTaskState.Error;

                    //用户主动取消了扫描，系统需要通知服务代理的终止扫描
                    RequestAbortScan(taskViewModel);
                    break;
                }
                //扫描成功了
                else if (IsScanRecon_CompletedSuccessfully())
                {
                    taskViewModel.CaliTaskState = CaliTaskState.Success;
                    break;
                }

                TryLogServiceStateHeartBeat(commandName, ++waittingTimes);
                await Task.Delay(1000);
            }

            logWrapper.Debug($"Ended RequestStartScanAsync");
        }

        /// <summary>
        /// 发起请求：开始扫描，然后（异步）等待状态
        /// </summary>
        /// <param name="taskViewModel"></param>
        /// <param name="scanReconParam"></param>
        /// <returns></returns>
        private async Task StartFreeScanAsync(CaliScanTaskViewModel taskViewModel, ScanReconParam scanReconParam, GeneralArgProtocolViewModel protocolVM)
        {
            logWrapper.Debug($"Beginning RequestStartScanAsync");

            CleanBeforeRequestStartScan(taskViewModel);

            string commandName = "StartScan";

            ScanParam scanParam = scanReconParam.ScanParameter;

            FreeScanHandler handler;

            XRaySourceIndex xRaySourceIndex = protocolVM.XRaySourceIndex;
            if (XRaySourceIndex.All == xRaySourceIndex)
            {
                handler = new FreeScanHandler(this._logger, this._uiLogger);
            }
            else
            {
                handler = new SingleExposureFreeScanHandler(xRaySourceIndex, this._logger, this._uiLogger);
            }

            _logger.Debug(ServiceCategory.AutoCali, $"Used the handler '{handler.GetType().Name}'");
            var freeScan = handler.GetFreeScan(scanReconParam);
            if (scanParam.ExposureMode == ExposureMode.Twelve)
            {
                freeScan.XDataAcquisitionParameters.ExposureParams.TwelveExposureModeXRaySourceStartIndex = protocolVM.TwelveExposureModeXRaySourceStartIndex;
            }

            UpdateTotalScanCount(scanReconParam);
            bool success = freeScan.StartDataAcquisition(scanReconParam);
            if (success)
            {
                //服务端成功收到命令，并开始扫描，异步持续返回进度
                PrintMessage($"Request '{commandName}' Successfully, and Async receive the service result ...");
                int waittingTimes = 0;
                string taskName = taskViewModel.Name;

                //2.倒计时提示曝光延迟
                var exposureDelaySecond = ConvertMicroToSecond(scanParam.ExposureDelayTime);
                await CountDownToRequestServiceAsync(_cancellationTokenSource, "Exposure", null, (int)exposureDelaySecond);

                while (true)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        PrintMessage($"User cancelled the task.");
                        taskViewModel.CaliTaskState = CaliTaskState.Canceled;

                        //用户主动取消了扫描，系统需要通知服务代理的终止扫描
                        //RequestAbortScan(taskViewModel);
                        freeScan.StopDataAcquisitionCommand.Execute(scanReconParam);
                        break;
                    }

                    if (IsScanRecon_Failed())
                    {
                        taskViewModel.CaliTaskState = CaliTaskState.Error;
                        //系统需要通知服务代理的终止扫描
                        freeScan.StopDataAcquisitionCommand.Execute(scanReconParam);
                        break;
                    }
                    //扫描成功了
                    else if (IsScanRecon_CompletedSuccessfully())
                    {
                        taskViewModel.CaliTaskState = CaliTaskState.Success;
                        break;
                    }

                    TryLogServiceStateHeartBeat(commandName, ++waittingTimes);
                    await Task.Delay(IntervalTimeForRefreshStatus);
                }
            }
            else
            {
                taskViewModel.CaliTaskState = CaliTaskState.Error;
            }

            freeScan.UnRegisterProxyEvents();
            logWrapper.Debug($"Ended RequestStartScanAsync");
        }

        /// <summary>
        /// 临时功能，扫描前插入PreOffset扫描。同时满足 （配置）PreOffsetFramesBySoftware > 0 和 Kv > 0 时生效
        /// </summary>
        /// <param name="scanTaskVM"></param>
        /// <param name="scanReconParam"></param>
        /// <param name="preOffsetFrames"></param>
        /// <returns></returns>
        private async Task<Tuple<string, string>> AutoPreOffset(CaliScanTaskViewModel scanTaskVM, ScanReconParam scanReconParam, uint preOffsetFrames)
        {
            #region 临时功能，扫描前插入PreOffset扫描

            var scanParam = scanReconParam.ScanParameter;
            //uint preOffsetFrames = protocolVM.PreOffsetFramesBySoftware;
            bool IsPreOffsetBySoftware = preOffsetFrames > 0 && scanParam.kV[0] > 0;
            //bool isDark = scanParam.Voltage [0] == 0;
            string rawDataFolder_PreOffset_1 = scanParam.RawDataDirectory;
            string rawDataFolder_PreOffset_2 = scanParam.RawDataDirectory;
            //if (IsPreOffsetBySoftware)
            //{
            //    //uint preOffsetFrames = 32;

            //    #region 临时功能，扫描前插入PreOffset扫描

            //    //[ToDo]Remove, 临时功能，供算法测试手动PreOffset
            //    bool isGainDynamic = (Gain.Dynamic == scanParam.Gain || Gain.ForceLow == scanParam.Gain);
            //    bool isGainFixed = !isGainDynamic;

            //    //保留旧参数
            //    var oldFuncMode = scanReconParam.ScanParameter.FunctionMode;
            //    var oldRawDataType = scanParam.RawDataType;

            //    var oldKV = scanParam.kV[0];
            //    var oldGain = scanParam.Gain;
            //    var oldScanUid = scanParam.ScanUID;

            //    var oldScanOption = scanParam.ScanOption;
            //    var oldScanPositionEnd = scanParam.ExposureStartPosition;
            //    var oldFramesPerCycle = scanParam.FramesPerCycle;
            //    var oldExposureDelayTime = scanParam.ExposureDelayTime;

            //    var CreatePreOffsetScanUid = () =>
            //    {
            //        string newRawDataTypeStr = "_Pre_" + scanParam.RawDataType;
            //        scanParam.ScanUID = oldScanUid.Substring(0, 16 - newRawDataTypeStr.Length) + newRawDataTypeStr;
            //    };

            //    #endregion 临时功能，扫描前插入PreOffset扫描

            //    #region 临时功能，扫描前插入PreOffset扫描

            //    scanReconParam.ScanParameter.FunctionMode = FunctionMode.Cali_DynamicGain_Offset;
            //    scanParam.ScanOption = ScanOption.AXIAL;
            //    scanParam.ExposureEndPosition = scanParam.ExposureStartPosition - 472;//todo,有问题
            //    scanParam.FramesPerCycle = preOffsetFrames;
            //    scanParam.kV[0] = 0;
            //    scanParam.ExposureDelayTime = 5 * 1000 * 1000;//单位us，减少一点暗场扫描的延迟等待时间

            //    if (isGainDynamic)
            //    {
            //        scanParam.RawDataType = RawDataType.DarkH;
            //        scanParam.Gain = Gain.Dynamic;
            //        CreatePreOffsetScanUid();
            //        await StartFreeScanAsync(scanTaskVM, scanReconParam, preOffsetFrames);
            //        rawDataFolder_PreOffset_1 = scanParam.RawDataDirectory;

            //        scanParam.RawDataType = RawDataType.DarkL;
            //        scanParam.Gain = Gain.ForceLow;
            //        CreatePreOffsetScanUid();
            //        await StartFreeScanAsync(scanTaskVM, scanReconParam, preOffsetFrames);
            //        rawDataFolder_PreOffset_2 = scanParam.RawDataDirectory;
            //    }
            //    else if (isGainFixed)
            //    {
            //        scanParam.RawDataType = RawDataType.Dark;
            //        scanParam.Gain = oldGain;
            //        CreatePreOffsetScanUid();
            //        await StartFreeScanAsync(scanTaskVM, scanReconParam, preOffsetFrames);
            //        rawDataFolder_PreOffset_1 = scanParam.RawDataDirectory;
            //        rawDataFolder_PreOffset_2 = string.Empty;
            //    }

            //    //恢复参数
            //    scanReconParam.ScanParameter.FunctionMode = oldFuncMode;
            //    scanParam.RawDataType = oldRawDataType;

            //    scanParam.kV[0] = oldKV;
            //    scanParam.Gain = oldGain;
            //    scanParam.ScanUID = oldScanUid;

            //    scanParam.ScanOption = oldScanOption;
            //    scanParam.ExposureEndPosition = oldScanPositionEnd;
            //    scanParam.FramesPerCycle = oldFramesPerCycle;
            //    scanParam.ExposureDelayTime = oldExposureDelayTime;

            //    #endregion 临时功能，扫描前插入PreOffset扫描
            //}

            #endregion 临时功能，扫描前插入PreOffset扫描

            return new(rawDataFolder_PreOffset_1, rawDataFolder_PreOffset_2);
        }

        private void TryLogServiceStateHeartBeat(string commandName, int times)
        {
            if (times % IntervalTimeForLogServiceStateHeartBeat == 0)
            {
                logWrapper.Info($"Async receive the service state for {commandName}, times: {times}");
            }
        }

        private void CleanBeforeRequestStartScan(ICaliTaskViewModel taskViewModel)
        {
            taskViewModel.IsCompleted = false;
            taskViewModel.CaliTaskState = CaliTaskState.Running;

            lastAcqReconStatusArgs = null;
        }

        private void RequestAbortScan(ICaliTaskViewModel taskViewModel)
        {
            //taskName = $"AbortScan, arg protocols[{scanIndex}] = {scanUID}";
            PrintMessage($"Send command: {taskViewModel.Name}");
            var commandResult = AcqReconProxy.Instance.AbortScan(new AbortOperation(AbortCause.UserAbort, false));
            if (CommandStatus.Success != commandResult?.Status)
            {
                PrintMessage($"Abort Scan Failed! Details are as follows.");
                PrintMessage(FormatCommandResultInfo(commandResult));
            }
            else
            {
                PrintMessage($"Abort Scan Successfully!");
                taskViewModel.CaliTaskState = CaliTaskState.Canceled;
            }
        }

        #endregion 扫描服务

        #region 检测设备是否可扫描，轮询重试多次（默认15次）

        private static readonly int RetryCount_Max = 15;

        /// <summary>
        /// 检测设备是否可扫描，轮询重试多次（默认5次）
        /// </summary>
        /// <param name="taskViewModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task CheckScanIsEnabledAsync(ICaliTaskViewModel taskViewModel, CancellationTokenSource cancellationToken)
        {
            PrintMessage($"Check whether the device is scannable ...");

            int loopCount = 1;
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    //用户主动取消了任务
                    PrintMessage($"The user cancelled the task when check scan whether enabled.");
                    taskViewModel.CaliTaskState = CaliTaskState.Canceled;
                    break;
                }

                //当前设备实时状态curRealTimeStatus，由代理服务提供的事件OnRealTimeChanged，不断吐发消息
                if (IsScanEnabled())
                {
                    //确认设备为可扫描状态，跳出检测轮询
                    PrintMessage("The device has been detected to be scannable.");
                    break;
                }

                if (loopCount >= RetryCount_Max)
                {
                    //重试多次，依然没有等到可扫描状态，重设任务状态为被动取消
                    PrintMessage($"The system passively cancelled the task because the device cannot be scannable and the retry count is exceeded [{RetryCount_Max}]!");
                    taskViewModel.CaliTaskState = CaliTaskState.Canceled;
                    break;
                }
                PrintMessage($"The device is not scannable, will retry for the {++loopCount}th time in 1 second.");
                await Task.Delay(1000);//间隔1000ms
            }
        }

        /// <summary>
        /// <summary>
        /// 倒计时请求开始扫描，给用户反悔的计划，用户可随时终止
        /// </summary>
        /// <param name="taskViewModel"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="taskName"></param>
        /// <param name="countDownMaxNum">倒计时个数</param>
        /// <returns></returns>
        private async Task CountDownToRequestServiceAsync(CancellationTokenSource cancellationToken, string taskName, Action cancelCallback, int countDownNum = 5)
        {
            while (countDownNum > 0)
            {
                PrintMessage($"{taskName} will start in {countDownNum}s");

                if (cancellationToken.IsCancellationRequested)
                {
                    //用户主动取消了任务
                    PrintMessage($"User cancelled to {taskName}.");
                    //taskViewModel.CaliTaskState = CaliTaskState.Canceled;
                    cancelCallback?.Invoke();
                    break;
                }

                await Task.Delay(1000);
                countDownNum--;
            }
        }

        /// <summary>
        /// 等待用户主动点击"下一步"
        /// </summary>
        /// <param name="taskViewModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        //private async Task PendingOnManualNext(ICaliTaskViewModel taskViewModel, CancellationTokenSource cancellationToken)
        //{
        //    PrintMessage($"You can go \"Next\"，or \"Stop\"");
        //    while (!goNextEvent.WaitOne(1000))
        //    {
        //        if (cancellationToken.IsCancellationRequested)
        //        {
        //            //用户主动取消了任务
        //            PrintMessage($"The user cancelled the task");
        //            taskViewModel.CaliTaskState = CaliTaskState.Canceled;
        //            break;
        //        }
        //    }
        //}

        #endregion 检测设备是否可扫描，轮询重试多次（默认15次）

        private void UpdateTaskStateCanceled(ICaliTaskViewModel taskViewModel)
        {
            if (taskViewModel == null || !taskViewModel.IsChecked || taskViewModel.IsCompleted)
            {
                return;
            }

            taskViewModel.IsCompleted = true;
            taskViewModel.CaliTaskState = CaliTaskState.Canceled;

            if (taskViewModel.SubTaskViewModels == null)
            {
                return;
            }

            foreach (ICaliTaskViewModel childTaskVM in taskViewModel.SubTaskViewModels)
            {
                UpdateTaskStateCanceled(childTaskVM);
            }
        }

        private void InitTaskState(IList<ICaliTaskViewModel> taskViewModels)
        {
            if (taskViewModels == null)
            {
                return;
            }

            foreach (var subTaskViewModel in taskViewModels)
            {
                InitTaskState(subTaskViewModel);
            }
        }

        private void InitTaskState(ICaliTaskViewModel taskViewModel)
        {
            InitTaskState(taskViewModel.SubTaskViewModels);

            taskViewModel.IsCompleted = false;
            taskViewModel.CaliTaskState = CaliTaskState.Created;
        }

        private static readonly LogWrapper logWrapper = new LogWrapper(ServiceCategory.AutoCali);

        private void OnCaliScenarioTaskFinished(CaliScenarioTaskViewModel caliScenarioTaskViewModel)
        {
            var caliScenario = caliScenarioTaskViewModel.Inner;
            //caliScenarioTaskViewModel.IsCompleted = true;

            var msg = $"Finished run the calibration scenario \"{caliScenario.Name}\"!";
            PrintMessage(msg);
            logWrapper.Debug(msg);

            Application.Current.Dispatcher.Invoke(delegate
            {
                DialogService.Instance.ShowInfo(msg);
            });

            caliScenarioTaskViewModel.CaliTaskEnded?.Invoke(this, null);//通知外界，任务结束了（正常完成/用户取消/错误终止）
        }

        private void OnCaliScenarioTaskCanceled(CaliScenarioTaskViewModel caliScenarioTaskViewModel, Exception exception)
        {
            //UpdateTaskStateCanceled(caliScenarioTaskViewModel);//已经由外部更新了状态

            var caliScenarioName = caliScenarioTaskViewModel.Inner.Name;
            string source = (exception is UserAbortException) ? "by user" : "by system error";
            var msg = $"Cancelled to execute the calibration scenario '{caliScenarioName}' {source}.";
            PrintMessage(msg);
            caliScenarioTaskViewModel.IsCompleted = true;

            Task.Run(() =>
            {
                Task.Delay(1000);//延迟1s，避免早于MCS因硬件错误弹窗，导致弹窗被重叠可能造成活动窗口在底层无法选中并操作

                Application.Current.Dispatcher.Invoke(delegate
                {
                    DialogService.Instance.ShowWarning(msg);
                });
            });

            caliScenarioTaskViewModel.CaliTaskEnded?.Invoke(this, null);//通知外界，任务结束了（正常完成/用户取消/错误终止）
        }

        #region 实时 注册 / 取消注册 采集事件
        private void OnUnRegiterAcqEvent(CaliScenarioTaskViewModel caliScenarioTaskViewModel)
        {
            CaliService.Instance.UnRegisterAcqEvent(caliScenarioTaskViewModel);
        }
        private async void OnRegriterAcqEvent(CaliScenarioTaskViewModel caliScenarioTaskViewModel)
        {
            CaliService.Instance.RegisterAcqEvent(caliScenarioTaskViewModel);
            await Task.Delay(1000);
        }
        #endregion 实时 注册 / 取消注册 采集事件

        private void OnUnRegiterEvent(CaliScenarioTaskViewModel caliScenarioTaskViewModel)
        {
            //不再监听扫描采集事件
            OnUnRegiterAcqEvent(caliScenarioTaskViewModel);
            //不再监听校准事件
            OnUnRegisterCalibrationEvent(caliScenarioTaskViewModel);
        }

        #region 实时 注册 / 取消注册 校准事件
        private void OnUnRegisterCalibrationEvent(CaliScenarioTaskViewModel caliScenarioTaskViewModel)
        {
            CaliService.Instance.UnRegisterCalibrationEvent(caliScenarioTaskViewModel);
        }

        private async Task OnRegisterCalibrationEventAsync(CaliScenarioTaskViewModel caliScenarioTaskViewModel)
        {
            CaliService.Instance.RegisterCalibrationEvent(caliScenarioTaskViewModel);
            await Task.Delay(1000);
        }
        #endregion 实时 注册 / 取消注册 采集事件

        /// <summary>
        /// 中断正常流程，跳到重复运行流程
        /// </summary>
        /// <param name="caliItemTaskViewModel"></param>
        /// <exception cref="UserRepeatException"></exception>
        //private void BreakTaskForCaliItemByThrowException(CaliItemTaskViewModel caliItemTaskViewModel)
        private void BreakTaskForProtocolByThrowException(ICaliTaskViewModel caliTaskViewModel)
        {
            PrintMessage($"用户选择 重新运行 {caliTaskViewModel.Name}");

            //恢复运行前状态：已准备
            foreach (var taskViewModel in caliTaskViewModel.SubTaskViewModels)
            {
                taskViewModel.CaliTaskState = CaliTaskState.Created;
            }
            throw new UserRepeatException();            //return; //break;//回到当前校准项目，重新开始
        }

        /// <summary>
        /// （试图）提示用户确认阅读校准项目的注意事项
        /// </summary>
        /// <param name="caliItemTaskViewModel"></param>
        /// <returns></returns>
        private bool TryConfirmToRunTaskForCaliItem(CalibrationItem caliItem)
        {
            bool isContinue = true;

            if (string.IsNullOrEmpty(caliItem.Attention))
            {
                return isContinue;
            }

            var msg = $"在运行 校准项目：{caliItem.Name} 之前，请确认以下注意事项：\r\n{caliItem.Attention}";
            var boxResult = DialogService.Instance.ShowConfirm(msg);
            if (!boxResult)
            {
                PrintMessage($"用户取消 运行校准项目：{caliItem.Name}");
                CancelScenarioTask(this, null);
                isContinue = false;
                return isContinue;
            }
            else
            {
                PrintMessage($"用户确认 运行校准项目：{caliItem.Name}");
                msg = $"校准项目：{caliItem.Name}，开始运行...";
                PrintMessage(msg);
                return isContinue;
            }
        }

        #endregion private methods

        #region registe event

        internal void OnAcqReconConnectionChanged(object arg1, ConnectionStatusArgs arg2)
        {
            AcqReconConnected = arg2.Connected;

            string connectionInfo = GetModuleConnectionInfo(MSG_Module_AcqRecon, AcqReconConnected);
            PrintConnectionInfo(connectionInfo, AcqReconConnected);

            if (!AcqReconConnected)
            {
                PrintMessage($"System Stop due to {connectionInfo}", PrintLevel.Error);
                StopRefreshCalcProgress();

                //更新
                scanCommandResult = new() { Sender = nameof(OnAcqReconConnectionChanged), Status = CommandStatus.Failure };
            }
        }

        private static readonly string MSG_Module_AcqRecon = "MRS-AcqRecon";
        private static readonly string MSG_Module_Device = "Device";
        private static readonly string MSG_Disconnected = "disconnected";
        private static readonly string MSG_Connected = "connected";

        public bool AcqReconConnected { get; set; } = DeviceSystem.Instance.AcqReconConnected;

        private void PrintConnectionInfo(string connectionInfo, bool isConnected)
        {
            if (isConnected)
            {
                PrintMessage(connectionInfo);
            }
            else
            {
                PrintMessage(connectionInfo, PrintLevel.Error);
            }
        }
        private string GetModuleConnectionInfo(string module, bool isConnected)
        {
            string connectionMsg = isConnected ? MSG_Connected : MSG_Disconnected;
            return $"{module} {connectionMsg}";
        }

        CommandResult scanCommandResult = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnScanReconErrorOccurred(object? sender, ErrorInfoEventArgs e)
        {
            var errorCodes = e.ErrorCodes;
            string firstErrorCode = errorCodes?.Codes.FirstOrDefault();

            scanCommandResult = new() { Sender = nameof(OnScanReconErrorOccurred) };
            scanCommandResult.AddErrorCode(firstErrorCode);

            logWrapper.Error($"Error Source 'ScanReconErrorOccurred', details:{JsonSerializeHelper.ToJson(errorCodes)}, scanCommandResult.Status:{scanCommandResult.Status}");

            PrintMessage($"Received and Show the error '{firstErrorCode}'", PrintLevel.Error);
            DialogService.Instance.ShowErrorCode(firstErrorCode);
        }

        internal void OnDeviceConnectionChanged(object arg1, ConnectionStatusArgs arg2)
        {
            bool isConnected = arg2.Connected;
            string connectionInfo = GetModuleConnectionInfo(MSG_Module_Device, isConnected);
            PrintConnectionInfo(connectionInfo, isConnected);

            if (isConnected)
            {
                return;
            }

            //立即更新扫描命令的结果为失败
            scanCommandResult = new() { Sender = nameof(OnDeviceConnectionChanged), Status = CommandStatus.Failure };
        }

        internal void OnDeviceErrorOccurred(object? sender, ErrorInfoEventArgs e)
        {
            PrintMessage($"Received Device Error:{JsonSerializeHelper.ToJson(e.ErrorCodes)}", PrintLevel.Error);
            scanCommandResult = new() { Sender = nameof(OnDeviceErrorOccurred), Status = CommandStatus.Failure };
            scanCommandResult.ErrorCodes = e.ErrorCodes;
        }

        #region 校准计算状态更新
        private AlgorithmCalculateStatusInfo _lastCalcStatusInfo;
        private static readonly string CalibrateProgressInfo_DetailFormatter = "Computing, Progress: {0}, Status: {1}, Elapsed: {2}";
        private static readonly string ApplyCalibrationTemplateProgressInfo_DetailFormatter = "Submitting calibration templates, Progress: {0}, Status: {1}, Elapsed: {2}";

        private string currentProgressInfoFormatter = CalibrateProgressInfo_DetailFormatter;

        private CancellationTokenSource refreshCalcProgressCancellationToken;
        private DateTime startRefreshDateTime;
        private DateTime latestRefreshDateTime = DateTime.Now;

        /// <summary>
        /// 最近一次的服务（命令）执行结果
        /// </summary>
        private CommandResult _commandResult = CommandResult.DefaultSuccessResult;
        private CommandResult GetCommandResult(AlgorithmCalculateStatusInfo calcStatusInfo)
        {
            var requestStatus = calcStatusInfo.RequestStatus;
            if (requestStatus < RequestStatus.Done)
            {
                throw new InvalidOperationException($"Not support the operation 'GetCommandResult' when RequestStatus is '{requestStatus}'");
            }

            CommandResult commandResult = null;
            if (requestStatus == RequestStatus.Failed)
            {
                commandResult = new CommandResult();
                commandResult.Status = CommandStatus.Failure;
                commandResult.AddErrorCode(calcStatusInfo.ErrorCode);
            }
            else
            {
                commandResult = CommandResult.DefaultSuccessResult;
            }

            return commandResult;
        }


        internal void OnCalcStatusChanged(object arg1, AlgorithmCalculateStatusInfo calcStatusInfo)
        {
            latestRefreshDateTime = DateTime.Now;
            _lastCalcStatusInfo = calcStatusInfo;
            logWrapper.Info($"[CalcStatusChanged] {JsonSerializeHelper.ToJson(calcStatusInfo)}");

            //ByteString bytes = arg2.ResultData;
            //string resultDataFormatString = (bytes == null) ? string.Empty : Encoding.UTF8.GetString(bytes.ToArray());
            if (calcStatusInfo.RequestStatus > RequestStatus.Doing)
            {
                isServiceStateFinished = true;

                //终止定时更新的Task
                CancelRefreshCalcProgress();
            }

            if (calcStatusInfo.ProgressTotalNum == 0)
            {
                startRefreshDateTime = DateTime.Now;
                //终止定时更新的Task
                CancelRefreshCalcProgress();//终止可能存在的上一次定时刷新任务没有被主动停止
                StartIntervalRefreshCalcProgress();
            }
            PrintCalcProgress(calcStatusInfo);
        }

        private static readonly List<string> ERROR_CODES_IGNOREABLE = new() { "ALG001000001", "ALG001000002", "ALG001000003", "ALG001000004" };
        private void MarkAsNonFailed(AlgorithmCalculateStatusInfo calcStatusInfo)
        {
            if (ERROR_CODES_IGNOREABLE.Contains(calcStatusInfo.ErrorCode))
            {
                PrintMessage("");
            }
        }

        private void PrintCalcProgress(AlgorithmCalculateStatusInfo calcStatusInfo)
        {
            if (calcStatusInfo == null)
            {
                return;
            }

            _logger.Debug(ServiceCategory.AutoCali, $"calcStatusInfo#{calcStatusInfo.GetHashCode()}");

            double percent = (calcStatusInfo.ProgressTotalNum == 0) ? 0 : (calcStatusInfo.ProgressFinishNum * 1.0 / calcStatusInfo.ProgressTotalNum);
            if (percent > 0.99)
            {
                percent = 0.99;
            }
            if (calcStatusInfo.RequestStatus == RequestStatus.Done)
            {
                percent = 1;
            }
            string calcProgressInfo = string.Format("{0:P}", percent);//eg,  string.Format("{0:P}", 0.12354)， =》 12.35%

            TimeSpan duration = DateTime.Now - startRefreshDateTime;
            string durationInfo = $"{duration.Hours:00}:{duration.Minutes:00}:{duration.Seconds:00}";
            PrintMessage(string.Format(currentProgressInfoFormatter, calcProgressInfo, calcStatusInfo.RequestStatus, durationInfo));
        }

        private void CancelRefreshCalcProgress()
        {
            refreshCalcProgressCancellationToken?.Cancel();
        }

        private Task refreshCalcProgressTask = null;
        private readonly int ForceRefreshInterval = 30;//30s
        private void StartIntervalRefreshCalcProgress()
        {
            refreshCalcProgressCancellationToken = new CancellationTokenSource();
            if (refreshCalcProgressTask == null)
            {
                refreshCalcProgressTask = new Task(async () =>
                {
                    int msForceRefreshInterval = ForceRefreshInterval * 1000;
                    while (true)
                    {
                        _logger.Debug(ServiceCategory.AutoCali, $"loop to the task#{refreshCalcProgressCancellationToken.GetHashCode()}");

                        await Task.Delay(msForceRefreshInterval);
                        if (refreshCalcProgressCancellationToken.IsCancellationRequested)
                        {
                            _logger.Debug(ServiceCategory.AutoCali, $"Break the task of [Refresh Calclation Progress] due to CancellationRequested.");
                            break;
                        }

                        TimeSpan interval = DateTime.Now - latestRefreshDateTime;
                        if (interval.TotalSeconds < ForceRefreshInterval)
                        {
                            _logger.Debug(ServiceCategory.AutoCali, $"interval < {ForceRefreshInterval} in task#{refreshCalcProgressCancellationToken.GetHashCode()}");
                            continue;
                        }

                        _logger.Debug(ServiceCategory.AutoCali, $" DispatcherWrapper.Invoke  PrintCalcProgress");
                        //更新耗时,即使没有更新进度
                        DispatcherWrapper.Invoke(() =>
                        {
                            PrintCalcProgress(_lastCalcStatusInfo);
                        });
                    }
                    _logger.Debug(ServiceCategory.AutoCali, $"Ended the task#{refreshCalcProgressCancellationToken.GetHashCode()} to refresh Calibration Progress");

                    refreshCalcProgressTask = null;
                });

                refreshCalcProgressTask.Start();
            }
        }
        private void StopRefreshCalcProgress()
        {
            refreshCalcProgressCancellationToken?.Cancel();
        }
        #endregion 校准计算状态更新

        private void UpdateRawDataDirectory(string rawDataDirectory)
        {
            if (null == _lastScanReconParam?.ScanParameter)
            {
                logWrapper.Warn("_lastScanReconParam is Null");
                return;
            }

            _lastScanReconParam.ScanParameter.RawDataDirectory = rawDataDirectory;
        }

        internal void OnAcqReconStatusChanged(object arg1, AcqReconStatusArgs arg2)
        {
            var preAcqReconStatusArgs = lastAcqReconStatusArgs;
            lastAcqReconStatusArgs = arg2;

            //获取后端返回的生数据路径，并更新到最后一次扫描参数上
            UpdateRawDataDirectory(arg2.RawDataPath);

            logWrapper.Debug($"AcqReconStatus Changed: {preAcqReconStatusArgs?.Status} -> {lastAcqReconStatusArgs?.Status}");
            if (lastAcqReconStatusArgs?.Status == preAcqReconStatusArgs?.Status)
            {
                logWrapper.Warn("AcqReconStatus NOT Changed!");
                return;
            }

            if (arg2.Status <= AcqReconStatus.Reconning)
            {
                return;
            }

            if (arg2.Status == AcqReconStatus.Finished)
            {
                string rawDataDirectory = arg2.RawDataPath;
                PrintMessage($"Completed to ScanRecon");
                logWrapper.Debug($"Got the rawDataDirectory:{rawDataDirectory}");
            }
            else
            {
                PrintMessage($"Failed to ScanRecon ");
            }
        }

        ///// <summary>
        ///// 服务代理是否已完成采集重建处理（异步）
        ///// </summary>
        //private bool isFinishedAcqRecon = false;

        /// <summary>
        /// 最近一次使用的扫描参数
        /// </summary>
        private ScanReconParam _lastScanReconParam;

        internal void OnRealTimeStateChanged(object arg1, RealtimeEventArgs arg2)
        {
            var prevRealTimeState = curRealTimeStatus;
            curRealTimeStatus = arg2.Status;
            logWrapper.Debug($"Real-time Status: {prevRealTimeState} -> {curRealTimeStatus}");

            if (curRealTimeStatus == RealtimeStatus.EmergencyScanStopped)
            {
                //ScanReconHelper.AlertEmergencyScanStopped();
                PrintMessage($"Emergency Scan Stopped.");
            }
        }

        /// <summary>
        /// 判断扫描是否失败
        /// </summary>
        /// <returns></returns>
        private bool IsScanRecon_Failed()
        {
            //string msg = $"[IsScanRecon_Failed] {JsonSerializeHelper.ToJson(scanCommandResult)},";
            //PrintMessage(msg, PrintLevel.Info);

            bool isFailed = lastAcqReconStatusArgs?.Status == AcqReconStatus.Cancelled
                || lastAcqReconStatusArgs?.Status == AcqReconStatus.Error
                || curRealTimeStatus == RealtimeStatus.EmergencyScanStopped
                || scanCommandResult?.Status == CommandStatus.Failure;
            if (isFailed)
            {
                PrintMessage("Failed to Scan.", PrintLevel.Error);

                logWrapper.Error($"Scan failed. The clue is RealtimeStatus:{RealtimeStatus.EmergencyScanStopped}" +
                    $", AcqReconStatus:{lastAcqReconStatusArgs?.Status}," +
                    $", ErrorCode:{JsonSerializeHelper.ToJson(lastAcqReconStatusArgs?.Errors)}," +
                    $", scanCommandResult?.Status:{scanCommandResult?.Status}");
            }

            return isFailed;
        }

        private int WaitSecondsToMarkAsCompleted = 0;
        private int MaxWaitSecondsToMarkAsCompleted = 60;//60秒
        /// <summary>
        /// 判断扫描是否成功
        /// </summary>
        /// <returns></returns>
        private bool IsScanRecon_CompletedSuccessfully()
        {
            string method = nameof(IsScanRecon_CompletedSuccessfully);

            if (_lastRawImageSavedEventArgs == null)
            {
                logWrapper.Debug($"Oops! [{method}] _lastRawImageSavedEventArgs should not be null");
            }
            if (lastAcqReconStatusArgs == null)
            {
                logWrapper.Debug($"Oops! [{method}] lastAcqReconStatusArgs should not be null");
            }

            string scanUID = _lastRawImageSavedEventArgs?.ScanUID;
            //不是当前扫描的事件，忽略
            if (scanUID != CurrentScanUID)
            {
                return false;
            }

            bool isRawImageSavedCompleted = _lastRawImageSavedEventArgs?.IsFinished == true;
            bool isAcqReconCompleted = lastAcqReconStatusArgs?.Status == AcqReconStatus.Finished;
            if (isAcqReconCompleted || isRawImageSavedCompleted)
            {
                if (isAcqReconCompleted && isRawImageSavedCompleted)
                {
                    WaitSecondsToMarkAsCompleted = 0;//清零
                    logWrapper.Debug($"[{method}] [Result] true, [Input] EventScanUID:{scanUID}, CurrentScanUID:{CurrentScanUID}, isAcqReconCompleted:{isAcqReconCompleted}, isRawImageSavedCompleted:{isRawImageSavedCompleted}");
                    return true;
                }
                else
                {
                    if (WaitSecondsToMarkAsCompleted > MaxWaitSecondsToMarkAsCompleted)
                    {
                        logWrapper.Debug($"[{method}] [Result] true, [Input] ScanUID:{scanUID}, CurrentScanUID:{CurrentScanUID}, Mark As Completed After Waiting {WaitSecondsToMarkAsCompleted}s, isAcqReconCompleted:{isAcqReconCompleted}, isRawImageSavedCompleted:{isRawImageSavedCompleted}");

                        WaitSecondsToMarkAsCompleted = 0;//清零
                        return true;
                    }
                    else
                    {
                        logWrapper.Debug($"[{method}] Waiting {WaitSecondsToMarkAsCompleted}s for both completed, [Input] ScanUID:{scanUID},  isAcqReconCompleted:{isAcqReconCompleted}, isRawImageSavedCompleted:{isRawImageSavedCompleted}");
                        WaitSecondsToMarkAsCompleted++;
                    }
                }
            }

            return false;
        }

        private bool IsCalibrate_CompletedSuccessfully()
        {
            return _lastCalcStatusInfo?.RequestStatus == RequestStatus.Done;
        }

        private bool IsCalibrate_Completed()
        {
            return _lastCalcStatusInfo?.RequestStatus == RequestStatus.Done || _lastCalcStatusInfo?.RequestStatus == RequestStatus.Failed;
        }

        private bool IsCalibrate_Error()
        {
            return _lastCalcStatusInfo?.RequestStatus == RequestStatus.Failed;
        }

        private void UpdateTotalScanCount(ScanReconParam scanReconParam)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                uint totalFrames = scanReconParam.ScanParameter.TotalFrames;
                uint autoDeleteFrames = scanReconParam.ScanParameter.AutoDeleteNum;
                TotalScanCount = (int)(totalFrames - autoDeleteFrames);
                logWrapper.Debug($"[{nameof(UpdateTotalScanCount)}] Set TotalScanCount '{TotalScanCount}'");
            });
        }

        ///// <summary>
        ///// 服务代理是否已完成生数据的保存（异步）
        ///// </summary>
        private RawImageSavedEventArgs _lastRawImageSavedEventArgs;

        internal void OnRawImageSaved(object arg1, RawImageSavedEventArgs rawImageSavedEventArgs)
        {
            if (rawImageSavedEventArgs?.ScanUID != CurrentScanUID)
            {
                logWrapper.Debug($"[{nameof(OnRawImageSaved)}] Skip the event due to ScanUID '{rawImageSavedEventArgs?.ScanUID}'not match the target '{CurrentScanUID}'");
                return;
            }

            _lastRawImageSavedEventArgs = rawImageSavedEventArgs;

            //实时刷新扫描进度
            ScannedProgress = rawImageSavedEventArgs.FinishCount;
            if (_lastRawImageSavedEventArgs.IsFinished)
            {
                logWrapper.Debug($"[{nameof(OnRawImageSaved)}] Recev Finished Flag for Scan '{_lastRawImageSavedEventArgs.ScanUID}', and CurrentScanUid '{CurrentScanUID}'");
                logWrapper.Debug($"[{nameof(OnRawImageSaved)}] Temp Skip check whether same between Scan '{_lastRawImageSavedEventArgs.ScanUID}', and CurrentScanUid '{CurrentScanUID}'");

                HandleAfterRawImageCompleted();
            }
            else if (ScannedProgress % 50 == 1)/* 每隔50张刷新到日志，避免太频繁 */
            {
                UpdateScanProgressInfo(_lastRawImageSavedEventArgs);
            }

            //MockAcqNormalMode_Receive(rawImageSavedEventArgs);
        }

        private void UpdateScanProgressInfo(RawImageSavedEventArgs rawImageSavedEventArgs)
        {
            string msg = (rawImageSavedEventArgs.IsFinished)
                ? string.Format(Calibration_Lang.Calibration_Scan_Progress_Done_Formatter, rawImageSavedEventArgs.FinishCount, TotalScanCount)
                : string.Format(Calibration_Lang.Calibration_Scan_Progress_Doing_Formatter, rawImageSavedEventArgs.FinishCount, TotalScanCount);
            PrintMessage(msg);
        }

        private void HandleAfterRawImageCompleted()
        {
            UpdateScanProgressInfo(_lastRawImageSavedEventArgs);

            string msg = $"HandleAfterRawImageCompleted | lastAcqReconStatusArgs.ScanUID={lastAcqReconStatusArgs?.ScanUID}, _lastRawImageSavedEventArgs.ScanUID={_lastRawImageSavedEventArgs.ScanUID}";
            logWrapper.Debug(msg);
            if (lastAcqReconStatusArgs?.ScanUID == _lastRawImageSavedEventArgs.ScanUID)
            {
                msg = "HandleAfterRawImageCompleted | call ResumeNextCommand()";
                logWrapper.Debug(msg);

                ResumeNextCommand("HandleAfterRawImageCompleted");

                //if (!IsAutoConfirmResult)
                //{
                //    string rawDataFolder = lastAcqReconStatusArgs.RawDataPath;
                //    PrintMessage($"Loading the raw images from {rawDataFolder}");
                //    //this.LoadRawImages(rawDataFolder);//调整到其他位置处理了
                //}
            }
            else
            {
                msg = "HandleAfterRawImageCompleted | not call ResumeNextCommand() due to not same ScanUID";
                logWrapper.Debug(msg);
            }
        }

        /// <summary>
        /// 检测设备是否可扫描，通过主动调用服务代理获取最新的实时状态。
        /// 如果使用被动接受服务代理的实时状态消息，有可能消息丢失没有收到导致始终无法闭环。
        /// </summary>
        /// <returns></returns>
        private bool IsScanEnabled()
        {
            var realtimeStatus = NV.CT.FacadeProxy.AcqReconProxy.Instance.CurrentDeviceSystem.RealtimeStatus;
            bool isScanEnabled = (realtimeStatus == RealtimeStatus.Standby || realtimeStatus == RealtimeStatus.Validated);

            string msg = string.Format("Device scan is {1} due to RealtimeStatus is {0}", realtimeStatus, (isScanEnabled ? "available" : "NOT available"));
            if (isScanEnabled)
            {
                logWrapper.Debug(msg);
            }
            else
            {
                PrintMessage(msg);
            }

            return isScanEnabled;
        }

        #endregion registe event

        #region 刷新图像

        /// <summary>
        /// 图像发生变化的事件，由事件中介响应（解耦）
        /// </summary>
        ///
        public static readonly string KEY_RAW_IMAGE_CHANGED_EVENT = "AutoCali.RawImageChangedEvent";

        /// <summary>
        /// 启动刷新图像更新的任务（异步0
        /// </summary>
        private void RaiseRawImageChanged(RawImageInfo rawImageInfo)
        {
            if (rawImageInfo == null)
            {
                return;
            }

            DispatcherWrapper.BeginInvoke(action, rawImageInfo);
        }

        private Action<RawImageInfo> action = (rawImageInfo) =>
        {
            string msg = "通知刷新图像...";
            LogService.Instance.Info(ServiceCategory.AutoCali, msg);
            EventMediator.Raise(KEY_RAW_IMAGE_CHANGED_EVENT, rawImageInfo);
        };

        /// <summary>
        /// 清空生数据图像
        /// </summary>
        private void ClearRawImages()
        {
            var imageChangedEventArgs = new RawImageChangedEventArgs();
            imageChangedEventArgs.ImageChangedType = ImageChangedType.Clear;

            RaiseRawImageChanged(imageChangedEventArgs);
        }

        private void TryClearRawImage()
        {
            //if (IsAutoNext)
            //{
            //    return;
            //}

            this.ScannedProgress = 0;
            this.TotalScanCount = 0;

            ClearRawImages();
        }

        /// <summary>
        /// 加载生数据图像
        /// </summary>
        /// <param name="rawDataFolder">生数据图像所在目录</param>
        private void LoadRawImages(string rawDataFolder)
        {
            var imageChangedEventArgs = new RawImageChangedEventArgs();
            imageChangedEventArgs.ImageChangedType = ImageChangedType.Add;
            imageChangedEventArgs.ScanReconParam = _lastScanReconParam;
            imageChangedEventArgs.FilePaths = GetRawImageFiles(rawDataFolder);

            this._logger.Debug(ServiceCategory.AutoCali, $"[{nameof(LoadRawImages)}] RawImageChangedEventArgs.rawDataFolder:{rawDataFolder}, FilePaths={string.Join(';', imageChangedEventArgs.FilePaths)}");
            this.RaiseRawImageChanged(imageChangedEventArgs);

            //WeakReferenceMessenger.Default.Send(new LoadRawDataDirectoryMessage(rawDataFolder));
        }

        private string[] GetRawImageFiles(string rawDataFolder)
        {
            if (string.IsNullOrEmpty(rawDataFolder))
            {
                return null;
            }

            string[] files = null;
            try
            {
                files = Directory.GetFiles(rawDataFolder, "*.raw");
            }
            catch (Exception ex)
            {
                logWrapper.Error($"Failed get the raw image files from path:{rawDataFolder}, exception:{ex.ToString()}");
            }

            return files;
        }

        #endregion 刷新图像

        #region "下一步"命令

        /// <summary>
        /// 恢复“下一步”命令可用
        /// </summary>
        private void ResumeNextCommand(string sender)
        {
            NextCommandEnabled = true;
            logWrapper.Debug($"Resume 'Next' Enabled from the sender '{sender}'");
        }

        /// <summary>
        /// 禁用“下一步”命令
        /// </summary>
        private void DisableNextCommand(string sender)
        {
            NextCommandEnabled = false;
            logWrapper.Info($"Disable 'Next' from the sender '{sender}'");
        }

        #endregion "下一步"命令

        private string FormatCommandResultInfo(CommandResult commandResult)
        {
            var codes = string.Join(",", commandResult.ErrorCodes.Codes);
            string msg = $"Got the Command Result={{status:\"{commandResult.Status}\", ErrorCodes:\"{codes}\"}}";
            return msg;
        }

        #region 更新运行日志

        private void InitMessagePrintService()
        {
            _uiLogger = new MessagePrintService(_logger) { XServiceCategory = ServiceCategory.AutoCali };
            _uiLogger.OnConsoleMessageChanged += OnConsoleMessageChanged;

            ////todo:remove after test
            //Task.Factory.StartNew(new Action(async () =>
            //{
            //    int loopcount = 0;
            //    while (true)
            //    {
            //        PrintMessage($"[{++loopcount}] Test print message by task simulator");
            //        //await Task.Delay(Random.Shared.Next(100, 5000));
            //        //await Task.Delay(Random.Shared.Next(50, 600));
            //        await Task.Delay(Random.Shared.Next(10, 60));
            //    }
            //}), TaskCreationOptions.LongRunning);
        }

        private void OnConsoleMessageChanged(object? sender, string e)
        {
            //必须用DispatcherWrapper跨UI线程，否则，执行一段时间TaskOutput对应UI必将无响应
            //必须将e作为参数传递进去，否则也会卡顿
            DispatcherWrapper.Invoke((string info) =>
            {
                //logWrapper.Debug($"[OnConsoleMessageChanged] [TaskOutput] set info, length: {TaskOutput.Length}");
                TaskOutput = info;//todo,输出重要节点信息到UI，包括错误码信息
            }, e);
        }

        /// <summary>
        /// 在UI上运行日志，并且记录日志。支持区分日志级别
        /// </summary>
        /// <param name="msg"></param>
        private void PrintMessage(string message, PrintLevel printLevel = PrintLevel.Info)
        {
            switch (printLevel)
            {
                case PrintLevel.Info: _uiLogger.PrintLoggerInfo(message); break;
                case PrintLevel.Warn: _uiLogger.PrintLoggerWarn(message); break;
                case PrintLevel.Error: _uiLogger.PrintLoggerError(message); break;
            }
        }

        #endregion 更新运行日志

        #region private Fields

        private static bool isInitProxy = false;
        /// <summary>
        /// 扫描任务的阻塞信号isAcqFinished，由AcqReconStatus的Finished状态来放行
        /// </summary>
        //private bool isAcqCaliFinished = false;

        /// <summary>
        /// 标识服务接口异步执行是否完成，比如，生成校准表，应用校准表等
        /// </summary>
        private bool isServiceStateFinished = false;

        /// <summary>
        /// 设备实时状态变化，更新标识“设备是否可用”。
        /// 使用信号量AutoResetEvent不准确，因为发起下一次扫描，需要同时判断：1.设备的可用性，2.acqReconStatus已完成，
        /// 而这二者时序无法保证先后
        /// </summary>
        private bool isScanEnabled = false;

        private int _previousTaskId = -1;//上一个任务Id，用来区分同一个任务的（可能出现的）不同信号(比如AcqFinished或者RawSavedFinishied）
        private int _currentTaskId = -1;//当前任务Id
        private AcqReconStatusArgs lastAcqReconStatusArgs;
        private RealtimeStatus curRealTimeStatus = RealtimeStatus.Init;

        //当前系统状态
        private SystemStatus curSystemStatus = default(SystemStatus);

        private DataTable _ArgProtocolDataTable;

        /// <summary>
        /// 取消异步任务的令牌
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// 是否应用校准表，更新到校准表的正式目录下。否则只是在临时目录下
        /// </summary>
        private bool isConfirmedApplyCalibrationResult = false;

        /// <summary>
        /// 已经扫描的个数
        /// </summary>

        private int mScannedProgress;

        public int ScannedProgress
        {
            get
            {
                return mScannedProgress;
            }
            set
            {
                if (mScannedProgress == value)
                {
                    return;
                }

                mScannedProgress = value;
                OnPropertyChanged(nameof(ScannedProgress));
            }
        }

        private int mTotalScanCount;

        public int TotalScanCount
        {
            get
            {
                return mTotalScanCount;
            }
            set
            {
                if (mTotalScanCount == value)
                {
                    return;
                }

                mTotalScanCount = value;
                OnPropertyChanged(nameof(TotalScanCount));
            }
        }

        /// <summary>
        /// “下一步”操作指令
        /// </summary>
        public DelegateCommand? NextCommand { get; private set; }

        /// <summary>
        /// “Redo”操作指令
        /// </summary>
        public DelegateCommand? RedoCommand { get; private set; }

        public bool NextCommandEnabled
        {
            get { return _NextCommandEnabled; }
            set
            {
                if (_NextCommandEnabled == value)
                {
                    return;
                }

                _NextCommandEnabled = value;
                OnPropertyChanged(Name_NextCommandEnabled);
            }
        }

        #region 命令

        /// <summary>
        /// “开始”操作指令
        /// </summary>
        public DelegateCommand? StartCommand { get; private set; }

        public bool StartCommandEnabled
        {
            get { return _StartCommandEnabled; }
            set
            {
                if (_StartCommandEnabled == value)
                {
                    return;
                }

                _StartCommandEnabled = value;
                OnPropertyChanged("StartCommandEnabled");
            }
        }

        /// <summary>
        /// “停止”操作指令
        /// </summary>
        public DelegateCommand? StopAllCommand { get; private set; }

        public DelegateCommand? StopCurrentCommand { get; private set; }

        public bool StartAllCommandEnabled
        {
            get { return _StopAllCommandEnabled; }
            set
            {
                if (_StopAllCommandEnabled == value)
                {
                    return;
                }

                _StopAllCommandEnabled = value;
                OnPropertyChanged("StartAllCommandEnabled");
            }
        }


        #endregion 命令

        private void InitCommand()
        {
            NextCommand = new DelegateCommand(() =>
            {
                this._logger.Debug(ServiceCategory.AutoCali, "User Click 'Next'");

                //goNextEvent.Set();
                _taskToken?.Release(new() { Sender = "UserNext" });
                //_goNextTaskCompletionSource?.TrySetResult(new());
            });
            RedoCommand = new DelegateCommand(() =>
            {
                this._logger.Debug(ServiceCategory.AutoCali, "User Click 'Redo'");

                //goNextEvent.Set();
                _taskToken?.Release(new() { Sender = "UserRedo" });
                //_goNextTaskCompletionSource?.TrySetResult(new());
            });


            StartCommand = new DelegateCommand(() => { RunScenarioTask(); });
            StopAllCommand = new DelegateCommand(() =>
            {
                var dialogResult = DialogService.Instance.ShowConfirm(Const_Message_ConfirmToCancel);
                if (!dialogResult)
                {
                    PrintMessage("User confirmed stopping all tasks.");
                    return;
                }

                CancelScenarioTask(null, null);
                //BlockResetEvent_ProgressInfo();

                //_taskToken?.Release(new AsyncCommandResult() { Status = CommandStatus.Cancelled, Sender = "UserAllStop" });
            });

            StopCurrentCommand = new DelegateCommand(() =>
            {
                var dialogResult = DialogService.Instance.ShowConfirm(Const_Message_ConfirmToStopCurrent);
                if (!dialogResult)
                {
                    PrintMessage("User confirmed stopping the current task.");
                    return;
                }

                CancelScenarioTask(null, null);
                //BlockResetEvent_ProgressInfo();

                //_taskToken?.Release(new AsyncCommandResult() { Sender = "UserStopAll" });
            });
        }

        private string Const_Message_ConfirmToCancel = "您确认要取消全部任务吗？";

        private string Const_Message_ConfirmToStopCurrent = "您确认要停止当前任务吗？";
        private bool _NextCommandEnabled;
        private bool _StartCommandEnabled = true;
        private bool _StopAllCommandEnabled = true;


        private static readonly string Name_NextCommandEnabled = nameof(NextCommandEnabled);

        #endregion private Fields
    }
}