using NV.CT.Service.Common.TaskProcessor.Services;
using System.Windows;
using System.Windows.Controls;

namespace TaskProcessor.WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ExampleExternalService externalService;
        NV.CT.Service.Common.TaskProcessor.Processors.ServiceController processor;
        Task<bool> startTask;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            this.btnStart.IsEnabled = false;

            _ = Task.Factory.StartNew(async () =>
            {
                externalService = new ExampleExternalService();
                processor = new NV.CT.Service.Common.TaskProcessor.Processors.ServiceController(externalService);

                processor.StatusChanged += (s, e) =>
                    Console.WriteLine($"[Client] Received the status changed to: {e.NewStatus}");
                processor.ProgressChanged += (s, e) =>
                    Console.WriteLine($"[Client] Received the progress: {e.Progress}%");

                Console.WriteLine("[Client] Request to start");
                _ = await processor.StartAsync();

                //bool result = await startTask;
                Console.WriteLine($"[Client] Ended for Request to start");
                ResetCommandEnabled();
            });
            //Console.Read();
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.btnStop.IsEnabled = false;

            _ = Task.Factory.StartNew(async () =>
            {
                // 模拟用户停止
                //Task.Delay(3000).Wait();
                Console.WriteLine("[Client] Request to stop");
                await processor.StopAsync();
                //Console.WriteLine($"Final result: {result} for Request to stop");

                Console.WriteLine($"[Client] Ended for Request to start");
                ResetCommandEnabled();
                //Console.Read();
            });
        }

        private void ResetCommandEnabled()
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.btnStop.IsEnabled = true;
                this.btnStart.IsEnabled = true;
            }));
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Title = this.outputText.Text;
        }
    }
}