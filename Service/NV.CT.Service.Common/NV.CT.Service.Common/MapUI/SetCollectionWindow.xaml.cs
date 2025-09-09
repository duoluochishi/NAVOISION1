using System.Windows;
using System.Windows.Controls;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.MapUI.Templates;

namespace NV.CT.Service.Common.MapUI
{
    /// <summary>
    /// SetCollectionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetCollectionWindow
    {
        public SetCollectionWindow(AbstractMapUITemplate template)
        {
            InitializeComponent();
            DataContext = template;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Control_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is DependencyObject dependency)
            {
                var listBoxItem = dependency.FindParent<ListBoxItem>();

                if (listBoxItem != null)
                {
                    listBoxItem.IsSelected = true;
                }
            }
        }
    }
}