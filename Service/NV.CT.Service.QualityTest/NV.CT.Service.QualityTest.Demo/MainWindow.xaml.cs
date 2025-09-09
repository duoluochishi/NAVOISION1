using System;
using System.Diagnostics;

namespace NV.CT.Service.QualityTest.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Process process = Process.GetCurrentProcess();
            process.Kill();
        }
    }
}