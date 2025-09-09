using NV.CT.DmsTest.Model;
using NV.CT.DmsTest.Utilities;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common.Models.ScanReconModels;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace NV.CT.DmsTest.Controllers
{
    public class ProxyWrapper
    {
        public static readonly ProxyWrapper Instance = new ProxyWrapper();
        private ProxyWrapper()
        {
        }


        public bool InitRegisteEvent()
        {
            bool InitStatus = true;
            try
            {
                DataAcquisitionProxy.Instance.DeviceConnectionChanged += OnConnectionChanged;
                DataAcquisitionProxy.Instance.SystemStatusChanged += OnSystemStatusChanged;
                DataAcquisitionProxy.Instance.CycleStatusChanged += OnCycleStateChanged;
                DataAcquisitionProxy.Instance.RealTimeStatusChanged += OnRealTimeChanged;
                DataAcquisitionProxy.Instance.RawImageSaved += OnRawImageSave;
                DataAcquisitionProxy.Instance.AcqReconStatusChanged += AcqStatusChanged;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());

                InitStatus = false;
            }
            return InitStatus;
        }

        public bool CloseRegisteEvent()
        {
            bool colseStatus = false;
            try
            {
                DataAcquisitionProxy.Instance.DeviceConnectionChanged -= OnConnectionChanged;
                DataAcquisitionProxy.Instance.SystemStatusChanged -= OnSystemStatusChanged;
                DataAcquisitionProxy.Instance.CycleStatusChanged -= OnCycleStateChanged;
                DataAcquisitionProxy.Instance.RealTimeStatusChanged -= OnRealTimeChanged;
                DataAcquisitionProxy.Instance.RawImageSaved -= OnRawImageSave;
                DataAcquisitionProxy.Instance.AcqReconStatusChanged -= AcqStatusChanged;

            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                colseStatus = false;
            }
            return colseStatus;
        }



        private bool CheckScanEnabled(RealtimeStatus curRealTimeState)
        {
            return curRealTimeState == RealtimeStatus.Standby || RealtimeStatus.Validated == curRealTimeState;
        }

        private void CheckAcqStatus()
        {
            if ((DmsTestAcqStatus == AcqReconStatus.Finished) && (RealtimeStatus >= RealtimeStatus.NormalScanStopped))
            {
                AutoResetEvent!.Set();
            }

        }

        #region "event "
        public void AcqStatusChanged(object arg1, AcqReconStatusArgs arg2)
        {

            DmsTestAcqStatus = arg2.Status;
            RawDataPath = arg2.RawDataPath;
            //Console.WriteLine("RawDataPath {0}", arg2.RawDataPath);
            CheckAcqStatus();
        }




        public void OnRawImageSave(object arg1, RawImageSavedEventArgs arg2)
        {

        }

        public void OnRealTimeChanged(object arg1, RealtimeEventArgs arg2)
        {
            ConsoleLogPrinter.Info($"RealTime Status {arg2.Status}");
            RealtimeStatus = arg2.Status;

        }

        public void OnSystemStatusChanged(object arg1, SystemStatusArgs arg2)
        {
            SystemStatus = arg2.Status;
        }

        public void OnCycleStateChanged(object arg1, CycleStatusArgs arg2)
        {
            CycleStatusArgs = arg2;
        }

        private bool? deviceConnected;
        private void OnConnectionChanged(object arg1, ConnectionStatusArgs arg2)
        {
            bool changed = false;
            bool isConnected = arg2.Connected;
            if (!deviceConnected.HasValue)
            {
                changed = true;
                deviceConnected = isConnected;
            }
            else if (deviceConnected.Value != isConnected)
            {
                changed = true;
                deviceConnected = isConnected;
            }

            if (changed)
            {
                if (deviceConnected.Value)
                {
                    ConsoleLogPrinter.Info("Device Connected!");
                }
                else
                {
                    ConsoleLogPrinter.Info("Device Disconnected!");
                }
            }
        }
        #endregion


        public bool RunScan(ScanReconParamModel scanReconParam)
        {
            ConsoleLogPrinter.Info($" Scan Start!");
            var res = NV.CT.Service.Common.Wrappers.ScanReconWrapper.StartScan(scanReconParam);

            if (CommandStatus.Success != res.Status)
            {
                Logger.Error(" Start Scan Failed!");
                return false;
            }
            AutoResetEvent?.Reset();
            // 等待扫描结束
            AutoResetEvent?.WaitOne();
            if (RealtimeStatus == RealtimeStatus.Error || DmsTestAcqStatus == AcqReconStatus.Error)
            {
                return false;
            }
            ConsoleLogPrinter.Info($" Scan End!");
            return true;
        }

        public bool StopScan()
        {

            //var res = NV.CT.FacadeProxy.DataAcquisitionProxy.Instance.StopDataAcquisition();
            //if (res.Status)
            //{
            //    Logger.Error("Stop Scan Failed!");
            //    return false;
            //}
            return true;
        }


        public bool AbortDmsTestCalc()
        {
            var res = NV.CT.FacadeProxy.DataAcquisitionProxy.Instance.StopDataAcquisition();
            if (!res.Status)
            {
                return false;
            }

            return true;
        }




        /// <summary>
        /// 将测试项的关键参数转换为扫描参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static ScanParamModel ToFreeProtocolScanParam(CoreScanParam param)
        {

            FreeProtocolScanParamModel scanParamModule = new FreeProtocolScanParamModel();

            scanParamModule.ExposureTime = param.ExpTime;
            scanParamModule.FrameTime = param.FrameTime;
            scanParamModule.Gain = param.Gain;
            scanParamModule.FramesPerCycle = 1000;
            scanParamModule.TotalFrames = param.TotalFrame;
            scanParamModule.AutoDeleteNum = param.AutoDelFrame;
            //scanParamModule.TotalFrames = param.FramesPerCycle;
            scanParamModule.Voltage.Add(param.Kv);
            scanParamModule.Current.Add(param.Ma);
            scanParamModule.PreOffsetEnable = Service.Common.Enums.EnableType.Enable;


            if (param.XRaySourceIndex != XRaySourceNumber.All)
            {
                scanParamModule.ScanOption = ScanOption.Surview;
                scanParamModule.XRaySourceIndex = (uint)param.XRaySourceIndex;
            }

            return scanParamModule;
        }




        #region "属性" 
        private AutoResetEvent? AutoResetEvent = new AutoResetEvent(false);


        /// <summary>
        /// 生数据根目录
        /// </summary>
        public string? RawDataPath { get; private set; }
        /// <summary>
        /// 设备实时状态变化，更新标识“设备是否可用”。
        /// 使用信号量AutoResetEvent不准确，因为发起下一次扫描，需要同时判断：1.设备的可用性，2.acqReconStatus已完成，
        /// 而这二者时序无法保证先后
        /// </summary>
        private bool IsScanEnabled { get; set; }
        /// <summary>
        /// 采集状态
        /// </summary>
        private AcqReconStatus DmsTestAcqStatus { get; set; }
        /// <summary>
        /// 实时状态
        /// </summary>
        private RealtimeStatus RealtimeStatus { get; set; }

        public SystemStatus SystemStatus { get; set; }

        public CycleStatusArgs CycleStatusArgs { get; set; }


        private ErrorCodes ErrorCodes { get; set; }

        #endregion

        public ErrorCodes GetErrorInfo()
        {
            return ErrorCodes;
        }

        public static object BytesToStuct(byte[] bytes, Type type, int offset)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(type);
            //byte数组长度小于结构体的大小
            if (size > bytes.Length)
            {
                //返回空
                return null;
            }
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将byte数组拷到分配好的内存空间
            Marshal.Copy(bytes, offset, structPtr, size);
            //将内存空间转换为目标结构体
            object obj = Marshal.PtrToStructure(structPtr, type)!;
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回结构体
            return obj;
        }

    }
}
