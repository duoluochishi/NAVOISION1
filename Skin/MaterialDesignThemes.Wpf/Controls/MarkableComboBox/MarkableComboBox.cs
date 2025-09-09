using MaterialDesignThemes.Wpf.Controls.MarkableTextBox;
using System.Windows;
using System.Windows.Controls;

namespace MaterialDesignThemes.Wpf.Controls.MarkableComboBox
{
    public class MarkableComboBox : ComboBox, IMarkableControlBase
    {
        public MarkableComboBox()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public MarkControlStatus MarkStatus
        {
            get => (MarkControlStatus)GetValue(MarkStatusProperty);
            set => SetValue(MarkStatusProperty, value);
        }

        public static DependencyProperty MarkStatusProperty =
            DependencyProperty.Register(nameof(MarkStatus), typeof(MarkControlStatus), typeof(MarkableComboBox), new PropertyMetadata(MarkControlStatus.Default));

    }
}
