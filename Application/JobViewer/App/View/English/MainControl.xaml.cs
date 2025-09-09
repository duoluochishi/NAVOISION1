using Microsoft.Extensions.DependencyInjection;
using NV.CT.JobViewer.Models;
using NV.CT.JobViewer.ViewModel;
using NV.CT.Language;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NV.CT.JobViewer.View.English
{
    /// <summary>
    /// MainControl.xaml 的交互逻辑
    /// </summary>
    public partial class MainControl : UserControl
    {
        private const string RECON_VIEWER = "ReconViewer";
        private const string PRINT_VIEWER = "PrintViewer";
        private const string ARCHIVE_VIEWER = "ArchiveViewer";
        private const string IMPORT_VIEWER = "ImportViewer";
        private const string ExPORT_VIEWER = "ExportViewer";

        TaskViewModel ViewModelTask { get; set; }
        public MainControl()
        {
            InitializeComponent();
        }
        public MainControl(List<ResourceDictionary> list, IntPtr mainHwnd)
        {
            InitializeComponent();
            if (list != null && list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Resources.MergedDictionaries.Add(list[i]);
                }
            }
            //System.Threading.Thread.Sleep(26000);
            ViewModelTask = Global.Instance?.ServiceProvider?.GetRequiredService<TaskViewModel>();
            if (ViewModelTask != null)
            {
                ViewModelTask.MainWindowAuxHwnd = mainHwnd;
            }
            DataContext = ViewModelTask; 

            var comboxItems = new List<ComboxItem>();
            comboxItems.Add(new ComboxItem(LanguageResource.Content_Recon, RECON_VIEWER));
            comboxItems.Add(new ComboxItem(LanguageResource.Content_Print, PRINT_VIEWER));
            comboxItems.Add(new ComboxItem(LanguageResource.Content_Archive, ARCHIVE_VIEWER));
            comboxItems.Add(new ComboxItem(LanguageResource.Content_Import, IMPORT_VIEWER));
            comboxItems.Add(new ComboxItem(LanguageResource.Content_Export, ExPORT_VIEWER));
            BodyListBox.ItemsSource = comboxItems;
            BodyListBox.SelectedIndex = 0;
        }

        private void BodyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboxItem? comboxItem = BodyListBox.SelectedItem as ComboxItem;
            switch (comboxItem?.Value)
            {
                case RECON_VIEWER:
                    gridPrint.Visibility = Visibility.Collapsed;
                    gridArchive.Visibility = Visibility.Collapsed;
                    gridImport.Visibility = Visibility.Collapsed;
                    gridExport.Visibility = Visibility.Collapsed;
                    gridRecon.Visibility = Visibility.Visible;
                    break;
                case PRINT_VIEWER:
                    gridRecon.Visibility = Visibility.Collapsed;
                    gridArchive.Visibility = Visibility.Collapsed;
                    gridImport.Visibility = Visibility.Collapsed;
                    gridExport.Visibility = Visibility.Collapsed;
                    gridPrint.Visibility = Visibility.Visible;
                    break;
                case ARCHIVE_VIEWER:
                    gridRecon.Visibility = Visibility.Collapsed;
                    gridPrint.Visibility = Visibility.Collapsed;
                    gridImport.Visibility = Visibility.Collapsed;
                    gridExport.Visibility = Visibility.Collapsed;
                    gridArchive.Visibility = Visibility.Visible;
                    break;
                case IMPORT_VIEWER:
                    gridRecon.Visibility = Visibility.Collapsed;
                    gridPrint.Visibility = Visibility.Collapsed;
                    gridArchive.Visibility = Visibility.Collapsed;
                    gridExport.Visibility = Visibility.Collapsed;
                    gridImport.Visibility = Visibility.Visible;
                    break;
                case ExPORT_VIEWER:
                    gridRecon.Visibility = Visibility.Collapsed;
                    gridPrint.Visibility = Visibility.Collapsed;
                    gridArchive.Visibility = Visibility.Collapsed;
                    gridImport.Visibility = Visibility.Collapsed;
                    gridExport.Visibility = Visibility.Visible;
                    break;
            }
            lblTabTitle.Content = comboxItem?.Text;
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == 1)
            {
                SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1665D8"));
                ((System.Windows.Controls.ProgressBar)sender).Foreground = brush;
            }
            else if (e.NewValue < 1)
            {
                SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA640"));
                ((System.Windows.Controls.ProgressBar)sender).Foreground = brush;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Global.Instance?.ServiceProvider?.GetRequiredService<TaskViewModel>().LoadOfflineReconTask();
        }
    }
}
