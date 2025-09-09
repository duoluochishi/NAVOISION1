//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/9/14 16:54:29           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.FacadeProxy.Common.EventArguments.OfflineMachine;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface IOfflineProxyService
{
    event EventHandler<bool> ConnectionStatusChanged;

    event EventHandler<(string Timestamp, string ErrorCode)> ErrorOccured;

    event EventHandler<OfflineMachineTaskStatusChangedEventArgs> TaskStatusChanged;

    event EventHandler<DicomImageSavedInfoReceivedEventArgs> ImageSaved;

    event EventHandler<OfflineMachineTaskProgressChangedEventArgs> ProgressChanged;

    bool IsConnected { get; }
}
