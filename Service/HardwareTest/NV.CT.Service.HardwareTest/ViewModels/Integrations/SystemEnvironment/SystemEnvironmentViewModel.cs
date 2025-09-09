using CommunityToolkit.Mvvm.Input;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Enums;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Models.Integrations.SystemEnvironment;
using NV.CT.Service.HardwareTest.Models.Integrations.SystemEnvironment.Parts;
using NV.CT.Service.HardwareTest.Services.Integrations.SystemEnvironment;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.ViewModels.Foundations;
using NV.CT.Service.Models;
using NV.MPS.Configuration;

namespace NV.CT.Service.HardwareTest.ViewModels.Integrations.SystemEnvironment
{
    internal partial class SystemEnvironmentViewModel(ILogService logService, ISystemEnvironmentSetService setService) : NavigationViewModelBase
    {
        #region Property

        public EnableType[] EnableTypes { get; } = [EnableType.Enable, EnableType.Disable];

        public SystemEnvironmentPartAbstract[] Parts { get; } =
        [
            new GantryPartModel(SystemConfig.CavitySetting),
            new PduPartModel(SystemConfig.CavitySetting),
        ];

        #endregion

        #region Command

        [RelayCommand]
        private void Set(SystemEnvironmentDataAbstract item)
        {
            var res = setService.Set(item);
            DialogResult(res.Result, res.IsErrorCode);
        }

        [RelayCommand]
        private void SetAll(SystemEnvironmentPartAbstract item)
        {
            var res = setService.SetAll(item);
            DialogResult(res.Result, res.IsErrorCode);
        }

        [RelayCommand]
        private void SetAllParts()
        {
            var res = setService.SetAllParts(Parts);
            DialogResult(res.Result, res.IsErrorCode);
        }

        #endregion

        #region Internal Method

        private void DialogResult(GenericResponse result, bool isErrorCode)
        {
            if (result.status)
            {
                DialogService.Instance.ShowInfo(result.message);
            }
            else
            {
                if (isErrorCode)
                {
                    DialogService.Instance.ShowErrorCode(result.message);
                }
                else
                {
                    DialogService.Instance.ShowError(result.message);
                }
            }
        }

        private void OnComponentCycleStatusChanged(object sender, CycleStatusArgs arg)
        {
            foreach (var part in Parts)
            {
                part.ReceivedComponentCycleStatus(arg);
            }
        }

        #endregion

        #region Register And Navigation

        private void RegisterProxyEvents()
        {
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SystemEnvironment}] Register events.");
            ComponentStatusProxy.Instance.CycleStatusChanged += OnComponentCycleStatusChanged;
        }

        private void UnRegisterProxyEvents()
        {
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SystemEnvironment}] Un-register events.");
            ComponentStatusProxy.Instance.CycleStatusChanged -= OnComponentCycleStatusChanged;
        }

        public override void BeforeNavigateToCurrentPage()
        {
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SystemEnvironment}] Enter [Cavity Temperature] testing page.");
            RegisterProxyEvents();
        }

        public override void BeforeNavigateToOtherPage()
        {
            UnRegisterProxyEvents();
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SystemEnvironment}] Leave [Cavity Temperature] testing page.");
        }

        #endregion
    }
}