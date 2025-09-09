using System.Windows;
using NV.CT.Service.Common;

namespace NV.CT.Service.AutoCali.Demo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DemoStartup.Startup();
            base.OnStartup(e);
        }
    }
}