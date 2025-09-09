using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.CTS;
using NV.CT.UI.ViewModel;
using NV.CT.UserConfig.ApplicationService.Impl;
using Prism.Commands;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace NV.CT.UserConfig.ViewModel;
public class ImageTextFontSetViewModel : BaseViewModel
{
    private readonly ImageTextSettingService _imageTextSettingService;

    private bool IsUIChanged = true;
    public ImageTextFontSetViewModel(ImageTextSettingService imageTextSettingService)
    {
        _imageTextSettingService = imageTextSettingService;
        _imageTextSettingService.ImageAnnotationSwitchChanged -= ImageTextSettingService_ImageAnnotationSwitchChanged;
        _imageTextSettingService.ImageAnnotationSwitchChanged += ImageTextSettingService_ImageAnnotationSwitchChanged;

        FontSizes = new ObservableCollection<short>(new short[] { 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 });

        Commands.Add("SetFontColorCommand", new DelegateCommand<string>(SetFontColorCommand));
        Commands.Add("SelectFontColorCommand", new DelegateCommand(SelectFontColorCommand));
    }

    private void ImageTextSettingService_ImageAnnotationSwitchChanged(object? sender, EventArgs<(string, ImageAnnotationSetting)> e)
    {
        if (e is null) return;

        _imageTextSettingService.CurrentImageAnnotationName = e.Data.Item1;
        CurrentImageAnnotationSetting = e.Data.Item2;
        IsUIChanged = false;
        CurrentFontSize = CurrentImageAnnotationSetting.FontSize;

        CurrentFontColor = System.Windows.Media.Color.FromRgb(byte.Parse(e.Data.Item2.FontColorR.ToString()), byte.Parse(e.Data.Item2.FontColorG.ToString()), byte.Parse(e.Data.Item2.FontColorB.ToString())).ToString();
        IsUIChanged = true;
    }

    private void SetFontSizeCommand(short size)
    {
        _imageTextSettingService.SetItemFontSize(size);
    }

    private void SetFontColorCommand(string color)
    {
        System.Windows.Media.Color tColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color);

        _imageTextSettingService.SetItemFontColor(tColor.R, tColor.G, tColor.B);
    }

    private void SelectFontColorCommand()
    {
        ColorDialog colorDlg = new ColorDialog();
        DialogResult dr = colorDlg.ShowDialog();
        if (dr == DialogResult.OK)
        {
            System.Drawing.Color tColor = colorDlg.Color;
            _imageTextSettingService.SetItemFontColor(tColor.R, tColor.G, tColor.B);
        }
    }

    private ImageAnnotationSetting currentImageAnnotationSetting = new ImageAnnotationSetting();
    public ImageAnnotationSetting CurrentImageAnnotationSetting
    {
        get => currentImageAnnotationSetting;
        set => SetProperty(ref currentImageAnnotationSetting, value);
    }

    private short currentFontSize = 12;
    public short CurrentFontSize
    {
        get => currentFontSize;
        set
        {
            if (SetProperty(ref currentFontSize, value) && IsUIChanged)
            {
                SetFontSizeCommand(value);
            }
        }
    }

    private string currentFontColor = string.Empty;
    public string CurrentFontColor
    {
        get => currentFontColor;
        set => SetProperty(ref currentFontColor, value);
    }

    private ObservableCollection<short> fontSizes = new ObservableCollection<short>();
    public ObservableCollection<short> FontSizes
    {
        get => fontSizes;
        set => SetProperty(ref fontSizes, value);
    }
}