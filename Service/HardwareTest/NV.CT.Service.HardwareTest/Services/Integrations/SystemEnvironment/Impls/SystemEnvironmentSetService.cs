using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.Components;
using NV.CT.FacadeProxy.Common.Models.Components;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Enums;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.Common.Utils;
using NV.CT.Service.HardwareTest.Models.Integrations.SystemEnvironment;
using NV.CT.Service.HardwareTest.Models.Integrations.SystemEnvironment.Data;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;
using NV.CT.Service.Models;
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.Service.HardwareTest.Services.Integrations.SystemEnvironment.Impls
{
    internal class SystemEnvironmentSetService(ILogService logService) : ISystemEnvironmentSetService
    {
        private readonly FrozenDictionary<ComponentTemperatureHumidityType, ExtremumItemField<int>> _temperatureHumidityMapDic = new Dictionary<ComponentTemperatureHumidityType, ExtremumItemField<int>>
        {
            { ComponentTemperatureHumidityType.AuxBoardHumidity, SystemConfig.CavitySetting.AuxBoard_Humidity },
            { ComponentTemperatureHumidityType.AuxBoardUpTemperature, SystemConfig.CavitySetting.AuxBoard_Top_Temperature },
            { ComponentTemperatureHumidityType.AuxBoardDownTemperature, SystemConfig.CavitySetting.AuxBoard_Bottom_Temperature },
            { ComponentTemperatureHumidityType.AuxBoardLeftTemperature, SystemConfig.CavitySetting.AuxBoard_Left_Temperature },
            { ComponentTemperatureHumidityType.AuxBoardRightTemperature, SystemConfig.CavitySetting.AuxBoard_Right_Temperature },
            { ComponentTemperatureHumidityType.PduTemperature, SystemConfig.CavitySetting.PDU_Temperature },
        }.ToFrozenDictionary();

        public (GenericResponse Result, bool IsErrorCode) Set(SystemEnvironmentDataAbstract item)
        {
            return item.DataType switch
            {
                SystemEnvironmentDataType.AlarmEnable => SetAlarmEnable((EnableDataModel)item),
                SystemEnvironmentDataType.TemperatureHumidityRange => SetTemperatureHumidityRange((RangeDataModel)item),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public (GenericResponse Result, bool IsErrorCode) SetAll(SystemEnvironmentPartAbstract partItem)
        {
            var res = SetAllCommand(partItem);

            if (!res.Result.status)
            {
                return res;
            }

            SetAllConfig(partItem);
            return SaveConfig();
        }

        public (GenericResponse Result, bool IsErrorCode) SetAllParts(SystemEnvironmentPartAbstract[] partItems)
        {
            var res = SetAllCommand(partItems);

            if (!res.Result.status)
            {
                return res;
            }

            SetAllConfig(partItems);
            return SaveConfig();
        }

        private (GenericResponse Result, bool IsErrorCode) SetAlarmEnable(EnableDataModel item)
        {
            var res = SetAlarmEnableCommand(item.PartType, item.IsEnable);

            if (!res.Result.status)
            {
                return res;
            }

            SetAlarmEnableConfig(item);
            return SaveConfig();
        }

        private (GenericResponse Result, bool IsErrorCode) SetTemperatureHumidityRange(params RangeDataModel[] items)
        {
            var res = SetTemperatureHumidityRangeCommand(items);

            if (!res.Result.status)
            {
                return res;
            }

            SetTemperatureHumidityRangeConfig(items);
            return SaveConfig();
        }

        private (GenericResponse Result, bool IsErrorCode) SetAlarmEnableCommand(SystemEnvironmentPartType partType, EnableType enableType)
        {
            var enable = enableType == EnableType.Enable;
            var res = partType switch
            {
                SystemEnvironmentPartType.GantryCavity => ComponentStatusProxy.Instance.SetAuxBoardHumidityTemperatureAlarmEnable(enable),
                SystemEnvironmentPartType.PduCavity => ComponentStatusProxy.Instance.SetPduHumidityTemperatureAlarmEnable(enable),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (res.Status != CommandStatus.Success)
            {
                var errorCode = res.ErrorCodes.Codes.First();
                logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SystemEnvironment}] Set {partType} Alarm {enableType} Failed: [{errorCode}] {errorCode.GetErrorCodeDescription()}");
                return (new(false, errorCode), true);
            }

            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SystemEnvironment}] Set {partType} Alarm {enableType} Successful");
            return (new(true, string.Empty), false);
        }

        private (GenericResponse Result, bool IsErrorCode) SetTemperatureHumidityRangeCommand(params RangeDataModel[] items)
        {
            var infos = items.Select(RangeConverter).ToArray();
            var res = ComponentStatusProxy.Instance.SetHumidityTemperatureAlarmRange(infos);

            if (res.Status != CommandStatus.Success)
            {
                var errorCode = res.ErrorCodes.Codes.First();
                logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SystemEnvironment}] Set Temperature Humidity Range Failed: [{errorCode}] {errorCode.GetErrorCodeDescription()}{Environment.NewLine}{JsonUtil.Serialize(infos)}");
                return (new(false, errorCode), true);
            }

            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SystemEnvironment}] Set Temperature Humidity Range Successful{Environment.NewLine}{JsonUtil.Serialize(infos)}");
            return (new(true, string.Empty), false);
        }

        private (GenericResponse Result, bool IsErrorCode) SetAllCommand(params SystemEnvironmentPartAbstract[] partItems)
        {
            var groups = partItems.SelectMany(i => i.DataInfos).GroupBy(i => i.DataType);

            foreach (var group in groups)
            {
                switch (group.Key)
                {
                    case SystemEnvironmentDataType.AlarmEnable:
                    {
                        foreach (var item in group.Cast<EnableDataModel>())
                        {
                            var res = SetAlarmEnableCommand(item.PartType, item.IsEnable);

                            if (!res.Result.status)
                            {
                                return res;
                            }
                        }

                        break;
                    }
                    case SystemEnvironmentDataType.TemperatureHumidityRange:
                    {
                        var items = group.Cast<RangeDataModel>().ToArray();
                        var res = SetTemperatureHumidityRangeCommand(items);

                        if (!res.Result.status)
                        {
                            return res;
                        }

                        break;
                    }
                }
            }

            return (new(true, string.Empty), false);
        }

        private void SetAlarmEnableConfig(params EnableDataModel[] items)
        {
            foreach (var item in items)
            {
                var enable = item.IsEnable == EnableType.Enable;

                switch (item.PartType)
                {
                    case SystemEnvironmentPartType.GantryCavity:
                    {
                        SystemConfig.CavitySetting.AuxBoard.IsMonitoring = enable;
                        break;
                    }
                    case SystemEnvironmentPartType.PduCavity:
                    {
                        SystemConfig.CavitySetting.PDU.IsMonitoring = enable;
                        break;
                    }
                }
            }
        }

        private void SetTemperatureHumidityRangeConfig(params RangeDataModel[] items)
        {
            foreach (var item in items)
            {
                var config = _temperatureHumidityMapDic[item.ComponentType];
                config.Max = (int)item.UpperLimit.ExpandTen();
                config.Min = (int)item.LowerLimit.ExpandTen();
            }
        }

        private void SetAllConfig(params SystemEnvironmentPartAbstract[] partItems)
        {
            var groups = partItems.SelectMany(i => i.DataInfos).GroupBy(i => i.DataType);

            foreach (var group in groups)
            {
                switch (group.Key)
                {
                    case SystemEnvironmentDataType.AlarmEnable:
                    {
                        var items = group.Cast<EnableDataModel>().ToArray();
                        SetAlarmEnableConfig(items);
                        break;
                    }
                    case SystemEnvironmentDataType.TemperatureHumidityRange:
                    {
                        var items = group.Cast<RangeDataModel>().ToArray();
                        SetTemperatureHumidityRangeConfig(items);
                        break;
                    }
                }
            }
        }

        private (GenericResponse Result, bool IsErrorCode) SaveConfig()
        {
            return SystemConfig.SaveCavitySetting() ? (new(true, HardwareTest_Lang.Hardware_SetSuccess), false) : (new(false, HardwareTest_Lang.Hardware_SaveConfigFailed), false);
        }

        private TemperatureHumidityRangeInfo RangeConverter(RangeDataModel item)
        {
            return new()
            {
                Type = item.ComponentType,
                UpperLimit = (int)item.UpperLimit.ExpandTen(),
                LowerLimit = (int)item.LowerLimit.ExpandTen(),
            };
        }
    }
}