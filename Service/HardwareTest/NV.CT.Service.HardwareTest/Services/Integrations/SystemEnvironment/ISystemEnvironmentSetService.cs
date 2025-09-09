using System.Collections.Generic;
using NV.CT.Service.HardwareTest.Models.Integrations.SystemEnvironment;
using NV.CT.Service.Models;

namespace NV.CT.Service.HardwareTest.Services.Integrations.SystemEnvironment
{
    internal interface ISystemEnvironmentSetService
    {
        public (GenericResponse Result, bool IsErrorCode) Set(SystemEnvironmentDataAbstract item);
        public (GenericResponse Result, bool IsErrorCode) SetAll(SystemEnvironmentPartAbstract partItem);
        public (GenericResponse Result, bool IsErrorCode) SetAllParts(SystemEnvironmentPartAbstract[] partItems);
    }
}