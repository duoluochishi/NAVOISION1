using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.QualityTest.Alg;
using NV.CT.Service.QualityTest.Alg.Models;
using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Extension;
using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Models.ItemEntryParam;
using NV.CT.Service.QualityTest.Services;
using NV.CT.Service.QualityTest.Views.MTF;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using NV.MPS.Annotations;
using NV.MPS.ImageControl;
using NV.MPS.ImageIO;

namespace NV.CT.Service.QualityTest.Views.QTUC
{
    /// <summary>
    /// HighContrastResolutionXY.xaml 的交互逻辑
    /// </summary>
    public partial class HighContrastResolutionXYUC
    {
        /// <summary>
        /// 控件和对应ROI框的字典缓存
        /// </summary>
        private readonly Dictionary<SeriesImageControl, RectAnnotation> _dic = new();

        private readonly IMessagePrintService _printLog;
        private readonly IIntegrationPhantomService _integrationPhantomService;
        private bool _isInShowImage;

        public HighContrastResolutionXYUC(IMessagePrintService printLog, IIntegrationPhantomService integrationPhantomService)
        {
            InitializeComponent();
            _printLog = printLog;
            _integrationPhantomService = integrationPhantomService;
            SetImageControlBorders(BorderIC1, BorderIC2, BorderIC3);
            AddDataGridColumns(DataGridParas);
        }

        public override bool CommandCanExecute(ImageFuncType type)
        {
            return type switch
            {
                ImageFuncType.RawView => false,
                ImageFuncType.CutView => false,
                ImageFuncType.CorrView => false,
                ImageFuncType.SinogramView => false,
                ImageFuncType.ReconView => false,
                ImageFuncType.Rect => false,
                ImageFuncType.Ellipse => false,
                ImageFuncType.Delete => false,
                _ => true
            };
        }

        public override ResultModel SetScanAndReconParam(ItemEntryModel model)
        {
            var position = _integrationPhantomService.GetPhysicalPosition();
            model.SetScanAndReconParam(position);
            return ResultModel.Create(true);
        }

        public override async Task<ResultModel> AfterRecon(ItemEntryModel model)
        {
            if (model.Param is not MTFParamModel param || !DataGridParas.Items.Contains(model))
            {
                return ResultModel.Create(true);
            }

            _isInShowImage = true;
            var ics = GetImageControls(model);
            var imageDataList = Directory.GetFiles(model.OfflineReconImageFolder).OrderBy(i => i).Select(i => new ImageData() { FilePath = i }).ToList();

            for (var i = 0; i < ics.Length; i++)
            {
                var ic = ics[i];
                var (startIndex, count, showIndex) = GetFileIndex(param.ImagePercent, i, imageDataList.Count);
                var curImageDataList = imageDataList.Skip(startIndex).Take(count).ToList();
                var curImageData = curImageDataList[showIndex];
                ic.AddImage(curImageDataList);
                ic.GotoImageByIndex(showIndex);
                await Task.Delay(100);
                var metalRes = AlgMethods.MetalIdentify(curImageData.FilePath, ConfigFolder);

                if (!metalRes.Success)
                {
                    _printLog.PrintLoggerError(string.Format(Quality_Lang.Quality_Alg_GetMetalFail, $"No.{i + 1}", metalRes.Code.GetErrorCodeDescription()));
                    continue;
                }

                var card = ic.LastImageControl?.Box.GetCard<AnnotationCard>();

                if (card is null)
                {
                    _printLog.PrintLoggerError(string.Format(Quality_Lang.Quality_Image_IC_AnnoCardNotFind, $"No.{i + 1}"));
                    continue;
                }

                ic.LastImageControl!.ContextMenu = null;
                var anno = GetAnnotation(ic);
                var imageCanvas = card.ImageCanvas;
                SetAnnotationPosition(anno, imageCanvas.PointToWindow(new Point(metalRes.Result.Item1.LeftTop.X, metalRes.Result.Item1.LeftTop.Y)), imageCanvas.PointToWindow(new Point(metalRes.Result.Item1.RightBottom.X, metalRes.Result.Item1.RightBottom.Y)));
                AddAnnosAndCal(model, ic, card, anno);
            }

            // SetCommand();
            model.Validate();
            _isInShowImage = false;
            return ResultModel.Create(true);
        }

        protected override void SICImageChanged(object? sender, ImageSourceChangedEventArgs e)
        {
            base.SICImageChanged(sender, e);

            if (_isInShowImage)
            {
                return;
            }

            if (sender is not SeriesImageControl ic)
            {
                return;
            }

            if (DataGridParas.SelectedItem is not ItemEntryModel model)
            {
                return;
            }

            if (ic.LastImageControl?.Box.GetCard<AnnotationCard>() is not { } card)
            {
                return;
            }

            var anno = GetAnnotation(ic);
            AddAnnosAndCal(model, ic, card, anno);
        }

        private RectAnnotation GetAnnotation(SeriesImageControl ic)
        {
            if (_dic.TryGetValue(ic, out var values))
            {
                return values;
            }

            var anno = new RectAnnotation() { ContextMenu = null };
            anno.AnnotationText.ContextMenu = null;
            anno.EditChangedMouseUp += Annotation_EditChange;
            _dic[ic] = anno;
            return anno;
        }

        protected override RectAnnotation[] GetAnnotations(SeriesImageControl ic)
        {
            return new[] { GetAnnotation(ic) };
        }

        private void Annotation_EditChange(object sender, AnnoEditChangedEventArgs args)
        {
            if (args.ChangeType != AnnoChange.Change || args.NewValue is not RectAnnotation anno || DataGridParas.SelectedItem is not ItemEntryModel model)
            {
                return;
            }

            var ic = _dic.First(i => i.Value == anno).Key;
            Calculate(model, ic);
            model.Validate();
        }

        private void SetAnnotationPosition(RectAnnotation anno, Point leftTop, Point rightBottom)
        {
            anno.LeftTop = leftTop;
            anno.RightBottom = rightBottom;
            anno.AnnotationTextPosition = leftTop with { Y = rightBottom.Y + 10 };
            anno.Flatten();
        }

        private void AddAnnosAndCal(ItemEntryModel model, SeriesImageControl ic, AnnotationCard card, AbstractAnnotation anno)
        {
            card.AddChild(anno);
            card.Calculate();
            Calculate(model, ic);
        }

        private void Calculate(ItemEntryModel model, SeriesImageControl ic)
        {
            var anno = _dic[ic];
            var imageCanvas = ic.LastImageControl.Box.GetCard<AnnotationCard>().ImageCanvas;
            var leftTop = imageCanvas.PointToImage(anno.LeftTop);
            var rightBottom = imageCanvas.PointToImage(anno.RightBottom);
            var mtfParam = new MTFAlgParam()
            {
                Path = ic.LastImageControl.ImageSource.DataInfo.FilePath,
                Item = new RectPoint()
                {
                    LeftTop = new Point2D(leftTop.X, leftTop.Y),
                    RightBottom = new Point2D(rightBottom.X, rightBottom.Y)
                },
            };
            var mtfRes = AlgMethods.MTF_XY(mtfParam, ConfigFolder);
            MTFAlgModel mtf;

            if (mtfRes.Success)
            {
                mtf = mtfRes.Result;
            }
            else
            {
                mtf = new MTFAlgModel();
                _printLog.PrintLoggerError(string.Format(Quality_Lang.Quality_Alg_InvokeError, mtfRes.Code.GetErrorCodeDescription()));
            }

            var index = GetImageControlIndex(model, ic);
            SetMeasuredValue(model.Param, index, mtf);
        }

        private void SetMeasuredValue(ItemEntryParamBaseModel paramBase, int i, MTFAlgModel algValue)
        {
            if (paramBase is not MTFParamModel param)
            {
                return;
            }

            switch (i)
            {
                case 0:
                {
                    param.Value.FirstMTF0Value = algValue.MTF0;
                    param.Value.FirstMTF2Value = algValue.MTF2;
                    param.Value.FirstMTF10Value = algValue.MTF10;
                    param.Value.FirstMTF50Value = algValue.MTF50;
                    param.Value.FirstMTFArray.Clear();
                    param.Value.FirstMTFArray.AddRange(algValue.MTFArray);
                    break;
                }
                case 1:
                {
                    param.Value.MediumMTF0Value = algValue.MTF0;
                    param.Value.MediumMTF2Value = algValue.MTF2;
                    param.Value.MediumMTF10Value = algValue.MTF10;
                    param.Value.MediumMTF50Value = algValue.MTF50;
                    param.Value.MediumMTFArray.Clear();
                    param.Value.MediumMTFArray.AddRange(algValue.MTFArray);
                    break;
                }
                case 2:
                {
                    param.Value.LastMTF0Value = algValue.MTF0;
                    param.Value.LastMTF2Value = algValue.MTF2;
                    param.Value.LastMTF10Value = algValue.MTF10;
                    param.Value.LastMTF50Value = algValue.MTF50;
                    param.Value.LastMTFArray.Clear();
                    param.Value.LastMTFArray.AddRange(algValue.MTFArray);
                    break;
                }
            }
        }

        private void ButtonCurve_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { DataContext: ItemEntryModel { Param: MTFParamModel param } })
            {
                return;
            }

            var win = new SelectImageWindow { Owner = Window.GetWindow(this) };

            if (win.ShowDialog() == false)
            {
                return;
            }

            var data = win.Select switch
            {
                UCImageSelectType.Left => new MTFCurveModel()
                {
                    MTF0Value = param.Value.FirstMTF0Value,
                    MTF2Value = param.Value.FirstMTF2Value,
                    MTF10Value = param.Value.FirstMTF10Value,
                    MTF50Value = param.Value.FirstMTF50Value,
                    LastMTFArray = param.Value.FirstMTFArray,
                },
                UCImageSelectType.Center => new MTFCurveModel()
                {
                    MTF0Value = param.Value.MediumMTF0Value,
                    MTF2Value = param.Value.MediumMTF2Value,
                    MTF10Value = param.Value.MediumMTF10Value,
                    MTF50Value = param.Value.MediumMTF50Value,
                    LastMTFArray = param.Value.MediumMTFArray,
                },
                UCImageSelectType.Right => new MTFCurveModel()
                {
                    MTF0Value = param.Value.LastMTF0Value,
                    MTF2Value = param.Value.LastMTF2Value,
                    MTF10Value = param.Value.LastMTF10Value,
                    MTF50Value = param.Value.LastMTF50Value,
                    LastMTFArray = param.Value.LastMTFArray,
                },
                _ => new MTFCurveModel(),
            };
            var curveWin = new CurveWindow(data) { Owner = Window.GetWindow(this) };
            curveWin.ShowDialog();
        }
    }
}