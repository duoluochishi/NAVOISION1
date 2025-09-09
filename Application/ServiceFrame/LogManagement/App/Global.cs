using System;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.ClientProxy;
using NV.MPS.Communication;

namespace NV.CT.LogManagement
{
    public class Global
    {
        private static ClientInfo? _clientInfo;
        private static MCSServiceClientProxy? _serviceClientProxy;
        private static readonly Lazy<Global> _instance = new Lazy<Global>(() => new Global());
        public static Global Instance => _instance.Value;

        private Global()
        {
        }

        public void CreateDefaultBuilder()
        {
        }

        public void Subscribe()
        {
            _clientInfo = new() { Id = $"[LogManagement]-{DateTime.Now:yyyyMMddHHmmss}" };

            _serviceClientProxy = ServiceFramework.Global.Instance.ServiceProvider.GetRequiredService<MCSServiceClientProxy>();
            _serviceClientProxy?.Subscribe(_clientInfo);
        }
        public void Unsubscribe()
        {
            _serviceClientProxy?.Unsubscribe(_clientInfo);
        }
    }
}