using Microsoft.Extensions.DependencyInjection;
using NV.CT.ClientProxy;
using NV.MPS.Communication;
using System;

namespace NV.CT.JobViewer
{
    public class Global
    {
        private static readonly Lazy<Global> _instance = new Lazy<Global>(() => new Global());

        private Global()
        {
        }

        public static Global? Instance => _instance.Value;

        public IServiceProvider? ServiceProvider { get; set; }

        private static JobClientProxy? _jobClientProxy;
        private static ClientInfo? _clientInfo;
        private static MCSServiceClientProxy? _serviceClientProxy;

        public void Subscribe()
        {
            var tag = $"[JobViewer]-{DateTime.Now:yyyyMMddHHmmss}";
            _clientInfo = new() { Id = tag };

            _serviceClientProxy = Program.ServiceProvider?.GetRequiredService<MCSServiceClientProxy>();
            _serviceClientProxy?.Subscribe(_clientInfo);

            _jobClientProxy = Program.ServiceProvider?.GetRequiredService<JobClientProxy>();
            _jobClientProxy?.Subscribe(_clientInfo);
        }
        public void Unsubscribe()
        {
            _jobClientProxy?.Unsubscribe(_clientInfo);
            _serviceClientProxy?.Unsubscribe(_clientInfo);
        }
    }
}
