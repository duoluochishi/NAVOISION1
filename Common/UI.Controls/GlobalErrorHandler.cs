//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/6/30 13:13:27     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.MPS.Exception;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NV.CT.UI.Controls;

public static class GlobalErrorHandler
{
    public static void Handling(ILogger logger)
    {
        const int MaxTimes = 10;
        Dictionary<string, int> ExceptionDictionary = new Dictionary<string, int>();

        //UI线程未捕获异常处理事件
        Dispatcher.CurrentDispatcher.UnhandledException += (_, e) =>
        {
            try
            {
                //把 Handled 属性设为true，表示此异常已处理，程序可以继续运行，不会强制退出  
                e.Handled = true;
                logger.LogError($"[Dispatcher] {e.Exception.Message}:{e.Exception.StackTrace}");
            }
            catch (Exception ex)
            {
                //此时程序出现严重异常，将强制结束退出
                logger.LogError($"[Dispatcher] {ex.Message}:{ex.StackTrace}]");
            }
        };

        //Task线程内未捕获异常处理事件
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            //设置该异常已察觉（这样处理后就不会引起程序崩溃
            e.SetObserved();
            if (e.Exception.InnerException is not null)
            {
                logger.LogError($"[TaskScheduler] InnerException: {e.Exception.InnerException.Message} => {e.Exception.InnerException.StackTrace}]");
            }
            if (e.Exception.InnerExceptions is not null && e.Exception.InnerExceptions.Any())
            {
                var sbErrors = new StringBuilder();
                sbErrors.AppendLine("[TaskScheduler] InnerExceptions");
                foreach (var exception in e.Exception.InnerExceptions)
                {
                    sbErrors.AppendLine($"{exception.Message} => {exception.StackTrace}");
                }
                logger.LogError(sbErrors.ToString());
            }
            //logger.LogError($"[TaskScheduler] {e.Exception.Message}:{e.Exception.StackTrace}]");
        };

        //非UI线程未捕获异常处理事件
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var stringBuilder = new StringBuilder();
            if (e.ExceptionObject is Exception)
            {
                stringBuilder.Append(((Exception)e.ExceptionObject).Message);
            }
            else
            {
                stringBuilder.Append(e.ExceptionObject);
            }

            logger.LogError($"[AppDomain.UnhandledException] {(e.IsTerminating ? "Runtime terminated => " : "")} {stringBuilder}");
        };

        AppDomain.CurrentDomain.FirstChanceException += (_, e) =>
        {
            if (e.Exception is System.ObjectDisposedException tmpEx1)
            {
                var stackFrames = new StackTrace(tmpEx1).GetFrames() ?? [];
                var frames = stackFrames.Select(x => x.GetMethod()).Select(m => $"{m.DeclaringType?.FullName}.{m.Name}");
                logger.LogError($"[AppDomain.ObjectDisposedException] {tmpEx1?.ObjectName} => {tmpEx1?.Message}, {tmpEx1?.StackTrace}, {Environment.NewLine}{string.Join(Environment.NewLine, frames.ToList())}");
			}

            if (e.Exception is System.NotSupportedException tmpEx2)
            {
                var stackFrames = new StackTrace(tmpEx2).GetFrames() ?? [];
                var frames = stackFrames.Select(x => x.GetMethod()).Select(m => $"{m.DeclaringType?.FullName}.{m.Name}");
                logger.LogError($"[AppDomain.NotSupportedException] => {tmpEx2?.Message}, {tmpEx2?.StackTrace}, {Environment.NewLine}{string.Join(Environment.NewLine, frames.ToList())}");
			}

			var exceptSources = new string[] { "System.Net.Sockets", "Grpc.Net.Client", "Grpc.Core.RpcException", "Grpc.Core" };
            if (exceptSources.Contains(e.Exception.Source)
                || e.Exception.Message.Contains("StatusCode=\"Unavailable\"")
                || e.Exception.Message.Contains("StatusCode=\"Unknown\"")
                || e.Exception.Message.Contains("A task was canceled")
                || (e.Exception.Message.Contains("Could not load file or assembly") && e.Exception.Message.Contains(".XmlSerializers"))
                || (e.Exception.Message.Contains("BAML") && e.Exception.ToString().Contains("System.Windows.Markup.XamlParseException"))
                || ((e.Exception.StackTrace is not null) && (e.Exception.StackTrace.Contains("at System.Windows.Media.Imaging.BitmapImage.get_Metadata()"))))
            {
                return;
            }



            if (e.Exception is NanoException)
            {
                var ex = e.Exception as NanoException;
                if (ex.ErrorCode.Contains("MCS"))
                {
                    var stackFrames = new StackTrace(e.Exception).GetFrames() ?? new StackFrame[0];
                    var frames = stackFrames.Select(x => x.GetMethod()).Select(m => $"{m.DeclaringType?.FullName}.{m.Name}");
                    logger.LogError($"[AppDomain.FirstChanceException] {ex.ErrorCode} => {ex.Message}, {ex.StackTrace}, {System.Environment.NewLine}{string.Join(System.Environment.NewLine, frames.ToList())}");
                }
            }
            else if ((e.Exception is System.IO.IOException || e.Exception is System.Net.Http.HttpRequestException)
                && (e.Exception.Message.Contains("The request was aborted.") || e.Exception.Message.Contains("An error occurred while sending the request.")))
            {
                //todo:待仔细研究修改，暂不处理
                //HttpRequestException (An error occurred while sending the request.)
                //IOException (The request was aborted.)
                //IOException (Unable to read data from the transport connection)
                //SocketException (10054)
                return;
            }
            else
            {

                var exception = e.Exception;
                var exceptionKey = e.Exception.GetType().FullName ?? "";
                try
                {
                    if (ExceptionDictionary.ContainsKey(exceptionKey) && ExceptionDictionary[exceptionKey] > MaxTimes)
                    {
                        return;
                    }

                    Task.Delay(10).Wait();

                    logger.LogError($"[AppDomain.FirstChanceException] ({exceptionKey}) {exception.Message} => {exception.StackTrace}");

                    if (ExceptionDictionary.ContainsKey(exceptionKey))
                    {
                        ExceptionDictionary[exceptionKey] += 1;
                    }
                    else
                    {
                        ExceptionDictionary[exceptionKey] = 1;
                    }
                }
                catch (Exception hex)
                {
                    logger.LogError($"[AppDomain.FirstChanceException] ({exceptionKey}) {hex.Message} => {hex.StackTrace}, {System.Environment.NewLine}{hex.ToString()}");
                }
            }
        };
    }
}
