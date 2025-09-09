using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System;
using System.Collections.Generic;

namespace NV.CT.UI.Controls
{
    public class LoadingResource
    {
        public static void LoadingInApplication()
        {
            BundledTheme bundledTheme = new BundledTheme();
            bundledTheme.BaseTheme = BaseTheme.Dark;
            bundledTheme.PrimaryColor = PrimaryColor.LightBlue;
            bundledTheme.SecondaryColor = SecondaryColor.LightGreen;
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(bundledTheme);

            ResourceDictionary languageResDic = new ResourceDictionary();
            languageResDic.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml", UriKind.RelativeOrAbsolute);
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(languageResDic);

            languageResDic = new ResourceDictionary();
            languageResDic.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.PopupBox.xaml", UriKind.RelativeOrAbsolute);
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(languageResDic);

            languageResDic = new ResourceDictionary();
            languageResDic.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Nanovision.xaml", UriKind.RelativeOrAbsolute);
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(languageResDic);
        }

        public static List<ResourceDictionary> LoadingInControl()
        {
            List<ResourceDictionary> resources = new List<ResourceDictionary>();
            BundledTheme bundledTheme = new BundledTheme();
            bundledTheme.BaseTheme = BaseTheme.Dark;
            bundledTheme.PrimaryColor = PrimaryColor.LightBlue;
            bundledTheme.SecondaryColor = SecondaryColor.LightGreen;
            resources.Add(bundledTheme);
            ResourceDictionary languageResDic = new ResourceDictionary();
            languageResDic.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml", UriKind.Absolute);
            resources.Add(languageResDic);

            languageResDic = new ResourceDictionary();
            languageResDic.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.PopupBox.xaml", UriKind.Absolute);
            resources.Add(languageResDic);

            languageResDic = new ResourceDictionary();
            languageResDic.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Nanovision.xaml", UriKind.Absolute);
            resources.Add(languageResDic);
            return resources;
        }
    }
}
