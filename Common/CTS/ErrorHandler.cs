//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/3 15:48:39     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using System.Text;

namespace NV.CT.CTS;

/// <summary>
/// 此仅给服务端使用，无用户界面
/// </summary>
public static class ErrorHandler
{
    public static void Handling(ILogger logger)
    {
        //Task线程内未捕获异常处理事件
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            //设置该异常已察觉（这样处理后就不会引起程序崩溃
            e.SetObserved();
            if (e.Exception.InnerException is not null)
            {
                //todo:屏蔽网络连接异常，这里需要调查端口或服务应用是否启动
                if (e.Exception.InnerException.Message.Contains("Unavailable") && e.Exception.InnerException.Message.Contains("Error connecting to subchannel.")) return;

                logger.LogError($"[TaskScheduler] InnerException: {e.Exception.InnerException.Message} => {e.Exception.InnerException.StackTrace}]");
            }
            if (e.Exception.InnerExceptions is not null && e.Exception.InnerExceptions.Any())
            {
                var sbErrors = new StringBuilder();
                sbErrors.AppendLine("[TaskScheduler] InnerExceptions");
                foreach (var exception in e.Exception.InnerExceptions)
                {
                    if (exception.Message.Contains("Unavailable") && exception.Message.Contains("Error connecting to subchannel.")) continue;
                    sbErrors.AppendLine($"{exception.Message} => {exception.StackTrace}");
                }
                logger.LogError(sbErrors.ToString());
            }
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

            logger.LogError($"[AppDomain.UnhandledException] {(e.IsTerminating ? "Runtime terminated => ":"")} {stringBuilder}");
        };
    }
}
