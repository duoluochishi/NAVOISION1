using NV.CT.CTS.Enums;
using NV.CT.Protocol.Models;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces
{
    public interface IScanTaskLayoutDecider
    {
        bool CanAccept(ScanModel scanModel);

        ScanTaskAvailableLayout GetScanTaskLayout(ScanModel scanModel);
    }

}
