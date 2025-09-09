//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/10/11 16:40:55           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.Examination.ApplicationService.Contract.ScanControl.Rule;

public class OfflineReadyRule : IUIControlEnableRule
{
    private readonly IOfflineConnectionService _offlineConnectionService;
    public OfflineReadyRule(IOfflineConnectionService offlineConnectionService)
    {
        _offlineConnectionService = offlineConnectionService;
    }

    public bool IsEnabled()
    {
        return _offlineConnectionService.IsConnected;
    }

    public string GetFailReason()
    {
        return "OfflineReadyService is not ok.";
    }

    public event EventHandler? UIStatusChanged;
}