using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace NV.CT.UI.Controls.Converter;

public class SelfCheckStateConverter : BasicsConverter
{
	private const string ERROR = "Error";
	private const string SUCCESS = "Success";
	private const string INPROGRESS = "InProgress";

	private readonly WriteableBitmap _errorImage = new(new BitmapImage(new Uri(
		"pack://application:,,,/NV.CT.UI.Controls;component/Icons/dosewarning.png", UriKind.RelativeOrAbsolute)));
	private readonly WriteableBitmap _successImage = new (new BitmapImage(new Uri(
		"pack://application:,,,/NV.CT.UI.Controls;component/Icons/finish.png", UriKind.RelativeOrAbsolute)));


	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is null)
		{
			return _errorImage;
		}

		var stringValue = value.ToString();
		WriteableBitmap? resultIcon;
		switch(stringValue)
		{
			case ERROR:
                resultIcon = _errorImage;
				break;
			case SUCCESS:
                resultIcon = _successImage;
				break;
			case INPROGRESS:
			default:
				resultIcon = _errorImage;
				break;
		}
		return resultIcon;

    }
}

