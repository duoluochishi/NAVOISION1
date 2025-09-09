using System.Collections.Generic;
using System.Linq;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Common.Models.Common;
using NV.CT.Service.Common.Models.ScanReconModels;

namespace NV.CT.Service.Common.Wrappers
{
    public static class ScanReconWrapper
    {
        /// <summary>
        /// 开始扫描
        /// </summary>
        /// <param name="item"></param>
        /// <returns>命令执行结果</returns>
        public static CommandResult StartScan(ScanReconParamModel item)
        {
            return StartScan(new List<ScanReconParamModel> { item });
        }

        /// <summary>
        /// 开始扫描
        /// </summary>
        /// <param name="items"></param>
        /// <remarks>
        /// 参数列表中的协议需要一致，即不要将正式协议和自由协议混在一个List内
        /// <para>请注意！自由协议暂不支持List，会强制仅取列表内的第1个元素。或使用其它的重载方法</para>
        /// </remarks>
        /// <returns>命令执行结果</returns>
        public static CommandResult StartScan(List<ScanReconParamModel> items)
        {
            if (items is not { Count: > 0 })
            {
                return new CommandResult { Status = CommandStatus.Success };
            }

            switch (items.First().ScanParameter)
            {
                case FreeProtocolScanParamModel freeModel:
                {
                    var info = freeModel.Converter();
                    var proxyRes = DataAcquisitionProxy.Instance.ConfigureDataAcquisition(info);
                    var result = new CommandResult { Status = CommandStatus.Success };

                    if (!proxyRes.Status)
                    {
                        result.Status = CommandStatus.Failure;
                        result.AddErrorCode(proxyRes.Message);
                        return result;
                    }

                    proxyRes = DataAcquisitionProxy.Instance.StartDataAcquisition(info);

                    if (!proxyRes.Status)
                    {
                        result.Status = CommandStatus.Failure;
                        result.AddErrorCode(proxyRes.Message);
                    }

                    return result;
                }
                default:
                {
                    var infos = items.Select(i => i.Converter()).ToList();
                    return AcqReconProxy.Instance.StartScan(infos);
                }
            }
        }

        /// <inheritdoc cref="OfflineMachineTaskProxy.CreateOfflineReconTask(ScanReconParam, TaskPriority)"/>
        public static TaskExecuteResult CreateOfflineReconTask(ScanReconParamModel info, TaskPriority priority)
        {
            return OfflineMachineTaskProxy.Instance.CreateOfflineReconTask(info.Converter(), priority);
        }
    }
}