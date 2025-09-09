using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;

namespace NV.CT.AppService.Contract;

public interface IApplicationCommunicationService
{
	bool Start(ApplicationRequest applicationRequest);
	bool Close(ApplicationRequest applicationRequest);
	void SetWindowHwnd(ConsoleHwndRequest consoleHwndRequest);
	void Active(ControlHandleModel control);

	/// <summary>
	/// 获取当前Active的进程
	/// </summary>
	ApplicationInfo? GetActiveApplication();

	/// <summary>
	/// 获取进程信息
	/// </summary>
	ApplicationInfo? GetApplicationInfo(ApplicationRequest appRequest);

	/// <summary>
	/// 进程是否存在
	/// </summary>
	bool IsExistsProcess(ApplicationRequest appRequest);

	/// <summary>
	/// 获取所有的进程
	/// </summary>
	ApplicationListResponse GetAllApplication();

	/// <summary>
	/// 进程状态变化通知
	/// </summary>
	public event EventHandler<ApplicationResponse>? ApplicationStatusChanged;

	/// <summary>
	/// 进程关闭通知
	/// </summary>
	public event EventHandler<ApplicationResponse>? NotifyApplicationClosing;

	/// <summary>
	/// 进程激活
	/// </summary>
	public event EventHandler<ControlHandleModel>? UiActivated;

	/// <summary>
	/// 通知当前唯一被激活的应用
	/// </summary>
	public void NotifyActiveApplication(ControlHandleModel controlHandleModel);

	public ControlHandleModel? GetCurrentActiveApplication();

	public event EventHandler<ControlHandleModel?>? ActiveApplicationChanged;

	///// <summary>
	///// 进程是否存在
	///// </summary>
	///// <returns></returns>
	//bool IsExistsProcess(string applicationName, string param);

	//bool StartProcess(string applicationName, string param);

	//IntPtr GetProcessHwnd(ApplicationRequest applicationRequest);

	//IntPtr GetWindowHwnd(ProcessStartPart processStartPart);

}
