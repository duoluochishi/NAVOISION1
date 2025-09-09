using System;
using System.Globalization;

namespace NV.CT.UI.Controls.Converter;
public class ReconStatusConvert : BasicsConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }
        string reconStatusValue = value.ToString();
        //if (reconStatusValue == "0")
        //{

        //    return "Undo";
        //}
        //if (reconStatusValue == "1")
        //{

        //    return "Wating";
        //}
        //else if (reconStatusValue == "2")
        //{

        //    return "Reconning";
        //}
        //else if(reconStatusValue == "3")
        //{

        //    return "ReconEnd";
        //}
        //else if (reconStatusValue == "4")
        //{

        //    return "ReconFaild";
        //}
        //else if (reconStatusValue == "5")
        //{

        //    return "Issued";
        //}
        return reconStatusValue;
    }
    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.Empty;
    }
}