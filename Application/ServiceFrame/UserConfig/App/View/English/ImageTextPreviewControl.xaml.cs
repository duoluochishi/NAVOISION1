using Microsoft.Extensions.DependencyInjection;
using NV.CT.UserConfig.ViewModel;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NV.CT.UserConfig.View.English;

/// <summary>
/// ImageTextPreviewControl.xaml 的交互逻辑
/// </summary>
public partial class ImageTextPreviewControl : UserControl
{
    private ImageTextPreviewViewModel _viewModel;
    private int originalPos = -1;
    public ImageTextPreviewControl()
    {
        InitializeComponent();     
        _viewModel = ServiceFramework.Global.Instance.ServiceProvider.GetRequiredService<ImageTextPreviewViewModel>();
        DataContext = _viewModel;
    }

    private static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject parentObject = VisualTreeHelper.GetParent(child);
        if (parentObject == null)
        {
            return null;
        }
        T parent = parentObject as T;
        if (null != parent)
        {
            return parent;
        }
        else
        {
            return FindParent<T>(parentObject);
        }
    }

    private void Item_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        originalPos = int.Parse(((TextBlock)sender).Tag.ToString());
        DragDrop.DoDragDrop((DependencyObject)sender, ((TextBlock)sender).Text, DragDropEffects.Move);
    }

    private void StackPanel_DragEnter(object sender, DragEventArgs e)
    {
        RunDragEnter(sender, e);
    }

    private void RunDragEnter(object sender, DragEventArgs e)
    {
        // 允许接受拖放的类型
        if (e.Data.GetDataPresent(DataFormats.Text))
        {
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private void StackPanel_DragOver(object sender, DragEventArgs e)
    {
        RunDragOver(sender, e);
    }

    private void RunDragOver(object sender, DragEventArgs e)
    {
        e.Handled = true;
    }

    private void StackPanel_Drop(object sender, DragEventArgs e)
    {
        RunDrop(sender, e);
    }

    private void RunDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.Text))
        {
            string data = (string)e.Data.GetData(DataFormats.Text);
            Point point = e.GetPosition((IInputElement)sender);

            StackPanel stackPanel = sender as StackPanel;
            int destRowId = 0;

            ListBox listBox = stackPanel.Children[0] as ListBox;
            double listBoxHeight = listBox.ActualHeight;
            int lineHeight = int.Parse(listBoxHeight.ToString()) / listBox.Items.Count;
            destRowId = int.Parse(point.Y.ToString()) / lineHeight;

            int destPos = int.Parse(((StackPanel)sender).Tag.ToString());

            if (originalPos != destPos) //只能在相等区域调整
            {
                return;
            }

            Task.Factory.StartNew(() =>
            {
                _viewModel.SetItem(originalPos, destPos, destRowId, data);
                originalPos = -1;
            });
        }
    }
}