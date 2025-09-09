using NV.CT.FacadeProxy.Common.Models.Generic;
using NV.CT.Service.HardwareTest.Share.Addresses;
using NV.CT.Service.HardwareTest.Share.Enums.Components;
using System;

namespace NV.CT.Service.HardwareTest.Services.Components.XRaySource
{
    public class XRaySourceCommandService
    {
        public static readonly XRaySourceCommandService Instance = new XRaySourceCommandService();

        private XRaySourceCommandService() { }

        public WriteCommand AssembleXRaySourceCalibrationCommand(uint interfaceIndex, uint calibrateStatus)
        {
            //根据interfaceIndex选择寄存器地址
            uint address = interfaceIndex switch
            {
                0 => ExposureRegisterAddresses.TubeInterface1CalibrateSwitch,
                1 => ExposureRegisterAddresses.TubeInterface2CalibrateSwitch,
                2 => ExposureRegisterAddresses.TubeInterface3CalibrateSwitch,
                3 => ExposureRegisterAddresses.TubeInterface4CalibrateSwitch,
                4 => ExposureRegisterAddresses.TubeInterface5CalibrateSwitch,
                5 => ExposureRegisterAddresses.TubeInterface6CalibrateSwitch,
                _ => throw new ArgumentException(nameof(interfaceIndex))
            };

            return CommandFactory.CreateWriteCommand("XRaySource Calibrate", address, calibrateStatus);
        }

        public WriteCommand AssembleXRayPromptLightSwitchCommand(XRayPromptLightSwitch lightSwitch)
        {
            return CommandFactory.CreateWriteCommand("XRay Prompt Light Switch", IfBoxRegisterAddresses.LightSwitch, (uint)lightSwitch);
        }
    }
}
