using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.QualityTest.ViewModels;

namespace NV.CT.Service.QualityTest
{
    internal static class VMManager
    {
        public static QTViewModel MainVM => Get<QTViewModel>();

        private static T Get<T>() where T : notnull
        {
            return Global.ServiceProvider.GetRequiredService<T>();
        }
    }
}