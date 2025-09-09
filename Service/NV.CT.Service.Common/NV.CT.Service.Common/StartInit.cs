using NV.CT.ServiceFramework.Contract;

namespace NV.CT.Service.Common
{
    public class StartInit : IInitializer
    {
        public void Initialize()
        {
            ProxyHelper.Instance.Init();
        }
    }
}