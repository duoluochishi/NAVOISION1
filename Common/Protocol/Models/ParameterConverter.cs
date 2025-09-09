using Newtonsoft.Json;

namespace NV.CT.Protocol.Models;
public static class ParameterConverter
{
	public static T Convert<T>(string parameterValue)
	{
		//TODO:使用表达式转换，有待确认（第一稿未能验证通过）
		var castType = typeof(T);
		if (typeof(T).IsEnum)
		{
			return (T)Enum.Parse(castType, parameterValue, true);
		}
		if (castType.Equals(typeof(string)))
		{
			return (T)(object)parameterValue;
		}
		if (castType.Equals(typeof(bool)))
		{
			int v = 0;
			if (int.TryParse(parameterValue, out v))
			{
				return (T)(object)System.Convert.ToBoolean(int.Parse(parameterValue));
			}
			else
			{
				bool bValue = false;
				bool.TryParse(parameterValue, out bValue);
				return (T)(object)bValue;
			}
		}
		if (castType.Equals(typeof(short)))
		{
			short shortValue = 0;
			short.TryParse(parameterValue, out shortValue);
			return (T)(object)shortValue;
		}
		if (castType.Equals(typeof(int)))
		{
			int intValue = 0;
			int.TryParse(parameterValue, out intValue);
			return (T)(object)intValue;
		}
		if (castType.Equals(typeof(long)))
		{
			long longValue = 0;
			long.TryParse(parameterValue, out longValue);
			return (T)(object)longValue;
		}
		if (castType.Equals(typeof(float)))
		{
			float floatValue = 0;
			float.TryParse(parameterValue, out floatValue);
			return (T)(object)floatValue;
		}
		if (castType.Equals(typeof(double)))
		{
			double doubleValue = 0;
			double.TryParse(parameterValue, out doubleValue);
			return (T)(object)doubleValue;
		}
		if (castType.Equals(typeof(decimal)))
		{
			decimal decimalValue = 0;
			decimal.TryParse(parameterValue, out decimalValue);
			return (T)(object)decimalValue;
		}
		if (castType.Equals(typeof(ushort)))
		{
			ushort ushortValue = 0;
			ushort.TryParse(parameterValue, out ushortValue);
			return (T)(object)ushortValue;
		}
		if (castType.Equals(typeof(uint)))
		{
			uint uintValue = 0;
			uint.TryParse(parameterValue, out uintValue);
			return (T)(object)uintValue;
		}
		if (castType.Equals(typeof(ulong)))
		{
			ulong ulongValue = 0;
			ulong.TryParse(parameterValue, out ulongValue);
			return (T)(object)ulongValue;
		}
		if (castType.IsArray)
		{
			if (!castType.IsEnum)
			{
				return JsonConvert.DeserializeObject<T>(parameterValue);
			}
			else
			{
				return (T)Enum.Parse(castType, parameterValue, true);
			}
		}
		if (castType.IsGenericType)
		{
			return JsonConvert.DeserializeObject<T>(parameterValue);
		}
		if (castType.Equals(typeof(DateTime)))
		{
			DateTime dateTime = DateTime.Now;
			DateTime.TryParse(parameterValue, out dateTime);
			return (T)(object)dateTime;
		}
		return default(T);
	}
}