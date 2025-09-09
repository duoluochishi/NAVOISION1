//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/3/15 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using System.Net;
using System.Text.RegularExpressions;

namespace NV.CT.ConfigManagement;
public class VerificationExtension
{
	public static bool IpFormatVerification(string ip)
	{
		return IPAddress.TryParse(ip, out var verification);
	}

	public static bool PortFormatVerification(string port)
	{
		bool flag = true;
		if (!int.TryParse(port, out var portInt) || (portInt < 0 || portInt > 65535))
		{
			flag = false;
		}
		return flag;
	}

	public static bool HasUpperCase(string str)
	{
		return !string.IsNullOrEmpty(str) && str.Any(c => char.IsUpper(c));
	}

	public static bool HasIsLowerCase(string str)
	{
		return !string.IsNullOrEmpty(str) && str.Any(c => char.IsLower(c));
	}

	public static bool IsSpecialCharacters(string input)
	{
		string pattern = @"[%&',;=?^……#@￥！（）()$\#/]+";
		Regex regex = new Regex(pattern);
		return regex.IsMatch(input);
	}
}