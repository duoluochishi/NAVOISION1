//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:54:59    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NV.CT.ClientProxy;
using NV.CT.CTS.Helpers;
using NV.CT.Logging;
using NV.CT.PatientBrowser.ApplicationService.Impl.Extensions;
using NV.CT.PatientBrowser.Extensions;
using NV.CT.PatientBrowser.View.English;
using NV.CT.UI.Controls;
using NV.CT.UI.Controls.Common;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog;
using NV.MPS.UI.Dialog.Extension;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace NV.CT.PatientBrowser;
public class Program
{
	private static EventWaitHandle ProgramStarted;
	public static IServiceProvider ServiceProvider;

	[STAThread]
	public static void Main(string[] args)
	{
		//Thread.Sleep(5000);
		SetCultureInfo();

		bool creatNew;
		ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "NV.CT.PatientBrowser", out creatNew);
		if (!creatNew)
		{
			ProgramStarted.Set();
			System.Environment.Exit(0);
			return;
		}
		var host = CreateDefaultBuilder(args).Build();
		host.Start();
		ServiceProvider = host.Services;
		Global.Instance.ServiceProvider = host.Services;
		CTS.Global.ServiceProvider = host.Services;
		var logger = ServiceProvider.GetRequiredService<ILogger<Program>>();

		GlobalErrorHandler.Handling(logger);

		System.Windows.Application app = new System.Windows.Application();
		LoadingResource.LoadingInApplication();

		Global.Instance.Subscribe();

		app.Exit += App_Exit;

		if (args.Length > 0)
		{
			var serverHandle = args[0];
			ConsoleSystemHelper.WindowHwnd = IntPtr.Parse(args[1]);
			try
			{
				var wih = new WindowInteropHelper(host.Services.GetRequiredService<DialogWindow>());
				wih.Owner = ConsoleSystemHelper.WindowHwnd;

				var view = new MainControl(LoadingResource.LoadingInControl());
				var hwndView = ViewHelper.ToHwnd(view);
				PipeHelper.ClientStream(serverHandle, hwndView.ToInt32());
				Dispatcher.Run();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + System.Environment.NewLine + ex.StackTrace);
			}
		}
		else
		{
			var windows = host.Services.GetRequiredService<MainWindow>();
			windows.Show();
			app.MainWindow = windows;
			ConsoleSystemHelper.WindowHwnd = ViewHelper.ToHwnd(windows);
			app.Run();
		}
	}

	private static void App_Exit(object sender, ExitEventArgs e)
	{
		Global.Instance.Unsubscribe();
	}

	public static IHostBuilder CreateDefaultBuilder(string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args);
		builder.ConfigureLogging(logBuilder =>
		{
			logBuilder.ClearProviders().SetMinimumLevel(LogLevel.Trace).AddNanoLogger();
		});
		builder.ConfigureAppConfiguration((context, config) =>
		{
			//TODO:配置注入
			config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "PatientBrowser/appsetting.json"), true, true);
			config.AddEnvironmentVariables();
		});
		builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
		builder.ConfigureContainer<ContainerBuilder>((context, container) =>
		{
			//TODO:Autofac注入               
			container.AddApplicationServiceContainer();
			container.AddViewModelContainer();
			container.AddDialogServiceContianer();
		});

		builder.ConfigureServices((context, services) =>
		{
			services.AddCommunicationClientServices();

			//TODO:AutoMapper注入              
			services.AddDomainMapper();
			services.AddApplicationMapper();
			//TODO:界面窗口注入
			services.TryAddScoped<MainWindow>();
		});
		return builder;
	}

	private static void SetCultureInfo()
	{
		//TODO : will get language from configuration file
		string language = ""; // string.Empty; //"chinese"

		CultureInfo currentCulture;
		if (language.ToLower() == "chinese")
		{
			currentCulture = new CultureInfo("zh-CN");
		}
		else if (language.ToLower() == "english")
		{
			currentCulture = new CultureInfo("en-US");
		}
		else
		{
			currentCulture = new CultureInfo("");
		}

		CultureInfo.DefaultThreadCurrentUICulture = currentCulture;
	}
}