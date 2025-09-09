using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.Common.Controls.Attachments.Messages;
using NV.CT.Service.Common.Controls.ViewModels;
using NVCTImageViewerInterop;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NV.CT.Service.Common.Controls.Views
{
    /// <summary>
    /// ImageViewer.xaml 的交互逻辑
    /// </summary>
    public partial class ImageViewer : UserControl
    {
        private string messengerToken;

        public ImageViewer()
        {
            InitializeComponent();

            var imageViewerViewModel = IocContainer.Instance.Services.GetRequiredService<ImageViewerViewModel>();
            if (imageViewerViewModel is not null)
            {
                messengerToken = imageViewerViewModel.MessengerToken;
            }

            DataContext = imageViewerViewModel;
        }

        /// <summary>
        /// 外界触发通过路径加载生数据，如果路径为空，将清空已有路径
        /// </summary>
        /// <param name="rawDataDirectory"></param>
        public void LoadRawDataDirectory(string rawDataDirectory)
        {
            WeakReferenceMessenger.Default.Send(new LoadRawDataDirectoryMessage(rawDataDirectory), messengerToken);
        }

        private void ImageControlScrollBar_DragDelta(object sender, DragDeltaEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new ScrollBarThumbChangedMessage(), messengerToken);
        }

        public void LoadRawDataSeries(List<ImageRawDataInfo> imageRawDataInfos)
        {
            WeakReferenceMessenger.Default.Send(new LoadRawDataSeriesMessage(imageRawDataInfos), messengerToken);
        }
    }
}
