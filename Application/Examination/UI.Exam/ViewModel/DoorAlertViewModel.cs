namespace NV.CT.UI.Exam.ViewModel;

public class DoorAlertViewModel : BaseViewModel
{
	private BitmapImage _infoImage = new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/dosewarning.png", UriKind.RelativeOrAbsolute));
	public BitmapImage InfoImage
	{
		get => _infoImage;
		set => SetProperty(ref _infoImage, value);
	}
}