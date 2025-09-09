using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;

namespace NV.CT.Service.HardwareTest.Models.Integrations.SystemEnvironment
{
    internal abstract class SystemEnvironmentDataAbstract(SystemEnvironmentPartType partType, SystemEnvironmentDataType dataType, string title) : ObservableObject
    {
        /// <summary>
        /// 所属的腔体类型
        /// </summary>
        public SystemEnvironmentPartType PartType { get; } = partType;

        /// <summary>
        /// 数据类型
        /// </summary>
        public SystemEnvironmentDataType DataType { get; } = dataType;

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; } = title;
    }
}