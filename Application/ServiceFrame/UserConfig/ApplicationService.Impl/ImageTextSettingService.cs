using NAudio.Gui;
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.CTS;
using NV.CT.CTS.Enums;

namespace NV.CT.UserConfig.ApplicationService.Impl;
public class ImageTextSettingService
{
    private readonly IImageAnnotationService _fourCornersService;
    public event EventHandler<EventArgs<(string, ImageAnnotationSetting)>> ImageAnnotationSwitchChanged;
    public event EventHandler<EventArgs<(AnnotationItem, bool)>> ImageAnnotationItemVisibilityChanged;
    public event EventHandler<EventArgs<(AnnotationItem, FourCornersLocation)>> ImageAnnotationItemLocationChanged;
    public event EventHandler<EventArgs<(string, short)>> FontSizeChanged;
    public event EventHandler<EventArgs<(string, string)>> FontColorChanged;

    public string CurrentImageAnnotationName { get; set; }

    public ImageTextSettingService(IImageAnnotationService fourCornersService)
    {
        _fourCornersService = fourCornersService;
        CurrentImageAnnotationConfig = _fourCornersService.GetConfigs();
    }

    public void SwitchViewType(string viewTypeName)
    {
        if (CurrentImageAnnotationSetting is null || string.IsNullOrEmpty(viewTypeName))
        {
            return;
        }
        CurrentImageAnnotationName = viewTypeName;
        SetCurrentSetting(viewTypeName);
        ImageAnnotationSwitchChanged.Invoke(this, new EventArgs<(string, ImageAnnotationSetting)>((viewTypeName, CurrentImageAnnotationSetting)));
    }

    public void SetItemVisibility(AnnotationItem annotationItem, bool isVisible)
    {
        SetCurrentSetting(CurrentImageAnnotationName);
        CurrentImageAnnotationSetting.AnnotationItemSettings.FirstOrDefault(x => x.Name == annotationItem.Name).Visibility = isVisible;
        ImageAnnotationItemVisibilityChanged.Invoke(this, new CTS.EventArgs<(AnnotationItem, bool)>((annotationItem, isVisible)));
    }

    public void SetItemLocation(AnnotationItem annotationItem, FourCornersLocation location)
    {
        SetCurrentSetting(CurrentImageAnnotationName);
        CurrentImageAnnotationSetting.AnnotationItemSettings.FirstOrDefault(x => x.Name == annotationItem.Name).Location = location;
        ImageAnnotationItemLocationChanged.Invoke(this, new CTS.EventArgs<(AnnotationItem, FourCornersLocation)>((annotationItem, location)));
    }

    public void SetItemFontSize(short fontSize)
    {
        SetCurrentSetting(CurrentImageAnnotationName);
        CurrentImageAnnotationSetting.FontSize = fontSize;
        FontSizeChanged.Invoke(this, new CTS.EventArgs<(string, short)>((CurrentImageAnnotationName, fontSize)));
    }

    public void SetItemFontColor(byte red, byte green, byte blue)
    {
        SetCurrentSetting(CurrentImageAnnotationName);

        CurrentImageAnnotationSetting.FontColorR = (short)red;
        CurrentImageAnnotationSetting.FontColorG = (short)green;
        CurrentImageAnnotationSetting.FontColorB = (short)blue;

        FontColorChanged.Invoke(this, new CTS.EventArgs<(string, string)>((CurrentImageAnnotationName, $"{CurrentImageAnnotationSetting.FontColorR.ToString()},{CurrentImageAnnotationSetting.FontColorG.ToString()},{CurrentImageAnnotationSetting.FontColorB.ToString()}")));
    }

    public void SetCurrentSetting(string name)
    {
        switch (name)
        {
            case ImageAnnotationSettingNames.ScanTopo:
                CurrentImageAnnotationSetting = CurrentImageAnnotationConfig.ScanTopoSettings;
                break;
            case ImageAnnotationSettingNames.ScanTomo:
                CurrentImageAnnotationSetting = CurrentImageAnnotationConfig.ScanTomoSettings;
                break;
            case ImageAnnotationSettingNames.View:
                CurrentImageAnnotationSetting = CurrentImageAnnotationConfig.ViewSettings;
                break;
            case ImageAnnotationSettingNames.Print:
                CurrentImageAnnotationSetting = CurrentImageAnnotationConfig.PrintSettings;
                break;
            case ImageAnnotationSettingNames.MPR:
                CurrentImageAnnotationSetting = CurrentImageAnnotationConfig.MPRSettings;
                break;
            case ImageAnnotationSettingNames.VR:
                CurrentImageAnnotationSetting = CurrentImageAnnotationConfig.VRSettings;
                break;
        }
    }

    public void SaveConfig()
    {
        SetBottomAnnotationItem();
        _fourCornersService.Save(CurrentImageAnnotationConfig);
    }

    private void SetBottomAnnotationItem()
    {
        var scanTopoLeftList = CurrentImageAnnotationConfig.ScanTopoSettings.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.LeftBottom && t.Visibility).OrderByDescending(t => t.Row).ToList();
        SetAnnotationItemSettingsRowLists(scanTopoLeftList, CurrentImageAnnotationConfig.ScanTopoSettings);
        var scanTopoRightList = CurrentImageAnnotationConfig.ScanTopoSettings.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.RightBottom && t.Visibility).OrderByDescending(t => t.Row).ToList();
        SetAnnotationItemSettingsRowLists(scanTopoRightList, CurrentImageAnnotationConfig.ScanTopoSettings);

        var scanTomoLeftList = CurrentImageAnnotationConfig.ScanTomoSettings.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.LeftBottom && t.Visibility).OrderByDescending(t => t.Row).ToList();
        SetAnnotationItemSettingsRowLists(scanTomoLeftList, CurrentImageAnnotationConfig.ScanTomoSettings);
        var scanTomoRightList = CurrentImageAnnotationConfig.ScanTomoSettings.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.RightBottom && t.Visibility).OrderByDescending(t => t.Row).ToList();
        SetAnnotationItemSettingsRowLists(scanTomoRightList, CurrentImageAnnotationConfig.ScanTomoSettings);

        var mprLeftList = CurrentImageAnnotationConfig.MPRSettings.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.LeftBottom && t.Visibility).OrderByDescending(t => t.Row).ToList();
        SetAnnotationItemSettingsRowLists(mprLeftList, CurrentImageAnnotationConfig.MPRSettings);
        var mprRightList = CurrentImageAnnotationConfig.MPRSettings.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.RightBottom && t.Visibility).OrderByDescending(t => t.Row).ToList();
        SetAnnotationItemSettingsRowLists(mprRightList, CurrentImageAnnotationConfig.MPRSettings);

        var printLeftList = CurrentImageAnnotationConfig.PrintSettings.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.LeftBottom && t.Visibility).OrderByDescending(t => t.Row).ToList();
        SetAnnotationItemSettingsRowLists(printLeftList, CurrentImageAnnotationConfig.PrintSettings);
        var printRightList = CurrentImageAnnotationConfig.PrintSettings.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.RightBottom && t.Visibility).OrderByDescending(t => t.Row).ToList();
        SetAnnotationItemSettingsRowLists(printRightList, CurrentImageAnnotationConfig.PrintSettings);

        var viewLeftList = CurrentImageAnnotationConfig.ViewSettings.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.LeftBottom && t.Visibility).OrderByDescending(t => t.Row).ToList();
        SetAnnotationItemSettingsRowLists(viewLeftList, CurrentImageAnnotationConfig.ViewSettings);
        var viewRightList = CurrentImageAnnotationConfig.ViewSettings.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.RightBottom && t.Visibility).OrderByDescending(t => t.Row).ToList();
        SetAnnotationItemSettingsRowLists(viewRightList, CurrentImageAnnotationConfig.ViewSettings);

        var vrLeftList = CurrentImageAnnotationConfig.VRSettings.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.LeftBottom && t.Visibility).OrderByDescending(t => t.Row).ToList();
        SetAnnotationItemSettingsRowLists(vrLeftList, CurrentImageAnnotationConfig.VRSettings);
        var vrRightList = CurrentImageAnnotationConfig.VRSettings.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.RightBottom && t.Visibility).OrderByDescending(t => t.Row).ToList();
        SetAnnotationItemSettingsRowLists(vrRightList, CurrentImageAnnotationConfig.VRSettings);
    }

    private void SetAnnotationItemSettingsRowLists(List<AnnotationItem> list, ImageAnnotationSetting imageAnnotationSetting)
    {
        if (!list.Any())
        {
            return;
        }

        int k = list.Count;
        list.ForEach(e =>
        {
            e.Row = k;
            if (e.GroupName != e.Name)
            {
                List<string> list = new List<string>(e.Name.Split('/'));
                list.ForEach(e1 =>
                {
                    if (e1.Equals("WW"))  //WW/WL特殊处理
                    {
                        e1 = "WW/WL";
                    }
                    SetAnnotationItemSettingsRowByName(e1, k, imageAnnotationSetting);
                });
            }
            else
            {
                SetAnnotationItemSettingsRowByName(e.Name, k, imageAnnotationSetting);
            }
            k -= 1;
        });
    }

    private void SetAnnotationItemSettingsRowByName(string name, int index, ImageAnnotationSetting imageAnnotationSetting)
    {
        if (string.IsNullOrEmpty(name) || index < 0)
        {
            return;
        }
        var model = imageAnnotationSetting.AnnotationItemSettings.FirstOrDefault(t => !string.IsNullOrEmpty(t.Name) && t.Name.Equals(name));
        if (model is not null)
        {
            model.Row = index;
        }
    }

    public ImageAnnotationConfig CurrentImageAnnotationConfig { get; set; } = new();

    public ImageAnnotationSetting CurrentImageAnnotationSetting { get; set; } = new();
}