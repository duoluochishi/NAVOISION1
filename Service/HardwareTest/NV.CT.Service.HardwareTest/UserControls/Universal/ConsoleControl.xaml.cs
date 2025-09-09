using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NV.CT.Service.HardwareTest.UserControls.Universal
{
    /// <summary>
    /// ConsoleControl.xaml 的交互逻辑
    /// </summary>
    public partial class ConsoleControl
    {
        public static readonly DependencyProperty ConsoleMessageProperty = DependencyProperty.Register(nameof(ConsoleMessage), typeof(string), typeof(ConsoleControl), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty ClearCommandProperty = DependencyProperty.Register(nameof(ClearCommand), typeof(ICommand), typeof(ConsoleControl), new PropertyMetadata(null));

        public ConsoleControl()
        {
            InitializeComponent();
        }

        public string ConsoleMessage
        {
            get => (string)GetValue(ConsoleMessageProperty);
            set => SetValue(ConsoleMessageProperty, value);
        }

        public ICommand ClearCommand
        {
            get => (ICommand)GetValue(ClearCommandProperty);
            set => SetValue(ClearCommandProperty, value);
        }

        private void ConsoleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConsoleTextBox.ScrollToEnd();
        }
    }
}