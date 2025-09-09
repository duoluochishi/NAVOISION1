using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Common.Models.Generic;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.TubeWarmUp.DAL.Dtos;
using NV.CT.Service.TubeWarmUp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace NV.CT.Service.TubeWarmUp.Services.Adapter
{
    //抽象出接口，方便单元测试
    public interface IWarmUpAdapter
    {
        event EventHandler<CycleStatusArgs> CycleStatusChenged;

        event EventHandler<SystemStatusArgs> SystemStatusChanged;

        event EventHandler<AcqReconStatusArgs> AcqReconStatusChenged;

        event EventHandler<string[]> ErrorOccured;

        DeviceSystem Device { get; }
        bool Connected { get; }

        CommandResult StartWarmUp(WarmUpTaskAdapter warmUpTaskAdapt, ScanParamDto scanParamDto);

        CommandResult StopWarmUp();
    }

    //预热代理适配器
    public class WarmUpProxyAdapter1 : IWarmUpAdapter
    {
        public DeviceSystem Device => WarmUpProxy.Instance.DeviceSystem;

        public bool Connected => Device.DeviceConnected;

        public event EventHandler<CycleStatusArgs> CycleStatusChenged;

        public event EventHandler<SystemStatusArgs> SystemStatusChanged;

        public event EventHandler<AcqReconStatusArgs> AcqReconStatusChenged;

        public event EventHandler<string[]> ErrorOccured;

        private readonly ILogService _logService;

        public WarmUpProxyAdapter1(ILogService logService)
        {
            _logService = logService;
            WarmUpProxy.Instance.Init(new IPPort { IP = "127.0.0.1", Port = 30030 });
            WarmUpProxy.Instance.CycleStatusChanged += OnCycleStatusChanged; ;
            WarmUpProxy.Instance.SystemStatusChanged += OnSystemStatusChanged; ;
        }

        private void OnCycleStatusChanged(object? sender, CycleStatusArgs e)
        {
            this.CycleStatusChenged?.Invoke(this, e);
        }

        private void OnSystemStatusChanged(object? sender, SystemStatusArgs e)
        {
            this.SystemStatusChanged?.Invoke(this, e);
        }

        public CommandResult StartWarmUp(WarmUpTaskAdapter task, ScanParamDto scanParamDto)
        {
            try
            {
                var para = task.GetWarmUpParam(scanParamDto);
                var res = WarmUpProxy.Instance.StartWarmUp(new List<WarmUpParam> { para });
                _logService.Info(ServiceCategory.TubeWarmUp, $"StartWarmUp {res.Status} {res.ErrorCodes.Codes.FirstOrDefault()}");
                return res;
            }
            catch (Exception e)
            {
                _logService.Error(ServiceCategory.TubeWarmUp, $"StartWarmUp exception", e);
                return new CommandResult { Status = CommandStatus.Failure };
            }
        }

        public CommandResult StopWarmUp()
        {
            try
            {
                var res = WarmUpProxy.Instance.AbortWarmUp();
                _logService.Info(ServiceCategory.TubeWarmUp, $"StopWarmUp {res.Status} {res.ErrorCodes.Codes.FirstOrDefault()}");
                return res;
            }
            catch (Exception e)
            {
                _logService.Error(ServiceCategory.TubeWarmUp, $"StopWarmUp exception", e);
                return new CommandResult { Status = CommandStatus.Failure };
            }
        }
    }

    /// <summary>
    /// 使用自由模式的采图接口
    /// </summary>
    public class WarmupProxyAdapter : IWarmUpAdapter
    {
        public DeviceSystem Device => DataAcquisitionProxy.Instance.DeviceSystem;

        public bool Connected =>/* DataAcquisitionProxy.Instance.ServiceConnected &&*/ DataAcquisitionProxy.Instance.CTBoxConnected;

        public event EventHandler<CycleStatusArgs> CycleStatusChenged;

        public event EventHandler<SystemStatusArgs> SystemStatusChanged;

        public event EventHandler<AcqReconStatusArgs> AcqReconStatusChenged;

        public event EventHandler<string[]> ErrorOccured;

        private readonly ILogService _logService;

        public WarmupProxyAdapter(ILogService logService)
        {
            _logService = logService;
            DataAcquisitionProxy.Instance.SystemStatusChanged += OnSystemStatusChanged;
            DataAcquisitionProxy.Instance.CycleStatusChanged += OnCycleStatusChanged;
            DataAcquisitionProxy.Instance.AcqReconStatusChanged += OnAcqReconStatusChanged;
        }

        private void OnAcqReconStatusChanged(object arg1, AcqReconStatusArgs arg2)
        {
            this.AcqReconStatusChenged?.Invoke(this, arg2);
        }

        private void OnCycleStatusChanged(object arg1, CycleStatusArgs arg2)
        {
            this.CycleStatusChenged?.Invoke(this, arg2);
        }

        private void OnSystemStatusChanged(object arg1, SystemStatusArgs arg2)
        {
            this.SystemStatusChanged?.Invoke(this, arg2);
        }

        public CommandResult StartWarmUp(WarmUpTaskAdapter warmUpTaskAdapt, ScanParamDto scanParamDto)
        {
            var parameter = warmUpTaskAdapt.GetDataAcquisitionParams(scanParamDto);
            var configResult = DataAcquisitionProxy.Instance.ConfigureDataAcquisition(parameter);
            var configResult1 = configResult.ToCommandResult();
            if (configResult1.Status != CommandStatus.Success)
            {
                return configResult1;
            }
            var startResult = DataAcquisitionProxy.Instance.StartDataAcquisition(parameter);
            return startResult.ToCommandResult();
        }

        public CommandResult StopWarmUp()
        {
            var result = DataAcquisitionProxy.Instance.StopDataAcquisition();
            return result.ToCommandResult();
        }
    }

    public static class GenericResponseExtension
    {
        public static CommandResult ToCommandResult(this GenericResponse<bool> response)
        {
            var commandResult = new CommandResult();
            commandResult.Status = response.Status ? CommandStatus.Success : CommandStatus.Failure;
            return commandResult;
        }
    }
}