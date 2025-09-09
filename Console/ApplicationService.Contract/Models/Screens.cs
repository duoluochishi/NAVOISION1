namespace NV.CT.Console.ApplicationService.Contract.Models;

/// <summary>
/// 这里部分命名与 SubProcess命名一致
/// </summary>
public enum Screens
{
	Welcome,
	SelfCheckSimple,
	SelfCheckDetail,
	Login,
	Home,
	SystemSetting,
	Main,//main页面是虚拟页面，业务逻辑根据具体情况显示哪个页面
	Emergency,
	Shutdown,
	LockScreen
}
