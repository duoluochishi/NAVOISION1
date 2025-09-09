using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.CTS.Encryptions;

public static class Base64Helper
{
	/// <summary>
	/// 对普通字符串进行base64转码
	/// </summary>
	public static string ToBase64String(this string str)
	{
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
	}

	/// <summary>
	/// 解码base64字符串为明文
	/// </summary>
	public static string DecryptBase64String(this string cipher)
	{
		return Encoding.UTF8.GetString(Convert.FromBase64String(cipher));
	}
}