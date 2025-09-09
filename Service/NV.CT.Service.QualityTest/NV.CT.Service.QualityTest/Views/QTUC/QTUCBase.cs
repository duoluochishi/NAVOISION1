using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NV.CT.Service.QualityTest.Models;
using NV.MPS.Annotations;
using NV.MPS.ImageControl;

namespace NV.CT.Service.QualityTest.Views.QTUC
{
    /// <summary>
    ///
    /// </summary>
    public abstract class QTUCBase : AbstractUCBase
    {
        protected QTUCBase() : this(3)
        {
        }

        protected QTUCBase(int imageControlCount) : base(imageControlCount)
        {
        }

        protected void TitleCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox cb || DataContext is not ItemModel { Entries.Count: > 0 } model)
            {
                return;
            }

            foreach (var item in model.Entries)
            {
                item.IsChecked = cb.IsChecked ?? false;
            }
        }

        protected void ItemCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ItemModel model)
            {
                return;
            }

            model.IsAllChecked = model.Entries!.All(i => i.IsChecked);
        }

        protected (int startIndex, int count, int showIndex) GetFileIndex(ImagePercentModel model, int index, int totalCount)
        {
            int startIndex;
            int count;
            int showIndex;
            var total = model.FirstPercent + model.MediumPercent + model.LastPercent;
            var firstCount = (int)((double)model.FirstPercent / total * totalCount);
            firstCount = firstCount < 1 ? 1 : firstCount;
            var mediumCount = (int)((double)model.MediumPercent / total * totalCount);
            mediumCount = mediumCount < 1 ? 1 : mediumCount;
            var lastCount = totalCount - firstCount - mediumCount;
            lastCount = lastCount < 1 ? 1 : lastCount;

            if (index <= 0)
            {
                startIndex = 0;
                count = firstCount;
                showIndex = (int)(model.FirstIndexPercent / 100d * count);
            }
            else if (index >= ICCount - 1)
            {
                startIndex = firstCount + mediumCount;
                count = lastCount;
                showIndex = (int)(model.LastIndexPercent / 100d * count);
            }
            else
            {
                startIndex = firstCount;
                count = mediumCount;
                showIndex = (int)(model.MediumIndexPercent / 100d * count);
            }

            startIndex = startIndex >= totalCount ? totalCount - count : startIndex;
            return (startIndex, count, showIndex);
        }

        protected abstract IEnumerable<AbstractAnnotation>? GetAnnotations(SeriesImageControl ic);

        public override ResultModel BeforeScan(ItemEntryModel param)
        {
            var ics = GetImageControls(param);

            foreach (var ic in ics)
            {
                var annos = GetAnnotations(ic);

                if (annos == null)
                {
                    continue;
                }

                var card = ic.LastImageControl?.Box.GetCard<AnnotationCard>();

                if (card != null)
                {
                    foreach (var anno in annos)
                    {
                        card.DeleteChild(anno);
                    }
                }

                ic.ClearImages();
            }

            return ResultModel.Create(true);
        }
    }
}