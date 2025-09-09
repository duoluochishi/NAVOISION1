using System.IO;
using System.Windows;

namespace MaterialDesignThemes.Wpf
{
    public static class ButtonAssist
    {
        private static readonly CornerRadius DefaultCornerRadius = new CornerRadius(2.0);

        private static object obj = new object();

        #region AttachedProperty : CornerRadiusProperty
        /// <summary>
        /// Controls the corner radius of the surrounding box.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty
            = DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(ButtonAssist), new PropertyMetadata(DefaultCornerRadius));

        public static CornerRadius GetCornerRadius(DependencyObject element) => (CornerRadius)element.GetValue(CornerRadiusProperty);
        public static void SetCornerRadius(DependencyObject element, CornerRadius value) => element.SetValue(CornerRadiusProperty, value);
        #endregion

        #region 控件IsEnable 属性,用于重写IsEnable样式
        public static bool GetIsEnable(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnableProperty);
        }

        public static void SetIsEnable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnableProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsEnable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnableProperty =
            DependencyProperty.RegisterAttached("IsEnable", typeof(bool), typeof(ButtonAssist), new PropertyMetadata(true, IsEnablePropertyChanged));

        private static void IsEnablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement uIElement)
            {
                var adorner = (UnableAdorner)uIElement.GetOrAddAdorner(typeof(UnableAdorner));
                if (adorner != null)
                {
                    adorner.SetEnable((bool)e.NewValue);
                }
                //else
                //{
                //    lock (obj)
                //    {
                //        var content = $"uielement is {uIElement.GetType().FullName} , uid is  {uIElement.Uid}";
                //        File.AppendAllText(@"D:\MaterialDesignThemes.Wpf.log", content);
                //    }
                //}
            }
        }
        #endregion
    }
}