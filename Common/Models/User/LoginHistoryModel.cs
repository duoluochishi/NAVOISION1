//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.CTS.Enums;

namespace NV.CT.Models;

public class LoginHistoryModel : BaseInfo
{
	public string Account { get; set; } = string.Empty;
	public string EncryptPassword { get; set; } = string.Empty;
	public string Behavior { get; set; } = string.Empty;
	public string Comments { get; set; } = string.Empty;
	public bool IsSuccess { get; set; } = false;
	public string FailReason { get; set; } = string.Empty;
}