using NV.CT.Service.Common;
using System.Windows;

namespace NV.CT.Service.Hardware.StartUp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            this.BeforeStartUp();
            base.OnStartup(e);
        }

        private void BeforeStartUp()
        {
            /** 临时调用Demo start up **/
            DemoStartup.Startup();
        }
    }
}
