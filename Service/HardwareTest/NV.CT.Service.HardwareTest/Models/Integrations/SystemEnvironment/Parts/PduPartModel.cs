using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums.Components;
using NV.CT.Service.HardwareTest.Models.Integrations.SystemEnvironment.Data;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.Service.HardwareTest.Models.Integrations.SystemEnvironment.Parts
{
    internal class PduPartModel : SystemEnvironmentPartAbstract
    {
        public PduPartModel(CavitySettingConfig config) : base(SystemEnvironmentPartType.PduCavity, "PDU Cavity")
        {
            DataInfos =
            [
                new EnableDataModel(SystemEnvironmentPartType.PduCavity, "Alarm Enable", config.PDU.IsMonitoring),
                new RangeDataModel(SystemEnvironmentPartType.PduCavity,
                                   ComponentTemperatureHumidityType.PduTemperature,
                                   "Temperature", "°C",
                                   ((double)config.PDU_Temperature.Max).ReduceTen(),
                                   ((double)config.PDU_Temperature.Min).ReduceTen()),
            ];
        }

        public override void ReceivedComponentCycleStatus(CycleStatusArgs arg)
        {
            ((RangeDataModel)DataInfos[1]).Value = ((double)arg.Device.PDU.Temperature).ReduceTen();
        }
    }
}