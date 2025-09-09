using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.UI.ViewModel;
using NV.CT.UserConfig.ApplicationService.Impl;
using Prism.Commands;
using System.Collections.ObjectModel;

namespace NV.CT.UserConfig.ViewModel;

public class ImageTextViewSetViewModel : BaseViewModel
{
    private readonly ImageTextSettingService _imageTextSettingService;
    public ImageTextViewSetViewModel(ImageTextSettingService imageTextSettingService)
    {
        _imageTextSettingService = imageTextSettingService;
        _imageTextSettingService.ImageAnnotationSwitchChanged -= ImageTextSettingService_ImageAnnotationSwitchChanged;
        _imageTextSettingService.ImageAnnotationSwitchChanged += ImageTextSettingService_ImageAnnotationSwitchChanged;
        Locations.Add(FourCornersLocation.LeftTop.ToString());
        Locations.Add(FourCornersLocation.RightTop.ToString());
        Locations.Add(FourCornersLocation.RightBottom.ToString());
        Locations.Add(FourCornersLocation.LeftBottom.ToString());

        Commands.Add("SetItemVisibilityCommand", new DelegateCommand<AnnotationItem>(SetItemVisibilityCommand));
        Commands.Add("SetItemLocationCommand", new DelegateCommand<AnnotationItem>(SetItemLocationCommand));
        Commands.Add("SetItemCommand", new DelegateCommand<AnnotationItem>(SetItemCommand));
    }

    private void SetItemCommand(AnnotationItem obj)
    {
        CurrentSelectedItem = obj;
    }

    private void ImageTextSettingService_ImageAnnotationSwitchChanged(object? sender, EventArgs<(string, ImageAnnotationSetting setting)> e)
    {
        if (e is null || e.Data.setting is null) { return; }
        Items = new ObservableCollection<AnnotationItem>(e.Data.setting.AnnotationItemSettings.FindAll(t => !string.IsNullOrEmpty(t.Name) && !t.Name.Contains("OrientationChar") && !t.Name.Equals("WL")));
    }

    private void SetItemVisibilityCommand(AnnotationItem item)
    {
        _imageTextSettingService.SetItemVisibility(item, item.Visibility);
        Items = new ObservableCollection<AnnotationItem>(_imageTextSettingService.CurrentImageAnnotationSetting.AnnotationItemSettings.FindAll(t => !string.IsNullOrEmpty(t.Name) && !t.Name.Contains("OrientationChar") && !t.Name.Equals("WL")));
    }

    private void SetItemLocationCommand(AnnotationItem item)
    {
        if (item is null) { return; }
        var location = item.Location;
        _imageTextSettingService.SetItemLocation(item, location);
        CurrentSelectedItem = item;
    }

    private ObservableCollection<AnnotationItem> items = new ObservableCollection<AnnotationItem>();
    public ObservableCollection<AnnotationItem> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    private AnnotationItem _currentSelectedItem;
    public AnnotationItem CurrentSelectedItem
    {
        get => _currentSelectedItem;
        set => SetProperty(ref _currentSelectedItem, value);
    }

    private ObservableCollection<string> locations = new ObservableCollection<string>();
    public ObservableCollection<string> Locations
    {
        get => locations;
        set => SetProperty(ref locations, value);
    }
}