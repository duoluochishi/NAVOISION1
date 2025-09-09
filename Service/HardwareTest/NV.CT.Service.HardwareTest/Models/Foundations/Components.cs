using NV.CT.Service.HardwareTest.Models.Foundations.Abstractions;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.Views.Components.Collimator;
using NV.CT.Service.HardwareTest.Views.Components.Detector;
using NV.CT.Service.HardwareTest.Views.Components.Table;
using NV.CT.Service.HardwareTest.Views.Components.XRaySource;
using NV.CT.Service.HardwareTest.Views.Integrations.ComponentEnablement;
using NV.CT.Service.HardwareTest.Views.Integrations.ComponentStatus;
using NV.CT.Service.HardwareTest.Views.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.Views.Integrations.ImageChain;
using NV.CT.Service.HardwareTest.Views.Integrations.SelfCheck;
using NV.CT.Service.HardwareTest.Views.Integrations.SystemEnvironment;

namespace NV.CT.Service.HardwareTest.Models.Foundations
{
    public class XRaySourceComponent : AbstractComponent
    {
        public XRaySourceComponent() : base()
        {
            this.Name = "XRaySource";
            this.ComponentTestItemMenu = new()
            {
                new ComponentTestItem { Name = "Comprehensive Testing", NavigationViewType = typeof(XRaySourceComprehensiveTestingView) },
                new ComponentTestItem { Name = XRaySourceKVMACoefficientDefaults.KVMACoefficients, NavigationViewType = typeof(XRaySourceKVMACoefficientsView) },
            };
            this.CurrentTestItem = this.ComponentTestItemMenu[0];
        }
    }

    public class CollimatorComponent : AbstractComponent
    {
        public CollimatorComponent() : base()
        {
            this.Name = "Collimator";
            this.ComponentTestItemMenu = new()
            {
                new ComponentTestItem { Name = "Collimator Calibration", NavigationViewType = typeof(CollimatorCalibrationView) }
            };
            this.CurrentTestItem = this.ComponentTestItemMenu[0];
        }
    }

    public class TableComponent : AbstractComponent
    {
        public TableComponent() : base()
        {
            this.Name = "Table";
            this.ComponentTestItemMenu = new()
            {
                new ComponentTestItem { Name = "Three-Axis Motion Testing", NavigationViewType = typeof(TableThreeAxisMotionTestingView) }
            };
            this.CurrentTestItem = this.ComponentTestItemMenu[0];
        }
    }

    public class DetectorComponent : AbstractComponent
    {
        public DetectorComponent() : base()
        {
            this.Name = "Detector";
            this.ComponentTestItemMenu = new()
            {
                new ComponentTestItem { Name = "Irradiation Data Statistics", NavigationViewType = typeof(DetectorStatsView) },
                new ComponentTestItem { Name = "Set Temperature", NavigationViewType = typeof(SetTemperatureView) },
            };
            this.CurrentTestItem = this.ComponentTestItemMenu[0];
        }
    }

    public class SystemComponent : AbstractComponent
    {
        public SystemComponent()
        {
            this.Name = "System";
            this.ComponentTestItemMenu =
            [
                new ComponentTestItem { Name = "Data Acquisition", NavigationViewType = typeof(DataAcquisitionTestingView) },
                new ComponentTestItem { Name = "Image Chain Testing", NavigationViewType = typeof(ImageChainTestingView) },
                new ComponentTestItem { Name = "Component Status", NavigationViewType = typeof(ComponentStatusTestingView) },
                new ComponentTestItem { Name = "Self Check", NavigationViewType = typeof(SelfCheckTestingView) },
                new ComponentTestItem { Name = "System Environment", NavigationViewType = typeof(SystemEnvironmentView) },
                new ComponentTestItem { Name = "Component Enablement", NavigationViewType = typeof(ComponentEnablementView) },
            ];
            this.CurrentTestItem = this.ComponentTestItemMenu[0];
        }
    }
}