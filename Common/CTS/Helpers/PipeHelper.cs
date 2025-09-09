using System.Diagnostics;
using System.IO.Pipes;

namespace NV.CT.CTS.Helpers;

public static class PipeHelper
{
    public static void ClientStream(string serverHandle, int data)
    {
        using var pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, serverHandle);
        using var writer = new StreamWriter(pipeClient);
        writer.AutoFlush = true;
        writer.WriteLine(data);
    }

    public static (IntPtr,Process) ServerStream(Action<Process> action, string executableApplication)
    {
        using var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
        var process = new Process {
            StartInfo = new ProcessStartInfo
            {
                FileName = executableApplication,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = $"{pipeServer.GetClientHandleAsString()}"
            }
        };
        process.Start();
        action?.Invoke(process);
        pipeServer.DisposeLocalCopyOfClientHandle();
        using var reader = new StreamReader(pipeServer);
        string? readString = reader.ReadLine();
        int readInt = 0;
        IntPtr hwnd = IntPtr.Zero;
        if (int.TryParse(readString, out readInt))
        {
            hwnd = new IntPtr(readInt);
        }
        return (hwnd, process);
    }

    public static (IntPtr, Process) ServerStream(Action<Process> action, string executableApplication, IntPtr hwndMainWindow, string parameters)
    {
        try
        {
            using var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executableApplication,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = $"{pipeServer.GetClientHandleAsString()} {hwndMainWindow.ToInt32()} {parameters}"
                },
                EnableRaisingEvents = true
            };
            if (process.Start())
            {
                //获取process相关信息，并验证
                //验证当前进程已启动，process
                //进程启动成功的处理
            }
            else
            {
                //handle process 启动失败，重试。
                //3次重试后抛出系统级错误，无法以指定参数启动进程。
            }
            action?.Invoke(process);
            pipeServer.DisposeLocalCopyOfClientHandle();
            using var reader = new StreamReader(pipeServer);
            string? readString = reader.ReadLine();
            int readInt = 0;
            IntPtr hwnd = IntPtr.Zero;
            if(int.TryParse(readString, out readInt))
            {
                hwnd = new IntPtr(readInt);
            }
            //var hwnd = new IntPtr(readInt);
            StartMonitorDump(process);
            return (hwnd, process);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private static void StartMonitorDump(Process process)
    {
        //todo:ProcDump不可用于商业
        //var dumpProcess = new Process
        //{
        //    StartInfo = new ProcessStartInfo
        //    {
        //        FileName = Path.Combine(RuntimeConfig.Instance.Bin, "procdump64.exe"),
        //        UseShellExecute = false,
        //        CreateNoWindow = true,
        //        Arguments = $"-ma -t {process.Id}"
        //    }
        //};
        //dumpProcess.Start();
    }
}
