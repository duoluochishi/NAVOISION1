using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NV.MPS.UI.Dialog
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:NV.MPS.UI.Dialog"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:NV.MPS.UI.Dialog;assembly=NV.MPS.UI.Dialog"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:BaseCustomWindow/>
    /// 作为自定义Window的基类，实现风格统一，并提供Close事件。
    /// 窗口内容由调用者自行实现，只需定义一个Window并继承BaseCustomWindow。
    /// </summary>
    [TemplatePart(Name = TitleGrid, Type = typeof(Grid))]
    public class BaseCustomWindow : Window
    {
        private const string TitleGrid = nameof(TitleGrid);
        private double _posX;
        private double _posY;
        public event Action? CloseWindowEvent;

        static BaseCustomWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseCustomWindow), new FrameworkPropertyMetadata(typeof(BaseCustomWindow)));
        }

        public BaseCustomWindow()
        {
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Activate();
            MouseDown += DialogWindow_MouseDown;
            MouseMove += DialogWindow_MouseMove;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template.FindName(TitleGrid, this) is Grid grid)
            {
                grid.MouseLeftButtonDown -= TitleGrid_MouseLeftButtonDown;
                grid.MouseLeftButtonDown += TitleGrid_MouseLeftButtonDown;
            }
        }

        private void TitleGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            CloseWindowEvent?.Invoke();
            Close();
        }

        private void DialogWindow_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(this);
            _posX = p.X; // private double posX is a class member
            _posY = p.Y; // private double posY is a class member
        }

        private void DialogWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //屏蔽弹窗之外的区域的点击事件
            if (_posX < 0 || _posX > Width || _posY < 0 || _posY > Height)
            {
                e.Handled = true;
            }
        }
    }
}