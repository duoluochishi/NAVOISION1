using NV.CT.Service.Common;
using System.Configuration;
using System.Data;
using System.Windows;

namespace NV.CT.DmsTest.StartApp
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
