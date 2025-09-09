using NV.CT.Service.HardwareTest.Models.Components.Detector;
using System;
using System.Text;

namespace NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition
{
    public class DetectorTemperatureData
    {
        public DetectorTemperatureData()
        {
            this.TimeStamp = DateTime.Now;
            this.Sources = new DetectorSource[16];
        }

        public DateTime TimeStamp { get; set; }
        public DetectorSource[] Sources { get; set; }

        #region Temperature Data

        public string TemperatureInformation => this.GetAllTemperatureInformation();
        private string GetAllTemperatureInformation() 
        {
            StringBuilder builder = new StringBuilder();
            /** 时间戳 **/
            builder.Append(TimeStamp.ToString("[yyyy-MM-dd HH:mm:ss.fff] "));
            /** 温度信息遍历 **/
            for (int i = 0; i < Sources.Length; i++) 
            {
                builder.Append($"Detector {(i + 1).ToString("00")} - {Sources[i].TemperatureInformation}");
            }
            return builder.ToString();
        }

        #endregion
    }

}
