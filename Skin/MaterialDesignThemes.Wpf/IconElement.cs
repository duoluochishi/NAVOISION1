using System;
using System.Windows.Media;
using System.Windows;

namespace MaterialDesignThemes.Wpf
{
    public class IconElement
    {
        public static readonly DependencyProperty BtnPngPathProperty = DependencyProperty.RegisterAttached(
         "BtnPngPath", typeof(String), typeof(IconElement), new PropertyMetadata(default(String)));

        public static void SetBtnPngPath(DependencyObject element, String value)
            => element.SetValue(BtnPngPathProperty, value);

        public static String GetBtnPngPath(DependencyObject element)
            => (String)element.GetValue(BtnPngPathProperty);

        public static readonly DependencyProperty GeometryProperty = DependencyProperty.RegisterAttached(
         "Geometry", typeof(Geometry), typeof(IconElement), new PropertyMetadata(default(Geometry)));

        public static void SetGeometry(DependencyObject element, Geometry value)
            => element.SetValue(GeometryProperty, value);

        public static Geometry GetGeometry(DependencyObject element)
            => (Geometry)element.GetValue(GeometryProperty);


        public static readonly DependencyProperty BtnIconColorProperty = DependencyProperty.RegisterAttached(
         "BtnIconColor", typeof(Brush), typeof(IconElement), new PropertyMetadata(default(Brush)));

        public static void SetBtnIconColor(DependencyObject element, Brush value)
            => element.SetValue(BtnIconColorProperty, value);

        public static Brush GetBtnIconColor(DependencyObject element)
            => (Brush)element.GetValue(BtnIconColorProperty);

        public static readonly DependencyProperty MouseOverColorProperty = DependencyProperty.RegisterAttached(
        "MouseOverColor", typeof(Brush), typeof(IconElement), new PropertyMetadata(default(Brush)));

        public static void SetMouseOverColor(DependencyObject element, Brush value)
            => element.SetValue(MouseOverColorProperty, value);

        public static Brush GetMouseOverColor(DependencyObject element)
            => (Brush)element.GetValue(MouseOverColorProperty);

        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached(
            "Width", typeof(double), typeof(IconElement), new PropertyMetadata(double.NaN));

        public static void SetWidth(DependencyObject element, double value)
            => element.SetValue(WidthProperty, value);

        public static double GetWidth(DependencyObject element)
            => (double)element.GetValue(WidthProperty);

        public static readonly DependencyProperty HeightProperty = DependencyProperty.RegisterAttached(
            "Height", typeof(double), typeof(IconElement), new PropertyMetadata(double.NaN));

        public static void SetHeight(DependencyObject element, double value)
            => element.SetValue(HeightProperty, value);

        public static double GetHeight(DependencyObject element)
            => (double)element.GetValue(HeightProperty);
    }
}
