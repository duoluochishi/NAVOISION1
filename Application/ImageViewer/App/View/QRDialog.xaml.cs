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
using System.Windows.Shapes;

namespace NV.CT.ImageViewer.View
{
    /// <summary>
    /// Interaction logic for QRDialog.xaml
    /// </summary>
    public partial class QRDialog : Window
    {
        public QRDialog()
        {
            InitializeComponent();
        }

        private void Btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        private bool IsWindowCaptionBar()
        {
            Point pp = Mouse.GetPosition(this);
            if (pp.Y >= 0 && pp.Y <= 40)
            {
                return true;
            }
            return false;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.LeftButton == MouseButtonState.Pressed) && (WindowState == WindowState.Normal) && IsWindowCaptionBar())
            {
                this.DragMove();
            }
        }
        public void SetBitmap(System.Drawing.Bitmap bitmap)
        {
            qrImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero,
                Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
