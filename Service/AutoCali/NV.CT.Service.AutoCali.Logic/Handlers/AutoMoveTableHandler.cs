using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic.Handlers
{
    /// <summary>
    /// 自动移床
    /// </summary>
    public class AutoMoveTableHandler
    {
        public AutoMoveTableHandler(
            ILogService logger,
            IMessagePrintService loggerUI,
            CancellationTokenSource cancellationTokenSource)
        {
            _logService = logger;
            _messagePrintService = loggerUI;

            _cancellationTokenSource = cancellationTokenSource;
        }

        public async Task<CommandResult> Run(int targetTableHorizontalPosition)
        {
            CommandResult commandResult = new() { Sender = $"[{nameof(this.GetType)}] [{nameof(Run)}]" };
            //自动移床一段距离到探测器边缘
            int currentTableHorizontalPositionFromDevice = Math.Abs(GetTableHorizontalPositionFromDevice());
            int delta = Math.Abs(currentTableHorizontalPositionFromDevice) - Math.Abs(targetTableHorizontalPosition);

            string msg = string.Empty;
            if (Math.Abs(delta) < CONST_DISTANCE_DEATA_AS_SAME)
            {
                msg = $"No need to move the table, as the current horizontal position({currentTableHorizontalPositionFromDevice}) " +
                    $"is close to the target({targetTableHorizontalPosition}).";
                this._messagePrintService.PrintLoggerInfo(msg);
                return commandResult;
            }

            string operationName = "水平移床";
            string messageToConfirmMoveTable = $"系统将自动 {operationName}, " +
                $"\n当前位置：{FormatTablePosition(currentTableHorizontalPositionFromDevice)}，" +
                $"\n目标位置：{FormatTablePosition(targetTableHorizontalPosition)}。" +
                $"\n如果不需要，请忽略此操作。";
            if (!DialogService.Instance.ShowConfirm(messageToConfirmMoveTable, operationName))
            {
                return commandResult;
            }

            commandResult = await AutoMoveTable(currentTableHorizontalPositionFromDevice, targetTableHorizontalPosition);
            return commandResult;
        }

        /// <summary>
        /// 自动移床到探测器边缘
        /// </summary>
        private async Task<CommandResult> AutoMoveTable(int currentTableHorizontalPosition, int targetTableHorizontalPosition)
        {
            CommandResult commandResult = new() { Sender = nameof(AutoMoveTable) };

            var horizontalPosition = (int)((-1) * targetTableHorizontalPosition);
            this._messagePrintService.PrintLoggerInfo($"Auto move the table position, horizontal: " +
                $"{FormatTablePosition(currentTableHorizontalPosition)} -> {FormatTablePosition(targetTableHorizontalPosition)}");

            var response = MotionControlProxy.Instance.StartMoveTable(new()
            {
                Direction = FacadeProxy.Models.MotionControl.Table.TableMoveDirection.Horizontal,
                HorizontalPosition = (uint)Math.Abs(horizontalPosition)
            });

            //命令发生失败
            if (!response.Status)
            {
                this._messagePrintService.PrintLoggerError($"Failed to send the command 'MoveTable', Message:{response.Message}");
                commandResult.Status = CommandStatus.Failure;
                commandResult.Description = response.Message;
                return commandResult;
            }

            //命令发生成功，等待床移动到位
            TimeSpan awaitSeconds = TimeSpan.FromSeconds(5);
            _messagePrintService.PrintLoggerInfo($"Succeed to send the command 'MoveTable'");
            _messagePrintService.PrintLoggerInfo($"Await Table Arrived in {awaitSeconds.TotalSeconds}s");
            await Task.Delay(awaitSeconds);

            //命令发生成功，主动判断床是否移动到位
            int positionArrivedCount = 0;
            while (true)
            {
                if (this._cancellationTokenSource.IsCancellationRequested)
                {
                    this._messagePrintService.PrintLoggerInfo($"User cancelled to move the table");
                    break;
                }
                await Task.Delay(1000);

                var currentHorizontalPosition = GetTableHorizontalPositionFromDevice();
                int delta = Math.Abs(currentHorizontalPosition) - Math.Abs(targetTableHorizontalPosition);
                if (Math.Abs(delta) < 100 /* 0.1mm */)
                {
                    if (positionArrivedCount++ < 5)
                    {
                        continue;
                    }

                    this._messagePrintService.PrintLoggerInfo($"Table arrived the target horizontal position({targetTableHorizontalPosition})");
                    break;
                }
            }

            this._logService.Debug(ServiceCategory.AutoCali, $"[{nameof(AutoMoveTable)}] Existed");
            return commandResult;
        }

        #region ScanPosition control

        private int GetTableHorizontalPositionFromDevice()
        {
            int realtimeTableHorizontalPos = DeviceSystem.Instance.Table.HorizontalPosition;
            this._messagePrintService.PrintLoggerInfo($"Got the table horizontal position({realtimeTableHorizontalPos}) from device.");

            return realtimeTableHorizontalPos;
        }

        private int GetTableVerticalPositionFromDevice()
        {
            int VerticalPositionFromDevice = (int)DeviceSystem.Instance.Table.VerticalPosition;
            this._messagePrintService.PrintLoggerInfo($"Got the table vertical position({VerticalPositionFromDevice}) from device.");

            return VerticalPositionFromDevice;
        }

        /// <summary>
        /// 格式化床码值，比如-303.12mm
        /// 负数，单位：毫秒mm
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string FormatTablePosition(int value)
        {
            return $"{(-1) * Math.Abs(value) * 1.0f / CONST_DISTANCE_UM_TO_MM_CEOF} mm";
        }

        #endregion

        #region private fields

        /// <summary>
        /// 距离差值小于多少当做相同
        /// 单位，微米um
        /// </summary>
        public static int CONST_DISTANCE_DEATA_AS_SAME = 10;

        /// <summary>
        /// 自动退床最小值
        /// 单位，微米um
        /// </summary>
        public static int CONST_MIN_POSITION_FOR_AUTO_MOVE = 30 * CONST_DISTANCE_UM_TO_MM_CEOF;

        /// <summary>
        /// 距离转换系数：
        /// 底层单位：微米um，上层UI显示为毫米mm
        /// </summary>
        public static int CONST_DISTANCE_UM_TO_MM_CEOF = 1000;

        private readonly ILogService _logService;
        private readonly IMessagePrintService _messagePrintService;

        private CancellationTokenSource _cancellationTokenSource;
        private TaskToken _taskToken;

        #endregion private fields
    }
}
