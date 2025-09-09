//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using ICSharpCode.SharpZipLib.Zip;

namespace NV.CT.NanoConsole.ViewModel;

public class BackendFixedItemsViewModel : BaseViewModel
{
	private readonly IShutdownService _shutdownService;
	private readonly IUserService _userService;
	private readonly IConsoleApplicationService _consoleAppService;
	private readonly IAuthorization _authorization;
	private readonly IDialogService _dialogService;
	private readonly ILoginHistoryService _loginHistoryService;
	private readonly ILogger<BackendFixedItemsViewModel> _logger;
	private readonly IApplicationCommunicationService _applicationCommunicationService;
	private SelfCheckWindow? _selfCheckWindow;

	public BackendFixedItemsViewModel(IConsoleApplicationService consoleAppService, IUserService userService, IShutdownService shutdownService, IAuthorization authorization, IDialogService dialogService, ILoginHistoryService loginHistoryService
	, ILogger<BackendFixedItemsViewModel> logger, IApplicationCommunicationService applicationCommunicationService)
	{
		_logger = logger;
		_dialogService = dialogService;
		_shutdownService = shutdownService;
		_userService = userService;
		_consoleAppService = consoleAppService;
		_authorization = authorization;
		_loginHistoryService = loginHistoryService;
		_applicationCommunicationService = applicationCommunicationService;
		_applicationCommunicationService.ActiveApplicationChanged += ActiveApplicationChanged;

		_authorization.CurrentUserChanged += Authorization_CurrentUserChanged;
		_consoleAppService.UiApplicationActiveStatusChanged += UiApplicationActiveStatusChanged;
		_consoleAppService.EnterIntoMainStatusChanged += EnterIntoMainStatusChanged;

		Commands.Add("OpenCommand", new DelegateCommand<string>(OpenCommand));
		Commands.Add("SwitchUser", new DelegateCommand(SwitchUser));
		Commands.Add("ChangePassword", new DelegateCommand(ChangePassword));
		Commands.Add("Logout", new DelegateCommand(Logout));
		Commands.Add("Restart", new DelegateCommand(Restart));
		Commands.Add("Shutdown", new DelegateCommand(Shutdown));
		Commands.Add("UserManual", new DelegateCommand(UserManual));
		Commands.Add("Logpackaging", new DelegateCommand(LogpackagingFunc));
		Commands.Add("ShowSelfCheckResult", new DelegateCommand(ShowSelfCheckResult));

		Task.Run(GetSystemVersionFromConfig);

		Task.Run(GetSelfCheckSummaryResult);
	}

	private void ActiveApplicationChanged(object? sender, ControlHandleModel? e)
	{
		DebugActive = $"{e?.ProcessStartContainerString}_{e?.ItemName}";
	}

	private void EnterIntoMainStatusChanged(object? sender, EventArgs e)
	{
		GetSelfCheckSummaryResult();
	}

	private void GetSelfCheckSummaryResult()
	{
		var fileValidationViewModel = CTS.Global.ServiceProvider.GetService<FileValidationViewModel>();
		var firmwareVersionViewModel = CTS.Global.ServiceProvider.GetService<FirmwareVersionViewModel>();

		if (fileValidationViewModel is null)
			return;
		if (firmwareVersionViewModel is null)
			return;

		UIInvoke(() =>
		{
			SelfCheckOk = fileValidationViewModel.IsValidationOk() && firmwareVersionViewModel.IsValidationOk();
		});
	}

	/// <summary>
	/// 显示自检简易结果
	/// </summary>
	private void ShowSelfCheckResult()
	{
		_selfCheckWindow = CTS.Global.ServiceProvider.GetService<SelfCheckWindow>();
		_selfCheckWindow?.ShowWindowDialog();

		_consoleAppService.ShowSelfCheckSummary();
	}

	private void UiApplicationActiveStatusChanged(object? sender, ControlHandleModel e)
	{
		if (e.ProcessStartContainer is ProcessStartPart.Auxilary)
			return;

		Application.Current?.Dispatcher?.Invoke(() =>
		{
			SettingCardActive = e.ItemName == Screens.SystemSetting.ToString() && e.ActiveStatus == ControlActiveStatus.Active;
		});
	}

	private void Authorization_CurrentUserChanged(object? sender, UserModel? e)
	{
		LoginUserName = e?.Account ?? e?.FirstName ?? string.Empty;
	}



	private bool CanExecuteCleanWork()
	{
		var allApps = _applicationCommunicationService.GetAllApplication();
		var attentionAppExist = allApps.ApplicationList.Any(n =>
			IsProcessEqual(n, ApplicationParameterNames.APPLICATIONNAME_EXAMINATION)
			|| IsProcessEqual(n, ApplicationParameterNames.APPLICATIONNAME_SERVICEFRAME, ApplicationParameterNames.APPLICATIONNAME_SERVICETOOLS));

		return !attentionAppExist;
	}

	private bool IsProcessEqual(ApplicationInfo appInfo, string processName, string childProcessName = "")
	{
		//如果是 ServiceFrame进程,则比较其 Parameter参数
		if (appInfo.ProcessName.Equals(ApplicationParameterNames.APPLICATIONNAME_SERVICEFRAME,
				StringComparison.OrdinalIgnoreCase))
		{
			return appInfo.Parameters.Equals(childProcessName, StringComparison.OrdinalIgnoreCase);
		}
		//不是ServiceFrame进程,则比较其进程名
		else
		{
			return appInfo.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase);
		}
	}

	private void TipCanNotProcess()
	{
		_dialogService.ShowDialog(false, MessageLeveles.Warning, "Warning", "Important work is doing , please close it manually and process again!", _ =>
		{
		}, ConsoleSystemHelper.WindowHwnd);
	}

	private void Logout()
	{
		if (!CanExecuteCleanWork())
		{
			TipCanNotProcess();
			return;
		}

		_consoleAppService.CloseAllApp();

		SwitchUser();
	}

	private void SwitchUser()
	{
		var user = _authorization.GetCurrentUser();
		if (user is not null)
		{
			//注销后面的账户服务数据
			_userService.LogOut(new AuthorizationRequest(user.Account, user.Password));

			//记录 登出历史
			AddLoginHistory(true, UserBehavior.Logout, user.Account);

			_consoleAppService.StartApp(Screens.Login);
		}
	}

	private void ChangePassword()
	{
		_consoleAppService.RequestChangePassword();
		CTS.Global.ServiceProvider.GetService<CredentialManagementWindow>()?.ShowWindowDialog(true);
	}

	private void Restart()
	{
		if (!CanExecuteCleanWork())
		{
			TipCanNotProcess();
			return;
		}

		//TODO: job服务里面找出后台任务,如果有后台任务,则不允许继续执行

		_dialogService.ShowDialog(true, MessageLeveles.Warning, "Warning", "Are you sure to restart?", dialogResult =>
		{
			if (dialogResult.Result is ButtonResult.OK)
			{
				_shutdownService.Restart();
			}
		}, ConsoleSystemHelper.WindowHwnd);
	}

	private void Shutdown()
	{
		if (!CanExecuteCleanWork())
		{
			TipCanNotProcess();
			return;
		}

		//TODO: job服务里面找出后台任务,如果有后台任务,则不允许继续执行

		_dialogService.ShowDialog(true, MessageLeveles.Warning, "Warning", "Are you sure to shutdown?", dialogResult =>
		{
			if (dialogResult.Result is ButtonResult.OK)
			{
				_consoleAppService.StartApp(Screens.Shutdown);
			}
		}, ConsoleSystemHelper.WindowHwnd);
	}

	private void UserManual()
	{
		//TODO:暂时未定
	}

	/// <summary>
	/// 自检是否ok
	/// </summary>
	private bool _selfCheckOk;
	public bool SelfCheckOk
	{
		get => _selfCheckOk;
		set
		{
			SetProperty(ref _selfCheckOk, value);

			SelfCheckShortTip = _selfCheckOk ? "success" : "fail";
		}
	}

	/// <summary>
	/// 简易提示
	/// </summary>
	private string _selfCheckShortTip;
	public string SelfCheckShortTip
	{
		get => _selfCheckShortTip;
		set => SetProperty(ref _selfCheckShortTip, value);
	}

	private void LogpackagingFunc()
	{
		var dstPath = Path.Combine(Directory.GetParent(RuntimeConfig.Console.Backup.Path).FullName, "Logs");
		List<string> list = new List<string>();

		list.Add(RuntimeConfig.Console.MCSLog.Path);
		list.Add(RuntimeConfig.OfflineRecon.MRSLog.Path);
		string offlineMachineLogsPath = @$"\\{RuntimeConfig.OfflineMachineIP}\e\Logs";
		list.Add(offlineMachineLogsPath);

		DateTime dt = DateTime.Now;
		string dateStr = dt.ToString("yyyyMMddHHmmss");
		string tempDir = Path.Combine(dstPath, string.Format($"Logs.{dateStr}"));
		string deseDir = Path.Combine(dstPath, string.Format($"Logs.{dateStr}.zip"));
		Task.Factory.StartNew(() =>
		{
			otherLogFilter = dt.ToString("yyyy-MM-dd");
			foreach (var item in list)
			{
				string lastPath = Path.GetFileName(item);
				if (!string.IsNullOrEmpty(lastPath))
				{
					string directoryPath = Path.Combine(tempDir, lastPath);
					CopyFileAndDir(item, directoryPath, dt.ToString("yyyyMMdd"));     //仅打包当天的
				}
			}
			CompressFile(new List<string> { tempDir }, deseDir, string.Format($"CreateTime:{dt.Ticks}"));
			Directory.Delete(tempDir, true);
		});
	}

	private void OpenCommand(string applicationName)
	{
		try
		{
			if (applicationName == ApplicationParameterNames.NANOCONSOLE_CONTENTWINDOW_SETTING)
			{
				_consoleAppService.StartApp(Screens.SystemSetting);
			}
			else
			{
				_consoleAppService.StartApp(applicationName);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex.Message, ex);
		}
	}

	private bool _settingCardActive;
	public bool SettingCardActive
	{
		get => _settingCardActive;
		set => SetProperty(ref _settingCardActive, value);
	}

	private string _versionInfo = string.Empty;
	public string VersionInfo
	{
		get => _versionInfo;
		set => SetProperty(ref _versionInfo, value);
	}

	private string _loginUserName = string.Empty;
	public string LoginUserName
	{
		get => _loginUserName;
		set => SetProperty(ref _loginUserName, value);
	}

	private string _debugActive = string.Empty;
	public string DebugActive
	{
		get => _debugActive;
		set => SetProperty(ref _debugActive, value);
	}

	int BufferSize = 4096;

	public bool CompressFile(IEnumerable<string> sourceList, string zipFilePath, string? comment = "", string? password = null, int compressionLevel = 6)
	{
		bool result = false;
		try
		{
			//检测目标文件所属的文件夹是否存在，如果不存在则建立
			string zipFileDirectory = Path.GetDirectoryName(zipFilePath);
			if (!Directory.Exists(zipFileDirectory))
			{
				Directory.CreateDirectory(zipFileDirectory);
			}

			Dictionary<string, string> dictionaryList = PrepareFileSystementities(sourceList);

			using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(zipFilePath)))
			{
				zipStream.Password = password;//设置密码
				zipStream.SetComment(comment);//添加注释
				zipStream.SetLevel(compressionLevel);//设置压缩等级

				foreach (string key in dictionaryList.Keys)//从字典取文件添加到压缩文件
				{
					if (File.Exists(key))//判断是文件还是文件夹
					{
						FileInfo fileItem = new FileInfo(key);
						using (FileStream readStream = fileItem.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							ZipEntry zipEntry = new ZipEntry(dictionaryList[key]);
							zipEntry.DateTime = fileItem.LastWriteTime;
							zipEntry.Size = readStream.Length;
							zipStream.PutNextEntry(zipEntry);
							int readLength = 0;
							byte[] buffer = new byte[BufferSize];
							do
							{
								readLength = readStream.Read(buffer, 0, BufferSize);
								zipStream.Write(buffer, 0, readLength);
							} while (readLength == BufferSize);

							readStream.Close();
						}
					}
					else//对文件夹的处理
					{
						ZipEntry zipEntry = new ZipEntry(dictionaryList[key] + "/");
						zipStream.PutNextEntry(zipEntry);
					}
				}

				zipStream.Flush();
				zipStream.Finish();
				zipStream.Close();
			}
			result = true;
		}
		catch (Exception ex)
		{
			_logger.LogInformation($"压缩文件失败 : {ex.Message} ,  StackTrace:{ex.StackTrace}");
		}
		return result;
	}

	string tubelogFilter = "TubeLog_";
	string otherLogFilter = "TubeLog_";

	public void CopyFileAndDir(string sourcePath, string dirPath, string filter)
	{
		if (!Directory.Exists(sourcePath))
		{
			return;
		}
		if (!Directory.Exists(dirPath))
		{
			Directory.CreateDirectory(dirPath);
		}
		IEnumerable<string> files = Directory.EnumerateFileSystemEntries(sourcePath);
		foreach (string file in files)
		{
			string fileName = Path.GetFileName(file);
			string despath = Path.Combine(dirPath, fileName);
			if (File.Exists(file))
			{
				if (fileName.Contains(filter) || fileName.Contains(tubelogFilter) || fileName.Contains(otherLogFilter))
				{
					File.Copy(file, despath, true);
				}
				continue;
			}
			else
			{
				CopyFileAndDir(file, despath, filter);
			}
		}
	}

	private Dictionary<string, string> PrepareFileSystementities(IEnumerable<string> sourceFileEntityPathList)
	{
		Dictionary<string, string> fileEntityDictionary = new Dictionary<string, string>();//文件字典
		string parentDirectoryPath = "";
		foreach (string fileEntityPath in sourceFileEntityPathList)
		{
			string path = fileEntityPath;
			//保证传入的文件夹也被压缩进文件
			if (path.EndsWith(@"\"))
			{
				path = path.Remove(path.LastIndexOf(@"\"));
			}
			parentDirectoryPath = Path.GetDirectoryName(path) + @"\";

			if (parentDirectoryPath.EndsWith(@":\\"))//防止根目录下把盘符压入的错误
			{
				parentDirectoryPath = parentDirectoryPath.Replace(@"\\", @"\");
			}
			//获取目录中所有的文件系统对象
			Dictionary<string, string> subDictionary = GetAllFileSystemEntities(path, parentDirectoryPath);

			//将文件系统对象添加到总的文件字典中
			foreach (string key in subDictionary.Keys)
			{
				if (!fileEntityDictionary.ContainsKey(key))//检测重复项
				{
					fileEntityDictionary.Add(key, subDictionary[key]);
				}
			}
		}
		return fileEntityDictionary;
	}

	private Dictionary<string, string> GetAllFileSystemEntities(string source, string topDirectory)
	{
		Dictionary<string, string> entitiesDictionary = new Dictionary<string, string>();
		entitiesDictionary.Add(source, source.Replace(topDirectory, ""));
		if (Directory.Exists(source))
		{
			//一次性获取下级所有目录，避免递归
			string[] directories = Directory.GetDirectories(source, "*.*", SearchOption.AllDirectories);
			foreach (string directory in directories)
			{
				entitiesDictionary.Add(directory, directory.Replace(topDirectory, ""));
			}

			string[] files = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);
			foreach (string file in files)
			{
				entitiesDictionary.Add(file, file.Replace(topDirectory, ""));
			}
		}
		return entitiesDictionary;
	}

	private void AddLoginHistory(bool isSuccess, UserBehavior behavior, string account, string failReason = "")
	{
		_loginHistoryService.Insert(new LoginHistoryModel()
		{
			Account = account,
			Behavior = behavior.ToString(),
			IsSuccess = isSuccess,
			Comments = string.Empty,
			CreateTime = DateTime.Now,
			FailReason = failReason,
			Id = Guid.NewGuid().ToString()
		});
	}

	#region get software version
	private void GetSystemVersionFromConfig()
	{
		var softwareVersion = ProductConfig.ProductSettingConfig.ProductSetting.SoftwareVersion;
		_logger.LogInformation($"GetSystemVersionFromConfig : {softwareVersion}");

		UIInvoke(() =>
		{
			VersionInfo = softwareVersion;
		});
	}

	/// <summary>
	/// 原先获取版本方法
	/// </summary>
	private void GetSystemVersion()
	{
		System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
		FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
		var version = fvi.FileVersion ?? string.Empty;

		_logger.LogInformation($"GetVersionInfo : {VersionInfo} , version :{version}");

		UIInvoke(() =>
		{
			VersionInfo = version;
		});
	}
	#endregion
}