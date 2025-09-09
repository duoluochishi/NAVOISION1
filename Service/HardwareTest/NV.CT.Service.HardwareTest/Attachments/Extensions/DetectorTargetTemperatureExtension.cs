using System.Collections.Generic;
using NV.CT.FacadeProxy.Common.Models.DetectorTemperature;
using NV.CT.Service.HardwareTest.Models.Components.Detector;
using NV.MPS.Environment;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    public static class DetectorTargetTemperatureExtension
    {
        public static DetectorTargetTemperature ToDetectorTargetTemperature(this DetectorTargetTemperatureModel item)
        {
            return new DetectorTargetTemperature()
            {
                DetectorIndex = item.DetectorIndex,
                Channel1TargetTemperature = (int)item.Detects[0].SetValue.ExpandTen(),
                Channel2TargetTemperature = (int)item.Detects[1].SetValue.ExpandTen(),
                Channel3TargetTemperature = (int)item.Detects[2].SetValue.ExpandTen(),
                Channel4TargetTemperature = (int)item.Detects[3].SetValue.ExpandTen(),
            };
        }

        public static IEnumerable<DetectorTargetTemperature> ToDetectorTargetTemperature(this IEnumerable<DetectorTargetTemperatureModel> items)
        {
            foreach (var item in items)
            {
                yield return item.ToDetectorTargetTemperature();
            }
        }
    }
}