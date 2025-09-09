using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.QualityTest.Alg;
using NV.CT.Service.QualityTest.Alg.Models;
using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Extension;
using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Models.ItemEntryParam;
using NV.CT.Service.QualityTest.Services;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using NV.MPS.Annotations;
using NV.MPS.ImageControl;
using NV.MPS.ImageIO;

namespace NV.CT.Service.QualityTest.Views.QTUC
{
    /// <summary>
    /// SliceThicknessHelicalUC.xaml 的交互逻辑
    /// </summary>
    public partial class SliceThicknessHelicalUC
    {
        /// <summary>
        /// 控件和对应Rect框的字典缓存
        /// </summary>
        private readonly Dictionary<SeriesImageControl, RectAnnotation[]> _dic = new();

        private readonly IMessagePrintService _printLog;
        private readonly IIntegrationPhantomService _integrationPhantomService;
        private bool _isInShowImage;

        public SliceThicknessHelicalUC(IMessagePrintService printLog, IIntegrationPhantomService integrationPhantomService)
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
            if (model.Param is not SingleValueParamModel param || !DataGridParas.Items.Contains(param))
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
                var metalWireRes = AlgMethods.MetalWireIdentify(curImageData.FilePath, ConfigFolder);

                if (!metalWireRes.Success)
                {
                    _printLog.PrintLoggerError(string.Format(Quality_Lang.Quality_Alg_GetMetalFail, $"No.{i + 1}", metalWireRes.Code.GetErrorCodeDescription()));
                    continue;
                }

                var card = ic.LastImageControl?.Box.GetCard<AnnotationCard>();

                if (card is null)
                {
                    _printLog.PrintLoggerError(string.Format(Quality_Lang.Quality_Image_IC_AnnoCardNotFind, $"No.{i + 1}"));
                    continue;
                }

                ic.LastImageControl!.ContextMenu = null;
                var annos = GetAnnotations(ic);
                var imageCanvas = card.ImageCanvas;
                var leftTop = imageCanvas.PointToWindow(new Point(metalWireRes.Result.Item1.LeftTop.X, metalWireRes.Result.Item1.LeftTop.Y));
                var rightBottom = imageCanvas.PointToWindow(new Point(metalWireRes.Result.Item1.RightBottom.X, metalWireRes.Result.Item1.RightBottom.Y));
                SetAnnotationPosition(annos[0], leftTop, rightBottom);
                leftTop = imageCanvas.PointToWindow(new Point(metalWireRes.Result.Item2.LeftTop.X, metalWireRes.Result.Item2.LeftTop.Y));
                rightBottom = imageCanvas.PointToWindow(new Point(metalWireRes.Result.Item2.RightBottom.X, metalWireRes.Result.Item2.RightBottom.Y));
                SetAnnotationPosition(annos[1], leftTop, rightBottom);
                AddAnnosAndCal(model, ic, card, annos);
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

            var annos = GetAnnotations(ic);
            AddAnnosAndCal(model, ic, card, annos);
        }

        protected override RectAnnotation[] GetAnnotations(SeriesImageControl ic)
        {
            if (_dic.TryGetValue(ic, out var values))
            {
                return values;
            }

            var rect1 = new RectAnnotation() { ContextMenu = null };
            var rect2 = new RectAnnotation() { ContextMenu = null };
            rect1.AnnotationText.ContextMenu = null;
            rect2.AnnotationText.ContextMenu = null;
            rect1.EditChangedMouseUp += Annotation_EditChange;
            rect2.EditChangedMouseUp += Annotation_EditChange;
            var result = new[] { rect1, rect2 };
            _dic[ic] = result;
            return result;
        }

        private void Annotation_EditChange(object sender, AnnoEditChangedEventArgs args)
        {
            if (args.ChangeType != AnnoChange.Change || args.NewValue is not RectAnnotation rectAnno || DataGridParas.SelectedItem is not ItemEntryModel model)
            {
                return;
            }

            var ic = _dic.First(i => i.Value.Contains(rectAnno)).Key;
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

        private void AddAnnosAndCal(ItemEntryModel model, SeriesImageControl ic, AnnotationCard card, IEnumerable<AbstractAnnotation> annos)
        {
            foreach (var anno in annos)
            {
                card.AddChild(anno);
            }

            card.Calculate();
            Calculate(model, ic);
        }

        private void Calculate(ItemEntryModel model, SeriesImageControl ic)
        {
            var annos = _dic[ic];
            var imageCanvas = ic.LastImageControl.Box.GetCard<AnnotationCard>().ImageCanvas;
            var leftTop1 = imageCanvas.PointToImage(annos[0].LeftTop);
            var rightBottom1 = imageCanvas.PointToImage(annos[0].RightBottom);
            var leftTop2 = imageCanvas.PointToImage(annos[1].LeftTop);
            var rightBottom2 = imageCanvas.PointToImage(annos[1].RightBottom);
            var stParam = new SliceThicknessAlgParam()
            {
                Path = ic.LastImageControl.ImageSource.DataInfo.FilePath,
                Item1 = new RectPoint()
                {
                    LeftTop = new Point2D(leftTop1.X, leftTop1.Y),
                    RightBottom = new Point2D(rightBottom1.X, rightBottom1.Y)
                },
                Item2 = new RectPoint()
                {
                    LeftTop = new Point2D(leftTop2.X, leftTop2.Y),
                    RightBottom = new Point2D(rightBottom2.X, rightBottom2.Y)
                },
            };
            var stRes = AlgMethods.SliceThicknessSpiral(stParam, ConfigFolder);
            float st;

            if (stRes.Success)
            {
                st = (stRes.Result.SliceThickness1 + stRes.Result.SliceThickness2) / 2;
            }
            else
            {
                st = 0;
                _printLog.PrintLoggerError(string.Format(Quality_Lang.Quality_Alg_InvokeError, stRes.Code.GetErrorCodeDescription()));
            }

            var index = GetImageControlIndex(model, ic);
            SetMeasuredValue(model.Param, index, st);
        }

        private void SetMeasuredValue(ItemEntryParamBaseModel paramBase, int i, double value)
        {
            if (paramBase is not SingleValueParamModel param)
            {
                return;
            }

            switch (i)
            {
                case 0:
                {
                    param.Value.FirstValue = value;
                    break;
                }
                case 1:
                {
                    param.Value.MediumValue = value;
                    break;
                }
                case 2:
                {
                    param.Value.LastValue = value;
                    break;
                }
            }
        }
    }
}