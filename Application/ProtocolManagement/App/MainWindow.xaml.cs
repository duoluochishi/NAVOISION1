using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using NV.CT.ProtocolManagement.Views.English;
using System;
using System.Collections.Generic;
using System.Windows;

namespace NV.CT.ProtocolManagement
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            List<ResourceDictionary> rscList = new List<ResourceDictionary>();//rscList
            #region  添加主题样式
            BundledTheme bundledTheme = new BundledTheme();
            bundledTheme.BaseTheme = BaseTheme.Dark;
            bundledTheme.PrimaryColor = PrimaryColor.LightBlue;
            bundledTheme.SecondaryColor = SecondaryColor.LightGreen;
            Application.Current.Resources.MergedDictionaries.Add(bundledTheme);
            rscList.Add(bundledTheme);
            ResourceDictionary languageResDic = new ResourceDictionary();
            languageResDic.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml", UriKind.Absolute);
            Application.Current.Resources.MergedDictionaries.Add(languageResDic);
            rscList.Add(languageResDic);
            languageResDic = new ResourceDictionary();
            languageResDic.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.PopupBox.xaml", UriKind.Absolute);
            Application.Current.Resources.MergedDictionaries.Add(languageResDic);
            rscList.Add(languageResDic);

            languageResDic = new ResourceDictionary();
            languageResDic.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Nanovision.xaml", UriKind.Absolute);
            Application.Current.Resources.MergedDictionaries.Add(languageResDic);
            rscList.Add(languageResDic);

            #endregion
            MainUserControl mainUC = new MainUserControl();

            container.Children.Add(mainUC);
            mainUC.Width = container.ActualWidth;
            mainUC.Height = container.ActualHeight;
        }
    }
}
