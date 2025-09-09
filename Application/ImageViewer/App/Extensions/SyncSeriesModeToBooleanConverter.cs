using NV.CT.ImageViewer.ViewModel;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.ImageViewer.Extensions;

public class SyncSeriesModeToBooleanConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		SyncSeriesMode s = (SyncSeriesMode)value;
		return s == (SyncSeriesMode)int.Parse(parameter.ToString());
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		bool isChecked = (bool)value;
		if (!isChecked)
		{
			return null;
		}
		return (SyncSeriesMode)int.Parse(parameter.ToString());
	}
}
