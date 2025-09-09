using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.QualityTest.Alg;
using NV.CT.Service.QualityTest.CTCalculator;
using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Initializer;
using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Services;
using NV.CT.Service.QualityTest.ViewModels;
using NV.CT.Service.QualityTest.Views.QTUC;
using NV.MPS.Annotations.Calculators;
using NV.MPS.ImageControl.Commands.Args;
using NV.MPS.UI.Generic;

namespace NV.CT.Service.QualityTest.Views
{
    /// <summary>
    /// QTView.xaml 的交互逻辑
    /// </summary>
    public partial class QTView
    {
        #region Field

        private bool _isFirstLoad = true;
        private QTViewModel _vm = null!;
        private readonly Dictionary<ItemModel, AbstractUCBase?> _ucDic;
        private readonly AbstractCalculatorFactory _calculatorFactory;

        #endregion

        public QTView()
        {
            InitializeComponent();
            _ucDic = new Dictionary<ItemModel, AbstractUCBase?>();
            _calculatorFactory = AnnotationCalculatorManager.Instance.GetCalculatorFactory("CT");
        }

        private void UCMain_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isFirstLoad)
            {
                ProgramInit.Init();
                _vm = Global.ServiceProvider.GetRequiredService<QTViewModel>();
                DataContext = _vm;
                _vm.InvalidateRequerySuggested = () =>
                {
                    if (CheckAccess())
                    {
                        CommandManager.InvalidateRequerySuggested();
                    }
                    else
                    {
                        Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
                    }
                };
                _vm.BeforeScan = BeforeScan;
                _vm.SetScanAndReconParam = SetScanAndReconParam;
                _vm.AfterRecon = AfterRecon;
                _isFirstLoad = false;
                var assembly = typeof(AbstractUCBase).Assembly;

                foreach (var item in _vm.Items)
                {
                    AbstractUCBase? uc = null;

                    if (!string.IsNullOrWhiteSpace(item.UCType))
                    {
                        var type = assembly.GetType(item.UCType, false);
                        uc = type == null ? null : Global.ServiceProvider.GetService(type) as AbstractUCBase;

                        if (uc != null)
                        {
                            uc.Tag = this;
                            uc.DataContext = item;
                        }
                    }

                    _ucDic.Add(item, uc);
                }
            }
            AlgMethods.GetPhantomLocateBalls("", "");

            _calculatorFactory.Register(AnnotationType.Ellipse.ToString(), new MyEllipseCTCalculator());
            _vm.OnLoaded();
        }

        private void UCMain_Unloaded(object sender, RoutedEventArgs e)
        {
            _vm.OnUnLoaded();
            _calculatorFactory.UnRegister(AnnotationType.Ellipse.ToString());
        }

        private void TextBoxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox)?.ScrollToEnd();
        }

        private void ListBoxQTItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox { SelectedItem: ItemModel model })
            {
                BorderUC.Child = _ucDic.GetValueOrDefault(model);
            }
        }

        #region Image Command

        private void ImageFuncCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            if (ListBoxQTItems?.SelectedItem is not ItemModel model || e.Parameter is not ImageFuncType type)
            {
                return;
            }

            var args = GetArgsByType(type);

            if (args == null)
            {
                return;
            }

            var uc = _ucDic[model];

            if (uc != null && uc.CommandCanExecute(type))
            {
                uc.SetCommand(args);
            }
        }

        private void ImageFuncCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;

            if (IsLoaded && _vm.IsScanning)
            {
                e.CanExecute = false;
                return;
            }

            if (ListBoxQTItems?.SelectedItem is ItemModel model && e.Parameter is ImageFuncType type)
            {
                var uc = _ucDic[model];
                e.CanExecute = uc != null && uc.CommandCanExecute(type);
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private ActionCommandArgs? GetArgsByType(ImageFuncType type)
        {
            // 此功能已移入独立数据分析工具中，缺失部分不再补齐
            return type switch
            {
                ImageFuncType.RawView => default,
                ImageFuncType.CutView => default,
                ImageFuncType.CorrView => default,
                ImageFuncType.SinogramView => default,
                ImageFuncType.ReconView => default,
                ImageFuncType.None => default,
                ImageFuncType.OneToOne => default,
                ImageFuncType.Fit => new ImageScaleToFitCommandArgs(),
                ImageFuncType.Zoom => new ImageZoomCommandArgs(),
                ImageFuncType.Pan => new ImagePanCommandArgs(),
                ImageFuncType.Rect => new ActionCommandArgs { Name = MedicalImageCommands.AnnotationRect.Name },
                ImageFuncType.Angle => new ActionCommandArgs { Name = MedicalImageCommands.AnnotationOpenAngle.Name },
                ImageFuncType.Ellipse => new ActionCommandArgs { Name = MedicalImageCommands.AnnotationEllipse.Name },
                ImageFuncType.Delete => default,
                _ => default
            };
        }

        #endregion

        #region Report

        private void ButtonReport_Click(object sender, RoutedEventArgs e)
        {
            var win = Global.ServiceProvider.GetRequiredService<ReportHeadInfoWindow>();
            win.Owner = Window.GetWindow(this);

            if (win.ShowDialog() == true)
            {
                _vm.GenerateReport();
                ReportBrowser.Navigate(Global.ReportFilePath);
                GridScan.Visibility = Visibility.Collapsed;
                ButtonStopScan.Visibility = Visibility.Collapsed;
                ButtonStopOfflineRecon.Visibility = Visibility.Collapsed;
                GridReport.Visibility = Visibility.Visible;
            }
        }

        private void ButtonReportExport_Click(object sender, RoutedEventArgs e)
        {
            var saveDlg = new SaveFileDialog()
            {
                Title = Quality_Lang.Quality_Report_ChooseExportPath,
                Filter = "html(*.html)|*.html",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                FileName = $"Performance_{DateTime.Now:yyyy.MM.dd-HH-mm}",
            };
            saveDlg.FileOk += SaveDlg_FileOk;
            saveDlg.ShowDialog();
        }

        private void SaveDlg_FileOk(object? sender, CancelEventArgs e)
        {
            if (sender is not SaveFileDialog saveDlg)
            {
                return;
            }

            saveDlg.FileOk -= SaveDlg_FileOk;
            var savePath = saveDlg.FileName;
            var dataStorageService = Global.ServiceProvider.GetRequiredService<IDataStorageService>();
            dataStorageService.SaveReportLastSessionDate();
            File.Copy(Global.ReportFilePath, savePath, true);
        }

        private void ButtonReportClose_Click(object sender, RoutedEventArgs e)
        {
            GridScan.Visibility = Visibility.Visible;
            ButtonStopScan.Visibility = Visibility.Visible;
            ButtonStopOfflineRecon.Visibility = Visibility.Visible;
            GridReport.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Scan Recon

        private ResultModel BeforeScan(ItemEntryModel param)
        {
            if (_ucDic.TryGetValue(param.Parent, out var uc) && uc != null)
            {
                return uc.CheckAccess() ? uc.BeforeScan(param) : Dispatcher.Invoke(() => uc.BeforeScan(param));
            }

            return ResultModel.Create(true);
        }

        private ResultModel SetScanAndReconParam(ItemEntryModel param)
        {
            if (_ucDic.TryGetValue(param.Parent, out var uc) && uc != null)
            {
                try
                {
                    return uc.SetScanAndReconParam(param);
                }
                catch (Exception e)
                {
                    LogService.Instance.Error(ServiceCategory.QualityTest, "SetScanAndReconParam Exception.", e);
                    return ResultModel.Create(false);
                }
            }

            return ResultModel.Create(false);
        }

        private async Task<ResultModel> AfterRecon(ItemEntryModel param)
        {
            if (_ucDic.TryGetValue(param.Parent, out var uc) && uc != null)
            {
                return uc.CheckAccess() ? await uc.AfterRecon(param) : await Dispatcher.Invoke(() => uc.AfterRecon(param));
            }

            return ResultModel.Create(true);
        }

        #endregion
    }
}