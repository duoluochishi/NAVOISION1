using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.Attachments.EventArguments;
using NV.CT.Service.HardwareTest.ViewModels.Universal;
using Panuon.WPF.UI;
using System.Windows;

namespace NV.CT.Service.HardwareTest.UserControls.Universal
{

    public partial class UniversalPopUpView : WindowX
    {
        public UniversalPopUpView()
        {
            InitializeComponent();
            /** 获取ViewModel **/
            var universalPopUpViewModel = Ioc.Default.GetRequiredService<UniversalPopUpViewModel>();
            /** 设置DataContext **/
            this.DataContext = universalPopUpViewModel;
            /** 注册调整窗体事件 **/
            universalPopUpViewModel.AdjustWindowEvent += UniversalPopUpViewModel_AdjustWindowPosition;
            /** 注册窗体事件 **/
            this.CloseButton.Click += (s, e) => this.Close();
        }

        private void UniversalPopUpViewModel_AdjustWindowPosition(AdjustWindowEventArgs args)
        {
            /** 设置窗体宽高 **/
            this.Width = args.Width;
            this.Height = args.Height;
            /** 获取宽高 **/
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.FullPrimaryScreenHeight;
            /** 设置开始位置 **/
            this.Left = (screenWidth / 2) - (this.Width / 2);
            this.Top = (screenHeight / 2) - (this.Height / 2);
        }
    }
}
