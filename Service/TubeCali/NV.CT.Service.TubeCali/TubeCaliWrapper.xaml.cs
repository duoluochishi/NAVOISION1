using NV.CT.Service.TubeCali.ViewModels;
using NV.CT.ServiceFramework.Contract;

namespace NV.CT.Service.TubeCali
{
    /// <summary>
    /// TubeCaliWrapper.xaml 的交互逻辑
    /// </summary>
    public partial class TubeCaliWrapper : IServiceControl
    {
        private const string ServiceAppName = "TubeCali";

        public TubeCaliWrapper()
        {
            InitializeComponent();
        }

        public string GetServiceAppID()
        {
            return ServiceAppName;
        }

        public string GetServiceAppName()
        {
            return ServiceAppName;
        }

        public string GetTipOnClosing()
        {
            if (TubeCaliView.DataContext is TubeCaliViewModel vm)
            {
                if (vm.IsDoingCali)
                {
                    return "Tube calibration is running!";
                }

                if (vm.IsDoingCheck)
                {
                    return "Tube calibration result check is running!";
                }

                return string.Empty;
            }

            return string.Empty;
        }
    }
}