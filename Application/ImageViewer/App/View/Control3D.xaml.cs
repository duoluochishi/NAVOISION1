//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ImageViewer.Extensions;
using NV.CT.ImageViewer.ViewModel;
using NV.MPS.Configuration;
using System.Windows.Controls;
using EventAggregator = NV.CT.ImageViewer.Extensions.EventAggregator;
namespace NV.CT.ImageViewer.View;

public partial class Control3D : UserControl
{
    private List<TextBlock> textBlocksViewAndMark = new();
    private List<TextBlock> textBlocksViewMode = new();
    private List<TextBlock> textBlocksAdvance = new();
    private readonly Image3DViewModel _vm;
    private readonly ILogger<Control3D>? _logger;
    private CustomWWWLWindow? _customWwwlWindow;
    public Control3D()
    {
        InitializeComponent();

        _vm = CTS.Global.ServiceProvider.GetRequiredService<Image3DViewModel>();
        DataContext = _vm;
        _logger = CTS.Global.ServiceProvider.GetService<ILogger<Control3D>>();

        //禁用右键菜单
        labelWwWl.ContextMenu = null;
        labelRotate.ContextMenu = null;
        labelMMPR.ContextMenu = null;
        labelLayout.ContextMenu = null;
        labelCut.ContextMenu = null;
        labelPreset.ContextMenu = null;

        menuWwWl.DataContext = DataContext;
        menuRotate.DataContext = DataContext;
        menuLayout.DataContext = DataContext;
        menuMMPR.DataContext = DataContext;
        menuCut.DataContext = DataContext;
        menuPreset.DataContext = DataContext;

        //txtWWWL.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelWwWl_OnMouseLeftButtonDown), true);
        //txtRotate.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelRotate_OnMouseLeftButtonDown), true);
        //txtMMPR.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelMMPR_OnMouseLeftButtonDown), true);
        txtLayout.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelLayout_OnMouseLeftButtonDown), true);
        //txtCut.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelCut_OnMouseLeftButtonDown), true);
        txtPreset.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelPreset_OnMouseLeftButtonDown), true);
        EventAggregator.Instance.GetEvent<Update3DStateButtonEvent>().Subscribe(UpdateStateButtonHighLight);
        EventAggregator.Instance.GetEvent<Update3DPreviousStateButtonEvent>().Subscribe(updatePreviousFocusedTextBlock);
        EventAggregator.Instance.GetEvent<Update3DSwitchButtonEvent>().Subscribe(UpdateSwitchButtonHighLight);
        EventAggregator.Instance.GetEvent<Update3DHotKeyEvent>().Subscribe(UpdateSwitchButtonState);
        Loaded += _3D_Loaded;
    }

    private void _3D_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        BindingViewAndMarkMouseLeftButtonDownEvent();
        SetMMPRDisplay();
    }
    private void SetMMPRDisplay()
    {
        var defaultTextBlock = textBlocksViewMode.FirstOrDefault(r => r.Background != null && r.Tag != null && ((ImageBrush)r.Background).ImageSource.ToString().Contains("1.png"));
        if (defaultTextBlock is null)
        {
            UpdateSwitchButtonHighLight(ConstName3D.txtMMPR);
        }
    }
    private void  BindingViewAndMarkMouseLeftButtonDownEvent()
    {
        var tbView = Global.FindVisualChild<TextBlock>(spView);
        var tbMark = Global.FindVisualChild<TextBlock>(spMark);
        textBlocksViewMode = Global.FindVisualChild<TextBlock>(spViewMode);
        textBlocksAdvance = Global.FindVisualChild<TextBlock>(spAdvance);
        textBlocksViewAndMark = tbView.Concat(tbMark).ToList();
        var textBlocks =tbView.Concat(tbMark).Concat(textBlocksViewMode).Concat(textBlocksAdvance).ToList();
        foreach (var item in textBlocks)
        {
            //踢掉两个 Label标签，View和Mark文本标签
            if (item.Tag is not null)
            {
                item.MouseLeftButtonDown += SwitchSelfAndHasFocusBackgroundImage;
            }
        }
    }
    private void UpdateSwitchButtonHighLight(string name)
    {
        TextBlock textBlock = (TextBlock)this.FindName(name);
        if (textBlock is not null)
        {
            var str = ((ImageBrush)textBlock.Background).ImageSource.ToString();
            var newStr = str.Replace("0.png", "1.png");
            ChangeBackgroundImage(newStr, textBlock);
        }
    }
    private void UpdateStateButtonHighLight(StateButton statebutton)
    {
        TextBlock textBlock = (TextBlock)this.FindName(statebutton.ButtonName);
        if (textBlock is not null)
        {
            var str = ((ImageBrush)textBlock.Background).ImageSource.ToString();
            string? newStr;
            if (statebutton.ButtonState)
            {
                newStr = str.Replace("1.png", "0.png");
            }
            else
            {
                newStr = str.Replace("0.png", "1.png");
            }
            ChangeBackgroundImage(newStr, textBlock);
        }
    }
    private void updatePreviousFocusedTextBlock(TextBlockListType textBlockListType)
    {
        IEnumerable<TextBlock>? previousFocusedTextBlocks = null;
        switch (textBlockListType)
        {
            case TextBlockListType.ViewAndMark:
                previousFocusedTextBlocks = textBlocksViewAndMark.Where(r => r.Background != null && r.Tag != null && ((ImageBrush)r.Background).ImageSource.ToString().Contains("1.png"));
                break;
            case TextBlockListType.ViewMode:
                previousFocusedTextBlocks = textBlocksViewMode.Where(r => r.Background != null && r.Tag != null && ((ImageBrush)r.Background).ImageSource.ToString().Contains("1.png"));
                break;
            case TextBlockListType.Advance:
                previousFocusedTextBlocks = textBlocksAdvance.Where(r => r.Background != null && r.Tag != null && ((ImageBrush)r.Background).ImageSource.ToString().Contains("1.png"));
                break;
            default:
                break;
        }
        if (previousFocusedTextBlocks is null)
            return;
        foreach (var item in previousFocusedTextBlocks)
        {
            var otherStr = ((ImageBrush)item.Background).ImageSource.ToString();
            if (otherStr.Contains("1.png"))
            {
                var newOtherStr = otherStr.Replace("1.png", "0.png");
                ChangeBackgroundImage(newOtherStr, item);
            }
        }
    }
    private void UpdateSwitchButtonState(string name)
    {
        Dispatcher.Invoke(() => {
            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, System.Windows.Input.MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = txtWWWL
            };
            txtWWWL.RaiseEvent(args);
        });
        UpdateSwitchButtonHighLight(name);
    }
    private void SwitchSelfAndHasFocusBackgroundImage(object sender, MouseButtonEventArgs e)
    {
        var textBlock = (TextBlock)sender;
        //var tag = textBlock.Tag;
        //if (tag is null)
        //    return;
        if (ConstName3D.stateButtonsNameList.Contains(textBlock.Name) || ConstName3D.functionalButtonsNameList.Contains(textBlock.Name))
        {
            return;
        }
        var str = ((ImageBrush)textBlock.Background).ImageSource.ToString();
        if (str.Contains("0.png"))
        {
            //将自己未被点击状态改为点击状态
            var newStr = str.Replace("0.png", "1.png");
            ChangeBackgroundImage(newStr, textBlock);

            //之前的被选中的 textBlock，去掉属于这个tag组里面的选中状态
            TextBlock? previousFocusedTextBlock = null;
            if (textBlocksViewAndMark.Contains(textBlock))
            {
                 previousFocusedTextBlock = textBlocksViewAndMark.FirstOrDefault(r => r.Background != null && r.Tag != null && ((ImageBrush)r.Background).ImageSource.ToString().Contains("1.png") && r != textBlock);
            }else if (textBlocksViewMode.Contains(textBlock))
            {
                previousFocusedTextBlock = textBlocksViewMode.FirstOrDefault(r => r.Background != null && r.Tag != null && ((ImageBrush)r.Background).ImageSource.ToString().Contains("1.png") && r != textBlock);
            }
            else if (textBlocksAdvance.Contains(textBlock))
            {
                previousFocusedTextBlock = textBlocksAdvance.FirstOrDefault(r => r.Background != null && r.Tag != null && ((ImageBrush)r.Background).ImageSource.ToString().Contains("1.png") && r != textBlock);
            }    
            if (previousFocusedTextBlock is null|| ConstName3D.stateButtonsNameList.Contains(previousFocusedTextBlock.Name) || ConstName3D.functionalButtonsNameList.Contains(previousFocusedTextBlock.Name))
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

    /// <summary>
    /// WWWL设置事件
    /// </summary>
    private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var tag = (sender as TextBlock)?.Tag;
        if (tag is null)
            return;

        var windowType = tag as WindowingInfo;
        if (windowType is null)
            return;

        try
        {
            if (windowType.BodyPart == "Custom")
            {
                //Custom情况下，自定义输入值
                CommonMethod.ShowCustomWWWLWindow(ViewScene.View3D);
            }
            else
            {
                _vm.CurrentImageViewer.SetWWWL3D(windowType.Width.Value, windowType.Level.Value);
            }
            updatePreviousFocusedTextBlock(TextBlockListType.ViewAndMark);
            UpdateSwitchButtonHighLight(ConstName3D.txtWWWL);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"parse ww/wl error {ex.Message}");
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
            _customWwwlWindow.SetScene(ViewScene.View3D);
            WindowDialogShow.Show(_customWwwlWindow);
        }
    }

    private void LabelMMPR_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        menuMMPR.IsOpen = true;
    }

    private void LabelWwWl_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        menuWwWl.IsOpen = true;
    }

    private void LabelRotate_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        menuRotate.IsOpen = true;
    }

    private void LabelLayout_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        menuLayout.IsOpen = true;
    }
    private void LabelCut_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        menuCut.IsOpen = true;
    }
    private void LabelPreset_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        menuPreset.IsOpen = true;
    }


}