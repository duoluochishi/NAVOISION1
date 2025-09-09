using NV.CT.Service.TubeWarmUp.DAL.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeWarmUp.Interfaces
{
    public interface IScanParamRespository
    {
        ScanParamDto GetScanParam();
    }
}
