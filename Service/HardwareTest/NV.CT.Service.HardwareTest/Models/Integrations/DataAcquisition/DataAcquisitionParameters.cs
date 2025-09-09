using NV.CT.Service.HardwareTest.Categories;
using NV.CT.Service.HardwareTest.Share.Enums;

namespace NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition
{
    public class DataAcquisitionParameters
    {
        public DataAcquisitionParameters()
        {
            /** Configure **/
            this.InitializeProperties();
            /** RegisterEvents **/
            this.RegisterEvents();
        }

        #region Initialize

        private void InitializeProperties()
        {
            this.ExposureParameters = new()
            {
                SlaveDevTest = 1,
                ExposureRelatedChildNodesConfig = 8193
                
            };
            this.DetectorParameters = new()
            {
                ExposureTime = this.ExposureParameters.ExposureTime,
                FrameTime = this.ExposureParameters.FrameTime,
                DelayExposureTime = this.ExposureParameters.ExposureDelayTime,
                TargetTemperature = 260,
                HeartBeatTimeInterval = 90,
                PreOffsetEnable = CommonSwitch.Enable
            };
        }

        #endregion

        #region Properties

        public ExposureBaseCategory ExposureParameters { get; set; } = null!;
        public DetectorBaseCategory DetectorParameters { get; set; } = null!;

        #endregion

        #region EventsRegister

        /** 当曝光参数中的ExposureTime、FrameTime更新时，同步更新探测器参数 **/
        private void RegisterEvents()
        {
            this.ExposureParameters.ExposureTimeChanged += ExposureParameters_ExposureTimeChanged;
            this.ExposureParameters.FrameTimeChanged += ExposureParameters_FrameTimeChanged;
            this.ExposureParameters.DelayExposureTimeChanged += ExposureParameters_DelayExposureTimeChanged;
        }

        private void ExposureParameters_DelayExposureTimeChanged(float newValue)
        {
            this.DetectorParameters.DelayExposureTime = newValue;
        }

        private void ExposureParameters_FrameTimeChanged(float newValue)
        {
            this.DetectorParameters.FrameTime = newValue;
        }

        private void ExposureParameters_ExposureTimeChanged(float newValue)
        {
            this.DetectorParameters.ExposureTime = newValue;
        }

        #endregion
    }
}