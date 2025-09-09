namespace NV.CT.AppService.Impl;

public class ApplicationCommunicationService : IApplicationCommunicationService
{
	private readonly ILogger<ApplicationCommunicationService> _logger;
	private readonly List<ApplicationInfo> _applicationInfoList = new();
	private readonly ProcessService _processService;
	private readonly List<SubProcessSetting> _subProcesses;
	private ControlHandleModel? _controlHandleModel;

	public event EventHandler<ApplicationResponse>? ApplicationStatusChanged;
	public event EventHandler<ApplicationResponse>? NotifyApplicationClosing;
	public event EventHandler<ControlHandleModel>? UiActivated;
	public event EventHandler<ControlHandleModel?>? ActiveApplicationChanged; 

	public ApplicationCommunicationService(IOptions<List<SubProcessSetting>> subProcesses, ILogger<ApplicationCommunicationService> logger, ProcessService processService)
	{
		_logger = logger;
		_subProcesses = subProcesses.Value;
		_processService = processService;

		_processService.ProcessStatusChanged += ProcessStatusChanged;
	}

    /// <summary>
    /// 订阅进程服务的进程状态变化通知
    /// </summary>
    private void ProcessStatusChanged(object? sender, ProcessInfo e)
	{
        var subProcessSetting = GetProcessInfo(e.ApplicationName, e.Parameter);

		switch (e.ProcessStatus)
		{
			case ProcessStatus.Starting:
				break;
			case ProcessStatus.Started:
				{
					if (_applicationInfoList.Count(t => t.ProcessName == e.ApplicationName && t.Parameters == e.Parameter) > subProcessSetting.MaxInstance)
						return;

					_applicationInfoList.Add(new ApplicationInfo
					{
						ProcessName = e.ApplicationName,
						Parameters = e.Parameter,
						ControlHwnd = e.Hwnd,
						Process = e.Process,
						Path = Path.Combine(RuntimeConfig.Console.MCSBin.Path, subProcessSetting?.FileName),
						MaxInstance = subProcessSetting.MaxInstance,
						SubProcessStatus = ProcessStatus.Started
					});

					ApplicationStatusChanged?.Invoke(this, new ApplicationResponse
					{
						ApplicationName = e.ApplicationName,
						Parameters = e.Parameter,
						ProcessId = e.ProcessId,
						ControlHandle = e.Hwnd,
						ProcessStartPart = subProcessSetting.StartPart == ProcessStartPart.Master.ToString() ? ProcessStartPart.Master : ProcessStartPart.Auxilary,
						Status = ProcessStatus.Started
					});
					break;
				}
			case ProcessStatus.Closed:
				{
					var appInfo = GetApplicationInfo(e.ProcessId);
					if (null != appInfo)
					{
						var closedProcessSetting = GetProcessInfo(appInfo.ProcessName, appInfo.Parameters);
						_applicationInfoList.RemoveAll(a => a.ProcessId == e.ProcessId);

						ApplicationStatusChanged?.Invoke(this, new ApplicationResponse
						{
							ApplicationName = appInfo.ProcessName,
							Parameters = appInfo.Parameters,
							Status = ProcessStatus.Closed,
							//IsMainConsole = closedProcessSetting?.StartPart == ProcessStartPart.Master.ToString() ? true : false,
							ProcessStartPart = closedProcessSetting?.StartPart == ProcessStartPart.Master.ToString() ? ProcessStartPart.Master : ProcessStartPart.Auxilary
						});
					}

					break;
				}
		}
	}

	[LogEntrance]
	public bool Start(ApplicationRequest request)
	{
		bool flag = false;
		string msg = string.Empty;
		try
		{
			if (string.IsNullOrEmpty(request.ApplicationName))
				return false;

			var applicationInfo = GetApplicationInfo(request.ApplicationName, request.Parameters);
			var subProcessSetting = GetProcessInfo(request.ApplicationName, request.Parameters);
			if (null != subProcessSetting)
			{
				//Starting阶段只是通知，不参与进程加入进程列表等的逻辑
				ApplicationStatusChanged?.Invoke(this, new ApplicationResponse
				{
					ApplicationName = request.ApplicationName,
					Parameters = request.Parameters,
					ProcessId = 0,
					ControlHandle = IntPtr.Zero,
					//IsMainConsole = subProcessSetting.StartMode == ProcessStartPart.Master.ToString() ? true : false,
					ProcessStartPart = subProcessSetting.StartPart == ProcessStartPart.Master.ToString() ? ProcessStartPart.Master : ProcessStartPart.Auxilary,
					Status = ProcessStatus.Starting
				});

				if (null == applicationInfo)
				{
					(flag, msg) = _processService.StartProcess(request.ApplicationName, request.Parameters,
						Path.Combine(RuntimeConfig.Console.MCSBin.Path, subProcessSetting.FileName),
						subProcessSetting.StartPart == ProcessStartPart.Master.ToString()
							? ConsoleWindowHwnd.Instance.MasterWindowHwnd
							: ConsoleWindowHwnd.Instance.AuxilaryWindowHwnd);
				}
				else
				{
					flag = true;
					ApplicationStatusChanged?.Invoke(this, new ApplicationResponse
					{
						ApplicationName = request.ApplicationName,
						Parameters = request.Parameters,
						ProcessId = applicationInfo.ProcessId,
						ControlHandle = applicationInfo.ControlHwnd,
						//IsMainConsole = subProcessSetting.StartMode == ProcessStartPart.Master.ToString(),
						ProcessStartPart = subProcessSetting.StartPart == ProcessStartPart.Master.ToString() ? ProcessStartPart.Master : ProcessStartPart.Auxilary,
						Status = ProcessStatus.Started
					});
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Application ({request.ApplicationName} {request.Parameters}) start exception: {ex.Message}");
		}

		return flag;
	}

	public bool Close(ApplicationRequest request)
	{
		try
		{
			if (string.IsNullOrEmpty(request.ApplicationName))
				return false;

			_logger.LogInformation($"AppService.Close: {request.ToJson()}");

			var applicationInfo = GetApplicationInfo(request.ApplicationName, request.Parameters);

			if (applicationInfo == null)
				return false;

			var process = GetProcessInfo(request.ApplicationName, request.Parameters);

			//bool isMainConsole = process?.StartPart == ProcessStartPart.Master.ToString();

			NotifyApplicationClosing?.Invoke(this, new ApplicationResponse
			{
				ApplicationName = request.ApplicationName,
				Parameters = request.Parameters,
				ControlHandle = applicationInfo.ControlHwnd,
				ProcessId = applicationInfo.ProcessId,
				Status = ProcessStatus.Closing,
				ExtraParameter = request.ExtraParameter,
				NeedConfirm = request.NeedConfirm
			});

			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, ex.Message);
			return false;
		}
	}

	private SubProcessSetting? GetProcessInfo(string applicationName, string parameters)
	{
		if (applicationName != ApplicationParameterNames.APPLICATIONNAME_SERVICEFRAME)
		{
			return _subProcesses.FirstOrDefault(t => t.ProcessName == applicationName);
		}

		return _subProcesses.FirstOrDefault(t => t.FileName == "NV.CT." + applicationName + ".exe" && t.ProcessName == parameters);
	}

	private ApplicationInfo? GetApplicationInfo(int processId)
	{
		return _applicationInfoList.FirstOrDefault(a => a.ProcessId == processId);
	}

	public ApplicationInfo? GetApplicationInfo(string applicationName, string parameters)
	{
		if (!string.IsNullOrEmpty(parameters))
		{
			return _applicationInfoList.FirstOrDefault(a => a.ProcessName == applicationName && a.Parameters == parameters);
		}

		return _applicationInfoList.FirstOrDefault(a => a.ProcessName == applicationName && a.Parameters == string.Empty);
	}

	public ApplicationInfo? GetApplicationInfo(ApplicationRequest appRequest)
	{
		var parameter = string.IsNullOrEmpty(appRequest.Parameters) ? string.Empty : appRequest.Parameters;

		return _applicationInfoList.FirstOrDefault(n => n.ProcessName == appRequest.ApplicationName && n.Parameters == parameter);
	}

	public void SetWindowHwnd(ConsoleHwndRequest request)
	{
		if (request.StartPart == ProcessStartPart.Master)
		{
			ConsoleWindowHwnd.Instance.MasterWindowHwnd = request.WindowHwnd;
		}
		else
		{
			ConsoleWindowHwnd.Instance.AuxilaryWindowHwnd = request.WindowHwnd;
		}
	}

	public bool IsExistsProcess(ApplicationRequest appRequest)
	{
		var appInfo = GetApplicationInfo(appRequest.ApplicationName, appRequest.Parameters);

		if(appInfo is null)
		{
			return false;
		}
		return appInfo?.ControlHwnd != IntPtr.Zero;
	}

	public ApplicationListResponse GetAllApplication()
	{
		return new ApplicationListResponse(_applicationInfoList);
	}

	public void Active(ControlHandleModel control)
	{
		//TODO: set this process to active status
		UiActivated?.Invoke(this,control);
	}

	public ApplicationInfo? GetActiveApplication()
	{
		return _applicationInfoList.FirstOrDefault(n => n.IsActive);
	}

	public void NotifyActiveApplication(ControlHandleModel controlHandleModel)
	{
		_controlHandleModel=controlHandleModel;

		ActiveApplicationChanged?.Invoke(this,controlHandleModel);
	}

	public ControlHandleModel? GetCurrentActiveApplication()
	{
		return _controlHandleModel;
	}


	#region not used

	//public IntPtr GetWindowHwnd(ProcessStartPart processStartPart)
	//{
	//	return processStartPart == ProcessStartPart.Master ? ConsoleWindowHwnd.Instance.MasterWindowHwnd : ConsoleWindowHwnd.Instance.AuxilaryWindowHwnd;
	//}

	//public IntPtr GetProcessHwnd(ApplicationRequest request)
	//{
	//	var applicationInfo = GetApplicationInfo(request.ApplicationName, request.Parameters);

	//	return applicationInfo?.ControlHwnd ?? IntPtr.Zero;
	//}


	//public bool StartProcess(string applicationName, string param)
	//{
	//	return Start(new ApplicationRequest
	//	{
	//		ApplicationName = applicationName,
	//		Parameters = param,
	//		Status = ProcessStatus.Starting,
	//	});
	//}

	//public bool IsExistsProcess(string applicationName, string param)
	//{
	//	var appInfo = GetApplicationInfo(applicationName, param);
	//	return appInfo?.ControlHwnd != IntPtr.Zero;
	//	//return GetProcessHwnd(new ApplicationRequest
	//	//{
	//	//	ApplicationName = applicationName,
	//	//	Parameters = param,
	//	//	Status = ProcessStatus.Starting,
	//	//}) != IntPtr.Zero;
	//}

	#endregion
}