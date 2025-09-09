//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/12/02 16:00:14     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection;

namespace NV.CT.ErrorCodes;

public static class ErrorCodeHelper
{
	private const string ErrorCodeSuffix_Code = "_Code";
	private const string ErrorCodeSuffix_Level = "_Level";
	private const string ErrorCodeSuffix_Description = "_Description";
	private const string ErrorCodeSuffix_Reason = "_Reason";
	private const string ErrorCodeSuffix_Solution = "_Solution";
	private const string ErrorCodeSuffix_ExceptionHandling = "_Handling";

	private const string ErrorNotDefined = "NotDefined";

	private const string ErrorCodeSuffixSeperator = "_";

	private static Dictionary<string, ErrorCode> ErrorCodeDic = new Dictionary<string, ErrorCode>();
	private static bool IsInit = false;

	public static void InitErrorCodes()
	{
		var props = typeof(ErrorCodeResource).GetProperties(BindingFlags.Public | BindingFlags.Static);

		if (props is null || props.Length == 0)
		{
			return;
		}

		foreach (var prop in props)
		{
			HandleSingleProp(prop);
		}
		IsInit = true;
	}

	public static ErrorCode GetErrorCode(string code)
	{
		if (!IsInit)
		{
			InitErrorCodes();
		}

		return ErrorCodeDic.Values.FirstOrDefault(x => string.Equals(x.Code.Trim(), code.Trim(), StringComparison.OrdinalIgnoreCase), GetDefaultErrorCode(ErrorNotDefined, code));
	}

	public static List<ErrorCode> GetErrorCodeList(IEnumerable<string> codes)
	{
		var retList = new List<ErrorCode>();
		foreach (var code in codes)
		{
			retList.Add(GetErrorCode(code));
		}
		return retList;
	}

	public static ErrorCode GetErrorByKey(string key)
	{
		if (!IsInit)
		{
			InitErrorCodes();
		}

		if (ErrorCodeDic.TryGetValue(key, out var result))
		{
			return result;
		}
		else
		{
			return GetDefaultErrorCode(key, ErrorNotDefined);
		}
	}

	private static void HandleSingleProp(PropertyInfo pi)
	{
		var pName = pi.Name;
		var key = GetPropertyKey(pName);
		if (string.IsNullOrEmpty(key))
		{
			return;                     // not a valid key
		}
		EnsureDicContent(key);

		var content = pi.GetValue(null) as string;
		if (content is not null)
		{
			FillSinglePropContent(pName, key, content);
		}
	}

	private static string GetPropertyKey(string pName)
	{
		if (!pName.Contains(ErrorCodeSuffixSeperator))
		{
			return string.Empty;
		}
		return pName.Substring(0, pName.LastIndexOf(ErrorCodeSuffixSeperator));
	}

	private static void EnsureDicContent(string key)
	{
		if (!ErrorCodeDic.ContainsKey(key))
		{
			ErrorCodeDic.Add(key, new ErrorCode(key));
		}
	}

	private static void FillSinglePropContent(string pName, string key, string content)
	{
		if (pName.EndsWith(ErrorCodeSuffix_Code))
		{
			ErrorCodeDic[key].Code = content;
		}
		else if (pName.EndsWith(ErrorCodeSuffix_Level))
		{
			try
			{
				ErrorCodeDic[key].Level = Enum.Parse<ErrorLevel>(content);
			}
			catch
			{
				ErrorCodeDic[key].Level = ErrorLevel.NotDefined;
			}
		}
		else if (pName.EndsWith(ErrorCodeSuffix_Description))
		{
			ErrorCodeDic[key].Description = content;

		}
		else if (pName.EndsWith(ErrorCodeSuffix_Reason))
		{
			ErrorCodeDic[key].Reason = content;

		}
		else if (pName.EndsWith(ErrorCodeSuffix_Solution))
		{
			ErrorCodeDic[key].Solution = content;
		}
		else if (pName.EndsWith(ErrorCodeSuffix_ExceptionHandling))
		{
			ErrorCodeDic[key].ExceptionHandling = content;
		}
		else
		{
			//not legal errorcode property, ignore
		}
	}

	private static ErrorCode GetDefaultErrorCode(string key, string code)
	{
		return new ErrorCode(ErrorNotDefined, code, ErrorLevel.NotDefined, ErrorNotDefined, ErrorNotDefined, ErrorNotDefined, ErrorNotDefined);
	}
}
