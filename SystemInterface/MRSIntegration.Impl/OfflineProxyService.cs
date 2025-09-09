//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/3/13 14:05:25     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.EventArguments.OfflineMachine;
using NV.CT.FacadeProxy.Essentials.Logger;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class OfflineProxyService : IOfflineProxyService
{
	private bool _isInitialized;
	private readonly ILogger<OfflineProxyService> _logger;

	public bool IsConnected { get; private set; }

	public event EventHandler<bool> ConnectionStatusChanged;
	public event EventHandler<(string Timestamp, string ErrorCode)> ErrorOccured;
	public event EventHandler<OfflineMachineTaskStatusChangedEventArgs> TaskStatusChanged;
	public event EventHandler<DicomImageSavedInfoReceivedEventArgs> ImageSaved;
	public event EventHandler<OfflineMachineTaskProgressChangedEventArgs> ProgressChanged;

	public OfflineProxyService(ILogger<OfflineProxyService> logger)
	{
		_isInitialized = false;
		_logger = logger;
		Initialize();
	}

	private void Initialize()
	{
		if (_isInitialized) return;

        OfflineMachineTaskProxy.Instance.OfflineMachineConnectionStatusChanged += (s, e) =>
		{
			IsConnected = e.IsConnected;
			ConnectionStatusChanged?.Invoke(this, IsConnected);
		};
        OfflineMachineTaskProxy.Instance.OfflineMachineErrorInfoReceived += (s, e) => {
            ErrorOccured?.Invoke(this, (e.TimeStamp, e.ErrorCode));
		};
        OfflineMachineTaskProxy.Instance.OfflineMachineTaskStatusChanged += (s, e) =>
		{
            _logger.LogDebug($"OfflineMachineTaskProxy.OfflineTaskStatusChanged, event arguments: {JsonConvert.SerializeObject(e)}");
            TaskStatusChanged?.Invoke(this, e);
		};
        OfflineMachineTaskProxy.Instance.OfflineMachineTaskProgressChanged += (s, e) => {
            ProgressChanged?.Invoke(this, e);
        };
        OfflineMachineTaskProxy.Instance.OfflineMachineTaskDicomImageSavedInfoReceived += (s, e) =>
		{
			ImageSaved?.Invoke(this, e);
		};

        try
		{
			LoggerManager.MicroLogger = _logger;
			_logger.LogDebug($"OfflineMachineTaskProxy.Initialize, current status: {OfflineMachineTaskProxy.Instance.IsOfflineMachineConnected}");
			IsConnected = OfflineMachineTaskProxy.Instance.IsOfflineMachineConnected;
			_isInitialized = true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"OfflineMachineTaskProxy.Initialize, exception: {ex.Message}");
		}
    }
}
