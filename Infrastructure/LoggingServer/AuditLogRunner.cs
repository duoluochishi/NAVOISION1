//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/7/8 9:46:44    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.MPS.Configuration;
using NV.TP.NamedPipeWrapper;
using System.Text;

namespace NV.CT.LoggingServer;

public class AuditLogRunner : IHostedService
{
    private ILogger _logger;
    private NamedPipeServer<string> _server;

    public AuditLogRunner(ILoggerFactory factory)
    {
        _logger = factory.CreateLogger("AuditLogging");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server = new NamedPipeServer<string>("AuditLogging");
        _server.ClientConnected += OnClientConnected;
        _server.ClientDisconnected += OnClientDisconnected;
        _server.ClientMessage += OnClientMessage;
        _server.Start();
        return Task.CompletedTask;
    }

    private void OnClientConnected(NamedPipeConnection<string, string> connection)
    {
        _logger.LogTrace($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} [TRC] (LoggingServer, NV.CT.LoggingServer.AuditLogRunner) Client ({connection.Name}) connected to server.");
    }

    private void OnClientDisconnected(NamedPipeConnection<string, string> connection)
    {
        _logger.LogTrace($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} [TRC] (LoggingServer, NV.CT.LoggingServer.AuditLogRunner) Client ({connection.Name}) disconnected to server.");
    }

    private void OnClientMessage(NamedPipeConnection<string, string> connection, string data)
    {
        var info = JsonConvert.DeserializeObject<AuditLogInfo>(data);
        if (SystemConfig.AuditLoggingConfig.AuditLogging.IsEncrypted)
        {
            var temp = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
            _logger.LogInformation($"{info.CreateTime:yyyy-MM-dd HH:mm:ss.ffff} [INF] (AuditLogger, {info.EntryPoint}) {temp}");
        }
        else
        {
            _logger.LogInformation($"{info.CreateTime:yyyy-MM-dd HH:mm:ss.ffff} [INF] (AuditLogger, {info.EntryPoint}) {data}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _server.Stop();
        _server = null;
        return Task.CompletedTask;
    }
}
