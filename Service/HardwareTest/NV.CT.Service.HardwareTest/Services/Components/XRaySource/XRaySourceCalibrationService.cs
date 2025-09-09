using NV.CT.Service.HardwareTest.Models.Components.XRaySource;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NV.CT.Service.HardwareTest.Services.Components.XRaySource
{
    public class XRaySourceCalibrationService
    {
        public static readonly XRaySourceCalibrationService Instance = new XRaySourceCalibrationService();

        private XRaySourceCalibrationService() { }

        #region Fields

        //Tube板的数量
        private const uint InterfaceCount = 6;
        //每个Tube板的射线源数量
        private const uint XRaySourcesPerInterface = 4;

        #endregion

        /// <summary>
        /// 计算射线源所在板的Index和Offset
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public (uint interfaceIndex, uint xRaySourceOffset) CalculateXRaySourceIndexAndOffset(XRayOriginSource source)
        {
            //板编号
            uint interfaceIndex = (source.Index - 1) / XRaySourcesPerInterface;
            //源偏移
            uint xRaySourceOffset = (source.Index - 1) % XRaySourcesPerInterface;

            return (interfaceIndex, xRaySourceOffset);
        }

        /// <summary>
        /// 根据interfaceIndex获取对应板的校准状态
        /// </summary>
        /// <param name="interfaceIndex"></param>
        /// <returns></returns>
        public uint GetTubeInterfaceCalibrationStatus(IEnumerable<XRayOriginSource> xRaySources, uint interfaceIndex)
        {
            //初始化interface状态
            int interfaceStatus = 0;
            //获取interface相关4源状态
            int index1 = (int)(0 + interfaceIndex * XRaySourcesPerInterface);
            int index2 = (int)(1 + interfaceIndex * XRaySourcesPerInterface);
            int index3 = (int)(2 + interfaceIndex * XRaySourcesPerInterface);
            int index4 = (int)(3 + interfaceIndex * XRaySourcesPerInterface);
            int status1 = xRaySources.ElementAt(index1).IsChecked ? 1 << 0 : 0 << 0;
            int status2 = xRaySources.ElementAt(index2).IsChecked ? 1 << 1 : 0 << 1;
            int status3 = xRaySources.ElementAt(index3).IsChecked ? 1 << 2 : 0 << 2;
            int status4 = xRaySources.ElementAt(index4).IsChecked ? 1 << 3 : 0 << 3;
            //更新状态
            interfaceStatus = status1 | status2 | status3 | status4;

            return (uint)interfaceStatus;
        }

        /// <summary>
        /// 根据当前板的校准状态，生成某个源被点击后的校准状态（即同板其他源状态不变）
        /// </summary>
        /// <param name="source"></param>
        /// <param name="xRaySourceOffset"></param>
        /// <param name="currentInterfaceCaliStatus"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public uint CalculateUpdatedCalibrationStatus(XRayOriginSource source, uint xRaySourceOffset, uint currentInterfaceCaliStatus)
        {
            return xRaySourceOffset switch
            {
                0 => source.IsChecked ? 1 << 0 | currentInterfaceCaliStatus : 0 << 0 | currentInterfaceCaliStatus,
                1 => source.IsChecked ? 1 << 1 | currentInterfaceCaliStatus : 0 << 1 | currentInterfaceCaliStatus,
                2 => source.IsChecked ? 1 << 2 | currentInterfaceCaliStatus : 0 << 2 | currentInterfaceCaliStatus,
                3 => source.IsChecked ? 1 << 3 | currentInterfaceCaliStatus : 0 << 3 | currentInterfaceCaliStatus,
                _ => throw new ArgumentException(nameof(xRaySourceOffset))
            };
        }

    }
}
