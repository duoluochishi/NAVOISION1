using NV.CT.ImageViewer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NV.CT.ImageViewer.View
{
    /// <summary>
    /// Interaction logic for ImageViewerScrollBar.xaml
    /// </summary>
    public partial class ImageViewerScrollBar : UserControl
    {
        public ImageViewerScrollBar()
        {
            InitializeComponent();
            DataContext = CTS.Global.ServiceProvider.GetService<ImagViewerScrollBarViewMode>();

        }
    }
}
