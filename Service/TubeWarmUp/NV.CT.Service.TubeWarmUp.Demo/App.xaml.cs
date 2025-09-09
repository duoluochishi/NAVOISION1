using NV.CT.Service.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace NV.CT.Service.TubeWarmUp.Demo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            var loader = new WarmupLoader();
            loader.ConfigureServices(null, null);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DemoStartup.Startup();
        }

        private System.Reflection.Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            string assemblyInfo = args.Name;
            var parts = assemblyInfo.Split(',');
            string name = parts[0];
            var version = Version.Parse(parts[1].Split('=')[1]);
            string fullName;
            fullName = System.IO.Path.Combine(System.Environment.CurrentDirectory,
                name + ".dll");
            Console.WriteLine($"load dll name {fullName}");
            if (!System.IO.File.Exists(fullName))
            {
                return null;
            }

            try
            {
                return Assembly.LoadFile(fullName);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}