//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.CT.CTS.Helpers;
using NV.MPS.Environment;
using Prism.Commands;
using Prism.Mvvm;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace NV.CT.UI.ViewModel;

public class BaseViewModel : BindableBase
{
	#region 命令字典
	private IDictionary<string, ICommand> commands = new ObservableDictionary<string, ICommand>();
	public IDictionary<string, ICommand> Commands
	{
		get
		{
			return commands;
		}
		set
		{
			SetProperty(ref commands, value);
		}
	}
	#endregion

	#region 扩展
	public bool SetProperty<T>(ref T storage, T value, Action<string, T> onChanged, string parameterName, [CallerMemberName] string propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(storage, value))
			return false;
		storage = value;
		if (onChanged is not null)
			onChanged(parameterName, value);
		RaisePropertyChanged(propertyName);
		return true;
	}
	#endregion

	#region 属性
	private bool _pageEnable;
	/// <summary>
	/// 页面是否可用
	/// </summary>
	public bool PageEnable
	{
		get => _pageEnable;
		set => SetProperty(ref _pageEnable, value);
	}

	private bool _isDevelopment = RuntimeConfig.IsDevelopment;
	public bool IsDevelopment
	{
		get => _isDevelopment;
		set => SetProperty(ref _isDevelopment, value);
	}

	//private bool isExaming = true;
	///// <summary>
	///// 是否检查中
	///// </summary>
	//public bool IsExaming
	//{
	//    get => isExaming;
	//    set => SetProperty(ref isExaming, value);
	//}
	#endregion

	#region 构造函数

	public BaseViewModel()
	{
		this.PropertyChanged += OnBaseViewModelPropertyChanged;
	}

	#endregion

	#region 私有方法 
	private void OnBaseViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		//确保用这种模式调用,因为属性变更调用很频繁
		System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
		{
			foreach (var item in Commands)
			{
				switch (item.Value)
				{
					case DelegateCommand delegateCommand:
						delegateCommand.RaiseCanExecuteChanged();
						break;
					case AsyncDelegateCommand asyncDelegateCommand:
						asyncDelegateCommand.RaiseCanExecuteChanged();
						break;
					default:
						// 对于泛型版本DelegateCommand<T>和AsyncDelegateCommand<T>，使用反射调用
						var method = item.Value.GetType().GetMethod("RaiseCanExecuteChanged");
						method?.Invoke(item.Value, null);
						break;
				}
			}
		});
	}

	#endregion

	#region 公共方法 
	public void UIInvoke(Action action, [CallerMemberName] string methodName = null)
	{
		System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
		{
			try
			{
				var sw = Stopwatch.StartNew();
				action.Invoke();
				sw.Stop();

				/*
				 * UI操作                         16ms     超过会直接掉帧
				 * 普通viewmodel调用               50ms    用户不会感觉到问题
				 * 后台加载,IO响应但会通过UI更新    100ms    超过就需要用async/await
				 * 调用比较重的服务或CPU密集操作    >200ms   需要后台线程或Task.Run
				 */
				if (sw.ElapsedMilliseconds >= 200)
				{
					CTS.Global.Logger.LogWarning(
						$"[UIInvoke][1] slow execution : {action.Method.DeclaringType}.{methodName} took {sw.ElapsedMilliseconds} ms");
				}
				else if (sw.ElapsedMilliseconds is >= 100 and < 200)
				{
					CTS.Global.Logger.LogWarning(
						$"[UIInvoke][2] need to care : {action.Method.DeclaringType}.{methodName} took {sw.ElapsedMilliseconds} ms");
				}
				else if (sw.ElapsedMilliseconds is >= 50 and < 100)
				{
					CTS.Global.Logger.LogWarning(
						$"[UIInvoke][3] user sensitive delay : {action.Method.DeclaringType}.{methodName} took {sw.ElapsedMilliseconds} ms");
				}
			}
			catch (Exception ex)
			{
				CTS.Global.Logger.LogWarning(ex, $"Failed to execute UI dispatch : {ex.Message}");
			}
		});
	}

	#endregion
}