using System;
using System.Globalization;

namespace NV.CT.UI.Controls.Converter
{
    public class IsSelectedConverter : BasicsConverter
    {
        private const string SELECTED_COLOR = "#FFA63F";
        private const string UNSELECTED_COLOR = "Transparent";

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? SELECTED_COLOR : UNSELECTED_COLOR;
            }
            else
            {
                return UNSELECTED_COLOR;
            }
        }
    }
}
