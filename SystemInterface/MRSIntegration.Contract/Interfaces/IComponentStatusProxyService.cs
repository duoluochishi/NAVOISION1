using NV.CT.FacadeProxy.Models.ComponentStatus;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface IComponentStatusProxyService
{
	/// <summary>
	/// 采集卡固件版本
	/// </summary>
	/// <returns></returns>
	IEnumerable<ComponentFirmwareInfo> GetAcqCardAndDetectorFirmwareVersion();

	/// <summary>
	/// 其他固件版本
	/// </summary>
	/// <returns></returns>
	IEnumerable<ComponentFirmwareInfo> GetAllComponentFirmwareVersion();

	/// <summary>
	/// 提供给外层使用的所有固件(包括采集卡和其他固件如bowtie,逆变器，IFBox,CTbox等)的版本
	/// </summary>
	/// <returns>版本列表</returns>
	IEnumerable<ComponentFirmwareInfo> GetAllComponentVersion();

}


