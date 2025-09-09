using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Essentials.Logger;
using NV.CT.FacadeProxy.Models;
using NV.MPS.Environment;
using System;

namespace NV.CT.Service.Common
{
    public class ProxyHelper
    {
        private bool _loaded;
        private static readonly Lazy<ProxyHelper> Singleton = new(() => new());
        public static ProxyHelper Instance => Singleton.Value;

        private ProxyHelper()
        {
        }

        internal void Init()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            LoggerManager.Logger = new ProxyLogWrapper();
            var config = RuntimeConfig.MRSServices;
            var acqReconServer = new ServerInfo(Convert(config.DeviceServer), Convert(config.ReconCommandServer), Convert(config.ReconStatusServer), Convert(config.ReconDataServer));
            var offlineReconServer = new OfflineServerInfo()
            {
                IP = config.OfflineCommandServer.IP,
                CmdPort = config.OfflineCommandServer.Port,
                StatePort = config.OfflineStatusServer.Port,
            };
            AcqReconProxy.Instance.Init(acqReconServer);
            UpgradeProxy.Instance.Init(acqReconServer.Device);
            AutoCalibrationProxy.Instance.Init(acqReconServer);
            ShutdownProxy.Instance.Init(acqReconServer, offlineReconServer);
        }

        private IPPort Convert(ServiceEndpoint point)
        {
            return new IPPort()
            {
                IP = point.IP,
                Port = point.Port,
            };
        }
    }
}