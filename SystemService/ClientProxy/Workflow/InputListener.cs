using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NV.CT.CTS.Extensions;
using NV.CT.Models;
using NV.CT.Models.MouseKeyboard;
using NV.CT.WorkflowService.Contract;
using NV.MPS.Communication;

namespace NV.CT.ClientProxy.Workflow;

public class InputListener : IInputListener
{
	private readonly MCSServiceClientProxy _clientProxy;

	public InputListener(MCSServiceClientProxy clientProxy)
	{
		_clientProxy = clientProxy;
	}
	public void Start()
	{
		_clientProxy.ExecuteCommand(new CommandRequest()
		{
			Namespace = typeof(IInputListener).Namespace,
			SourceType = nameof(IInputListener),
			ActionName = nameof(IInputListener.Start),
			Data = string.Empty
		});
	}

	public void Reset()
	{
		_clientProxy.ExecuteCommand(new CommandRequest()
		{
			Namespace = typeof(IInputListener).Namespace,
			SourceType = nameof(IInputListener),
			ActionName = nameof(IInputListener.Reset),
			Data = string.Empty
		});
	}

	public void Stop()
	{
		_clientProxy.ExecuteCommand(new CommandRequest()
		{
			Namespace = typeof(IInputListener).Namespace,
			SourceType = nameof(IInputListener),
			ActionName = nameof(IInputListener.Stop),
			Data = string.Empty
		});
	}

	public void Dispose()
	{
		_clientProxy.ExecuteCommand(new CommandRequest()
		{
			Namespace = typeof(IInputListener).Namespace,
			SourceType = nameof(IInputListener),
			ActionName = nameof(IInputListener.Dispose),
			Data = string.Empty
		});
	}

	public event EventHandler<ListenerStatus>? StatusChanged;
	public event EventHandler<ListenerStatus>? IdleTimeOccured;
}