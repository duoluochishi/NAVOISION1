
using NV.CT.CTS;
using NV.CT.UI.ViewModel;
using NV.CT.UserConfig.ApplicationService.Impl;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;

namespace NV.CT.UserConfig.ViewModel;
public class ImageTextTypeListViewModel : BaseViewModel
{
    private readonly ImageTextSettingService _imageTextSettingService;

    public ImageTextTypeListViewModel(ImageTextSettingService imageTextSettingService)
    {
        _imageTextSettingService = imageTextSettingService;
        Commands.Add("LoadImageAnnotationCommand", new DelegateCommand<string>(LoadImageAnnotationCommand));
    }

    private void LoadImageAnnotationCommand(string imageAnnotationName)
    {
        _imageTextSettingService.SwitchViewType(imageAnnotationName);
        LeftWidth = ImageAnnotationTabs.FindIndex(t => t.ImageAnnotationName == imageAnnotationName) * 200;
    }

    public void LoadFirst()
    {
        LoadImageAnnotationCommand(ImageAnnotationSettingNames.ScanTopo);
    }

    public void LoadImageAnnotationNames()
    {
        List<ImageAnnotationTab> list = new List<ImageAnnotationTab>();

        list.Add(new ImageAnnotationTab { ImageAnnotationName = ImageAnnotationSettingNames.ScanTopo });
        list.Add(new ImageAnnotationTab { ImageAnnotationName = ImageAnnotationSettingNames.ScanTomo });
        list.Add(new ImageAnnotationTab { ImageAnnotationName = ImageAnnotationSettingNames.View });
        list.Add(new ImageAnnotationTab { ImageAnnotationName = ImageAnnotationSettingNames.Print });
        list.Add(new ImageAnnotationTab { ImageAnnotationName = ImageAnnotationSettingNames.MPR });
        list.Add(new ImageAnnotationTab { ImageAnnotationName = ImageAnnotationSettingNames.VR });

        ImageAnnotationTabs.Clear();
        ImageAnnotationTabs.AddRange(list);

        ActiveTypes.Add(ImageAnnotationSettingNames.ScanTopo, false);
        ActiveTypes.Add(ImageAnnotationSettingNames.ScanTomo, false);
        ActiveTypes.Add(ImageAnnotationSettingNames.View, false);
        ActiveTypes.Add(ImageAnnotationSettingNames.Print, false);
        ActiveTypes.Add(ImageAnnotationSettingNames.MPR, false);
        ActiveTypes.Add(ImageAnnotationSettingNames.VR, false);
    }

    private List<ImageAnnotationTab> imageAnnotationTabs = new List<ImageAnnotationTab>();
    public List<ImageAnnotationTab> ImageAnnotationTabs
    {
        get => imageAnnotationTabs;
        set => SetProperty(ref imageAnnotationTabs, value);
    }

    private ObservableDictionary<string, bool> activeTypes = new ObservableDictionary<string, bool>();
    public ObservableDictionary<string, bool> ActiveTypes
    {
        get => activeTypes;
        set => SetProperty(ref activeTypes, value);
    }
    private double leftWidth = 0;
    public double LeftWidth
    {
        get => leftWidth;
        set => SetProperty(ref leftWidth, value);
    }
}

public class ImageAnnotationTab : BindableBase
{
    private string imageAnnotationName = string.Empty;
    public string ImageAnnotationName
    {
        get => imageAnnotationName;
        set => SetProperty(ref imageAnnotationName, value);
    }

    private string statusBackground = string.Empty;
    public string StatusBackground
    {
        get => statusBackground;
        set => SetProperty(ref statusBackground, value);
    }
}