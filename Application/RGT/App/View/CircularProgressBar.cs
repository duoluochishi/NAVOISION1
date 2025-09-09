using System.Windows.Media.Animation;

namespace NV.CT.RGT.View;

public class CircularProgressBar : ProgressBar
{
    public CircularProgressBar()
    {
        ValueChanged += CircularProgressBar_ValueChanged;
    }

    void CircularProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var bar = sender as CircularProgressBar;
        double currentAngle = bar.Angle;
        double targetAngle = e.NewValue / bar.Maximum * 359.999;
        DoubleAnimation anim = new DoubleAnimation(currentAngle, targetAngle, TimeSpan.FromMilliseconds(500));
        bar.BeginAnimation(AngleProperty, anim, HandoffBehavior.SnapshotAndReplace);
    }

    public double Angle
    {
        get => (double)GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    public static readonly DependencyProperty AngleProperty =
        DependencyProperty.Register(nameof(Angle), typeof(double), typeof(CircularProgressBar), new PropertyMetadata(0.0));

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(CircularProgressBar), new PropertyMetadata(10.0));

    public double BrushStrokeThickness
    {
        get => (double)GetValue(BrushStrokeThicknessProperty);
        set => SetValue(BrushStrokeThicknessProperty, value);
    }

    public static readonly DependencyProperty BrushStrokeThicknessProperty =
        DependencyProperty.Register(nameof(BrushStrokeThickness), typeof(double), typeof(CircularProgressBar), new PropertyMetadata(1.0));

}
