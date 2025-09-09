//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/26 9:07:12     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;

public interface IMeasurementStatusService
{
    void RaiseMeasurementLoadingFailed(string measurementId);

    void RaiseMeasurementLoaded(string measurementId);

    void RaiseMeasurementCancelled(string measurementId);

    void RasiseMeasurementAborted(string measurementId, bool modelType, string scanId, string reconId, FailureReasonType reasonType);

    void RaiseMeasurementDone(string measurementId);

    event EventHandler<CTS.EventArgs<string>> MeasurementLoadingFailed;
    event EventHandler<CTS.EventArgs<string>> MeasurementLoaded;
    event EventHandler<CTS.EventArgs<(string MeasurementId, bool ModelType, string ScanId, string ReconId, FailureReasonType ReconType)>>? MeasurementAborted;
    event EventHandler<CTS.EventArgs<string>>? MeasurementCanceled;
    event EventHandler<CTS.EventArgs<string>>? MeasurementDone;
}
