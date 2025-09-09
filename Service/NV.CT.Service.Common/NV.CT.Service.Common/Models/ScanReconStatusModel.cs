using System.Collections.Generic;
using System.Linq;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Service.Common.Enums;

namespace NV.CT.Service.Common.Models
{
    public class ScanReconStatusModel
    {
        private readonly List<(string, ScanReconStatusType)> _errorCodes = new();

        /// <summary>
        /// 实时状态
        /// </summary>
        public RealtimeStatus? RealtimeStatus { get; set; }

        /// <summary>
        /// 实时重建状态
        /// </summary>
        public AcqReconStatus? AcqReconStatus { get; set; }

        /// <summary>
        /// 是否手动取消采集
        /// </summary>
        public bool IsCancelManual { get; set; }

        /// <summary>
        /// 获取采集重建状态
        /// </summary>
        /// <returns></returns>
        public ScanReconStatus GetScanReconStatus() => ScanReconHelper.GetScanReconStatus(RealtimeStatus, AcqReconStatus, IsCancelManual);

        /// <summary>
        /// 添加错误码
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="statusType"></param>
        public void AddErrorCode(string errorCode, ScanReconStatusType statusType)
        {
            if (string.IsNullOrWhiteSpace(errorCode) || _errorCodes.Contains((errorCode, statusType)))
            {
                return;
            }

            _errorCodes.Add((errorCode, statusType));
        }

        /// <summary>
        /// 添加错误码
        /// </summary>
        /// <param name="errorCodes"></param>
        /// <param name="statusType"></param>
        public void AddErrorCode(IEnumerable<string> errorCodes, ScanReconStatusType statusType)
        {
            foreach (var errorCode in errorCodes)
            {
                AddErrorCode(errorCode, statusType);
            }
        }

        /// <summary>
        /// 获取错误码（返回错误码列表中的第一个）
        /// </summary>
        /// <returns></returns>
        public (string ErrorCode, ScanReconStatusType StatusType)? GetErrorCode() => _errorCodes.FirstOrDefault();

        /// <summary>
        /// 获取错误码列表
        /// </summary>
        /// <returns></returns>
        public (string ErrorCode, ScanReconStatusType StatusType)[] GetErrorCodes() => _errorCodes.ToArray();

        public void Reset()
        {
            RealtimeStatus = null;
            AcqReconStatus = null;
            IsCancelManual = false;
            _errorCodes.Clear();
        }
    }
}