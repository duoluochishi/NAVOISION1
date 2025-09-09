//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/1/23 9:28:29           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace NV.CT.UI.Exam.View;

public class GraduatedScale : FrameworkElement
{
    bool _lastIsNotNumber = true;
    int _endPosition = 0;

    public Color BottomHorizontalLineColor
    {
        get { return (Color)GetValue(BottomHorizontalLineColorProperty); }
        set { SetValue(BottomHorizontalLineColorProperty, value); }
    }

    public static readonly DependencyProperty BottomHorizontalLineColorProperty =
        DependencyProperty.Register("BottomHorizontalLineColor", typeof(Color), typeof(GraduatedScale), new PropertyMetadata(Colors.White));

    public bool WhetherToDisplayTheEndScale
    {
        get { return (bool)GetValue(WhetherToDisplayTheEndScaleProperty); }
        set { SetValue(WhetherToDisplayTheEndScaleProperty, value); }
    }

    public static readonly DependencyProperty WhetherToDisplayTheEndScaleProperty =
        DependencyProperty.Register("WhetherToDisplayTheEndScale", typeof(bool), typeof(GraduatedScale), new PropertyMetadata(false));

    public double ScaleWidth
    {
        get { return (double)GetValue(ScaleWidthProperty); }
        set { SetValue(ScaleWidthProperty, value); }
    }

    public static readonly DependencyProperty ScaleWidthProperty =
        DependencyProperty.Register("ScaleWidth", typeof(double), typeof(GraduatedScale), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender), ValueIsNegativeNumber);

    public double ScaleHeight
    {
        get { return (double)GetValue(ScaleHeightProperty); }
        set { SetValue(ScaleHeightProperty, value); }
    }

    public static readonly DependencyProperty ScaleHeightProperty =
        DependencyProperty.Register("ScaleHeight", typeof(double), typeof(GraduatedScale), new FrameworkPropertyMetadata(52d, FrameworkPropertyMetadataOptions.AffectsRender), ValueIsNegativeNumber);

    public int FontSize
    {
        get { return (int)GetValue(FontSizeProperty); }
        set { SetValue(FontSizeProperty, value); }
    }

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register("FontSize", typeof(int), typeof(GraduatedScale), new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.AffectsRender), ValueIsNegativeNumber);

    public double TheHeightOfTheLowerScale
    {
        get { return (double)GetValue(TheHeightOfTheLowerScaleProperty); }
        set { SetValue(TheHeightOfTheLowerScaleProperty, value); }
    }

    public static readonly DependencyProperty TheHeightOfTheLowerScaleProperty =
        DependencyProperty.Register("TheHeightOfTheLowerScale", typeof(double), typeof(GraduatedScale), new FrameworkPropertyMetadata(10d, FrameworkPropertyMetadataOptions.AffectsRender), ValueIsNegativeNumber);

    public double TheHeightOfTheHigherScale
    {
        get { return (double)GetValue(TheHeightOfTheHigherScaleProperty); }
        set { SetValue(TheHeightOfTheHigherScaleProperty, value); }
    }

    public static readonly DependencyProperty TheHeightOfTheHigherScaleProperty =
        DependencyProperty.Register("TheHeightOfTheHigherScale", typeof(double), typeof(GraduatedScale), new FrameworkPropertyMetadata(15d, FrameworkPropertyMetadataOptions.AffectsRender), ValueIsNegativeNumber);

    public double RadiusY
    {
        get { return (double)GetValue(RadiusYProperty); }
        set { SetValue(RadiusYProperty, value); }
    }

    public static readonly DependencyProperty RadiusYProperty =
        DependencyProperty.Register("RadiusY", typeof(double), typeof(GraduatedScale), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender), ValueIsNegativeNumber);

    public double RadiusX
    {
        get { return (double)GetValue(RadiusXProperty); }
        set { SetValue(RadiusXProperty, value); }
    }

    public static readonly DependencyProperty RadiusXProperty =
        DependencyProperty.Register("Radioux", typeof(double), typeof(GraduatedScale), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender), ValueIsNegativeNumber);

    public Brush BorderColor
    {
        get { return (Brush)GetValue(BorderColorProperty); }
        set { SetValue(BorderColorProperty, value); }
    }

    public static readonly DependencyProperty BorderColorProperty =
        DependencyProperty.Register("BorderColor", typeof(Brush), typeof(GraduatedScale), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush BackGround
    {
        get { return (Brush)GetValue(BackGroundProperty); }
        set { SetValue(BackGroundProperty, value); }
    }

    public static readonly DependencyProperty BackGroundProperty =
        DependencyProperty.Register("BackGround", typeof(Brush), typeof(GraduatedScale), new FrameworkPropertyMetadata(Brushes.Pink, FrameworkPropertyMetadataOptions.AffectsRender));

    public int ConvertToScale
    {
        get { return (int)GetValue(ConvertToScaleProperty); }
        set { SetValue(ConvertToScaleProperty, value); }
    }

    public static readonly DependencyProperty ConvertToScaleProperty =
        DependencyProperty.Register("ConvertToScale", typeof(int), typeof(GraduatedScale), new FrameworkPropertyMetadata(5, FrameworkPropertyMetadataOptions.AffectsRender), ValueIsNegativeNumber);

    public int RightOffset
    {
        get { return (int)GetValue(RightOffsetProperty); }
        set { SetValue(RightOffsetProperty, value); }
    }

    public static readonly DependencyProperty RightOffsetProperty =
        DependencyProperty.Register("RightOffset", typeof(int), typeof(GraduatedScale), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush ColorOfNumbers
    {
        get { return (Brush)GetValue(ColorOfNumbersProperty); }
        set
        {
            SetValue(ColorOfNumbersProperty, value);
        }
    }

    public static readonly DependencyProperty ColorOfNumbersProperty =
        DependencyProperty.Register("ColorOfNumbers", typeof(Brush), typeof(GraduatedScale), new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

    public Color ColorOfScaleLine
    {
        get { return (Color)GetValue(ColorOfScaleLineProperty); }
        set { SetValue(ColorOfScaleLineProperty, value); }
    }

    public static readonly DependencyProperty ColorOfScaleLineProperty =
        DependencyProperty.Register("ColorOfScaleLine", typeof(Color), typeof(GraduatedScale), new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.AffectsRender));


    public double DigitalHeight
    {
        get { return (double)GetValue(DigitalHeightProperty); }
        set { SetValue(DigitalHeightProperty, value); }
    }

    public static readonly DependencyProperty DigitalHeightProperty =
        DependencyProperty.Register("DigitalHeight", typeof(double), typeof(GraduatedScale), new FrameworkPropertyMetadata(20d, FrameworkPropertyMetadataOptions.AffectsRender), ValueIsNegativeNumber);

    public int AFewLinesApartHigher
    {
        get { return (int)GetValue(AFewLinesApartHigherProperty); }
        set { SetValue(AFewLinesApartHigherProperty, value); }
    }

    public static readonly DependencyProperty AFewLinesApartHigherProperty =
        DependencyProperty.Register("AFewLinesApartHigher", typeof(int), typeof(GraduatedScale), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender), ValueIsNegativeNumber);

    public int FigureBlank
    {
        get { return (int)GetValue(FigureBlnkProperty); }
        set { SetValue(FigureBlnkProperty, value); }
    }

    public static readonly DependencyProperty FigureBlnkProperty =
        DependencyProperty.Register("FigureBlank", typeof(int), typeof(GraduatedScale), new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.AffectsRender), ValueIsNegativeNumber);

    public int ScaleInterval
    {
        get { return (int)GetValue(ScaleIntervalProperty); }
        set { SetValue(ScaleIntervalProperty, value); }
    }

    public static readonly DependencyProperty ScaleIntervalProperty =
        DependencyProperty.Register("ScaleInterval", typeof(int), typeof(GraduatedScale), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender));

    public double HeightOfSale
    {
        get { return (double)GetValue(HeightOfSaleProperty); }
        set { SetValue(HeightOfSaleProperty, value); }
    }

    public static readonly DependencyProperty HeightOfSaleProperty =
        DependencyProperty.Register("HeightOfSale", typeof(double), typeof(GraduatedScale), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender), ValueIsNegativeNumber);

    public double StartValue
    {
        get { return (double)GetValue(StartValueProperty); }
        set { SetValue(StartValueProperty, value); }
    }

    public static readonly DependencyProperty StartValueProperty =
        DependencyProperty.Register("StartValue", typeof(double), typeof(GraduatedScale), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public double EndValue
    {
        get { return (double)GetValue(EndValueProperty); }
        set { SetValue(EndValueProperty, value); }
    }

    public static readonly DependencyProperty EndValueProperty =
        DependencyProperty.Register("EndValue", typeof(double), typeof(GraduatedScale), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public double FontHeight
    {
        get { return (double)GetValue(FontHeightProperty); }
        set { SetValue(FontHeightProperty, value); }
    }

    public static readonly DependencyProperty FontHeightProperty =
        DependencyProperty.Register("FontHeight", typeof(double), typeof(GraduatedScale), new FrameworkPropertyMetadata(-30d, FrameworkPropertyMetadataOptions.AffectsRender));

    public double RectangleHeight
    {
        get { return (double)GetValue(RectangleHeightProperty); }
        set { SetValue(RectangleHeightProperty, value); }
    }

    public static readonly DependencyProperty RectangleHeightProperty =
        DependencyProperty.Register("RectangleHeight", typeof(double), typeof(GraduatedScale), new FrameworkPropertyMetadata(30d, FrameworkPropertyMetadataOptions.AffectsRender));

    protected override void OnRender(DrawingContext drawingContext)
    {
        _lastIsNotNumber = (EndValue - StartValue) % FigureBlank != 0;

        if (StartValue > EndValue)
        {
            return;
        }
        GetEndPosition();
        for (int i = 0; i <= (int)((EndValue - StartValue) / FigureBlank); i++)
        {
            var numberText = new FormattedText(
                (StartValue + i * FigureBlank).ToString(),
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface("Microsoft SongTi"),
                FontSize, ColorOfNumbers,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            if (EndValue - (StartValue + i * FigureBlank) >= 2 || EndValue - (StartValue + i * FigureBlank) == 0)
            {
                drawingContext.DrawText(
                    numberText,
                    new Point((FigureBlank * i + RightOffset) * ConvertToScale - 2, ScaleHeight - DigitalHeight - FontSize - FontHeight));
            }
            drawingContext.DrawLine(
                new Pen(
                    new SolidColorBrush(ColorOfScaleLine), 1),
                    new Point((FigureBlank * i + RightOffset) * ConvertToScale, ScaleHeight - DigitalHeight - FontSize + 15),
                    new Point((FigureBlank * i + RightOffset) * ConvertToScale, ScaleHeight - 2));
        }

        var StartY = ScaleHeight - 2;

        for (int i = 0; i <= (int)((EndValue - StartValue) / ScaleInterval); i++)
        {
            if (i * ScaleInterval % FigureBlank != 0)
            {
                if (i % (AFewLinesApartHigher + 1) == 0)
                {
                    drawingContext.DrawLine(
                        new Pen(
                            new SolidColorBrush(ColorOfScaleLine), 1),
                            new Point((ScaleInterval * i + RightOffset) * ConvertToScale, StartY),
                            new Point((ScaleInterval * i + RightOffset) * ConvertToScale, ScaleHeight - TheHeightOfTheHigherScale));
                }
                else
                {
                    drawingContext.DrawLine(
                        new Pen(
                            new SolidColorBrush(ColorOfScaleLine), 1),
                            new Point((ScaleInterval * i + RightOffset) * ConvertToScale, StartY),
                            new Point((ScaleInterval * i + RightOffset) * ConvertToScale, ScaleHeight - TheHeightOfTheLowerScale));
                }
            }
        }

        if (WhetherToDisplayTheEndScale && _lastIsNotNumber)
        {
            drawingContext.DrawText(
                new FormattedText(
                    Math.Round(EndValue, 1).ToString(),
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Microsoft SongTi"),
                    FontSize, ColorOfNumbers,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip),
                new Point((EndValue - StartValue + RightOffset) * ConvertToScale - 10, ScaleHeight - DigitalHeight - FontSize - FontHeight));

            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(ColorOfScaleLine), 1),
                new Point((EndValue - StartValue + RightOffset) * ConvertToScale, StartY),
                new Point((EndValue - StartValue + RightOffset) * ConvertToScale, ScaleHeight - TheHeightOfTheLowerScale));
        }

        drawingContext.DrawLine(
                new Pen(new SolidColorBrush(BottomHorizontalLineColor), 1),
                new Point(RightOffset * ConvertToScale, ScaleHeight - DigitalHeight - FontSize - FontHeight - 2),
                new Point((EndValue - StartValue + RightOffset) * ConvertToScale, ScaleHeight - DigitalHeight - FontSize - FontHeight - 2));
    }

    static bool ValueIsNegativeNumber(object value)
    {
        if (value.GetType() == typeof(double))
        {
            return (double)value >= 0;
        }
        else
        {
            return (int)value >= 0;
        }
    }

    private void GetEndPosition()
    {
        int temp = 0;
        for (int i = 10; i < int.MaxValue; i *= 10)
        {
            if (EndValue * ConvertToScale / i * (FontSize / 10) <= 1)
            {
                _endPosition = temp;
                return;
            }
            else
            {
                temp++;
            }
        }
    }
}