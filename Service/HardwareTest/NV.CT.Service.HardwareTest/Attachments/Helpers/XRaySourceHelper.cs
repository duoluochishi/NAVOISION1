using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Service.HardwareTest.Models.Components.XRaySource;
using NV.CT.Service.HardwareTest.Share.Enums.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NV.CT.Service.HardwareTest.Attachments.Helpers
{
    public static class XRaySourceHelper
    {

        /// <summary>
        /// 计算焦点值
        /// </summary>
        /// <param name="focal"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static uint CalculateFocusValue(FocalType focal, XRaySourceIndex index)
        {
            /** 小焦点 **/
            if (focal == FocalType.Small)
            {
                return 0;
            }
            /** 大焦点 **/
            else
            {
                if (index == XRaySourceIndex.All)
                {
                    return 15;
                }
                else
                {
                    return ((uint)index % 4) switch
                    {
                        0 => 8,
                        1 => 1,
                        2 => 2,
                        3 => 4,
                        _ => throw new ArgumentException(nameof(index))
                    };
                }
            }
        }

        /// <summary>
        /// 解析射线源状态
        /// </summary>
        /// <returns></returns>
        public static XRaySourceStatus AnalyseXRaySourceStatus(uint content) 
        {
            if ((content & 0x1) == 0)
            {
                return XRaySourceStatus.Offline;
            }
            else 
            {
                if (((content >> 5) & 0x1) == 1)
                {
                    return XRaySourceStatus.Error;
                }
                else if(((content >> 7) & 0x1) == 1)
                {
                    return XRaySourceStatus.CalibrationComplete;
                }
                else if (((content >> 8) & 0x1) == 1)
                {
                    return XRaySourceStatus.CalibrationFailed;
                }

                return XRaySourceStatus.Online;
            }
        }

        /// <summary>
        /// 更新射线源状态
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="dose"></param>
        public static void UpdateXRaySourceDose(IEnumerable<XRayOriginSource> sources, XRaySourceDose dose) 
        {
            switch (dose.Type) 
            {
                case XRaySourceDoseType.kV:
                    {
                        sources.ElementAt((int)dose.Index - 1).Voltage = dose.Value;
                    }
                    break;
                case XRaySourceDoseType.mA:
                    {
                        sources.ElementAt((int)dose.Index - 1).Current = dose.Value;
                    }
                    break;
                case XRaySourceDoseType.ms:
                    {
                        sources.ElementAt((int)dose.Index - 1).ExposureTime = dose.Value;
                    }
                    break;
            }
        }

    }
}
