using Microsoft.Extensions.DependencyInjection;
using NV.CT.UserConfig.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NV.CT.UserConfig.View.English;

/// <summary>
/// ImageTextTypeListControl.xaml 的交互逻辑
/// </summary>
public partial class ImageTextTypeListControl : UserControl
{
    private ImageTextTypeListViewModel _viewModel;
    public ImageTextTypeListControl()
    {
        InitializeComponent();
        this.Loaded += ImageTextTypeListControl_Loaded;      
        _viewModel = ServiceFramework.Global.Instance.ServiceProvider.GetRequiredService<ImageTextTypeListViewModel>();
        this.DataContext = _viewModel;
        _viewModel.LoadImageAnnotationNames();
    }

    private void ImageTextTypeListControl_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.LoadFirst();
    }
}
