using NV.CT.FacadeProxy;
using NV.CT.Service.Common;
using System;
using System.Drawing.Text;

namespace NV.CT.Service.AutoCali.Logic
{
    /// <summary>
    /// 校准服务
    /// </summary>
    internal class CaliService
    {
        private CaliService() { }

        public static CaliService Instance { get => Inner.CaliServiceInstance; }

        private static class Inner
        {
            internal static CaliService CaliServiceInstance;

            private static bool flag;

            static Inner()
            {
                if (flag)
                {
                    return;
                }

                CaliServiceInstance = new CaliService();

                //ProxyHelper.Instance.Init();

                flag = true;
            }
        }
        private CaliScenarioTaskViewModel latestTaskViewModel;

        internal void InitEvent(CaliScenarioTaskViewModel caliScenarioTaskViewModel)
        {
            string block = "CaliService.InitEvent";
            LogService.Instance.Info(ServiceCategory.AutoCali, $"Beginning {block}");

            try
            {
                logger.Info($"Inited by CaliService(={this.GetHashCode()})");
                
                RegisterMrsEvent(caliScenarioTaskViewModel);

                latestTaskViewModel = caliScenarioTaskViewModel;
            }
            catch (Exception ex)
            {
                logger.Error("InitEvent", ex);
            }
            finally
            {
                LogService.Instance.Info(ServiceCategory.AutoCali, $"Ended {block}");
            }
        }

        /// <summary>
        /// 是否已经注册了采集事件
        /// </summary>
        private bool isRegisteredMrsEvent = false;
        public void RegisterMrsEvent(CaliScenarioTaskViewModel caliScenarioTaskViewModel)
        {
            string method = nameof(RegisterMrsEvent);
            logger.Info($"[{method}] Entered, [Input] CaliScenarioTaskViewModel(={caliScenarioTaskViewModel?.GetHashCode()})");

            if (caliScenarioTaskViewModel == null)
            {
                return;
            }

            //先卸载注册事件，避免多次重复注册
            logger.Debug($"[{method}] First, Try to Undo Register Mrs Event");
            UnRegisterMrsEvent(caliScenarioTaskViewModel);

            logger.Debug($"[{method}] Then Register Mrs Event");
            AcqReconProxy.Instance.ScanReconConnectionChanged += caliScenarioTaskViewModel.OnAcqReconConnectionChanged;
            isRegisteredMrsEvent = true;
            logger.Debug($"[{method}] Set 'isRegisteredMrsEvent'={isRegisteredMrsEvent}");
        }

        public void UnRegisterMrsEvent(CaliScenarioTaskViewModel caliScenarioTaskViewModel)
        {
            string method = nameof(UnRegisterMrsEvent);
            logger.Info($"[{method}] Entered, [Input] CaliScenarioTaskViewModel(={caliScenarioTaskViewModel?.GetHashCode()})");

            if (caliScenarioTaskViewModel == null)
            {
                return;
            }

            AcqReconProxy.Instance.ScanReconConnectionChanged -= caliScenarioTaskViewModel.OnAcqReconConnectionChanged;
            isRegisteredMrsEvent = false;
            logger.Debug($"[{method}] Set 'isRegisteredMrsEvent'={isRegisteredMrsEvent}");
        }

        public void UnRegisterAcqEvent(CaliScenarioTaskViewModel caliScenarioTaskViewModel)
        {
            string method = nameof(UnRegisterAcqEvent);
            logger.Info($"[{method}] Entered, [Input] CaliScenarioTaskViewModel(={caliScenarioTaskViewModel?.GetHashCode()})");

            if (caliScenarioTaskViewModel == null)
            {
                return;
            }

            AcqReconProxy.Instance.RealTimeStatusChanged -= caliScenarioTaskViewModel.OnRealTimeStateChanged;

            AcqReconProxy.Instance.RawImageSaved -= caliScenarioTaskViewModel.OnRawImageSaved;
            AcqReconProxy.Instance.AcqReconStatusChanged -= caliScenarioTaskViewModel.OnAcqReconStatusChanged;            
            AcqReconProxy.Instance.ScanReconErrorOccurred -= caliScenarioTaskViewModel.OnScanReconErrorOccurred;

            AcqReconProxy.Instance.DeviceConnectionChanged -= caliScenarioTaskViewModel.OnDeviceConnectionChanged;
            AcqReconProxy.Instance.DeviceErrorOccurred -= caliScenarioTaskViewModel.OnDeviceErrorOccurred;
        }

        public void RegisterAcqEvent(CaliScenarioTaskViewModel caliScenarioTaskViewModel)
        {
            string method = nameof(RegisterAcqEvent);
            logger.Info($"[{method}] Entered, [Input] CaliScenarioTaskViewModel(={caliScenarioTaskViewModel?.GetHashCode()})");

            if (caliScenarioTaskViewModel == null)
            {
                return;
            }

            logger.Debug($"[{method}] First, Try to Undo Register Acq Event");
            UnRegisterAcqEvent(caliScenarioTaskViewModel);

            logger.Debug($"[{method}] Then Register Acq Event");
            AcqReconProxy.Instance.RealTimeStatusChanged += caliScenarioTaskViewModel.OnRealTimeStateChanged;

            AcqReconProxy.Instance.RawImageSaved += caliScenarioTaskViewModel.OnRawImageSaved;
            AcqReconProxy.Instance.AcqReconStatusChanged += caliScenarioTaskViewModel.OnAcqReconStatusChanged;
            AcqReconProxy.Instance.ScanReconErrorOccurred += caliScenarioTaskViewModel.OnScanReconErrorOccurred;

            AcqReconProxy.Instance.DeviceConnectionChanged += caliScenarioTaskViewModel.OnDeviceConnectionChanged;
            AcqReconProxy.Instance.DeviceErrorOccurred += caliScenarioTaskViewModel.OnDeviceErrorOccurred;
        }

        public void UnRegisterCalibrationEvent(CaliScenarioTaskViewModel caliScenarioTaskViewModel)
        {
            string method = nameof(UnRegisterCalibrationEvent);

            logger.Debug($"[{method}] Entered, [Input] CaliScenarioTaskViewModel(={caliScenarioTaskViewModel?.GetHashCode()})");
            if (caliScenarioTaskViewModel == null)
            {
                return;
            }

            AutoCalibrationProxy.Instance.CalcStatusChanged -= caliScenarioTaskViewModel.OnCalcStatusChanged;
        }

        public void RegisterCalibrationEvent(CaliScenarioTaskViewModel caliScenarioTaskViewModel)
        {
            string method = nameof(RegisterCalibrationEvent);
            logger.Info($"[{method}] Entered, [Input] CaliScenarioTaskViewModel(={caliScenarioTaskViewModel?.GetHashCode()})");

            if (caliScenarioTaskViewModel == null)
            {
                return;
            }

            logger.Debug($"[{method}] First, Try to Undo Register Calibration Event");
            UnRegisterCalibrationEvent(caliScenarioTaskViewModel);

            logger.Debug($"[{method}] Then Register Calibration Event");
            AutoCalibrationProxy.Instance.CalcStatusChanged += caliScenarioTaskViewModel.OnCalcStatusChanged;
        }

        private static readonly LogWrapper logger = new LogWrapper(ServiceCategory.AutoCali);
    }
}
