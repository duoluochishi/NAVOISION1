//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
namespace NV.CT.AppService.Impl;

public class ScreenManagementService : IScreenManagement
{
	public void Lock(string reason)
	{
		LockScreenStatusChanged?.Invoke(this, reason);
	}

	public void Unlock(string reason)
	{
		UnlockScreenStatusChanged?.Invoke(this, reason);
	}

	public event EventHandler<string>? LockScreenStatusChanged;

	public event EventHandler<string>? UnlockScreenStatusChanged;
}