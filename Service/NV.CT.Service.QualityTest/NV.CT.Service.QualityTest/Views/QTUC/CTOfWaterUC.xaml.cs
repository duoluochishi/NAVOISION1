using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Models.ItemEntryParam;
using NV.CT.Service.QualityTest.Services;
using NV.CT.Service.QualityTest.Utilities;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using NV.MPS.Annotations;
using NV.MPS.Annotations.Calculators;
using NV.MPS.ImageControl;
using NV.MPS.ImageIO;

namespace NV.CT.Service.QualityTest.Views.QTUC
{
    /// <summary>
    /// CTOfWaterAndNoiseUC.xaml 的交互逻辑
    /// </summary>
    public partial class CTOfWaterUC
    {
        /// <summary>
        /// 控件和对应ROI框的字典缓存
        /// </summary>
        private readonly Dictionary<SeriesImageControl, EllipseAnnotation> _dic = new();

        private readonly IMessagePrintService _printLog;
        private readonly IIntegrationPhantomService _integrationPhantomService;
        private bool _isInShowImage;

        public CTOfWaterUC(IMessagePrintService printLog, IIntegrationPhantomService integrationPhantomService)
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
            var position = model.Param switch
            {
                SingleValueParamModel m => m.WaterLayer switch
                {
                    WaterPhantomLayerType.Water20 => _integrationPhantomService.GetWater20Position(),
                    WaterPhantomLayerType.Water30 => _integrationPhantomService.GetWater30Position(),
                    _ => _integrationPhantomService.GetWater30Position()
                },
                _ => _integrationPhantomService.GetWater30Position()
            };
            model.SetScanAndReconParam(position);
            return ResultModel.Create(true);
        }

        public override async Task<ResultModel> AfterRecon(ItemEntryModel model)
        {
            if (model.Param is not SingleValueParamModel param || !DataGridParas.Items.Contains(model))
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
                var imageData = curImageDataList[showIndex];
                imageData = ImageControlUtility.CreateImageData(imageData.FilePath);
                ic.AddImage(curImageDataList);
                ic.GotoImageByIndex(showIndex);
                await Task.Delay(100);
                var card = ic.LastImageControl?.Box.GetCard<AnnotationCard>();

                if (card is null)
                {
                    _printLog.PrintLoggerError(string.Format(Quality_Lang.Quality_Image_IC_AnnoCardNotFind, $"No.{i + 1}"));
                    continue;
                }

                ic.LastImageControl!.ContextMenu = null;
                var anno = GetAnnotation(ic);
                var imageCanvas = card.ImageCanvas;
                var xCenter = imageData.ImageWidth / 2;
                var yCenter = imageData.ImageHeight / 2;
                var radius = imageData.ImageWidth * 0.05;
                SetAnnotationPosition(anno, imageCanvas.PointToWindow(new Point(xCenter - radius, yCenter - radius)), imageCanvas.PointToWindow(new Point(xCenter + radius, yCenter + radius)));
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

        private EllipseAnnotation GetAnnotation(SeriesImageControl ic)
        {
            if (_dic.TryGetValue(ic, out var values))
            {
                return values;
            }

            var anno = new EllipseAnnotation() { ContextMenu = null };
            anno.AnnotationText.ContextMenu = null;
            anno.EditChangedMouseUp += Annotation_EditChange;
            _dic[ic] = anno;
            return anno;
        }

        protected override EllipseAnnotation[] GetAnnotations(SeriesImageControl ic)
        {
            return new[] { GetAnnotation(ic) };
        }

        private void Annotation_EditChange(object sender, AnnoEditChangedEventArgs args)
        {
            if (args.ChangeType != AnnoChange.Change || args.NewValue is not EllipseAnnotation anno || DataGridParas.SelectedItem is not ItemEntryModel model)
            {
                return;
            }

            var ic = _dic.First(i => i.Value == anno).Key;
            Calculate(model, ic);
            model.Validate();
        }

        private void SetAnnotationPosition(EllipseAnnotation anno, Point leftTop, Point rightBottom)
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
            var index = GetImageControlIndex(model, ic);
            var ctValue = anno.MeasureResults.ResultList.FirstOrDefault(x => x.ResultName == ResultNameTable.Average)?.Content ?? 0;
            SetMeasuredValue(model.Param, index, ctValue);
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