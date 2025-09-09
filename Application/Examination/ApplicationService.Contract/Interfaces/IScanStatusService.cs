//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/25 16:56:42     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Protocol.Models;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;

public interface IScanStatusService
{
    MeasurementModel CurrentMeasurement { get; set; }

    ScanModel CurrentScan { get; set; }

    void CancelMeasurement();

    SystemStatus PreviewStatus { get; }

    SystemStatus CurrentStatus { get; }

    event EventHandler<CTS.EventArgs<string>>? ScanStarted;

    event EventHandler<CTS.EventArgs<(string, string, bool)>>? ScanCancelled;

    event EventHandler<CTS.EventArgs<(string ScanId, RealDoseInfo DoseInfo, bool IsCancelled)>>? ScanDone;

    event EventHandler<CTS.EventArgs<(string ScanId, RealDoseInfo DoseInfo, bool IsEmergencyStopped, bool IsUserCancelled)>>? ScanAborted;

    event EventHandler<EventArgs<(string ScanId, string ImagePath)>>? RawDataSaved;
}
