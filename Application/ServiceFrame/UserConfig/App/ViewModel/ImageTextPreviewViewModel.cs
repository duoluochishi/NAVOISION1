using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.UI.ViewModel;
using NV.CT.UserConfig.ApplicationService.Impl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NV.CT.UserConfig.ViewModel;
public class ImageTextPreviewViewModel : BaseViewModel
{
    private readonly ImageTextSettingService _imageTextSettingService;
    private ObservableDictionary<int, ObservableCollection<AnnotationItem>> dictionary = new ObservableDictionary<int, ObservableCollection<AnnotationItem>>();
    public ImageTextPreviewViewModel(ImageTextSettingService imageTextSettingService)
    {
        _imageTextSettingService = imageTextSettingService;
        _imageTextSettingService.ImageAnnotationSwitchChanged -= ImageTextSettingService_ImageAnnotationSwitchChanged;
        _imageTextSettingService.ImageAnnotationSwitchChanged += ImageTextSettingService_ImageAnnotationSwitchChanged;

        _imageTextSettingService.ImageAnnotationItemVisibilityChanged -= ImageTextSettingService_ImageAnnotationItemVisibilityChanged;
        _imageTextSettingService.ImageAnnotationItemVisibilityChanged += ImageTextSettingService_ImageAnnotationItemVisibilityChanged;

        _imageTextSettingService.ImageAnnotationItemLocationChanged -= ImageTextSettingService_ImageAnnotationItemLocationChanged;
        _imageTextSettingService.ImageAnnotationItemLocationChanged += ImageTextSettingService_ImageAnnotationItemLocationChanged;

        _imageTextSettingService.FontSizeChanged -= ImageTextSettingService_FontSizeChanged;
        _imageTextSettingService.FontSizeChanged += ImageTextSettingService_FontSizeChanged;

        _imageTextSettingService.FontColorChanged -= ImageTextSettingService_FontColorChanged;
        _imageTextSettingService.FontColorChanged += ImageTextSettingService_FontColorChanged;
    }

    private void ImageTextSettingService_ImageAnnotationSwitchChanged(object? sender, EventArgs<(string, ImageAnnotationSetting setting)> e)
    {
        if (e is null)
        {
            return;
        }
        ObservableCollection<AnnotationItem> vLeftTopItems, vRightTopItems, vRightBottomItems, vLeftBottomItems;

        SetCornersItems(out vLeftTopItems, e.Data.setting.AnnotationItemSettings, 0, FourCornersLocation.LeftTop);
        LeftTopItems = vLeftTopItems;
        SetCornersItems(out vRightTopItems, e.Data.setting.AnnotationItemSettings, 2, FourCornersLocation.RightTop);
        RightTopItems = vRightTopItems;
        SetCornersItems(out vRightBottomItems, e.Data.setting.AnnotationItemSettings, 4, FourCornersLocation.RightBottom);
        RightBottomItems = vRightBottomItems;
        SetCornersItems(out vLeftBottomItems, e.Data.setting.AnnotationItemSettings, 6, FourCornersLocation.LeftBottom);
        LeftBottomItems = vLeftBottomItems;

        CenterTopName = e.Data.setting.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.CenterTop)[0].Name;
        CenterRightName = e.Data.setting.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.RightCenter)[0].Name;
        CenterBottomName = e.Data.setting.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.CenterBottom)[0].Name;
        CenterLeftName = e.Data.setting.AnnotationItemSettings.FindAll(t => t.Location == FourCornersLocation.LeftCenter)[0].Name;

        CurrentFontSize = e.Data.setting.FontSize;
        CurrentFontColor = System.Windows.Media.Color.FromRgb(byte.Parse(e.Data.setting.FontColorR.ToString()), byte.Parse(e.Data.setting.FontColorG.ToString()), byte.Parse(e.Data.setting.FontColorB.ToString())).ToString();// e.FontColorR;
    }

    private void ImageTextSettingService_ImageAnnotationItemVisibilityChanged(object? sender, EventArgs<(AnnotationItem annotationItem, bool isVisibility)> e)
    {
        if (e is null || e.Data.annotationItem is null)
        {
            return;
        }

        if (string.IsNullOrEmpty(e.Data.annotationItem.GroupName))
        {
            _imageTextSettingService.CurrentImageAnnotationSetting.AnnotationItemSettings.Find(t => t.Name == e.Data.annotationItem.Name).Visibility = e.Data.isVisibility;
        }
        else
        {
            List<AnnotationItem> items = _imageTextSettingService.CurrentImageAnnotationSetting.AnnotationItemSettings.FindAll(t => t.GroupName == e.Data.annotationItem.GroupName);
            items.ForEach(t =>
            {
                t.Visibility = e.Data.isVisibility;
            });
        }
        ObservableCollection<AnnotationItem> vLeftTopItems, vRightTopItems, vRightBottomItems, vLeftBottomItems;
        if (e.Data.annotationItem.Location == FourCornersLocation.LeftTop)
        {
            SetCornersItems(out vLeftTopItems, _imageTextSettingService.CurrentImageAnnotationSetting.AnnotationItemSettings, 0, FourCornersLocation.LeftTop);
            LeftTopItems = vLeftTopItems;
        }
        if (e.Data.annotationItem.Location == FourCornersLocation.RightTop)
        {
            SetCornersItems(out vRightTopItems, _imageTextSettingService.CurrentImageAnnotationSetting.AnnotationItemSettings, 2, FourCornersLocation.RightTop);
            RightTopItems = vRightTopItems;
        }
        if (e.Data.annotationItem.Location == FourCornersLocation.RightBottom)
        {
            SetCornersItems(out vRightBottomItems, _imageTextSettingService.CurrentImageAnnotationSetting.AnnotationItemSettings, 4, FourCornersLocation.RightBottom);
            RightBottomItems = vRightBottomItems;
        }
        if (e.Data.annotationItem.Location == FourCornersLocation.LeftBottom)
        {
            SetCornersItems(out vLeftBottomItems, _imageTextSettingService.CurrentImageAnnotationSetting.AnnotationItemSettings, 6, FourCornersLocation.LeftBottom);
            LeftBottomItems = vLeftBottomItems;
        }
    }

    private void ImageTextSettingService_ImageAnnotationItemLocationChanged(object? sender, EventArgs<(AnnotationItem, FourCornersLocation)> e)
    {
        if (e is null)
        {
            return;
        }
        _imageTextSettingService.SetCurrentSetting(_imageTextSettingService.CurrentImageAnnotationName);
        ObservableCollection<AnnotationItem> vLeftTopItems, vRightTopItems, vRightBottomItems, vLeftBottomItems;

        SetCornersItems(out vLeftTopItems, _imageTextSettingService.CurrentImageAnnotationSetting.AnnotationItemSettings, 0, FourCornersLocation.LeftTop);
        LeftTopItems = vLeftTopItems;

        SetCornersItems(out vRightTopItems, _imageTextSettingService.CurrentImageAnnotationSetting.AnnotationItemSettings, 2, FourCornersLocation.RightTop);
        RightTopItems = vRightTopItems;

        SetCornersItems(out vRightBottomItems, _imageTextSettingService.CurrentImageAnnotationSetting.AnnotationItemSettings, 4, FourCornersLocation.RightBottom);
        RightBottomItems = vRightBottomItems;

        SetCornersItems(out vLeftBottomItems, _imageTextSettingService.CurrentImageAnnotationSetting.AnnotationItemSettings, 6, FourCornersLocation.LeftBottom);
        LeftBottomItems = vLeftBottomItems;
    }

    private void SetCornersItems(out ObservableCollection<AnnotationItem> items, List<AnnotationItem> settings, int pos, FourCornersLocation location)
    {
        if (location == FourCornersLocation.LeftTop || location == FourCornersLocation.RightTop || location == FourCornersLocation.CenterTop)
        {
            items = new ObservableCollection<AnnotationItem>(settings.FindAll(t => t.Location == location && t.Visibility)
                .Select(t => new AnnotationItem
                {
                    Name = t.Name,
                    TextSource = t.TextSource,
                    DicomTagGroup = t.DicomTagGroup,
                    DicomTagId = t.DicomTagId,
                    TextPrefix = t.TextPrefix,
                    TextSuffix = t.TextSuffix,
                    Visibility = t.Visibility,
                    Row = t.Row,
                    Column = t.Column,
                    Location = t.Location,
                    GroupName = !string.IsNullOrEmpty(t.GroupName) ? t.GroupName : t.Name
                }).ToList()
                .OrderBy(t => t.Row)
                .GroupBy(x => x.GroupName)
                .Select(g => new AnnotationItem
                {
                    GroupName = g.Key,
                    Name = string.Join("/", g.ToList().OrderBy(i => i.Column).Select(n => n.Name).ToArray()).Replace("WL/WL", "WL"),        //WW/WL特殊处理
                    Visibility = g.FirstOrDefault() is AnnotationItem annotation ? annotation.Visibility : false,
                    Row = g.FirstOrDefault() is AnnotationItem annotationRow ? annotationRow.Row : 0,
                }
                ));
        }
        else
        {
            items = new ObservableCollection<AnnotationItem>(settings.FindAll(t => t.Location == location && t.Visibility)
                .Select(t => new AnnotationItem
                {
                    Name = t.Name,
                    TextSource = t.TextSource,
                    DicomTagGroup = t.DicomTagGroup,
                    DicomTagId = t.DicomTagId,
                    TextPrefix = t.TextPrefix,
                    TextSuffix = t.TextSuffix,
                    Visibility = t.Visibility,
                    Row = t.Row,
                    Column = t.Column,
                    Location = t.Location,
                    GroupName = !string.IsNullOrEmpty(t.GroupName) ? t.GroupName : t.Name
                }).ToList()
                .OrderByDescending(t => t.Row)
                .GroupBy(x => x.GroupName)
                .Select(g => new AnnotationItem
                {
                    GroupName = g.Key,
                    Name = string.Join("/", g.ToList().OrderBy(i => i.Column).Select(n => n.Name).ToArray()).Replace("WL/WL", "WL"),        //WW/WL特殊处理
                    Visibility = g.FirstOrDefault() is AnnotationItem annotation ? annotation.Visibility : false,
                    Row = g.FirstOrDefault() is AnnotationItem annotationRow ? annotationRow.Row : 0,
                }
                ));
        }
        if (!dictionary.ContainsKey(pos))
        {
            dictionary.Add(pos, items);
        }
        else
        {
            dictionary[pos] = items;
        }
    }

    private void ImageTextSettingService_FontSizeChanged(object? sender, EventArgs<(string, short fontSize)> e)
    {
        if (e is null)
        {
            return;
        }
        CurrentFontSize = e.Data.fontSize;
    }

    private void ImageTextSettingService_FontColorChanged(object? sender, EventArgs<(string, string)> e)
    {
        if (e is null)
        {
            return;
        }
        string[] colors = e.Data.Item2.Split(',');
        string color = string.Format("#{0:X2}{1:X2}{2:X2}", byte.Parse(colors[0]), byte.Parse(colors[1]), byte.Parse(colors[2]));
        CurrentFontColor = color;
    }

    public void SetItem(int originalPos, int destPos, int destRowId, string item)
    {
        var oriItem = dictionary[originalPos].FirstOrDefault(t => t.Name == item);
        if (oriItem is null)
        {
            return;
        }
        Application.Current.Dispatcher.Invoke(new Action(() =>
        {
            string groupName = oriItem.GroupName;
            int count = dictionary[originalPos].Count(t => t.Row == oriItem.Row && t.GroupName == groupName);
            List<AnnotationItem> oriList = dictionary[originalPos].Where(t => t.Row == oriItem.Row && t.GroupName == groupName).ToList();
            List<AnnotationItem> newItemList = oriList.Clone();

            for (int j = 0; j < oriList.Count; j++)
            {
                var model = dictionary[originalPos].FirstOrDefault(t => t.Name == oriList[j].Name);
                if (model is not null)
                {
                    dictionary[originalPos].Remove(model);
                }
            }
            for (int i = 0; i < newItemList.Count; i++)
            {
                newItemList[i].Row = destRowId;
                newItemList[i].Location = (FourCornersLocation)Enum.Parse(typeof(FourCornersLocation), destPos.ToString());
                if (destRowId < dictionary[destPos].Count)
                {
                    dictionary[destPos].Insert(destRowId, newItemList[i]);
                }
                else
                {
                    dictionary[destPos].Add(newItemList[i]);
                }
            }
        }));
        SetOriginalPos(originalPos);
        SetDestPos(destPos, item);
        SetBottom(destPos);
    }

    private void SetOriginalPos(int originalPos)
    {
        int k = 0;
        dictionary[originalPos].ForEach(e =>
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
                    SetAnnotationItemSettingsRowByName(e1, k);
                });
            }
            else
            {
                SetAnnotationItemSettingsRowByName(e.Name, k);
            }
            k += 1;
        });
    }

    private void SetDestPos(int destPos, string item)
    {
        int k = 0;
        dictionary[destPos].ForEach(e =>
        {
            e.Row = k;
            if (e.Name != e.GroupName)
            {
                List<string> list = new List<string>(e.Name.Split('/'));
                list.ForEach(e1 =>
                {
                    if (e1.Equals("WW"))   //WW/WL特殊处理
                    {
                        e1 = "WW/WL";
                    }
                    SetAnnotationItemSettingsRowByName(e1, k);
                });
                if (e.Name == item)
                {
                    list.ForEach(e1 =>
                    {
                        if (e1.Equals("WW"))   //WW/WL特殊处理
                        {
                            e1 = "WW/WL";
                        }
                        SetAnnotationItemSettingsLocationByName(e1, e.Location);

                    });
                }
            }
            else
            {
                SetAnnotationItemSettingsRowByName(e.Name, k);
                if (e.Name == item)
                {
                    SetAnnotationItemSettingsLocationByName(e.Name, e.Location);
                }
            }
            k += 1;
        });
    }

    private void SetBottom(int pos)
    {
        FourCornersLocation location = FourCornersLocation.LeftTop;
        var model = dictionary[pos].FirstOrDefault();
        if (model is AnnotationItem annotation)
        {
            location = annotation.Location;
        }
        if (location == FourCornersLocation.CenterBottom
          || location == FourCornersLocation.LeftBottom
          || location == FourCornersLocation.RightBottom)
        {
            int k = dictionary[pos].Count;
            dictionary[pos].ForEach(e =>
            {
                e.Row = k;
                if (e.GroupName != e.Name)
                {
                    List<string> list = new List<string>(e.Name.Split('/'));
                    list.ForEach(e1 =>
                    {
                        if (e1.Equals("WW"))   //WW/WL特殊处理
                        {
                            e1 = "WW/WL";
                        }
                        SetAnnotationItemSettingsRowByName(e1, k);
                    });
                }
                else
                {
                    SetAnnotationItemSettingsRowByName(e.Name, k);
                }
                k -= 1;
            });
        }
    }

    private void SetAnnotationItemSettingsRowByName(string name, int index)
    {
        if (string.IsNullOrEmpty(name) || index < 0)
        {
            return;
        }
        var model = _imageTextSettingService.CurrentImageAnnotationSetting.AnnotationItemSettings.FirstOrDefault(t => !string.IsNullOrEmpty(t.Name) && t.Name.Equals(name));
        if (model is not null)
        {
            model.Row = index;
        }
    }

    private void SetAnnotationItemSettingsLocationByName(string name, FourCornersLocation fourCornersLocation)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        var model = _imageTextSettingService.CurrentImageAnnotationSetting.AnnotationItemSettings.FirstOrDefault(t => !string.IsNullOrEmpty(t.Name) && t.Name.Equals(name));
        if (model is not null)
        {
            model.Location = fourCornersLocation;
        }
    }

    private ObservableCollection<AnnotationItem> leftTopItems = new ObservableCollection<AnnotationItem>();
    public ObservableCollection<AnnotationItem> LeftTopItems
    {
        get => leftTopItems;
        set => SetProperty(ref leftTopItems, value);
    }

    private ObservableCollection<AnnotationItem> rightTopItems = new ObservableCollection<AnnotationItem>();
    public ObservableCollection<AnnotationItem> RightTopItems
    {
        get => rightTopItems;
        set => SetProperty(ref rightTopItems, value);
    }

    private ObservableCollection<AnnotationItem> rightBottomItems = new ObservableCollection<AnnotationItem>();
    public ObservableCollection<AnnotationItem> RightBottomItems
    {
        get => rightBottomItems;
        set => SetProperty(ref rightBottomItems, value);
    }

    private ObservableCollection<AnnotationItem> leftBottomItems = new ObservableCollection<AnnotationItem>();
    public ObservableCollection<AnnotationItem> LeftBottomItems
    {
        get => leftBottomItems;
        set => SetProperty(ref leftBottomItems, value);
    }

    private short currentFontSize = 12;
    public short CurrentFontSize
    {
        get => currentFontSize;
        set => SetProperty(ref currentFontSize, value);
    }

    private string currentFontColor = "Black";
    public string CurrentFontColor
    {
        get => currentFontColor;
        set => SetProperty(ref currentFontColor, value);
    }

    private string centerTopName = string.Empty;
    public string CenterTopName
    {
        get => centerTopName;
        set => SetProperty(ref centerTopName, value);
    }

    private string centerRightName = string.Empty;
    public string CenterRightName
    {
        get => centerRightName;
        set => SetProperty(ref centerRightName, value);
    }

    private string centerBottomName = string.Empty;
    public string CenterBottomName
    {
        get => centerBottomName;
        set => SetProperty(ref centerBottomName, value);
    }

    private string centerLeftName = string.Empty;
    public string CenterLeftName
    {
        get => centerLeftName;
        set => SetProperty(ref centerLeftName, value);
    }
}