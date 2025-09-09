using NV.CT.ServiceFramework.Model;

namespace NV.CT.ServiceFramework.Contract;

public static class ComponentService
{
	//private static readonly ConcurrentDictionary<string, object> LocalStore = new();

	///// <summary>
	///// 当数据变化时触发（key, newValue）
	///// </summary>
	//public static event Action<string, object?>? DataChanged;

	public static event EventHandler<List<ComponentExchange>>? ComponentDataExchanged;

	public static void NotifyComponentExchange(List<ComponentExchange> list)
	{
		ComponentDataExchanged?.Invoke(null,list);
	}

	///// <summary>
	///// 设置数据（会触发事件通知）
	///// </summary>
	//public static void Set<T>(string key, T value)
	//{
	//	if (value is null)
	//		return;

	//	LocalStore[key] = value;

	//	// 通知监听者
	//	DataChanged?.Invoke(key, value);
	//}

	///// <summary>
	///// 获取数据
	///// </summary>
	//public static T? Get<T>(string key, T? defaultValue = default)
	//{
	//	if (LocalStore.TryGetValue(key, out var value))
	//	{
	//		if (value is T tValue)
	//			return tValue;
	//	}
	//	return defaultValue;
	//}

	///// <summary>
	///// 删除数据（会触发事件通知，值为 null）
	///// </summary>
	//public static void Remove(string key)
	//{
	//	if (LocalStore.TryRemove(key, out _))
	//	{
	//		DataChanged?.Invoke(key, null);
	//	}
	//}

	///// <summary>
	///// 清空数据（会触发事件通知，每个键都会通知一次）
	///// </summary>
	//public static void Clear()
	//{
	//	foreach (var key in LocalStore.Keys)
	//	{
	//		LocalStore.TryRemove(key, out _);

	//		DataChanged?.Invoke(key, null);
	//	}
	//}
}