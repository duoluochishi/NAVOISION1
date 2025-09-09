//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ImageViewer.ViewModel;

namespace NV.CT.ImageViewer.Extensions;

public class ViewModelModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<MainControlViewerModel>().SingleInstance();
        builder.RegisterType<Image2DViewModel>().SingleInstance();
        builder.RegisterType<Image3DViewModel>().SingleInstance();
        builder.RegisterType<SeriesViewModel>().SingleInstance();
        builder.RegisterType<HistorySeriesViewModel>().SingleInstance();
		builder.RegisterType<StudyViewModel>().SingleInstance();
        builder.RegisterType<BatchReconViewModel>().SingleInstance();
        builder.RegisterType<StudyFilterViewModel>().SingleInstance();
        builder.RegisterType<StudyHistoryViewModel>().SingleInstance();
        builder.RegisterType<ImagViewerScrollBarViewMode>().SingleInstance();
        builder.RegisterType<ScreenshotViewModel>().SingleInstance();
        builder.RegisterType<SimpleWebServer>().SingleInstance();
        builder.RegisterType<CustomRotateViewModel>().SingleInstance();       
    }
}