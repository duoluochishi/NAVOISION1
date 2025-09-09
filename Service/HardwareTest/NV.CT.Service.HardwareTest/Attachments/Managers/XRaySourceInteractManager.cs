using NV.CT.FacadeProxy.Common.Models.Generic;
using NV.CT.FacadeProxy.Registers.Helpers;
using NV.CT.Service.HardwareTest.Attachments.Managers.Abstractions;
using NV.CT.Service.HardwareTest.Models.Components.XRaySource;
using NV.CT.Service.HardwareTest.Services.Components.XRaySource;
using NV.CT.Service.HardwareTest.Share.Enums;
using NV.CT.Service.HardwareTest.Share.Enums.Components;
using NV.CT.Service.HardwareTest.Share.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NV.CT.Service.HardwareTest.Attachments.Managers
{
    public class XRaySourceInteractManager : IXRaySourceInteractManager
    {
        public XRaySourceInteractManager(){ }

        #region Fields

        //灯控
        private static volatile bool isXRayPromptLightOn = false;

        #endregion

        #region XRaySource Calibration

        public GenericResponse<bool, IEnumerable<LoggerMessage>> SwitchXRaySourceCalibration(
            IEnumerable<XRayOriginSource> allSources, XRayOriginSource source, string componentName)
        {
            //消息builder
            List<LoggerMessage> messages = new List<LoggerMessage>();
            //更新信息
            messages.Add(new($"[{componentName}] Start to switch XRay source calibration status.", PrintLevel.Info));
            //获取板编号和源偏移
            (uint interfaceIndex, uint xRaySourceOffset) = XRaySourceCalibrationService.Instance.CalculateXRaySourceIndexAndOffset(source);
            //获取当前对应interface的4源状态
            uint currentInterfaceCaliStatus = XRaySourceCalibrationService.Instance.GetTubeInterfaceCalibrationStatus(allSources, interfaceIndex);
            //计算更新后的interface的4源状态
            uint updateInterfaceCaliStatus = XRaySourceCalibrationService.Instance.CalculateUpdatedCalibrationStatus(source, xRaySourceOffset, currentInterfaceCaliStatus);
            //训管指令
            var calibrationCommand = XRaySourceCommandService.Instance.AssembleXRaySourceCalibrationCommand(interfaceIndex, updateInterfaceCaliStatus);
            //单发指令
            bool status = CommandHelper.SendSingleWriteCommand(calibrationCommand);
            //状态校验
            if (!status)
            {
                //更新信息
                messages.Add(new($"[{componentName}] Failed to send XRaySource calibration command due to a bad connection.", PrintLevel.Error));

                return new(false, messages);
            }
            //更新信息
            messages.Add(new($"[{componentName}] The XRaySource calibration command [{calibrationCommand.Name}] has been sent.", PrintLevel.Info));
            //当前开启校准且当前灯不亮 -> 亮灯
            if (source.IsChecked && !isXRayPromptLightOn)
            {
                //亮灯
                var response = SwitchXRayPromptLight(XRayPromptLightSwitch.ON, componentName);
                //更新信息
                messages.AddRange(response.message);
                //校验
                if (!response.status)
                {
                    return new(false, messages);
                }
            }
            //当前关闭校准且其余源均未处于校准状态 -> 灭灯
            else if (isXRayPromptLightOn && !allSources.Any(t => t.IsChecked == true))
            {
                //灭灯
                var response = SwitchXRayPromptLight(XRayPromptLightSwitch.OFF, componentName);
                //更新信息
                messages.AddRange(response.message);
                //校验
                if (!response.status)
                {
                    return new(false, messages);
                }
            }

            return new(true, messages);
        }

        public GenericResponse<bool, IEnumerable<LoggerMessage>> SwitchXRayPromptLight(XRayPromptLightSwitch lightSwitch, string componentName)
        {
            //消息builder
            List<LoggerMessage> messages = new List<LoggerMessage>();
            //灯控指令
            var promptLightSwitchCommand = XRaySourceCommandService.Instance.AssembleXRayPromptLightSwitchCommand(lightSwitch);
            //单发指令
            bool status = CommandHelper.SendSingleWriteCommand(promptLightSwitchCommand);
            //状态校验
            if (!status)
            {
                //更新信息
                messages.Add(new($"[{componentName}] Failed to send [{Enum.GetName(lightSwitch)}] switch prompt light command due to a bad connection.", PrintLevel.Error));

                return new(false, messages);
            }
            //更新信息
            messages.Add(new($"[{componentName}] The [{Enum.GetName(lightSwitch)}] switch prompt light command [{promptLightSwitchCommand.Name}] has been sent.", PrintLevel.Info));
            //更新亮灯状态
            isXRayPromptLightOn = (lightSwitch == XRayPromptLightSwitch.ON);

            return new(true, messages);
        }

        #endregion

    }
}
