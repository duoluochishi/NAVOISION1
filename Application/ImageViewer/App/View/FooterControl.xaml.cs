//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ImageViewer.ViewModel;

namespace NV.CT.ImageViewer.View;

public partial class FooterControl
{
	public FooterControl()
	{
		InitializeComponent();
		DataContext = CTS.Global.ServiceProvider.GetService<SeriesViewModel>();
        btnPrint.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelPrint_OnMouseLeftButtonDown), true);
        btnArchive.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelArchive_OnMouseLeftButtonDown), true);
        menuPrint.DataContext = DataContext;
        menuArchive.DataContext = DataContext;
    }

    private void LabelPrint_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        menuPrint.IsOpen = true;
    }

    private void LabelArchive_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        menuArchive.IsOpen = true;
    }
}