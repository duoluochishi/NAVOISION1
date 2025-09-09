using System.Reflection;

namespace NV.CT.Recon.Extensions;

public static class MethodTimeLogger
{
	public static void Log(MethodBase methodName, long milliseconds, string parameters)
	{
		var msg = $"[perf]: {methodName.Name} \t time : {milliseconds}ms , parameters: {parameters}";

		//// 输出到控制台
		//Console.WriteLine(msg);

		//// 输出到调试窗口
		//Debug.WriteLine(msg);

		// 可选：写入文件
		//File.AppendAllText(@"F:\performance_log.txt", msg + Environment.NewLine);
		CTS.Global.Logger.LogInformation(msg);
	}
}