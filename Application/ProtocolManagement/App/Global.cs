using Microsoft.Extensions.DependencyInjection;
using NV.CT.ClientProxy;
using NV.MPS.Communication;
using System;

namespace NV.CT.ProtocolManagement
{
    public class Global
    {
        public string SelectNodeID = string.Empty;
        public string SelectTemplateID = string.Empty;

        private static Lazy<Global> _instance = new Lazy<Global>(() => new Global());

        public IServiceProvider ServiceProvider { get; set; }

        private ClientInfo _clientInfo;
        private MCSServiceClientProxy _serviceClientProxy;

        private Global()
        {
        }

        public static Global Instance => _instance.Value;

        public void Initialize()
        {

        }

        public void Subscribe()
        {
            var tag = $"[ProtocolManagement]-{DateTime.Now:yyyyMMddHHmmss}";

            _clientInfo = new ClientInfo { Id = $"tag" };
            _serviceClientProxy = ServiceProvider?.GetRequiredService<MCSServiceClientProxy>();
            _serviceClientProxy.Subscribe(_clientInfo);
        }

        public void Unsubscribe()
        {
            _serviceClientProxy?.Unsubscribe(_clientInfo);
        }
    }
}