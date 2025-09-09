using System;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Service.Common.Enums;

namespace NV.CT.Service.Common
{
    /// <summary>
    /// 扫描和实时重建业务的辅助类
    /// <para/>比如急停弹窗警告、真实状态解析等
    /// </summary>
    public static class ScanReconHelper
    {
        public static void AlertEmergencyScanStopped()
        {
            string msg = "Emergency Scan Stopped";
            DialogService.Instance.ShowError(msg);
        }

        /// <summary>
        /// 根据实时状态和采集、实时重建状态获取简化版、更实用版的扫描、采集、实时重建状态
        /// </summary>
        /// <param name="realtimeStatus">实时状态</param>
        /// <param name="acqReconStatus">实时重建状态</param>
        /// <param name="isCancelManual">是否手动执行了取消操作</param>
        /// <returns></returns>
        /// <remarks>
        /// 判断依据来自表格：<seealso href="https://www.kdocs.cn/l/cbBcABtCUFVQ"/>
        /// </remarks>
        /// <exception cref="NotSupportedException"></exception>
        public static ScanReconStatus GetScanReconStatus(RealtimeStatus? realtimeStatus, AcqReconStatus? acqReconStatus, bool isCancelManual)
        {
            return realtimeStatus switch
            {
                null or
                RealtimeStatus.None or 
                RealtimeStatus.Init or 
                RealtimeStatus.Standby or 
                RealtimeStatus.Validated => acqReconStatus switch
                {
                    null => ScanReconStatus.UnStarted,
                    _ => throw ThrowUnsupportedCombine(),
                },
                RealtimeStatus.ParamConfig => acqReconStatus switch
                {
                    null or
                    AcqReconStatus.Loaded or
                    AcqReconStatus.Reconning or
                    AcqReconStatus.Finished or
                    AcqReconStatus.Error => ScanReconStatus.Inprogress,
                    _ => throw ThrowUnsupportedCombine(),
                },
                RealtimeStatus.MovingPartEnable or 
                RealtimeStatus.MovingPartEnabling or 
                RealtimeStatus.MovingPartEnabled or 
                RealtimeStatus.ExposureEnable or 
                RealtimeStatus.ExposureStarted => acqReconStatus switch
                {
                    null or
                    AcqReconStatus.Loaded or
                    AcqReconStatus.Reconning or 
                    AcqReconStatus.Finished => ScanReconStatus.Inprogress,
                    _ => throw ThrowUnsupportedCombine(),
                },
                RealtimeStatus.ExposureSpoting or
                RealtimeStatus.ExposureSpotingIdle or
                RealtimeStatus.ExposureFinished => acqReconStatus switch
                {
                    null or
                    AcqReconStatus.Loaded or
                    AcqReconStatus.Reconning or 
                    AcqReconStatus.Finished => ScanReconStatus.Inprogress,
                    AcqReconStatus.Error => ScanReconStatus.Error,
                    _ => throw ThrowUnsupportedCombine(),
                },
                RealtimeStatus.ScanStopping => acqReconStatus switch
                {
                    null or
                    AcqReconStatus.Loaded or
                    AcqReconStatus.Reconning or
                    AcqReconStatus.Finished => ScanReconStatus.Inprogress,
                    AcqReconStatus.Error => ScanReconStatus.Error,
                    _ => throw ThrowUnsupportedCombine(),
                },
                RealtimeStatus.NormalScanStopped => acqReconStatus switch
                {
                    null or
                    AcqReconStatus.Loaded or
                    AcqReconStatus.Reconning => ScanReconStatus.Inprogress,
                    AcqReconStatus.Finished => isCancelManual ? ScanReconStatus.Cancelled : ScanReconStatus.Finished,
                    AcqReconStatus.Error => ScanReconStatus.Error,
                    AcqReconStatus.Cancelled => ScanReconStatus.Cancelled,
                    _ => throw ThrowUnsupportedCombine(),
                },
                RealtimeStatus.Error => acqReconStatus switch
                {
                    AcqReconStatus.Loaded or
                    AcqReconStatus.Reconning => ScanReconStatus.Inprogress,
                    null or
                    AcqReconStatus.Finished or
                    AcqReconStatus.Error or
                    AcqReconStatus.Cancelled => ScanReconStatus.Error,
                    _ => throw ThrowUnsupportedCombine(),
                },
                RealtimeStatus.EmergencyScanStopped => acqReconStatus switch
                {
                    AcqReconStatus.Loaded or
                    AcqReconStatus.Reconning => ScanReconStatus.Inprogress,
                    null or
                    AcqReconStatus.Finished or
                    AcqReconStatus.Error or
                    AcqReconStatus.Cancelled => ScanReconStatus.Emergency,
                    _ => throw ThrowUnsupportedCombine(),
                },
                _ => throw ThrowUnsupportedCombine(),
            };

            Exception ThrowUnsupportedCombine()
            {
                var ex = new NotSupportedException($"Not Supported Scan-Recon Status Combine: RealtimeStatus - {(realtimeStatus?.ToString() ?? "null")}, AcqReconStatus - {(acqReconStatus?.ToString() ?? "null")}");
                LogService.Instance.Error(ServiceCategory.Common, $"{nameof(ScanReconHelper)}.{nameof(GetScanReconStatus)} Throw NotSupportedException.", ex);
                return ex;
            }
        }
    }
}