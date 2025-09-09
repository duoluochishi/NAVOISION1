using Microsoft.Xaml.Behaviors;
using System.Windows.Input;

namespace NV.CT.UI.Exam.Extensions;

public class DragMoveBehavior : Behavior<UIElement>
{
    Canvas? parent;
    bool isDown;
    System.Windows.Point prePosition;
    System.Windows.Point initPosition;
    private ISmartPositioningService? _smartPositioningService;
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.PreviewMouseLeftButtonDown += MouseLeftButtonDown;
        AssociatedObject.PreviewMouseLeftButtonUp += MouseLeftButtonUp;
        AssociatedObject.MouseLeave += MouseLeave;

        if (LogicalTreeHelper.GetParent(AssociatedObject) is Canvas canvas)
        {
            parent = canvas;
            parent.PreviewMouseMove += MouseMove;
            if (_smartPositioningService is null)
            {
                _smartPositioningService = Global.ServiceProvider.GetRequiredService<ISmartPositioningService>();
            }
        }
    }

    private void MouseMove(object sender, MouseEventArgs e)
    {
        if (!isDown)
        {
            return;
        }
        var currentPosition = GetPosition(e);

        //设置控件距离左侧的位置   
        double offsetx = currentPosition.X - prePosition.X;
        double left = Canvas.GetLeft(AssociatedObject);
        double length = double.IsNaN(left) ? 0 : left + offsetx;
        if (length >= 0 && length <= parent?.ActualWidth - AssociatedObject.RenderSize.Width)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Canvas.SetLeft(AssociatedObject, length);
            });
        }

        //控制控件距离顶部的位子
        double offsety = currentPosition.Y - prePosition.Y;
        double top = Canvas.GetTop(AssociatedObject);
        double topLength = double.IsNaN(top) ? 0 : top + offsety;
        if (topLength >= 0 && topLength <= parent?.ActualHeight - AssociatedObject.RenderSize.Height)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Canvas.SetTop(AssociatedObject, topLength);
            });
        }
        prePosition = currentPosition;

        //移动range线触发range值变更
        double position = Canvas.GetLeft(AssociatedObject);
        if (AssociatedObject is StackPanel control && _smartPositioningService is not null)
        {
            if (control.Name.Contains("RangeStartControl"))
            {
                _smartPositioningService.GetRangePotition(PotisionStartEndLine.StartLine, position);
            }
            if (control.Name.Contains("RangeEndControl"))
            {
                _smartPositioningService.GetRangePotition(PotisionStartEndLine.EndLine, position);
            }
        }
    }

    private void MouseLeave(object sender, MouseEventArgs e)
    {
        isDown = false;
        AssociatedObject.ReleaseMouseCapture();
    }

    private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        AssociatedObject.ReleaseMouseCapture();
        if (_smartPositioningService is not null)
        {
            _smartPositioningService.IsManual = false;
            isDown = false;
            if (AssociatedObject is StackPanel control && _smartPositioningService is not null)
            {
                var moveDistanceX = GetPosition(e).X - initPosition.X;
                if (control.Name.Contains("RangeStartControl"))
                {
                    _smartPositioningService.SetRangePotitionChanged(PotisionStartEndLine.StartLine, moveDistanceX);
                }

                if (control.Name.Contains("RangeEndControl"))
                {
                    _smartPositioningService.SetRangePotitionChanged(PotisionStartEndLine.EndLine, moveDistanceX);
                }
            }
        }
    }

    private void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (isDown)
        {
            return;
        }
        if (_smartPositioningService is not null)
        {
            _smartPositioningService.IsManual = true;
        }
        isDown = true;
        prePosition = GetPosition(e);
        initPosition = GetPosition(e);
        AssociatedObject.CaptureMouse();
    }

    System.Windows.Point GetPosition(MouseEventArgs e)
    {
        return e.GetPosition(parent);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewMouseLeftButtonDown -= MouseLeftButtonDown;
        AssociatedObject.PreviewMouseLeftButtonUp -= MouseLeftButtonUp;
        AssociatedObject.MouseLeave += MouseLeave;
        if (parent is not null)
        {
            parent.PreviewMouseMove -= MouseMove;
        }
    }
}