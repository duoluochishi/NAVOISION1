//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/7 10:58:04           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.Models;

public class AuthorizationRequest
{
	public string Username { get; set; } = string.Empty;

	public string Password { get; set; } = string.Empty;

	public string PermissionCode { get; set; } = string.Empty;

	public AuthorizationRequest(string username, string pwd, string permissionCode = "")
	{
		Username = username;
		Password = pwd;
		PermissionCode = permissionCode;
	}
}