using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.QualityTest.CTCalculator;
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
    /// HomogeneityUC.xaml 的交互逻辑
    /// </summary>
    public partial class HomogeneityUC
    {
        /// <summary>
        /// 控件和对应ROI框的字典缓存
        /// <para>ROI数组按顺序分别为上、下、左、右、中</para>
        /// </summary>
        private readonly Dictionary<SeriesImageControl, EllipseAnnotation[]> _dic = new();

        private readonly IMessagePrintService? _printLog;
        private readonly IIntegrationPhantomService _integrationPhantomService;
        private bool _isInShowImage;

        public HomogeneityUC()
        {
            _integrationPhantomService = null!;
        }

        public HomogeneityUC(IMessagePrintService? printLog, IIntegrationPhantomService integrationPhantomService)
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
                HomogeneityParamModel m => m.WaterLayer switch
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
            if (model.Param is not HomogeneityParamModel param || !DataGridParas.Items.Contains(model))
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
                curImageData = ImageControlUtility.CreateImageData(curImageData.FilePath);
                ic.AddImage(curImageDataList);
                ic.GotoImageByIndex(showIndex);
                await Task.Delay(100);
                var card = ic.LastImageControl?.Box.GetCard<AnnotationCard>();

                if (card is null)
                {
                    _printLog?.PrintLoggerError(string.Format(Quality_Lang.Quality_Image_IC_AnnoCardNotFind, $"No.{i + 1}"));
                    continue;
                }

                ic.LastImageControl!.ContextMenu = null;
                var annos = GetAnnotations(ic);
                var imageCanvas = card.ImageCanvas;
                var width = curImageData.ImageWidth;
                var height = curImageData.ImageHeight;
                var xCenter = curImageData.ImageWidth / 2d;
                var yCenter = curImageData.ImageHeight / 2d;
                var radius = width * 0.05;
                var diam = radius * 2;
                var space = width * 0.15;
                SetAnnotationPosition(annos[0], imageCanvas.PointToWindow(new Point(xCenter - radius, space)), imageCanvas.PointToWindow(new Point(xCenter + radius, diam + space)));
                SetAnnotationPosition(annos[1], imageCanvas.PointToWindow(new Point(xCenter - radius, height - space - diam)), imageCanvas.PointToWindow(new Point(xCenter + radius, height - space)));
                SetAnnotationPosition(annos[2], imageCanvas.PointToWindow(new Point(space, yCenter - radius)), imageCanvas.PointToWindow(new Point(diam + space, yCenter + radius)));
                SetAnnotationPosition(annos[3], imageCanvas.PointToWindow(new Point(width - space - diam, yCenter - radius)), imageCanvas.PointToWindow(new Point(width - space, yCenter + radius)));
                SetAnnotationPosition(annos[4], imageCanvas.PointToWindow(new Point(xCenter - radius, yCenter - radius)), imageCanvas.PointToWindow(new Point(xCenter + radius, yCenter + radius)));
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

        protected override EllipseAnnotation[] GetAnnotations(SeriesImageControl ic)
        {
            if (_dic.TryGetValue(ic, out var values))
            {
                return values;
            }

            var top = new EllipseAnnotation() { ContextMenu = null };
            var bottom = new EllipseAnnotation() { ContextMenu = null };
            var left = new EllipseAnnotation() { ContextMenu = null };
            var right = new EllipseAnnotation() { ContextMenu = null };
            var center = new EllipseAnnotation() { ContextMenu = null };
            top.AnnotationText.ContextMenu = null;
            bottom.AnnotationText.ContextMenu = null;
            left.AnnotationText.ContextMenu = null;
            right.AnnotationText.ContextMenu = null;
            center.AnnotationText.ContextMenu = null;
            top.EditChangedMouseUp += Annotation_EditChange;
            bottom.EditChangedMouseUp += Annotation_EditChange;
            left.EditChangedMouseUp += Annotation_EditChange;
            right.EditChangedMouseUp += Annotation_EditChange;
            center.EditChangedMouseUp += Annotation_EditChange;
            var result = new[] { top, bottom, left, right, center };
            _dic[ic] = result;
            return result;
        }

        private void Annotation_EditChange(object sender, AnnoEditChangedEventArgs args)
        {
            if (args.ChangeType != AnnoChange.Change || args.NewValue is not EllipseAnnotation anno || DataGridParas.SelectedItem is not ItemEntryModel model)
            {
                return;
            }

            Calculate(model, anno);
            model.Validate();
        }

        private void SetAnnotationPosition(EllipseAnnotation anno, Point leftTop, Point rightBottom)
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

        private void Calculate(ItemEntryModel model, EllipseAnnotation anno)
        {
            var ic = _dic.First(i => i.Value.Contains(anno)).Key;
            var annos = _dic[ic];
            var icIndex = GetImageControlIndex(model, ic);
            var annoIndex = annos.ToList().IndexOf(anno);
            var centerCT = annos[4].MeasureResults.ResultList.FirstOrDefault(x => x.ResultName == ResultNameTable.Average)?.Content ?? 0;

            for (var i = 0; i < 4; i++)
            {
                if (anno != annos[4] && annoIndex != i)
                {
                    continue;
                }

                var currentAnno = annos[i];
                var dValue = (currentAnno.MeasureResults.ResultList.FirstOrDefault(o => o.ResultName == ResultNameTable.Average)?.Content ?? 0) - centerCT;
                var dResult = currentAnno.MeasureResults.ResultList.FirstOrDefault(o => o.ResultName == MyEllipseCTCalculator.DValue);

                if (dResult != null)
                {
                    dResult.Content = dValue;
                }

                UpdateAnnotationTextContent(currentAnno);
                SetMeasuredValue(model.Param, icIndex, i, dValue);
            }
        }

        private void Calculate(ItemEntryModel model, SeriesImageControl ic)
        {
            var annos = _dic[ic];
            var icIndex = GetImageControlIndex(model, ic);
            var centerCT = annos[4].MeasureResults.ResultList.FirstOrDefault(x => x.ResultName == ResultNameTable.Average)?.Content ?? 0;

            for (var i = 0; i < annos.Length; i++)
            {
                var anno = annos[i];
                var dValue = (anno.MeasureResults.ResultList.FirstOrDefault(o => o.ResultName == ResultNameTable.Average)?.Content ?? 0) - centerCT;
                var dResult = anno.MeasureResults.ResultList.FirstOrDefault(o => o.ResultName == MyEllipseCTCalculator.DValue);

                if (dResult != null)
                {
                    dResult.Content = dValue;
                }

                UpdateAnnotationTextContent(anno);
                SetMeasuredValue(model.Param, icIndex, i, dValue);
            }
        }

        private void UpdateAnnotationTextContent(AbstractAnnotation anno)
        {
            anno.AnnotationTextContent = anno.MeasureResults.Format();
            anno.InvalidateVisual();
        }

        private void SetMeasuredValue(ItemEntryParamBaseModel paramBase, int icIndex, int annoIndex, double value)
        {
            if (paramBase is not HomogeneityParamModel param)
            {
                return;
            }

            switch (icIndex)
            {
                case 0:
                {
                    switch (annoIndex)
                    {
                        case 0:
                        {
                            param.Value.FirstOClock3Value = value;
                            break;
                        }
                        case 1:
                        {
                            param.Value.FirstOClock6Value = value;
                            break;
                        }
                        case 2:
                        {
                            param.Value.FirstOClock9Value = value;
                            break;
                        }
                        case 3:
                        {
                            param.Value.FirstOClock12Value = value;
                            break;
                        }
                    }

                    break;
                }
                case 1:
                {
                    switch (annoIndex)
                    {
                        case 0:
                        {
                            param.Value.MediumOClock3Value = value;
                            break;
                        }
                        case 1:
                        {
                            param.Value.MediumOClock6Value = value;
                            break;
                        }
                        case 2:
                        {
                            param.Value.MediumOClock9Value = value;
                            break;
                        }
                        case 3:
                        {
                            param.Value.MediumOClock12Value = value;
                            break;
                        }
                    }

                    break;
                }
                case 2:
                {
                    switch (annoIndex)
                    {
                        case 0:
                        {
                            param.Value.LastOClock3Value = value;
                            break;
                        }
                        case 1:
                        {
                            param.Value.LastOClock6Value = value;
                            break;
                        }
                        case 2:
                        {
                            param.Value.LastOClock9Value = value;
                            break;
                        }
                        case 3:
                        {
                            param.Value.LastOClock12Value = value;
                            break;
                        }
                    }

                    break;
                }
            }
        }
    }
}