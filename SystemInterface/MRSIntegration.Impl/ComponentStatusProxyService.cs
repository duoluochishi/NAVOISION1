using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Models.ComponentStatus;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class ComponentStatusProxyService : IComponentStatusProxyService
{
	public IEnumerable<ComponentFirmwareInfo> GetAcqCardAndDetectorFirmwareVersion()
	{
		return ComponentStatusProxy.Instance.GetAcqCardAndDetectorFirmwareVersion();
	}

	public IEnumerable<ComponentFirmwareInfo> GetAllComponentFirmwareVersion()
	{
		return ComponentStatusProxy.Instance.GetAllComponentFirmwareVersion();
	}

	public IEnumerable<ComponentFirmwareInfo> GetAllComponentVersion()
	{
		try
		{
			var list1 = ComponentStatusProxy.Instance.GetAcqCardAndDetectorFirmwareVersion();
			var list2 = ComponentStatusProxy.Instance.GetAllComponentFirmwareVersion();

			return (list1??new List<ComponentFirmwareInfo>()).Union(list2??new List<ComponentFirmwareInfo>());
		}
		catch (Exception)
		{
			return new List<ComponentFirmwareInfo>();
		}
	}
}