//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/4/13 11:13:52       V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class CTBoxStatusService : ICTBoxStatusService
{
    private PartStatus _status = PartStatus.Normal;
    private readonly IRealtimeStatusProxyService _realtimeStatusService;

    public CTBoxStatusService(IRealtimeStatusProxyService realtimeStatusService)
    {
        _realtimeStatusService = realtimeStatusService;
        _realtimeStatusService.CycleStatusChanged += RealtimeStatusService_CycleStatusChanged;
    }

    private void RealtimeStatusService_CycleStatusChanged(object? sender, CTS.EventArgs<Contract.Models.DeviceSystem> e)
    {
        var boxStatus = e.Data.CTBox.Status;
        if (boxStatus != _status)
        {
            _status = boxStatus;
            StatusChanged?.Invoke(this, _status);
        }
    }

    public PartStatus Status => _status;

    public event EventHandler<PartStatus>? StatusChanged;
}
