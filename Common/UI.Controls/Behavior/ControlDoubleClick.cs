using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NV.CT.UI.Controls.Behavior
{

    public class ControlDoubleClick : DependencyObject
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(ControlDoubleClick),
                new PropertyMetadata(OnCommandChanged));

        public static ICommand GetCommand(Control target)
        {
            return (ICommand)target.GetValue(CommandProperty);
        }

        public static void SetCommand(Control target, ICommand value)
        {
            target.SetValue(CommandProperty, value);
        }

        private static void Element_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Control control)
            {
                var command = GetCommand(control);
                if (command.CanExecute(null))
                {
                    command.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as Control;
            if (control is not null)
                control.PreviewMouseDoubleClick += Element_PreviewMouseDoubleClick;
        }
    }
}
