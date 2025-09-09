using NV.CT.Models.MouseKeyboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.WorkflowService.Contract;

public interface IInputListener
{
	//void Start(ListenerStartParameter parameter);
	void Start();
	void Reset();
	void Stop();
	void Dispose();
	event EventHandler<ListenerStatus>? IdleTimeOccured;
	event EventHandler<ListenerStatus>? StatusChanged;
}