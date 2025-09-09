//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
namespace NV.CT.AppService.Contract;

public interface IScreenManagement
{
	void Lock(string reason);
	void Unlock(string reason);

	event EventHandler<string>? LockScreenStatusChanged;
	event EventHandler<string>? UnlockScreenStatusChanged;
}

public enum LockReason
{
	EmergencyStop,
	SystemError
}