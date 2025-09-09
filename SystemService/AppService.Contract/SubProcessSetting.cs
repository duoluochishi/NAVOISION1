//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.AppService.Contract;

public class SubProcessSetting
{
	public string ProcessName { get; set; } = string.Empty;
	public string FileName { get; set; } = string.Empty;
	public string StartPart { get; set; } = string.Empty;
	public string StartMode { get; set; } = string.Empty;
	/// <summary>
	/// 是否固定,固定的意思是属于主控台或副控台的固定部分,不是动态打开的进程这种
	/// </summary>
	public bool IsFixed { get; set; }
	public int MaxInstance { get; set; }
}
