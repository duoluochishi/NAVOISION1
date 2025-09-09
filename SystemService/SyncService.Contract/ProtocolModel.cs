using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.SyncService.Contract;

public class SyncProtocolModel
{
	public string Id { get; set; }
	public string ProtocolName { get; set; }
	public CTS.Enums.BodyPart BodyPart { get; set; }
	public bool IsAdult { get; set; }
	public bool IsEnhanced { get; set; }
	public bool IsEmergency { get; set; }

	public string PatientPosition { get; set; }
	public bool IsDefaultMatch { get; set; }
	public bool IsFactory { get; set; }
	public bool IsOnTop { get; set; }
	public string Description { get; set; }
	public string ScanMode { get; set; }
}

public class SyncProtocolResponse
{
	public string BodyPart { get; set; }
	public List<SyncProtocolModel> ProtocolList { get; set; }
}