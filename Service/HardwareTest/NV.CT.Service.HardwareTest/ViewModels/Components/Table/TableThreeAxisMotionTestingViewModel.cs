using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.DeviceInteract.Models;
using NV.CT.FacadeProxy.Essentials.EventArguments;
using NV.CT.FacadeProxy.Essentials.RegisterAddresses;
using NV.CT.FacadeProxy.Models.MotionControl.Table;
using NV.CT.FacadeProxy.Registers;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Extensions;
using NV.CT.Service.HardwareTest.Attachments.Helpers;
using NV.CT.Service.HardwareTest.Categories;
using NV.CT.Service.HardwareTest.Models.Components.Table;
using NV.CT.Service.HardwareTest.Services.Universal.PrintMessage.Abstractions;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.Share.Enums.Components;
using NV.CT.Service.HardwareTest.ViewModels.Foundations;
using System;

namespace NV.CT.Service.HardwareTest.ViewModels.Components.Table
{
    public partial class TableThreeAxisMotionTestingViewModel : NavigationViewModelBase
    {
        private readonly ILogService logService;
        private readonly IMessagePrintService messagePrintService;

        public TableThreeAxisMotionTestingViewModel(
            ILogService logService, 
            IMessagePrintService messagePrintService)
        {
            this.logService = logService;
            this.messagePrintService = messagePrintService;
            //Initialize
            InitializeProperties();
        }

        #region Initialize

        /// <summary>
        /// 初始化Properties
        /// </summary>
        private void InitializeProperties() 
        {
            TableParameters = new();
            TableSource = new();
        }

        #endregion

        #region Fields

        private const float TableArrivedThreshold = 1f;

        #endregion

        #region Properties

        /// <summary>
        /// 扫描床控制参数
        /// </summary>
        [ObservableProperty]
        private TableBaseCategory tableParameters = null!;

        /// <summary>
        /// 扫描床源状态
        /// </summary>
        [ObservableProperty]
        private TableSource tableSource = null!;

        /// <summary>
        /// 扫描床水平运动状态
        /// </summary>
        [ObservableProperty]
        private TableMoveStatus horizontalMoveStatus = TableMoveStatus.NotArrived;

        /// <summary>
        /// 扫描床垂直运动状态
        /// </summary>
        [ObservableProperty]
        private TableMoveStatus verticalMoveStatus = TableMoveStatus.NotArrived;

        /// <summary>
        /// 扫描床X轴运动状态
        /// </summary>
        [ObservableProperty]
        private TableMoveStatus axisXMoveStatus = TableMoveStatus.NotArrived;

        /// <summary>
        /// 水平运动状态变化
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        partial void OnHorizontalMoveStatusChanged(TableMoveStatus oldValue, TableMoveStatus newValue)
        {
            messagePrintService.PrintLoggerInfo($"Horizontal move status changed from [{Enum.GetName(oldValue)}] to [{Enum.GetName(newValue)}].");
        }
        
        /// <summary>
        /// 垂直运动状态变化
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        partial void OnVerticalMoveStatusChanged(TableMoveStatus oldValue, TableMoveStatus newValue)
        {
            messagePrintService.PrintLoggerInfo($"Vertical move status changed from [{Enum.GetName(oldValue)}] to [{Enum.GetName(newValue)}].");
        }

        /// <summary>
        /// X轴运动状态变化
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        partial void OnAxisXMoveStatusChanged(TableMoveStatus oldValue, TableMoveStatus newValue)
        {
            messagePrintService.PrintLoggerInfo($"AxisX move status changed from [{Enum.GetName(oldValue)}] to [{Enum.GetName(newValue)}].");
        }

        /// <summary>
        /// 水平误差
        /// </summary>
        [ObservableProperty]
        private float horizontalDeviation;

        /// <summary>
        /// 垂直误差
        /// </summary>
        [ObservableProperty]
        private float verticalDeviation;

        /// <summary>
        /// X轴误差
        /// </summary>
        [ObservableProperty]
        private float axisXDeviation;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanStartTest))]
        [NotifyCanExecuteChangedFor(nameof(StartMoveCommand))]
        private bool inTesting = false;

        public bool CanStartTest => !InTesting;

        /// <summary>
        /// 控制台消息
        /// </summary>
        [ObservableProperty]
        private string consoleMessage = string.Empty;

        #endregion

        #region Events

        #region Events Registration

        private void RegisterProxyEvents() 
        {
            MotionControlProxy.Instance.CycleStatusChanged += MotionControlProxy_CycleStatusChanged;
            MotionControlProxy.Instance.EventDataReceived += MotionControlProxy_EventDataReceived;
        }

        private void UnRegisterProxyEvents()
        {
            MotionControlProxy.Instance.CycleStatusChanged -= MotionControlProxy_CycleStatusChanged;
            MotionControlProxy.Instance.EventDataReceived -= MotionControlProxy_EventDataReceived;
        }

        private void RegisterMessagePrinterEvents()
        {
            messagePrintService.OnConsoleMessageChanged += MessagePrintService_OnConsoleMessageChanged;
        }

        private void UnRegisterMessagePrinterEvents()
        {
            messagePrintService.OnConsoleMessageChanged -= MessagePrintService_OnConsoleMessageChanged;
        }

        private void RegisterInputTableParameterEvents() 
        {
            TableParameters.HorizontalPositionChanged += TableParameters_HorizontalPositionChanged;
            TableParameters.VerticalPositionChanged += TableParameters_VerticalPositionChanged;
            TableParameters.AxisXPositionChanged += TableParameters_AxisXPositionChanged;
        }

        private void UnRegisterInputTableParameterEvents()
        {
            TableParameters.HorizontalPositionChanged -= TableParameters_HorizontalPositionChanged;
            TableParameters.VerticalPositionChanged -= TableParameters_VerticalPositionChanged;
            TableParameters.AxisXPositionChanged -= TableParameters_AxisXPositionChanged;
        }

        #endregion

        #region Cycle Status

        private void MotionControlProxy_CycleStatusChanged(object sender, CycleStatusArgs args)
        {
            // 获取DeviceSystem
            var deviceSystem = args.Device;
            //更新扫描床状态 
            ComponentStatusHelper.UpdateTableStatusByCycle(TableSource, deviceSystem);
        }

        #endregion

        #region Event Data

        private void MotionControlProxy_EventDataReceived(object sender, EventDataEventArgs args)
        {
            //解析实时数据
            var keyValuePair = EventDataHelper.ParseEventData(args.EventDataInfo.Data);
            //筛选床状态
            if (keyValuePair.address == TableRegisterAddresses.TableSystemStatus) 
            {
                //打印
                logService.Info(ServiceCategory.HardwareTest, 
                    $"[{ComponentDefaults.TableComponent}] Received table event data, address:{keyValuePair.address.ToString("X")}, value:{keyValuePair.content.ToString("X")}.");
                //床水平到位
                if (MotionControlHelper.IsTableHorizontallyInPlace(keyValuePair.content)) 
                {
                    //更新误差
                    HorizontalDeviation = TableSource.HorizontalPosition - TableParameters.HorizontalPosition;
                    //更新状态
                    HorizontalMoveStatus = TableMoveStatus.Arrived;
                    //查看任务是否完成
                    ValidateTestCompletion();
                }
                //床垂直到位
                if (MotionControlHelper.IsTableVerticallyInPlace(keyValuePair.content)) 
                {
                    //更新误差
                    VerticalDeviation = TableSource.VerticalPosition - TableParameters.VerticalPosition;
                    //更新状态
                    VerticalMoveStatus = TableMoveStatus.Arrived;
                    //查看任务是否完成
                    ValidateTestCompletion();
                }
                //床X轴到位
                if (MotionControlHelper.IsTableAxisXInPlace(keyValuePair.content)) 
                {
                    //更新误差
                    AxisXDeviation = TableSource.AxisXPosition - TableParameters.AxisXPosition;
                    //更新状态
                    AxisXMoveStatus = TableMoveStatus.Arrived;
                    //查看任务是否完成
                    ValidateTestCompletion();
                }
            }
        }

        #endregion

        #region Table Parameters

        private void TableParameters_HorizontalPositionChanged(object? sender, float inputHorizontalPosition)
        {
            if (inputHorizontalPosition - TableSource.HorizontalPosition > TableArrivedThreshold)
            {
                HorizontalMoveStatus = TableMoveStatus.NotArrived;
            }
        }

        private void TableParameters_VerticalPositionChanged(object? sender, float inputVerticalPosition)
        {
            if (inputVerticalPosition - TableSource.VerticalPosition > TableArrivedThreshold)
            {
                VerticalMoveStatus = TableMoveStatus.NotArrived;
            }
        }

        private void TableParameters_AxisXPositionChanged(object? sender, float inputAxisXPosition)
        {
            if (inputAxisXPosition - TableSource.AxisXPosition > TableArrivedThreshold) 
            {
                AxisXMoveStatus = TableMoveStatus.NotArrived;
            }
        }

        #endregion

        #endregion

        #region Table Control

        [RelayCommand(CanExecute = nameof(CanStartTest))]
        private void StartMove() 
        {
            bool status = StartMoveCommon();
            //测试中
            if (status) 
            {
                InTesting = true;
            }
        }

        [RelayCommand]
        private void StopMove() 
        {
            StopMoveCommon();
            InTesting = false;
        }

        /// <summary>
        /// 开始扫描床移动Common
        /// </summary>
        private bool StartMoveCommon() 
        {
            //发送移床指令
            var response = MotionControlProxy.Instance.StartMoveTable(TableParameters.ToProxyTableParams());
            //判定
            if (response.Status)
            {
                messagePrintService.PrintLoggerInfo(response.Message);
            }
            else 
            {
                messagePrintService.PrintLoggerError(response.Message);
            }

            return response.Status;
        }

        /// <summary>
        /// 停止扫描床移动Common
        /// </summary>
        private bool StopMoveCommon() 
        {
            //发送移床指令
            var response = MotionControlProxy.Instance.StopMoveTable();
            //判定
            if (response.Status)
            {
                messagePrintService.PrintLoggerInfo(response.Message);
            }
            else
            {
                messagePrintService.PrintLoggerError(response.Message);
            }

            return response.Status;
        }

        #endregion

        #region Test Control

        /// <summary>
        /// 验证测试结束
        /// </summary>
        private void ValidateTestCompletion() 
        {
            switch (TableParameters.Direction) 
            {
                case TableMoveDirection.Horizontal: 
                    {
                        if (HorizontalMoveStatus == TableMoveStatus.Arrived) 
                        {
                            InTesting = false;
                            //打印
                            messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.TableComponent}] Table is now horizontally in place.");
                        }
                    }
                    break;
                case TableMoveDirection.Vertical:
                    {
                        if (VerticalMoveStatus == TableMoveStatus.Arrived)
                        {
                            InTesting = false;
                            //打印
                            messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.TableComponent}] Table is now vertically in place.");
                        }
                    }
                    break;
                case TableMoveDirection.Union:
                    {
                        if (HorizontalMoveStatus == TableMoveStatus.Arrived && VerticalMoveStatus == TableMoveStatus.Arrived)
                        {
                            InTesting = false;
                            //打印
                            messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.TableComponent}] Table is now horizontally & vertically in place.");
                        }
                    }
                    break;
                case TableMoveDirection.AxisX:
                    {
                        if (AxisXMoveStatus == TableMoveStatus.Arrived)
                        {
                            InTesting = false;
                            //打印
                            messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.TableComponent}] Table is now axisxly in place.");
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Message Printer

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
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.TableComponent}] Enter [Table Three-Axis Motion Testing] page.");
            //注册Proxy事件
            RegisterProxyEvents();
            //注册消息打印
            RegisterMessagePrinterEvents();
            //注册目标位置变化事件
            RegisterInputTableParameterEvents();
        }

        public override void BeforeNavigateToOtherPage()
        {
            //取消Proxy事件
            UnRegisterProxyEvents();
            //取消消息打印
            UnRegisterMessagePrinterEvents();
            //取消注册目标位置变化事件
            UnRegisterInputTableParameterEvents();
            //记录
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.TableComponent}] Leave [Table Three-Axis Motion Testing] page.");
        }

        #endregion

    }
}
