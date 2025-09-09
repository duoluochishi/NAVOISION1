//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/10/19 13:38:00           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.InterventionalScan.Models;
using NV.CT.InterventionScan.ApplicationService.Contract;
using NV.CT.InterventionScan.Extensions;
using NV.CT.InterventionScan.View;
using NV.CT.InterventionScan.ViewModel;
using NV.MPS.Configuration;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace NV.CT.InterventionScan.Layout;

public partial class LayoutControl : UserControl
{
    private readonly IInterventionService _interventionService;
    private List<TextBlock> textBlocks = new();
    private CustomWWWLWindow? _customWwwlWindow;
    public LayoutControl(IInterventionService interventionService)
    {
        InitializeComponent();
        _interventionService = interventionService;
        DataContext = CTS.Global.ServiceProvider?.GetRequiredService<LayoutViewModel>();

        labelLayout.ContextMenu = null;
        labelWwWl.ContextMenu = null;
        labelROI.ContextMenu = null;

        popLayout.DataContext = this.DataContext;
        menuROI.DataContext = this.DataContext;
        menuWwWl.DataContext = this.DataContext;

        //TextBlock点击事件与对应的菜单关联上
        txtLayout.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelLayout_OnMouseLeftButtonDown), true);
        txtWWWL.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelWwWl_OnMouseLeftButtonDown), true);
        txtRotate.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelRotate_OnMouseLeftButtonDown), true);
    }

    private void SwitchSelfAndHasFocusBackgroundImage(object sender, MouseButtonEventArgs e)
    {
        var textBlock = (TextBlock)sender;
        var str = ((ImageBrush)textBlock.Background).ImageSource.ToString();
        if (str.Contains("0.png"))
        {
            //将自己未被点击状态改为点击状态
            var newStr = str.Replace("0.png", "1.png");
            ChangeBackgroundImage(newStr, textBlock);

            //之前的被选中的 textBlock，去掉属于这个tag组里面的选中状态
            var previousFocusedTextBlock = textBlocks.FirstOrDefault(r => r.Background != null && r.Tag != null && ((ImageBrush)r.Background).ImageSource.ToString().Contains("1.png") && r != textBlock);
            if (previousFocusedTextBlock is null)
                return;

            var otherStr = ((ImageBrush)previousFocusedTextBlock.Background).ImageSource.ToString();
            if (otherStr.Contains("1.png"))
            {
                var newOtherStr = otherStr.Replace("1.png", "0.png");
                ChangeBackgroundImage(newOtherStr, previousFocusedTextBlock);
            }
        }
    }

    private void ChangeBackgroundImage(string str, TextBlock textBlock)
    {
        var imageSourceConverter = new ImageSourceConverter();
        var imgSource = imageSourceConverter.ConvertFromString(str);
        if (imgSource != null)
        {
            ((ImageBrush)textBlock.Background).ImageSource = (ImageSource)imgSource;
        }
    }

    private void LabelWwWl_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        menuWwWl.IsOpen = true;
    }

    private void LabelLayout_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        popLayout.IsOpen = true;
    }

    private void LabelRotate_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        menuROI.IsOpen = true;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        textBlocks = Global.FindVisualChild<TextBlock>(grdIcon);
        var total = textBlocks.Count;
        var images = textBlocks.Where(n => n.Tag != null).Select(n => ((ImageBrush)n.Background).ImageSource.ToString()).ToList();
        var emptyList = textBlocks.Except(textBlocks.Where(n => n.Tag != null)).ToList();
        foreach (var item in textBlocks)
        {
            //踢掉两个 Label标签，View和Mark文本标签
            if (item.Tag is not null)
            {
                item.MouseLeftButtonDown += SwitchSelfAndHasFocusBackgroundImage;
            }
        }
        BindKeyBinding();
    }

    private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var tag = (sender as TextBlock)?.Tag;
        if (tag is null)
            return;

        var windowType = tag as WindowingInfo;
        if (windowType is null)
            return;

        if (windowType.BodyPart == "Custom")
        {
            //Custom情况下，自定义输入值
            ShowCustomWWWLWindow();
        }
        else
        {
            _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_WL, $"{windowType.Width.Value}*{windowType.Level.Value}");
        }
    }

    public void ShowCustomWWWLWindow()
    {
        _customWwwlWindow = CTS.Global.ServiceProvider?.GetRequiredService<CustomWWWLWindow>();

        if (_customWwwlWindow is null)
        {
            _customWwwlWindow = CTS.Global.ServiceProvider?.GetRequiredService<CustomWWWLWindow>();
        }
        if (_customWwwlWindow != null)
        {
            WindowDialogShow.DialogShow(_customWwwlWindow);
        }
    }

    private void BindKeyBinding()
    {
        var windowTypes = UserConfig.WindowingConfig.Windowings;
        if (windowTypes is null)
        {
            return;
        }
        //添加自定义ww/wl
        windowTypes.Add(new WindowingInfo()
        {
            Width = new ItemField<int>() { Value = 0 },
            Level = new ItemField<int>() { Value = 0 },
            BodyPart = "Custom",
            Shortcut = "F12",
            Description = "Custom",
        });

        windowTypes.ForEach(item =>
        {
            CreateKeyBinding(this, item.Shortcut, (_, _) =>
            {
                if (item.BodyPart == "Custom")
                {
                    ShowCustomWWWLWindow();
                }
                else
                {
                    _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_WL, $"{item.Width.Value}*{item.Level.Value}");
                }
            });
        });
    }

    public bool CreateKeyBinding(UIElement target, string hotKey, ExecutedRoutedEventHandler handler)
    {
        if (target == null || string.IsNullOrEmpty(hotKey) || handler == null)
        {
            return false;
        }
        try
        {
            Key key = (Key)Enum.Parse(typeof(Key), hotKey);

            RoutedUICommand routedUICommand = new RoutedUICommand();
            CommandBinding commandBinding = new CommandBinding(routedUICommand, handler);
            KeyBinding keyBinding = new KeyBinding(routedUICommand, new KeyGesture(key));

            target.CommandBindings.Add(commandBinding);
            target.InputBindings.Add(keyBinding);
        }
        catch
        {
            return false;
        }
        return true;
    }
}