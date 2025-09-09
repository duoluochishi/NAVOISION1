//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:44:32    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.TP.NamedPipeWrapper;

namespace NV.CT.LoggingServer;

/// <summary>
/// 主启动服务
/// </summary>
public class MainRunner : IHostedService
{
    private readonly ILogger<MainRunner> _logger;
    private NamedPipeServer<string> _server;

    public MainRunner(ILogger<MainRunner> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server = new NamedPipeServer<string>("Logging");
        _server.ClientConnected += OnClientConnected;
        _server.ClientDisconnected += OnClientDisconnected;
        _server.ClientMessage += OnClientMessage;
        _server.Start();
        return Task.CompletedTask;
    }

    private void OnClientConnected(NamedPipeConnection<string, string> connection)
    {
        _logger.LogTrace($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} [TRC] (LoggingServer, NV.CT.LoggingServer.MainRunner) Client ({connection.Name}) connected to server.");
    }

    private void OnClientDisconnected(NamedPipeConnection<string, string> connection)
    {
        _logger.LogTrace($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} [TRC] (LoggingServer, NV.CT.LoggingServer.MainRunner) Client ({connection.Name}) disconnected to server.");
    }

    private void OnClientMessage(NamedPipeConnection<string, string> connection, string data)
    {
        var info = JsonConvert.DeserializeObject<LogInfo>(data);
        switch (info.Level)
        {
            case LogLevel.Critical:
                _logger.LogCritical(info.Exception, $"{info.CreateTime:yyyy-MM-dd HH:mm:ss.ffff} [FTL] ({info.ClientName}, {info.ClassName}) {info.Message}");
                break;
            case LogLevel.Error:
                _logger.LogError(info.Exception, $"{info.CreateTime:yyyy-MM-dd HH:mm:ss.ffff} [ERR] ({info.ClientName}, {info.ClassName}) {info.Message}");
                break;
            case LogLevel.Warning:
                _logger.LogWarning(info.Exception, $"{info.CreateTime:yyyy-MM-dd HH:mm:ss.ffff} [WRN] ({info.ClientName}, {info.ClassName}) {info.Message}");
                break;
            case LogLevel.Information:
                _logger.LogInformation(info.Exception, $"{info.CreateTime:yyyy-MM-dd HH:mm:ss.ffff} [INF] ({info.ClientName}, {info.ClassName}) {info.Message}");
                break;
            case LogLevel.Debug:
                _logger.LogDebug(info.Exception, $"{info.CreateTime:yyyy-MM-dd HH:mm:ss.ffff} [DBG] ({info.ClientName}, {info.ClassName}) {info.Message}");
                break;
            default:
                _logger.LogTrace(info.Exception, $"{info.CreateTime:yyyy-MM-dd HH:mm:ss.ffff} [TRC] ({info.ClientName}, {info.ClassName}) {info.Message}");
                break;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} [TRC] (LoggingServer, NV.CT.LoggingServer.MainRunner) Close logging service.");
        _server.Stop();
        _server = null;
        return Task.CompletedTask;
    }
}
