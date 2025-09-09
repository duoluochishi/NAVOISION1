//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:43:11    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
using NV.CT.Print.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace NV.CT.Print.View
{
    /// <summary>
    /// ImageOperationView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageOperationView : UserControl
    {
        public ImageOperationView()
        {
            InitializeComponent();

            DataContext = CTS.Global.ServiceProvider.GetRequiredService<ImageOperationViewModel>();

            this.scrollBarPage.MouseWheel += OnMouseScroll;
        }

        private void OnMouseScroll(object sender, MouseWheelEventArgs e)
        {
            var vm = (ImageOperationViewModel)DataContext;
            //Trace.WriteLine($"******* ScrollBarValue is:{vm.ScrollBarValue}  Delta is:{e.Delta}");

            if (e.Delta > 0)
            {
                //roll up
                if (vm.ScrollBarValue <= vm.MinPageNumber)
                {
                    vm.ScrollBarValue = vm.MinPageNumber;
                    return;
                }

                vm.ScrollBarValue -= this.scrollBarPage.SmallChange;
            }
            else
            {
                //roll down
                if (vm.ScrollBarValue >= vm.MaxPageNumber)
                {
                    vm.ScrollBarValue = vm.MaxPageNumber;
                    return;
                }

                vm.ScrollBarValue += this.scrollBarPage.SmallChange;
            }
        }
    }
}
