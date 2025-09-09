//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using Newtonsoft.Json;
using NV.CT.AppService.Contract;
using NV.MPS.Communication;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Models;

namespace NV.CT.ClientProxy.Application
{
	public class ApplicationCommunicationService : IApplicationCommunicationService
	{
		private readonly MCSServiceClientProxy _clientProxy;

#pragma warning disable 67


		public ApplicationListResponse GetAllApplication()
		{
			var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
			{
				Namespace = typeof(IApplicationCommunicationService).Namespace,
				SourceType = nameof(IApplicationCommunicationService),
				ActionName = nameof(IApplicationCommunicationService.GetAllApplication),
				Data = string.Empty
			});
			if (commandResponse.Success)
			{
				var res = JsonConvert.DeserializeObject<ApplicationListResponse> (commandResponse.Data);
				return res;
			}
			return new ApplicationListResponse();
		}

		public event EventHandler<ApplicationResponse>? ApplicationStatusChanged;
		public event EventHandler<ApplicationResponse>? NotifyApplicationClosing;
		public event EventHandler<ControlHandleModel>? UiActivated;
		public event EventHandler<ControlHandleModel?>? ActiveApplicationChanged;

		//public event EventHandler? ApplicationExited;
#pragma warning restore 67

		public ApplicationCommunicationService(MCSServiceClientProxy clientProxy)
		{
			_clientProxy = clientProxy;
		}

		public bool Start(ApplicationRequest applicationRequest)
		{
			bool res = false;
			var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
			{
				Namespace = typeof(IApplicationCommunicationService).Namespace,
				SourceType = nameof(IApplicationCommunicationService),
				ActionName = nameof(IApplicationCommunicationService.Start),
				Data = applicationRequest.ToJson()
			});
			if (commandResponse.Success)
			{
				res = Convert.ToBoolean(commandResponse.Data);//.DeserializeTo<ApplicationResponse>();
			}
			return res;
		}
		public bool Close(ApplicationRequest applicationRequest)
		{
			var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
			{
				Namespace = typeof(IApplicationCommunicationService).Namespace,
				SourceType = nameof(IApplicationCommunicationService),
				ActionName = nameof(IApplicationCommunicationService.Close),
				Data = applicationRequest.ToJson()
			});
			if (commandResponse.Success)
			{
				var res = Convert.ToBoolean(commandResponse.Data);
				return res;
			}
			return false;
		}

		//public IntPtr GetProcessHwnd(ApplicationRequest applicationRequest)
		//{
		//	var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
		//	{
		//		Namespace = typeof(IApplicationCommunicationService).Namespace,
		//		SourceType = nameof(IApplicationCommunicationService),
		//		ActionName = nameof(IApplicationCommunicationService.GetProcessHwnd),
		//		Data = applicationRequest.ToJson()
		//	});
		//	if (commandResponse.Success)
		//	{
		//		var token = JsonConvert.DeserializeObject<JToken>(commandResponse.Data);
		//		var val = token["value"].ToString();
		//		var res = IntPtr.Parse(val);
		//		return res;
		//	}
		//	return IntPtr.Zero;
		//}

		public void SetWindowHwnd(ConsoleHwndRequest request)
		{
			var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
			{
				Namespace = typeof(IApplicationCommunicationService).Namespace,
				SourceType = nameof(IApplicationCommunicationService),
				ActionName = nameof(IApplicationCommunicationService.SetWindowHwnd),
				Data = request.ToJson()
			});
		}

		//public IntPtr GetWindowHwnd(ProcessStartPart processStartPart)
		//{
		//	var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
		//	{
		//		Namespace = typeof(IApplicationCommunicationService).Namespace,
		//		SourceType = nameof(IApplicationCommunicationService),
		//		ActionName = nameof(IApplicationCommunicationService.GetWindowHwnd),
		//		Data = processStartPart.ToJson(),
		//	});
		//	if (commandResponse.Success)
		//	{
		//		var token = JsonConvert.DeserializeObject<JToken>(commandResponse.Data);
		//		var val = token["value"].ToString();
		//		var res = IntPtr.Parse(val);
		//		return res;
		//	}
		//	return IntPtr.Zero;
		//}

		//public bool StartProcess(string applicationName, string param)
		//{
		//	ApplicationRequest request = new ApplicationRequest
		//	{
		//		ApplicationName = applicationName,
		//		Parameters = param,
		//		Status = CTS.Enums.ProcessStatus.Starting,
		//	};
		//	return Start(request);
		//}

		public bool IsExistsProcess(ApplicationRequest appRequest)
		{
			var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
			{
				Namespace = typeof(IApplicationCommunicationService).Namespace,
				SourceType = nameof(IApplicationCommunicationService),
				ActionName = nameof(IApplicationCommunicationService.IsExistsProcess),
				Data = appRequest.ToJson()
			});
			if (commandResponse.Success)
			{
				var res = Convert.ToBoolean(commandResponse.Data);
				return res;
			}
			return false;
		}

		public ApplicationInfo? GetApplicationInfo(ApplicationRequest appRequest)
		{
			var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
			{
				Namespace = typeof(IApplicationCommunicationService).Namespace,
				SourceType = nameof(IApplicationCommunicationService),
				ActionName = nameof(IApplicationCommunicationService.GetApplicationInfo),
				Data = appRequest.ToJson()
			});
			if (commandResponse.Success)
			{
				var res = JsonConvert.DeserializeObject<ApplicationInfo?>(commandResponse.Data);
				return res;
			}
			return null;
		}

		public void Active(ControlHandleModel control)
		{
			_clientProxy.ExecuteCommand(new CommandRequest()
			{
				Namespace = typeof(IApplicationCommunicationService).Namespace,
				SourceType = nameof(IApplicationCommunicationService),
				ActionName = nameof(IApplicationCommunicationService.Active),
				Data = control.ToJson()
			});
		}

		public ApplicationInfo? GetActiveApplication()
		{
			var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
			{
				Namespace = typeof(IApplicationCommunicationService).Namespace,
				SourceType = nameof(IApplicationCommunicationService),
				ActionName = nameof(IApplicationCommunicationService.GetActiveApplication),
				Data = string.Empty
			});
			if (commandResponse.Success)
			{
				var res = JsonConvert.DeserializeObject<ApplicationInfo?>(commandResponse.Data);
				return res;
			}
			return null;
		}

		public void NotifyActiveApplication(ControlHandleModel controlHandleModel)
		{
			_clientProxy.ExecuteCommand(new CommandRequest()
			{
				Namespace = typeof(IApplicationCommunicationService).Namespace,
				SourceType = nameof(IApplicationCommunicationService),
				ActionName = nameof(IApplicationCommunicationService.NotifyActiveApplication),
				Data = controlHandleModel.ToJson()
			});
		}

		public ControlHandleModel? GetCurrentActiveApplication()
		{
			var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
			{
				Namespace = typeof(IApplicationCommunicationService).Namespace,
				SourceType = nameof(IApplicationCommunicationService),
				ActionName = nameof(IApplicationCommunicationService.GetCurrentActiveApplication),
				Data = string.Empty
			});
			if (commandResponse.Success)
			{
				if (string.IsNullOrEmpty(commandResponse.Data))
				{
					return null;
				}

				var res = JsonConvert.DeserializeObject<ControlHandleModel?>(commandResponse.Data);
				return res;
			}
			return null;
		}

		//public bool IsExistsProcess(string applicationName, string param)
		//{
		//ApplicationRequest request = new ApplicationRequest
		//{
		//	ApplicationName = applicationName,
		//	Parameters = param,
		//	Status = CTS.Enums.ProcessStatus.Starting,
		//};
		//return GetProcessHwnd(request) == IntPtr.Zero;
		//}
	}
}