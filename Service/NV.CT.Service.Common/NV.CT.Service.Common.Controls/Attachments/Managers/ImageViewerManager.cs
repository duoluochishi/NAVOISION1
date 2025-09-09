using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignThemes.Wpf;
using NV.CT.Service.Common.Controls.Attachments.Extensions;
using NV.CT.Service.Common.Controls.Attachments.Helpers;
using NV.CT.Service.Common.Controls.Attachments.Messages;
using NV.CT.Service.Common.Controls.Enums;
using NV.CT.Service.Common.Controls.Models.Foundations;
using NV.CT.Service.Common.Controls.Models.Foundations.Abstractions;
using NV.CT.Service.Common.Controls.Universal.Interfaces;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Models;
using NVCTImageViewerInterop;
using RawDataHelperWrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using DetectorMap = NVCTImageViewerInterop.DetectorMap;

namespace NV.CT.Service.Common.Controls.Attachments.Managers
{
    public partial class ImageViewerManager : ObservableRecipient
    {
        public ImageViewerManager(string messengerToken)
        {
            this._messengerToken = messengerToken;

            InitializeProperties();
            InitEvent();
        }

        #region Initialize

        private void InitializeProperties()
        {
            //初始化图像互斥操作
            ImageViewerActions = new()
            {
                new ImageViewerMutexAction(ImageViewerActionType.Zoom, false, PackIconKind.MagnifyPlusOutline),
                new ImageViewerMutexAction(ImageViewerActionType.Drag, false, PackIconKind.DragVariant),
                new ImageViewerMutexAction(ImageViewerActionType.Rotate, false, PackIconKind.CropRotate),
                new ImageViewerMutexAction(ImageViewerActionType.Rectangle, false, PackIconKind.VectorSquareClose),
                new ImageViewerMutexAction(ImageViewerActionType.Circle, false, PackIconKind.DotsCircle),
                new ImageViewerMutexAction(ImageViewerActionType.RectangleWWWL, false, PackIconKind.ShapeRectanglePlus),
                new ImageViewerMutexAction(ImageViewerActionType.Angle, false, PackIconKind.AngleAcute),/* 角度线 */

                new ImageViewerNormalAction(ImageViewerActionType.AutoWWWL, false, PackIconKind.AutoFix),
                new ImageViewerNormalAction(ImageViewerActionType.OneToOne, false, PackIconKind.Numeric1BoxMultipleOutline),
                new ImageViewerNormalAction(ImageViewerActionType.AutoFit, false, PackIconKind.BorderHorizontal),
                new ImageViewerNormalAction(ImageViewerActionType.ResetView, false, PackIconKind.Replay),
                new ImageViewerNormalAction(ImageViewerActionType.ClearView, false, PackIconKind.DeleteSweepOutline)
            };
        }

        private void InitEvent()
        {
            //Messages
            WeakReferenceMessenger.Default.Unregister<ScrollBarThumbChangedMessage, string>(this, _messengerToken!);
            WeakReferenceMessenger.Default.Register<ScrollBarThumbChangedMessage, string>(this, _messengerToken!, ImageContentControlScrollBarThumbChanged);

            WeakReferenceMessenger.Default.Unregister<ImageSortedMessage, string>(this, _messengerToken!);
            WeakReferenceMessenger.Default.Register<ImageSortedMessage, string>(this, _messengerToken!, ImageChainImageSortedCompleted);
        }
        #endregion

        #region Properties

        /// <summary>
        /// 图像控件
        /// </summary>
        [ObservableProperty]
        private UniversalImageViewer imageViewer = null!;

        /// <summary>
        /// 图像控件操作Actions
        /// </summary>
        public ObservableCollection<AbstractImageViewerAction> ImageViewerActions { set; get; } = null!;

        /// <summary>
        /// 当前激活的图像操作
        /// </summary>
        private ImageViewerActionType CurrentImageViewerActionType { set; get; } = ImageViewerActionType.Scroll;

        /// <summary>
        /// 当前载入的图像类型
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanRunRawDataActions))]
        private ImageType currentLoadImageType = ImageType.RawData;

        /// <summary>
        /// 是否可以执行ImageCut和IamgeSort
        /// </summary>
        public bool CanRunRawDataActions => CurrentLoadImageType == ImageType.RawData && RawDataPool is not null;

        /// <summary>
        /// 载入的图像张数
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ImageLoadMessage))]
        private int loadedImageCount = 0;

        /// <summary>
        /// 当前显示的图像Index
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ImageLoadMessage))]
        private int currentImageIndex = 0;

        /// <summary>
        /// 图像显示信息
        /// </summary>
        public string ImageLoadMessage => $"{CurrentImageIndex}  /  {LoadedImageCount}";

        /// <summary>
        /// 生数据
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanRunRawDataActions))]
        private IEnumerable<RawData>? rawDataPool;

        #endregion

        #region Image Control Events

        /// <summary>
        /// 图像控件载入
        /// </summary>
        /// <param name="view"></param>
        [RelayCommand]
        private void ImageContentControlLoaded(object view)
        {
            if (ImageViewer != null)
            {
                return;
            }

            //更新图像控件 
            var control = (ContentControl)view;
            //图像控件初始化 
            this.ImageViewer = new((int)control.ActualWidth, (int)control.ActualHeight);

            //设置背景色，生数据适合使用灰色（不建议用黑色）
            ImageViewer.SetBackgroundColor(Color.Gray);

            //开启滚动翻页
            ImageViewer.EnableScrollSlicing(true);
            //绑定翻页事件
            ImageViewer.ImageViewerSliceChanged += ImageContentControlSliceChanged;
        }

        /// <summary>
        /// 图像控件Size变化
        /// </summary>
        /// <param name="view"></param>
        [RelayCommand]
        private void ImageContentControlSizeChanged(object view)
        {
            //更新图像控件 
            var control = (ContentControl)view;
            //Move View 
            ImageViewer.MoveView(0, 0, (int)control.ActualWidth, (int)control.ActualHeight);
        }

        /// <summary>
        /// 图像控件翻页回调
        /// </summary>
        /// <param name="index"></param>
        /// <param name="totalIndex"></param>
        private void ImageContentControlSliceChanged(int index, int totalIndex)
        {
            if (RawDataPool is null)
            {
                return;
            }
            //更新Index
            CurrentImageIndex = index + 1;
        }

        /// <summary>
        /// 滚动条滑块拖拽
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="message"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ImageContentControlScrollBarThumbChanged(object recipient, ScrollBarThumbChangedMessage message)
        {
            ImageViewer?.SetDisplayIndex(CurrentImageIndex - 1);
        }

        #endregion

        #region ImageViewer Actions

        /// <summary>
        /// 执行互斥操作
        /// </summary>
        /// <param name="mutexAction"></param>
        [RelayCommand]
        private void ExecuteImageViewerMutexAction(ImageViewerMutexAction mutexAction)
        {
            KeepImageViewerActionMutex(mutexAction);
        }

        /// <summary>
        /// 保持互斥按钮的互斥性
        /// </summary>
        /// <param name="mutexAction"></param>
        private void KeepImageViewerActionMutex(ImageViewerMutexAction mutexAction)
        {
            //若当前无操作，或与操作为相同类型 
            if (CurrentImageViewerActionType == ImageViewerActionType.Scroll || CurrentImageViewerActionType == mutexAction.ActionType)
            {
                //更新操作 
                UpdateMutexAction(mutexAction);
            }
            //停止当前操作，转换为所选操作 
            else
            {
                //当前操作对应的实例 
                var currentAction = (ImageViewerMutexAction)ImageViewerActions.FirstOrDefault(t => t.ActionType == CurrentImageViewerActionType)!;
                //校验 
                if (currentAction is not null)
                {
                    //状态置为false 
                    currentAction.IsChecked = false;
                    //关闭当前操作 
                    UpdateMutexAction(currentAction);
                }
                //开启选中操作 
                UpdateMutexAction(mutexAction);
            }
        }

        /// <summary>
        /// 更新互斥状态
        /// </summary>
        /// <param name="mutexAction"></param>
        private void UpdateMutexAction(ImageViewerMutexAction mutexAction)
        {
            //更新操作 
            switch (mutexAction.ActionType)
            {
                case ImageViewerActionType.Zoom: ImageViewer.EnableZoom(mutexAction.IsChecked); break;
                case ImageViewerActionType.Drag: ImageViewer.EnableDragMove(mutexAction.IsChecked); break;
                case ImageViewerActionType.Rotate: ImageViewer.EnableRotate(mutexAction.IsChecked); break;
                case ImageViewerActionType.Rectangle: ImageViewer.CreateRectangleROI(mutexAction.IsChecked); break;
                case ImageViewerActionType.Circle: ImageViewer.CreateCircleROI(mutexAction.IsChecked); break;
                case ImageViewerActionType.RectangleWWWL: ImageViewer.CreateRectangleWWWL(mutexAction.IsChecked); break;
                case ImageViewerActionType.Angle: ImageViewer.CreateAngleROI(mutexAction.IsChecked); break;
            }
            //更新状态 
            if (mutexAction.IsChecked)
            {
                CurrentImageViewerActionType = mutexAction.ActionType;
            }
            else
            {
                ImageViewer.EnableScrollSlicing(true);
                CurrentImageViewerActionType = ImageViewerActionType.Scroll;
            }
        }

        /// <summary>
        /// 执行普通操作
        /// </summary>
        /// <param name="normalAction"></param>
        [RelayCommand]
        private void ExecuteImageViewerNormalAction(ImageViewerNormalAction normalAction)
        {
            //数据有效性校验
            if (CurrentLoadImageType == ImageType.RawData && RawDataPool is null)
            {
                return;
            }
            //执行Action
            switch (normalAction.ActionType)
            {
                case ImageViewerActionType.AutoWWWL:
                    {
                        if (CurrentLoadImageType == ImageType.RawData)
                        {
                            //计算窗宽、窗位
                            (int WW, int WL) = CalculateWWWL(RawDataPool!.ElementAt(CurrentImageIndex));
                            //设置窗宽、窗位
                            ImageViewer.SetWWWL(WW, WL);
                        }
                    }
                    break;
                case ImageViewerActionType.OneToOne: ImageViewer.SwitchToOneToOnePixel(); break;
                case ImageViewerActionType.AutoFit: ImageViewer.SwitchBackFromOneToOnePixel(); break;
                case ImageViewerActionType.ResetView: ImageViewer.ResetView(); break;
                case ImageViewerActionType.ClearView:
                    {
                        //清空View
                        ClearImageViewer();
                        //释放
                        RawDataReadWriteHelper.Instance.Release();
                    }
                    break;
            }
        }

        /// <summary>
        /// 清空图像控件
        /// </summary>
        public void ClearImageViewer()
        {
            //清空ROI
            //ImageViewer.RemoveROI();
            //清空View
            ImageViewer.ClearView();
            //清空池子
            RawDataPool = null;
            //重置Index
            CurrentImageIndex = 0;
            //重置Count
            LoadedImageCount = 0;
        }

        #endregion

        #region Image Cut

        /// <summary>
        /// ImageCut
        /// </summary>
        /// <returns></returns>
        public GenericResponse CutImage()
        {
            //执行ImageCut
            var response = RawDataReadWriteHelper.Instance.RunImageCut(RawDataCutProgressChanging);
            //校验
            if (!response.status)
            {
                return new(false, $"Failed to cut image, error codes: {response.message}");
            }
            //显示ImageCut数据
            LoadRawDataList(response.data);

            return new(false, "Image after cut has been loaded.");
        }

        /// <summary>
        /// 切图进度回调
        /// </summary>
        private void RawDataCutProgressChanging(int count, int total)
        {

        }

        /// <summary>
        /// ImageSort
        /// </summary>
        private void SortImage()
        {

        }

        #endregion

        #region Image Sort

        private void ImageChainImageSortedCompleted(object recipient, ImageSortedMessage message)
        {
            //记录排序后的生数据
            RawDataPool = message.sortedImages;
            //显示排序后的生数据
            ImageViewer.SortRawDataList(RawDataPool.Select(t => t.Id));
            //重置Index
            CurrentImageIndex = 1;
        }

        #endregion

        #region Data Loader

        public void LoadRawDataList(ScanSeries scanSeries)
        {
            //清空ImageViewer
            ClearImageViewer();
            //记录加载数据类型
            CurrentLoadImageType = ImageType.RawData;
            //曝光数据记录
            RawDataPool = scanSeries.ToEnumerableExposureData();
            //图像控件数据转换
            var imageRawDataInfos = scanSeries.ToEnumerableImageRawDataInfo();
            var dectorMaps = scanSeries.ToDetectorMaps();

            //加载数据
            ImageViewer.LoadRawDataList(imageRawDataInfos, dectorMaps);
            //更新显示
            CurrentImageIndex = 1;
            LoadedImageCount = imageRawDataInfos.Count();
        }

        public void LoadDicomSeries(string directory)
        {
            //清空ImageViewer
            ClearImageViewer();
            //记录加载数据类型
            CurrentLoadImageType = ImageType.Dicom;
            //加载Dicom
            ImageViewer.LoadDicomSeries(directory);
        }

        /// <summary>
        /// 加载生数据序列（多个）
        /// </summary>
        /// <param name="imageRawDataSeries"></param>
        public void LoadImageRawDataSeries(List<ImageRawDataInfo> imageRawDataSeries)
        {
            //清空ImageViewer
            ClearImageViewer();
            //记录加载数据类型
            CurrentLoadImageType = ImageType.RawData;

            //加载数据
            //加载生数据系列（不是路径）的，无法提供DectorMap，暂时提供默认的
            List<DetectorMap> detectorMaps = new List<DetectorMap>();
            Console.WriteLine("[LoadImageRawDataSeries] [Warn] Use the empty DectorMaps,due to the sender not support List<DetectorMap>");

            ImageViewer.LoadRawDataList(imageRawDataSeries, detectorMaps);
        }

        #endregion

        #region Calculate

        /// <summary>
        /// 计算窗宽、窗位
        /// </summary>
        /// <returns></returns>
        public static (int WW, int WL) CalculateWWWL(RawData data)
        {
            (double max, double min) = data.PixelType switch
            {
                RawDataHelperWrapper.PixelType.Ushort => Calculate<ushort>(data),
                RawDataHelperWrapper.PixelType.Float => Calculate<float>(data),
                _ => (0, 0)
            };

            int ww = Convert.ToInt32(max - min);
            int wl = Convert.ToInt32((min + max) / 2);

            return (ww, wl);
        }

        /// <summary>
        /// 计算生数据最大最小值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static unsafe (T, T) Calculate<T>(RawData data) where T : unmanaged, IComparable<T>
        {
            var count = data.ImageSizeX * data.ImageSizeY;
            var span = new Span<T>(data.Data.ToPointer(), count);
            var max = span[0];
            var min = span[0];

            for (int i = 1; i < count; i++)
            {
                if (span[i].CompareTo(max) > 0)
                {
                    max = span[i];
                }

                if (span[i].CompareTo(min) < 0)
                {
                    min = span[i];
                }
            }

            return (max, min);
        }

        #endregion

        private ILogService logService;
        private string _messengerToken;
    }
}
