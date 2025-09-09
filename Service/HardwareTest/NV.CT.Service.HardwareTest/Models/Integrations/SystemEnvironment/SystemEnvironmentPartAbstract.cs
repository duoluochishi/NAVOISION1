using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;

namespace NV.CT.Service.HardwareTest.Models.Integrations.SystemEnvironment
{
    internal abstract class SystemEnvironmentPartAbstract(SystemEnvironmentPartType partType, string title) : ObservableObject
    {
        public SystemEnvironmentPartType PartType { get; } = partType;
        public string Title { get; } = title;
        public SystemEnvironmentDataAbstract[] DataInfos { get; protected init; } = [];
        public abstract void ReceivedComponentCycleStatus(CycleStatusArgs arg);
    }
}