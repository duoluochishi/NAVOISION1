using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Models.MouseKeyboard;

public class ListenerStatus
{
	public TimerStatus Status { get; set; }
	public int DueTime { get; set; }
	public int Period { get; set; }
	public TimeSpan? ThresholdTime { get; set; }
	public int? IdledSeconds { get; set; }
}