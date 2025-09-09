//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Models;

namespace NV.CT.AppService.Contract;

/// <summary>
/// GRPC shutdown service
/// </summary>
public interface IShutdownService
{
	/// <summary>
	/// shutdown this computer only include mcs and mrs
	/// </summary>
	BaseCommandResult Shutdown();

	List<BaseCommandResult> CanShutdown();

	/// <summary>
	/// shutdown all the system,include mcs,mrs,embedded system,etc
	/// </summary>
	BaseCommandResult ShutdownSystem();

	List<BaseCommandResult> CanShutdownSystem();

	/// <summary>
	/// restart only restart mcs and mrs
	/// </summary>
	void Restart();

	void GetCurrentShutdownStatus();


	event EventHandler? ShutdownStatusChanged;
}
