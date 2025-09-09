using NV.CT.AppService.Contract;
using NV.CT.Console.ApplicationService.Contract.Models;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using System.Collections.ObjectModel;
using NV.CT.CTS.Models;

namespace NV.CT.Console.ApplicationService.Contract.Interfaces;

public interface IConsoleApplicationService
{
	/// <summary>
	/// 应用程序正要启动
	/// </summary>
	[Obsolete("多此一举??")]
	event EventHandler? UiApplicationActiveStarting;

	/// <summary>
	/// 某个应用的active状态变化事件
	/// </summary>
	event EventHandler<ControlHandleModel>? UiApplicationActiveStatusChanged;
	event EventHandler ExaminationClosedAndStartNewExamination;
	event EventHandler<ApplicationResponse> ApplicationClosing;

	/// <summary>
	/// 用户发起移床命令
	/// </summary>
	event EventHandler? MoveTableStarted;

	/// <summary>
	/// 顶部和底部状态栏变化
	/// </summary>
	event EventHandler<bool>? ToggleHeaderFooterChanged;

	Dictionary<ProcessStartPart, ObservableCollection<CardModel>> CardModels { get; set; }

	bool IsSwitchExamination { get; set; }

	//[Obsolete("not correct method")]
	//void ActiveHwnd(string applicationName, string parameters);

	List<ControlHandleModel> ControlHandleModelList { get; set; }

	/// <summary>
	/// 获取进程信息
	/// </summary>
	ControlHandleModel? GetControlHandleModel(string applicationName, string parameters = "");

	/// <summary>
	/// 启动应用(控件)
	/// </summary>
	/// <param name="screen">控件页面</param>
	void StartApp(Screens screen);

	/// <summary>
	/// 启动应用(进程)
	/// </summary>
	void StartApp(string appName, string parameters = "");

	/// <summary>
	/// 关闭应用
	/// </summary>
	void CloseApp(string appName, string parameters = "", bool needConfirm = true);

	/// <summary>
	/// 主要是关闭 非ServiceTool和检查进程之外的进程(进程包括独立进程和卡片式进程)
	/// </summary>
	void CloseAllApp();

	/// <summary>
	/// 将主副控台句柄下发到进程服务里面,进程启动用得到
	/// </summary>
	void SetWindowHwnd(ProcessStartPart processStartPart, IntPtr windowHwnd);

	void StartMoveTable();

	void ToggleHeaderFooter(bool toggle);

	event EventHandler? ChangePasswordStarted;
	void RequestChangePassword();

	void ShowSelfCheckSummary();
	event EventHandler? ShowSelfCheckSummaryStarted;

	void EnterIntoMain();
	event EventHandler? EnterIntoMainStatusChanged;

	/// <summary>
	/// 将[Active]状态切换到某个平台
	/// </summary>
	public void SwitchToPlatform(ProcessStartPart processStartPart);

	///// <summary>
	///// 检查进程是否开启
	///// </summary>
	//bool IsExaminationOpened();

	//void AddConsoleControlHwnd(string applicationName, string parameters, IntPtr controlHwnd);

	//event EventHandler<EventArgs<(bool, TopPartition)>> CardStyleReset;

	//ControlHandleModel GetControlViewHost(string applicationName, string parameters);

	//IntPtr GetControlHwnd(string applicationName, string parameters, bool isConsoleControl);

	///// <summary>
	///// 启动应用(进程)
	///// </summary>
	///// <param name="appName">应用名称</param>
	//void StartApp(string appName);

	///// <summary>
	///// 关闭应用
	///// </summary>
	///// <param name="appName">应用名称</param>
	//void CloseApp(string appName);

	//[Obsolete("not used anymore")]
	//void CloseHwnd(string applicationName, string parameters);

	//void DeactivateHwnd(string applicationName, string parameters);

	//[Obsolete]
	//void ResetCardStyle(bool isMainConsole, TopPartition topPartition);

	//void ResetCardStyle(ResetCardNotify  notify);

	//void ResetAuxilaryCardStyle();

	//void ResetPrimaryCardStyle();


	//event EventHandler<EventArgs<(CardModel, ProcessStartPart)>> CardBackgroundUpdating;

	///// <summary>
	///// 启动应用，非控件
	///// </summary>
	//[Obsolete("need to remove this method")]
	//void ActiveHwnd(string applicationName, string parameters);


}