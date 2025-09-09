using NV.CT.Service.TubeWarmUp.DAL;
using NV.CT.Service.TubeWarmUp.DAL.Dtos;
using NV.CT.Service.TubeWarmUp.Interfaces;
using NV.MPS.Configuration;
using System;

namespace NV.CT.Service.TubeWarmUp.Services
{
    public class ConfigService : IConfigService
    {
        private WarmUpConfigRepository _configRepository;

        public ConfigService()
        {
            this._configRepository = new WarmUpConfigRepository();
        }

        public WarmUpConfigDto GetConfig()
        {
            return this._configRepository.Get(0);
        }

        public (ushort min, ushort max) GetTubeHeatCaps()
        {
            var sourceComponentInfo = SystemConfig.SourceComponentConfig.SourceComponent;

            return ((ushort)sourceComponentInfo.PreheatCapThreshold.Value, (ushort)sourceComponentInfo.AlertHeatCapThreshold.Value);
        }

        public int GetStandbyTimeout()
        {
            return (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
        }
    }
}