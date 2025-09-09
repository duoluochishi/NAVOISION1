using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums.Components;
using NV.CT.Service.HardwareTest.Models.Integrations.SystemEnvironment.Data;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.Service.HardwareTest.Models.Integrations.SystemEnvironment.Parts
{
    internal class GantryPartModel : SystemEnvironmentPartAbstract
    {
        public GantryPartModel(CavitySettingConfig config) : base(SystemEnvironmentPartType.GantryCavity, "Gantry Cavity")
        {
            DataInfos =
            [
                new EnableDataModel(SystemEnvironmentPartType.GantryCavity, "Alarm Enable", config.AuxBoard.IsMonitoring),
                new RangeDataModel(SystemEnvironmentPartType.GantryCavity,
                                   ComponentTemperatureHumidityType.AuxBoardHumidity,
                                   "Humidity", "%",
                                   ((double)config.AuxBoard_Humidity.Max).ReduceTen(),
                                   ((double)config.AuxBoard_Humidity.Min).ReduceTen()),
                new RangeDataModel(SystemEnvironmentPartType.GantryCavity,
                                   ComponentTemperatureHumidityType.AuxBoardUpTemperature,
                                   "Up Temperature", "°C",
                                   ((double)config.AuxBoard_Top_Temperature.Max).ReduceTen(),
                                   ((double)config.AuxBoard_Top_Temperature.Min).ReduceTen()),
                new RangeDataModel(SystemEnvironmentPartType.GantryCavity,
                                   ComponentTemperatureHumidityType.AuxBoardDownTemperature,
                                   "Down Temperature", "°C",
                                   ((double)config.AuxBoard_Bottom_Temperature.Max).ReduceTen(),
                                   ((double)config.AuxBoard_Bottom_Temperature.Min).ReduceTen()),
                new RangeDataModel(SystemEnvironmentPartType.GantryCavity,
                                   ComponentTemperatureHumidityType.AuxBoardLeftTemperature,
                                   "Left Temperature", "°C",
                                   ((double)config.AuxBoard_Left_Temperature.Max).ReduceTen(),
                                   ((double)config.AuxBoard_Left_Temperature.Min).ReduceTen()),
                new RangeDataModel(SystemEnvironmentPartType.GantryCavity,
                                   ComponentTemperatureHumidityType.AuxBoardRightTemperature,
                                   "Right Temperature", "°C",
                                   ((double)config.AuxBoard_Right_Temperature.Max).ReduceTen(),
                                   ((double)config.AuxBoard_Right_Temperature.Min).ReduceTen()),
            ];
        }

        public override void ReceivedComponentCycleStatus(CycleStatusArgs arg)
        {
            ((RangeDataModel)DataInfos[1]).Value = ((double)arg.Device.AuxBoard.Humidity).ReduceTen();
            ((RangeDataModel)DataInfos[2]).Value = ((double)arg.Device.AuxBoard.UpTemperature).ReduceTen();
            ((RangeDataModel)DataInfos[3]).Value = ((double)arg.Device.AuxBoard.DownTemperature).ReduceTen();
            ((RangeDataModel)DataInfos[4]).Value = ((double)arg.Device.AuxBoard.LeftTemperature).ReduceTen();
            ((RangeDataModel)DataInfos[5]).Value = ((double)arg.Device.AuxBoard.RightTemperature).ReduceTen();
        }
    }
}