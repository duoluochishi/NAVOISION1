using NV.CT.ServiceFramework.Model;
using NV.MPS.Configuration;

namespace NV.CT.ServiceFramework.Contract;

public static class ComponentService
{
	public static event EventHandler<List<ComponentExchange>>? ComponentDataExchanged;

	public static void NotifyComponentExchange(List<ComponentExchange> list)
	{
		ComponentDataExchanged?.Invoke(null,list);
	}

	/// <summary>
	/// 更新校准表状态
	/// </summary>
	/// <param name="typeName">校准类型</param>
	/// <param name="caliParam">校准参数</param>
	/// <param name="isValid">是否有效</param>
	/// <returns>是否更新成功</returns>
	public static bool UpdateCalibrationStatus(string typeName,CalibrationParameter caliParam,bool isValid)
	{
		return SystemConfig.UpdateCalibrationStatus(typeName, caliParam, isValid);
	}

	/// <summary>
	/// 获取当前校准表状态
	/// </summary>
	/// <returns>当前校准表是否有效</returns>
	public static bool GetCalibrationStatus()
	{
		return SystemConfig.IsCalibrationValid;
	}

}