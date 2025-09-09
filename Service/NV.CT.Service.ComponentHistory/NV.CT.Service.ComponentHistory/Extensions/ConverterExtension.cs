using System;
using NV.CT.FacadeProxy.Common.Enums.Components;
using NV.CT.Service.ComponentHistory.Models;
using NV.CT.ServiceFramework.Model;
using NV.MPS.Configuration;

namespace NV.CT.Service.ComponentHistory.Extensions
{
    internal static class ConverterExtension
    {
        public static DeviceComponentType ToDeviceComponentType(this SerialNumberComponentType type)
        {
            return type switch
            {
                SerialNumberComponentType.XRaySourceTankbox => DeviceComponentType.XRaySourceTankbox,
                SerialNumberComponentType.XRaySourceBuckbox => DeviceComponentType.XRaySourceBuckbox,
                SerialNumberComponentType.Collimator => DeviceComponentType.Collimator,
                SerialNumberComponentType.TransmitBoard => DeviceComponentType.TransmissionBoard,
                SerialNumberComponentType.DetectorUnit => DeviceComponentType.DetectorUnit,
                SerialNumberComponentType.TemperatureControlBoard => DeviceComponentType.TemperatureControlBoard,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static ComponentInfo ToComponentInfo(this ComponentEntryItemModel item, DateTime installTime)
        {
            return new()
            {
                Id = item.ID,
                ComponentType = item.ComponentType.ToDeviceComponentType(),
                SerialNumber = item.DeviceSN,
                UsingBeginTime = installTime,
            };
        }

        public static ComponentExchange ToComponentExchange(this ComponentEntryItemModel item)
        {
            return new()
            {
                Id = item.ID,
                DeviceType = item.ComponentType.ToDeviceComponentType(),
            };
        }

        public static ComponentEntryItemHistoryModel ToHistoryModel(this ComponentEntryItemModel item)
        {
            return new()
            {
                Name = $"{item.SeniorName} {item.Name}",
                SN = item.LocalSN,
                InstallTime = item.InstallTime,
                RetireTime = null,
            };
        }
    }
}