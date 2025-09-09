using NV.CT.Service.Common.TaskProcessor.Services;

namespace NV.CT.Service.Common.TaskProcessor.App
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var externalService = new ExampleExternalService();
            var  processor = new Processors.ServiceController(externalService);

            processor.StatusChanged += (s, e) =>
                Console.WriteLine($"Status changed to: {e.NewStatus}");
            processor.ProgressChanged += (s, e) =>
                Console.WriteLine($"Progress: {e.Progress}%");

            var startTask = processor.StartAsync();

            // 模拟用户停止
            await Task.Delay(3000);
            Console.WriteLine("Request to stop");
            await processor.StopAsync();

            bool result = await startTask;
            Console.WriteLine($"Final result: {result}");

            Console.Read();
        }
    }
}
