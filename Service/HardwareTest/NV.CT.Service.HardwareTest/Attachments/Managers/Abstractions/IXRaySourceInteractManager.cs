using NV.CT.FacadeProxy.Common.Models.Generic;
using NV.CT.Service.HardwareTest.Models.Components.XRaySource;
using NV.CT.Service.HardwareTest.Share.Enums.Components;
using NV.CT.Service.HardwareTest.Share.Models;
using System.Collections.Generic;

namespace NV.CT.Service.HardwareTest.Attachments.Managers.Abstractions
{
    public interface IXRaySourceInteractManager
    {
        GenericResponse<bool, IEnumerable<LoggerMessage>> SwitchXRaySourceCalibration(IEnumerable<XRayOriginSource> allSources, XRayOriginSource source, string componentName);
        GenericResponse<bool, IEnumerable<LoggerMessage>> SwitchXRayPromptLight(XRayPromptLightSwitch lightSwitch, string componentName);
    }
}