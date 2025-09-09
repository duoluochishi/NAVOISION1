using NV.CT.Service.TubeWarmUp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NV.CT.Service.TubeWarmUp.Converters
{
    public class TubeHeatCapStatusConverter : IValueConverter
    {
        public Brush Low { get; set; }
        public Brush Normal { get; set; }
        public Brush AboveNormal { get; set; }
        public Brush High { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
            {
                return Low;
            }
            if (value is TubeHeatCapStatus status)
            {
                return status switch
                {
                    TubeHeatCapStatus.Low => Low,
                    TubeHeatCapStatus.Normal => Normal,
                    TubeHeatCapStatus.AboveNormal => AboveNormal,
                    TubeHeatCapStatus.High => High,
                    _ => Low,
                };
            }
            return Low;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
