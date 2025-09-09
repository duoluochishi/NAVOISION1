using NV.CT.Service.TubeWarmUp.DAL.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeWarmUp.Interfaces
{
    public interface IConfigService
    {
        WarmUpConfigDto GetConfig();

        (ushort min, ushort max) GetTubeHeatCaps();

        /// <summary>
        /// 获取调用开始扫描接口时等待Standby状态的超时时间
        /// 单位 ms
        /// </summary>
        /// <returns></returns>
        int GetStandbyTimeout();
    }
}