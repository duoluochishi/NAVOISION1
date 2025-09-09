using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace NV.CT.Screenshot;

public class ScreenshotOptions
{
    public ScreenshotOptions()
    {
        BackgroundOpacity = 0.5;
        SelectionRectangleBorderBrush = Brushes.LimeGreen;
    }

    public double BackgroundOpacity { get; set; }

    public Brush SelectionRectangleBorderBrush { get; set; }

}