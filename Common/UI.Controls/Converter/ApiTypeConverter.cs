using System;
using System.Globalization;

namespace NV.CT.UI.Controls.Converter
{
    public class ApiTypeConverter: BasicsConverter
    {
        private const string PREVOICE = "Pre-voice";
        private const string POSTVOICE = "Post-voice";

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return booleanValue ? PREVOICE : POSTVOICE;
            }
            else
            {
                return PREVOICE;
            }
        }
    }
}
