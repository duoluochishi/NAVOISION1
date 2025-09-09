using Microsoft.Extensions.DependencyInjection;
using NV.CT.JobViewer.Models;
using NV.CT.JobViewer.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NV.CT.JobViewer.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vmTask = Program.ServiceProvider?.GetRequiredService<TaskViewModel>();
            DataContext = vmTask;
            gridImport.DataContext = Global.Instance?.ServiceProvider?.GetRequiredService<ImportTaskViewModel>();
            gridExport.DataContext = Global.Instance?.ServiceProvider?.GetRequiredService<ExportTaskViewModel>();
            gridArchive.DataContext = Global.Instance?.ServiceProvider?.GetRequiredService<ArchiveTaskViewModel>();
            List<ComboxItem> comboxItems = new List<ComboxItem>();
            comboxItems.Add(new ComboxItem("Recon", "ReconViewer"));
            comboxItems.Add(new ComboxItem("Print", "PrintViewer"));
            comboxItems.Add(new ComboxItem("Archive", "ArchiveViewer"));
            comboxItems.Add(new ComboxItem("Import", "ImportViewer"));
            comboxItems.Add(new ComboxItem("Export", "ExportViewer"));
            BodyListBox.ItemsSource = comboxItems;
            BodyListBox.SelectedIndex = 0;
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BodyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboxItem? comboxItem = BodyListBox.SelectedItem as ComboxItem;
            switch (comboxItem?.Value)
            {
                case "ReconViewer":
                    gridPrint.Visibility = Visibility.Collapsed;
                    gridArchive.Visibility = Visibility.Collapsed;
                    gridImport.Visibility = Visibility.Collapsed;
                    gridExport.Visibility = Visibility.Collapsed;
                    gridRecon.Visibility = Visibility.Visible;
                    break;
                case "PrintViewer":
                    gridRecon.Visibility = Visibility.Collapsed;
                    gridArchive.Visibility = Visibility.Collapsed;
                    gridImport.Visibility = Visibility.Collapsed;
                    gridExport.Visibility = Visibility.Collapsed;
                    gridPrint.Visibility = Visibility.Visible;
                    break;
                case "ArchiveViewer":
                    gridRecon.Visibility = Visibility.Collapsed;
                    gridPrint.Visibility = Visibility.Collapsed;
                    gridImport.Visibility = Visibility.Collapsed;
                    gridExport.Visibility = Visibility.Collapsed;
                    gridArchive.Visibility = Visibility.Visible;
                    break;
                case "ImportViewer":
                    gridRecon.Visibility = Visibility.Collapsed;
                    gridPrint.Visibility = Visibility.Collapsed;
                    gridArchive.Visibility = Visibility.Collapsed;
                    gridExport.Visibility = Visibility.Collapsed;
                    gridImport.Visibility = Visibility.Visible;
                    break;
                case "ExportViewer":
                    gridRecon.Visibility = Visibility.Collapsed;
                    gridPrint.Visibility = Visibility.Collapsed;
                    gridArchive.Visibility = Visibility.Collapsed;
                    gridImport.Visibility = Visibility.Collapsed;
                    gridExport.Visibility = Visibility.Visible;
                    break;

            }
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

        private void barImportTask_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == 110)
            {
                SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1665D8"));
                ((System.Windows.Controls.ProgressBar)sender).Foreground = brush;
            }
            else if (e.NewValue < 110)
            {
                SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA640"));
                ((System.Windows.Controls.ProgressBar)sender).Foreground = brush;
            }
        }

        private void barExportTask_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == 100)
            {
                SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1665D8"));
                ((System.Windows.Controls.ProgressBar)sender).Foreground = brush;
            }
            else if (e.NewValue < 100)
            {
                SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA640"));
                ((System.Windows.Controls.ProgressBar)sender).Foreground = brush;
            }
        }
    }
}
