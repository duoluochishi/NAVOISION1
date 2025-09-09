using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace NV.CT.UI.Controls.Converter;

public class FileValidationStateConverter : BasicsConverter
{
	private const string Added = "Added";
	private const string Modified = "Modified";
	private const string Deleted = "Deleted";
	private const string Normal = "Normal";

	private readonly WriteableBitmap _addedImage = new(new BitmapImage(new Uri(
		"pack://application:,,,/NV.CT.UI.Controls;component/Icons/add_02.png", UriKind.RelativeOrAbsolute)));
	private readonly WriteableBitmap _modifiedImage = new (new BitmapImage(new Uri(
		"pack://application:,,,/NV.CT.UI.Controls;component/Icons/Custom.png", UriKind.RelativeOrAbsolute)));
	private readonly WriteableBitmap _deletedImage = new(new BitmapImage(new Uri(
		"pack://application:,,,/NV.CT.UI.Controls;component/Icons/close.png", UriKind.RelativeOrAbsolute)));
	private readonly WriteableBitmap _normalImage = new(new BitmapImage(new Uri(
		"pack://application:,,,/NV.CT.UI.Controls;component/Icons/finish.png", UriKind.RelativeOrAbsolute)));


	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is null)
		{
			return _normalImage;
		}

		var stringValue = value.ToString();
		WriteableBitmap? resultIcon;
		switch(stringValue)
		{
			case Added:
                resultIcon = _addedImage;
				break;
			case Modified:
                resultIcon = _modifiedImage;
				break;
			case Deleted:
				resultIcon = _deletedImage;
				break;
			case Normal:
				resultIcon = _normalImage;
				break;
			default:
				resultIcon = _normalImage;
				break;
		}
		return resultIcon;

    }
}

