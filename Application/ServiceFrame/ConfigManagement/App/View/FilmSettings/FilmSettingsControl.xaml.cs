using NV.CT.ConfigManagement.ViewModel;
using NV.CT.ServiceFramework.Contract;
using NV.CT.UI.Controls.Panel;
using System.Threading;
using System.Windows.Media;

namespace NV.CT.ConfigManagement.View
{
    /// <summary>
    /// FilmSettingsControl.xaml 的交互逻辑
    /// </summary>
    public partial class FilmSettingsControl : UserControl, IServiceControl
    {        
        private bool _isMouseDown = false; //鼠标是否按下
        private bool _isResizing = false;  //是否正在改变大小        
        private Point _mouseDownPosition; //鼠标按下的位置
        private Thickness _mouseDownMargin; //鼠标按下控件的Margin
        private const int RESIZE_MIN_WIDTH = 30; //调整图片的最小宽度
        private const int RESIZE_MIN_HEIGHT = 30; //调整图片的最小高度

        public FilmSettingsControl()
        {
            InitializeComponent();
            DataContext = CTS.Global.ServiceProvider?.GetRequiredService<FilmSettingsViewModel>();
        }

        public string GetServiceAppID()
        {
            return string.Empty;
        }

        public string GetServiceAppName()
        {
            return string.Empty;
        }

        public string GetTipOnClosing()
        {
            return string.Empty;
        }

        private void ResizablePanel_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var resizablePanel = sender as ResizablePanel;
            //判断鼠标是否点击在调整大小手柄上
            if (resizablePanel.ResizeHandle.Contains(e.GetPosition(resizablePanel)))
            {
                this.Cursor = System.Windows.Input.Cursors.SizeNWSE;
                this._isResizing = true;
                this._isMouseDown = false;
                resizablePanel.CaptureMouse();
            }
            else
            {
                this._isMouseDown = true;
                this._isResizing = false;
                _mouseDownPosition = e.GetPosition(this);
                _mouseDownMargin = resizablePanel.Margin;
                resizablePanel.CaptureMouse();
            }
        }

        private void ResizablePanel_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var resizablePanel = sender as ResizablePanel;
            Point mousePos = e.GetPosition(resizablePanel);
            if (resizablePanel.ResizeHandle.Contains(mousePos))
            {
                this.Cursor = System.Windows.Input.Cursors.SizeNWSE;
            }
            else
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }

            if (_isMouseDown)
            {
                var pos = e.GetPosition(this);
                var dp = pos - _mouseDownPosition;
                resizablePanel.Margin = new Thickness(_mouseDownMargin.Left + dp.X, _mouseDownMargin.Top + dp.Y, _mouseDownMargin.Right - dp.X, _mouseDownMargin.Bottom - dp.Y);
            }
            else if (_isResizing)
            {
                // 调整大小逻辑
                if (mousePos.X >= RESIZE_MIN_WIDTH)
                {
                    resizablePanel.Width = mousePos.X;
                }
                if (mousePos.Y >= RESIZE_MIN_HEIGHT)
                {
                    resizablePanel.Height = mousePos.Y;
                }
                this.Cursor = System.Windows.Input.Cursors.SizeNWSE;
            }
        }

        private void ResizablePanel_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Cursor = System.Windows.Input.Cursors.Arrow;
            this._isResizing = false;
            this._isMouseDown = false;
            var resizablePanel = sender as ResizablePanel;
            resizablePanel.ReleaseMouseCapture();
        }

        private void ResizablePanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) 
        {
            if (e.ClickCount == 2)
            {
                var image = sender as Image;
                this.SetLogo(image.Tag.ToString());
            }
        }
        private void Button_SetLogo(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var button = sender as Button;
            this.SetLogo(button.Tag.ToString());
        }

        private void SetLogo(string sectionType)
        {
            _isMouseDown = false;
            _isResizing = false;
            var viewModel = this.DataContext as FilmSettingsViewModel;
            viewModel.SetLogo(sectionType);
        }

        private void Button_ClearLogo(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isMouseDown = false;
            _isResizing = false;

            var button = sender as Button;
            var viewModel = this.DataContext as FilmSettingsViewModel;
            viewModel.ClearLogo(button.Tag.ToString());
        }

        private void Button_Save(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.RefreshAcutalHeight();
        }

        private void RefreshAcutalHeight()
        {
            var viewModel = this.DataContext as FilmSettingsViewModel;
            viewModel.ActualHeaderHeight = (float)gridHeaderArea.ActualHeight;
            viewModel.ActualFooterHeight = (float)gridFooterArea.ActualHeight;
        }

        public Point GetRelativePosition(UIElement childElement, UIElement parentElement)
        {
            GeneralTransform childTransform = childElement.TransformToAncestor(parentElement);
            Point childPoint = childTransform.Transform(new Point(0, 0));
            return childPoint;
        }
    }
}
