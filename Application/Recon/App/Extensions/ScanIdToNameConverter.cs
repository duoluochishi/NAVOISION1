using System.Globalization;
using System.Windows.Data;

namespace NV.CT.Recon.Extensions;

public class ScanIdToNameConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is null)
			return string.Empty;

		var reconRangeViewModel = CTS.Global.ServiceProvider.GetService<ReconRangeViewModel>();
		if (reconRangeViewModel == null)
			return string.Empty;

		var isSuccess = reconRangeViewModel.ScanIdDictionary.TryGetValue(value.ToString(), out string retValue);
		return isSuccess ? retValue : string.Empty;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}