using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.DatabaseService.Contract.Models;

public class UpdateStudyProtocolReq
{
	public string StudyId { get; set; }
	public string Protocol { get; set; }
}

