using NV.CT.NP.Tools.DataTransfer.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NV.CT.NP.Tools.DataTransfer.Converter
{
    public class ExportStatus2StrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (ExportStatus)value;
            if (status == ExportStatus.Success)
                return "success";
            else if (status == ExportStatus.Fail)
                return "fail";
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
