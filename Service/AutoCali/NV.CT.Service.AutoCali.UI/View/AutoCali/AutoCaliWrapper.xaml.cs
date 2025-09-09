using NV.CT.Service.AutoCali.Logic;
using NV.CT.Service.Common;
using NV.CT.ServiceFramework.Contract;
using System.Windows.Controls;

namespace NV.CT.Service.AutoCali.UI
{
    /// <summary>
    /// AutoCaliWrapper.xaml 的交互逻辑
    /// </summary>
    public partial class AutoCaliWrapper : UserControl, IServiceControl
    {
        public AutoCaliWrapper()
        {
            //MCS服务框架在加载条目的时候，就会将控件创建出来
            string block = "AutoCaliWrapper.ctor";
            LogService.Instance.Info(ServiceCategory.AutoCali, $"Beginning {block}");

            InitializeComponent();

            LogService.Instance.Info(ServiceCategory.AutoCali, $"Ended {block}");
        }

        #region 适应MCS服务框架接口要求
        public static readonly string Service_App_Name = "AutoCalibration";

        public string GetServiceAppName()
        {
            return Service_App_Name;
        }

        public string GetServiceAppID()
        {
            return Service_App_Name;
        }

        public string GetTipOnClosing()
        {
            string block = "AutoCaliWrapper.GetTipOnClosing";
            LogService.Instance.Info(ServiceCategory.AutoCali, $"Beginning {block}, Tip when Exiting={_caliViewModel?.TipOnExiting}");

            return _caliViewModel?.TipOnExiting;

        }
        #endregion 适应MCS服务框架接口要求


        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //不要根据自带IsLoaded或者自定义的_isLoaded来过滤，否则不会执行加载控件
            if (_isLoaded) return;

            string block = "AutoCaliWrapper.UserControl_Loaded";
            LogService.Instance.Info(ServiceCategory.AutoCali, $"Beginning {block}");

            _caliViewModel = new AutoCaliViewModel();
            _caliViewModel.ServiceAppName = Service_App_Name;
            this.DataContext = _caliViewModel;

            _isLoaded = true;
            LogService.Instance.Info(ServiceCategory.AutoCali, $"Ended {block}");
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //string block = "AutoCaliWrapper.UserControl_Unloaded";
            //LogService.Instance.Info(ServiceCategory.AutoCali, $"Beginning {block}");

            //_caliViewModel?.UnregisterCaliTaskEvent();
            //_caliViewModel = null;
            //this.DataContext = null;

            //LogService.Instance.Info(ServiceCategory.AutoCali, $"Ended {block}");
        }

        private AbstractCaliViewModel _caliViewModel;
        private bool _isLoaded = false;
    }
}
