using System;
using System.Globalization;

namespace NV.CT.UI.Controls.Converter
{
    public class CorrectionOriginalRecordConverter : BasicsConverter
    {
        private const string NORMAL_COLOR = "#FFFFFF";
        private const string ORIGINAL_COLOR = "#FFA63F";

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? ORIGINAL_COLOR : NORMAL_COLOR;
            }
            else
            {
                return NORMAL_COLOR;
            }
        }
    }
}
