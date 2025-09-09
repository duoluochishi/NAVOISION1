using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Models;
using NV.MPS.Annotations;
using NV.MPS.Environment;
using NV.MPS.ImageControl;
using NV.MPS.ImageRender;
using NV.MPS.UI.Generic;

namespace NV.CT.Service.QualityTest.Views.QTUC
{
    public abstract class AbstractUCBase : UserControl
    {
        #region Field

        /// <summary>
        /// 算法配置文件存放目录
        /// </summary>
        protected static readonly string ConfigFolder = RuntimeConfig.Console.MRSConfig.Path;

        private readonly IList<ParamDisplayItemModel>? _paramDisplayItems;
        private readonly List<Border> _icBorders;
        private readonly Dictionary<ItemEntryModel, SeriesImageControl[]> _icDic;
        private ActionCommandArgs? _actionCommand;
        protected readonly int ICCount;

        #endregion

        protected AbstractUCBase(int imageControlCount)
        {
            _paramDisplayItems = Global.ServiceProvider.GetService<ParamDisplayItemModel[]>();
            ICCount = imageControlCount <= 0 ? 1 : imageControlCount;
            _icBorders = new List<Border>(ICCount);
            _icDic = new Dictionary<ItemEntryModel, SeriesImageControl[]>();
        }

        protected void AddDataGridColumns(DataGrid dataGrid)
        {
            if (_paramDisplayItems == null || _paramDisplayItems.Count == 0)
            {
                return;
            }

            foreach (var item in _paramDisplayItems)
            {
                var column = new DataGridTextColumn()
                {
                    Header = item.Header,
                    IsReadOnly = true,
                    ElementStyle = FindResource("CenterStyle") as Style,
                    MinWidth = item.MinWidth,
                    Width = item.Width switch
                    {
                        { } when double.TryParse(item.Width, out var width) => width,
                        "*" => new DataGridLength(1.0, DataGridLengthUnitType.Star),
                        _ => DataGridLength.Auto,
                    },
                    Binding = new Binding(item.BindingPath),
                };
                dataGrid.Columns.Add(column);
            }
        }

        protected void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid { SelectedItem: ItemEntryModel model })
            {
                for (int i = 0; i < ICCount; i++)
                {
                    if (_icBorders.Count >= i + 1)
                    {
                        _icBorders[i].Child = GetImageControl(model, i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < ICCount; i++)
                {
                    if (_icBorders.Count >= i + 1)
                    {
                        _icBorders[i].Child = null;
                    }
                }
            }
        }

        protected SeriesImageControl GetImageControl(ItemEntryModel model, int index)
        {
            return GetImageControls(model)[index];
        }

        protected SeriesImageControl[] GetImageControls(ItemEntryModel model)
        {
            if (!_icDic.ContainsKey(model))
            {
                var ics = new SeriesImageControl[ICCount];

                for (int i = 0; i < ICCount; i++)
                {
                    var ic = CreateSIC();
                    ic.ImageChanged += SICImageChanged;
                    ics[i] = ic;

                    if (_actionCommand != null)
                    {
                        ic.OnCommand(_actionCommand);
                    }
                }

                _icDic[model] = ics;
            }

            return _icDic[model];

            SeriesImageControl CreateSIC() => new() { ContextMenu = null };
        }

        protected virtual void SICImageChanged(object? sender, ImageSourceChangedEventArgs e)
        {
            if (e.OldImageSource == null)
            {
                return;
            }

            if ((sender as SeriesImageControl)?.LastImageControl?.Box.GetCard<AnnotationCard>() is { } card)
            {
                card.ImageSource.DisplayState = e.OldImageSource.DisplayState.Clone() as ImageDisplayState;
                card.ImageCanvas.DoTransform();
            }
        }

        protected int GetImageControlIndex(ItemEntryModel model, SeriesImageControl ic)
        {
            if (_icDic.TryGetValue(model, out var ics))
            {
                if (ics.Contains(ic))
                {
                    for (int i = 0; i < ics.Length; i++)
                    {
                        if (ics[i] == ic)
                        {
                            return i;
                        }
                    }
                }
            }

            return -1;
        }

        protected void SetImageControlBorders(params Border[] borders)
        {
            _icBorders.Clear();
            _icBorders.AddRange(borders.Length >= ICCount ? borders[..ICCount] : borders);

            foreach (var border in borders)
            {
                border.PreviewKeyDown += (_, e) =>
                {
                    if (e.Key == Key.Delete && e.OriginalSource is AbstractAnnotation)
                    {
                        e.Handled = true;
                    }
                };
            }
        }

        protected void SetCommand()
        {
            if (_actionCommand == null)
            {
                return;
            }

            foreach (var control in _icDic.SelectMany(i => i.Value))
            {
                control.OnCommand(_actionCommand);
            }
        }

        public void SetCommand(ActionCommandArgs command)
        {
            _actionCommand = command;
            SetCommand();
        }

        public virtual bool CommandCanExecute(ImageFuncType type)
        {
            return true;
        }

        public abstract ResultModel BeforeScan(ItemEntryModel param);
        public abstract ResultModel SetScanAndReconParam(ItemEntryModel model);
        public abstract Task<ResultModel> AfterRecon(ItemEntryModel param);
    }
}