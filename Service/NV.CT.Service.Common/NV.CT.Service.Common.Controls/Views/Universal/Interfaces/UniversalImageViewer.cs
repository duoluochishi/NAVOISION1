using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.Common.Interfaces;
using NVCTImageViewerInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.Integration;
using System.Drawing;
using DetectorMap = NVCTImageViewerInterop.DetectorMap;

namespace NV.CT.Service.Common.Controls.Universal.Interfaces
{
    public class UniversalImageViewer
    {
        private ILogService logService;

        public UniversalImageViewer(int width, int height)
        {
            /** 获取loggerService **/
            logService = IocContainer.Instance.Services.GetRequiredService<ILogService>();
            /** 初始化前置准备 **/
            BeforeInitializeUniversalImageViewer(width, height);
            /** 创建RawDataView **/
            CreateView(0, 0, width, height);
            /** 初始化后置工作 **/
            AfterInitializeUniversalImageViewer();
        }

        #region Fields

        /** 图像库 **/
        private NvImageViewerWrapperCLI cliWrapper = null!;
        /** 窗体Handle **/
        private IntPtr winformHandle;
        /** View Handle **/
        private int imageViewHandle;
        /**  **/
        private List<OverlayText> overlayTexts = new List<OverlayText>();

        #endregion

        #region Properties

        /** 窗体控件承载 **/
        public WindowsFormsHost XWindowsFormsHost { get; set; } = null!;

        #endregion

        #region Initialize

        private void BeforeInitializeUniversalImageViewer(int width, int height)
        {
            logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start Initialize UniversalImageViewer {width}*{height}");
            /** 实例化 WrapperCLI **/
            cliWrapper = new NvImageViewerWrapperCLI();
            /** 初始化 WindowFormsHost **/
            XWindowsFormsHost = new WindowsFormsHost();
            XWindowsFormsHost.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
            System.Windows.Forms.UserControl _userControlWinForm = new System.Windows.Forms.UserControl();
            winformHandle = _userControlWinForm.Handle;
            XWindowsFormsHost.Child = _userControlWinForm;
            /** 初始化 WrapperCLI **/
            cliWrapper.Initialize();
        }

        private void AfterInitializeUniversalImageViewer()
        {
            this.InitializeOverlayTextDisplay();
            this.RegisterEvents();
        }

        private void InitializeOverlayTextDisplay()
        {
            /** 设置DetectorID OverlayText **/
            OverlayText overlayText_DetectorID = new OverlayText()
            {
                PreText = "DetectorID: ",
                InterMode = OverlayTextInterMode.Overlay_DetectorID,
                PostText = "",
                Visibility = true,
                DicomElementNumber = "",
                DicomGroupNumber = "",
                Column = 0,
                Row = 1,
                Alignment = OverlayTextPosition.UpperLeft
            };
            /** 记录 **/
            this.overlayTexts.Add(overlayText_DetectorID);

            /** 设置PixelBodyPosition OverlayText **/
            OverlayText overlayText_PixelBodyPosition = new OverlayText()
            {
                PreText = "",
                InterMode = OverlayTextInterMode.Overlay_PixelBodyPos,
                PostText = "",
                Visibility = true,
                DicomElementNumber = "",
                DicomGroupNumber = "",
                Column = 0,
                Row = 2,
                Alignment = OverlayTextPosition.UpperLeft
            };
            /** 记录 **/
            this.overlayTexts.Add(overlayText_PixelBodyPosition);

            /** 设置WL OverlayText **/
            OverlayText overlayText_PixelValue = new OverlayText()
            {
                PreText = "V: ",
                InterMode = OverlayTextInterMode.Overlay_PixelValue,
                PostText = "",
                Visibility = true,
                DicomElementNumber = "",
                DicomGroupNumber = "",
                Column = 0,
                Row = 3,
                Alignment = OverlayTextPosition.UpperLeft
            };
            /** 记录 **/
            this.overlayTexts.Add(overlayText_PixelValue);

            /** 设置WW OverlayText **/
            OverlayText overlayText_WW = new OverlayText()
            {
                PreText = "WW: ",
                InterMode = OverlayTextInterMode.Overlay_WW,
                PostText = "",
                Visibility = true,
                DicomElementNumber = "",
                DicomGroupNumber = "",
                Column = 1,
                Row = 1,
                Alignment = OverlayTextPosition.LowerLeft
            };
            /** 记录 **/
            this.overlayTexts.Add(overlayText_WW);

            /** 设置WL OverlayText **/
            OverlayText overlayText_WL = new OverlayText()
            {
                PreText = " WL: ",
                InterMode = OverlayTextInterMode.Overlay_WL,
                PostText = "",
                Visibility = true,
                DicomElementNumber = "",
                DicomGroupNumber = "",
                Column = 2,
                Row = 1,
                Alignment = OverlayTextPosition.LowerLeft
            };
            /** 记录 **/
            this.overlayTexts.Add(overlayText_WL);

            /** 设置SourceID OverlayText **/
            OverlayText overlayText_SourceID = new OverlayText()
            {
                PreText = "SourceID: ",
                InterMode = OverlayTextInterMode.Overlay_SourceID,
                PostText = "",
                Visibility = true,
                DicomElementNumber = "",
                DicomGroupNumber = "",
                Column = 0,
                Row = 1,
                Alignment = OverlayTextPosition.UpperRight
            };
            /** 记录 **/
            this.overlayTexts.Add(overlayText_SourceID);

            /** 设置FrameSeriesNumber OverlayText **/
            OverlayText overlayText_FrameSeriesNumber = new OverlayText()
            {
                PreText = "FrameSeriesNumber: ",
                InterMode = OverlayTextInterMode.Overlay_FrameSeriesNumber,
                PostText = "",
                Visibility = true,
                DicomElementNumber = "",
                DicomGroupNumber = "",
                Column = 0,
                Row = 2,
                Alignment = OverlayTextPosition.UpperRight
            };
            /** 记录 **/
            this.overlayTexts.Add(overlayText_FrameSeriesNumber);

            /** 设置TablePosition OverlayText **/
            OverlayText overlayText_TablePosition = new OverlayText()
            {
                PreText = "TablePosition: ",
                InterMode = OverlayTextInterMode.Overlay_TablePosition,
                PostText = "",
                Visibility = true,
                DicomElementNumber = "",
                DicomGroupNumber = "",
                Column = 0,
                Row = 3,
                Alignment = OverlayTextPosition.UpperRight
            };
            /** 记录 **/
            this.overlayTexts.Add(overlayText_TablePosition);

            /** 设置GantryRotateAngle OverlayText **/
            OverlayText overlayText_GantryRotateAngle = new OverlayText()
            {
                PreText = "GantryRotateAngle: ",
                InterMode = OverlayTextInterMode.Overlay_GantryRotateAngle,
                PostText = "",
                Visibility = true,
                DicomElementNumber = "",
                DicomGroupNumber = "",
                Column = 0,
                Row = 4,
                Alignment = OverlayTextPosition.UpperRight
            };
            /** 记录 **/
            this.overlayTexts.Add(overlayText_GantryRotateAngle);

            /** 设置Slope0 OverlayText **/
            OverlayText overlayText_Slope0 = new OverlayText()
            {
                PreText = "Slope0: ",
                InterMode = OverlayTextInterMode.Overlay_Slope0,
                PostText = "",
                Visibility = true,
                DicomElementNumber = "",
                DicomGroupNumber = "",
                Column = 0,
                Row = 1,
                Alignment = OverlayTextPosition.LowerRight
            };
            /** 记录 **/
            this.overlayTexts.Add(overlayText_Slope0);

            /** 设置Slope1 OverlayText **/
            OverlayText overlayText_Slope1 = new OverlayText()
            {
                PreText = "Slope1: ",
                InterMode = OverlayTextInterMode.Overlay_Slope1,
                PostText = "",
                Visibility = true,
                DicomElementNumber = "",
                DicomGroupNumber = "",
                Column = 0,
                Row = 2,
                Alignment = OverlayTextPosition.LowerRight
            };
            /** 记录 **/
            this.overlayTexts.Add(overlayText_Slope1);
            /** 初始化OverlayText **/
            cliWrapper.InitOverlayText(imageViewHandle, new(), overlayTexts, 0);
            /** 显示鼠标位置值 **/
            cliWrapper.ShowCursorRelativeValue(imageViewHandle, true);
        }

        #endregion

        #region Events

        public event Action<int, int>? ImageViewerSliceChanged;

        private void RegisterEvents()
        {
            logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer RegisterEvents");
            cliWrapper.RectInformationNotify += ImageViewer_RectInformationNotify;
            cliWrapper.RawDataPixelNotify += ImageViewer_RawDataPixelNotify;
            cliWrapper.SliceChangedNotify += ImageViewer_SliceChangedNotify;
            logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer RegisterEvents");
        }

        private void UnRegisterEvents()
        {
            logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer RegisterEvents");
            cliWrapper.RectInformationNotify -= ImageViewer_RectInformationNotify;
            cliWrapper.RawDataPixelNotify -= ImageViewer_RawDataPixelNotify;
            cliWrapper.SliceChangedNotify -= ImageViewer_SliceChangedNotify;
            logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer RegisterEvents");
        }

        private void ImageViewer_RawDataPixelNotify(int sender, List<double> args)
        {

        }

        private void ImageViewer_RectInformationNotify(int sender, List<ROI_RectInformation> args)
        {

        }

        private void ImageViewer_SliceChangedNotify(int viewHandle, int index, double slicePosition, int totalIndex)
        {
            ImageViewerSliceChanged?.Invoke(index, totalIndex);
        }

        #endregion

        #region View Related

        public int CreateView(int left, int top, int width, int height)
        {
            /** 获取ViweHandle **/
            var tempHandle = cliWrapper.CreateRawDataView(winformHandle, left, top, width, height);
            /** 记录 **/
            imageViewHandle = tempHandle;

            return tempHandle;
        }

        public void MoveView(int left, int top, int width, int height)
        {
            cliWrapper.MoveView(imageViewHandle, left, top, width, height);
        }

        public void ClearView()
        {
            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer ClearViewData");
                cliWrapper.ClearViewData(imageViewHandle);
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer ClearViewData");
            });
        }

        public void ResetView()
        {
            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer ResetView");

                cliWrapper.RemoveSeriesAllROI(imageViewHandle);
                cliWrapper.ResetView(imageViewHandle);
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer ResetView");
            });
        }

        #endregion

        #region RawData Related

        /// <summary>
        /// 设置背景色，其中 Color.Gray 同T03效果
        /// 如果不设置，默认是黑色
        /// </summary>
        /// <param name="color"></param>
        public void SetBackgroundColor(Color color)
        {
            SetBackgroundColor(color.R, color.G, color.B);
        }

        /// <summary>
        ///  设置背景色
        /// </summary>
        /// <param name="colorR">0-255</param>
        /// <param name="colorG">0-255</param>
        /// <param name="colorB">0-255</param>
        public void SetBackgroundColor(byte colorR, byte colorG, byte colorB)
        {
            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                SetBackgroundColorCore(colorR, colorG, colorB);
            });
        }

        private void SetBackgroundColorCore(byte colorR, byte colorG, byte colorB)
        {
            logService.Debug(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer SetBackgroundColorCore");

            double rRatio = colorR / 255.0;
            double gRatio = colorG / 255.0;
            double bRatio = colorB / 255.0;

            cliWrapper.SetBackgroundColor(imageViewHandle, rRatio, gRatio, bRatio);

            logService.Debug(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer SetBackgroundColorCore");
        }

        public void LoadRawData(ImageRawDataInfo rawData, IEnumerable<DetectorMap> detectorMaps)
        {
            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer SetImageRawData");
                cliWrapper.SetDetectorMap(imageViewHandle, detectorMaps.ToList());
                cliWrapper.SetImageRawData(imageViewHandle, rawData, false);
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer SetImageRawData");
            });
        }

        public void LoadRawDataList(IEnumerable<ImageRawDataInfo> rawDataList, IEnumerable<DetectorMap> detectorMaps)
        {
            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer SetImageRawDataList");
                cliWrapper.SetDetectorMap(imageViewHandle, detectorMaps.ToList());
                cliWrapper.SetImageRawDataList(imageViewHandle, rawDataList.ToList(), false);
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer SetImageRawDataList");
            });
        }

        public void SetDisplayIndex(int index)
        {
            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer SetDisplayIndex");
                cliWrapper.SetSliceIndex(imageViewHandle, index);
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer SetDisplayIndex");
            });
        }

        public void SortRawDataList(IEnumerable<int> sortedIDs)
        {
            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer SortRawDataList");
                cliWrapper.ChangeRawDataListOrder(imageViewHandle, sortedIDs.ToList());
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer SortRawDataList");
            });
        }

        public void AddRawData(ImageRawDataInfo rawData)
        {
            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer AddRawData");
                cliWrapper.AddRawData(imageViewHandle, rawData);
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer AddRawData");
            });
        }

        public void AddRawDataRange(IEnumerable<ImageRawDataInfo> rawDataList)
        {
            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer AddRawDataRange");
                cliWrapper.AddRawDataRange(imageViewHandle, rawDataList.ToList());
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer AddRawDataRange");
            });
        }

        public void DeleteRawData(int id)
        {
            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer DeleteRawData");
                cliWrapper.DeleteRawData(imageViewHandle, id);
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer DeleteRawData");
            });
        }

        public void DeleteRawDataRange(IEnumerable<int> deleteIDs)
        {
            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer DeleteRawData");
                cliWrapper.DeleteRawDataRange(imageViewHandle, deleteIDs.ToList());
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer DeleteRawData");
            });
        }

        #endregion

        #region Dicom Related

        public void LoadDicomSeries(string directory)
        {
            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer LoadDicomSeries");
                cliWrapper.SetViewDataFloder(imageViewHandle, directory);
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer LoadDicomSeries");
            });
        }

        public int GetLoadDicomCount()
        {
            int num = 0;

            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer LoadDicomSeries");
                num = cliWrapper.GetSliceNumber(imageViewHandle);
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer LoadDicomSeries");
            });

            return num;
        }

        #endregion

        #region View Actions

        public void EnableScrollSlicing(bool enable)
        {
            if (enable)
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer Scroll Slicing.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.MouseWheelButtonForwardAndBackward, ViewMouseAction.BrowserMouseAction_Slicing, 0);
            }
            else
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer Scroll Slicing.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.MouseWheelButtonForwardAndBackward, ViewMouseAction.MouseAction_None, 0);
            }
        }

        public void EnableZoom(bool enable)
        {
            if (enable)
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer Zoom.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Zoom, 0);
            }
            else
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer Zoom.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_None, 0);
            }
        }

        public void EnableDragMove(bool enable)
        {
            if (enable)
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer DragMove.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Move, 0);
            }
            else
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer DragMove.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_None, 0);
            }
        }

        public void EnableRotate(bool enable)
        {
            if (enable)
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer Rotate.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Rotate, 0);
            }
            else
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer Rotate.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_None, 0);
            }
        }

        public void SetWWWL(int ww, int wl)
        {
            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer ResetView");
                cliWrapper.SetWWWL(imageViewHandle, ww, wl);
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer ResetView");
            });
        }

        public void EnableWWWL(bool enable)
        {
            if (enable)
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer WWWL.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, 0);
            }
            else
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer WWWL.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_None, 0);
            }
        }

        public void SwitchToOneToOnePixel()
        {
            logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Switch to OneToOnePixel.");
            cliWrapper.OneToOnePixels(imageViewHandle, true);
        }

        public void SwitchBackFromOneToOnePixel()
        {
            logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Switch back from OneToOnePixel.");
            cliWrapper.OneToOnePixels(imageViewHandle, false);
        }

        public void CreateRectangleROI(bool enable)
        {
            if (enable)
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer Rectangle ROI.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Rect_RawData);
            }
            else
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer Rectangle ROI.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_None, 0);
            }
        }

        public void CreateCircleROI(bool enable)
        {
            if (enable)
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer Circle ROI.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Circle_RawData);
            }
            else
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer Circle ROI.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_None, 0);
            }
        }

        public void CreateRectangleWWWL(bool enable)
        {
            if (enable)
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer Rectangle WW/WL.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Rect_WWWL);
            }
            else
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer Rectangle WW/WL.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_None, 0);
            }
        }

        /// <summary>
        /// 画角度
        /// </summary>
        /// <param name="enable"></param>
        public void CreateAngleROI(bool enable)
        {
            if (enable)
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer Angle ROI.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Angle);
            }
            else
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer Angle ROI.");
                cliWrapper.SetViewMouseAction(imageViewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_None, 0);
            }
        }

        public void RemoveROI()
        {
            logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Remove ROI.");
            cliWrapper.RemoveSeriesAllROI(imageViewHandle);
        }

        public void DrawHorizontalLine(double y, double[] rgb)
        {
            XWindowsFormsHost.Dispatcher.Invoke(() =>
            {
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: Start UniversalImageViewer DrawHorizontalLine");
                cliWrapper.CreateCalibrationLine(imageViewHandle, 0, y, rgb);
                logService.Info(ServiceCategory.Common, $"[UniversalImageViewer] Action info: End UniversalImageViewer DrawHorizontalLine");
            });
        }

        #endregion

    }
}
