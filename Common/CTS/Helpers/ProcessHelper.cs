using System.Diagnostics;
using System.Text;

namespace NV.CT.CTS.Helpers;

public class ProcessHelper
{
    public static bool Exist(string processName)
    {
        return Process.GetProcessesByName(processName).Any();
    }

    public static bool Execute(string processFile,string arguments,out string output,out string error)
    {
	    var process = new Process
	    {
		    StartInfo = new ProcessStartInfo
		    {
			    FileName = processFile,
			    UseShellExecute = false,
			    CreateNoWindow = true,
			    RedirectStandardOutput = true,
			    RedirectStandardError = true,
			    Arguments = $" {arguments}",
			    StandardOutputEncoding = Encoding.UTF8
		    }
	    };

	    process.Start();
	    // 捕获输出内容
	    output = process.StandardOutput.ReadToEnd();
	    error = process.StandardError.ReadToEnd();

	    process.WaitForExit();
	    // 如果 mysqldump 成功运行，将输出内容保存到文件
	    if (process.ExitCode == 0)
	    {
		    return true;
	    }
		return false;
	}

	/// <summary>
	/// 执行外部进程，支持输出捕获和超时控制。
	/// </summary>
	/// <param name="processFile">可执行文件路径</param>
	/// <param name="arguments">命令行参数</param>
	/// <param name="timeoutMilliseconds">超时时间（毫秒），默认 60 秒</param>
	/// <param name="output">标准输出内容</param>
	/// <param name="error">标准错误内容</param>
	/// <returns>执行成功返回 true，失败或超时返回 false</returns>
	public static bool ExecuteAsync(
		string processFile,
		string arguments,
		out string output,
		out string error,
		int timeoutMilliseconds = 60000)
	{
		var outputBuilder = new StringBuilder();
		var errorBuilder = new StringBuilder();

		using var process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = processFile,
				Arguments = arguments,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				StandardOutputEncoding = Encoding.UTF8
			},
			EnableRaisingEvents = true
		};

		process.OutputDataReceived += (_, e) =>
		{
			if (e.Data != null)
				outputBuilder.AppendLine(e.Data);
		};

		process.ErrorDataReceived += (_, e) =>
		{
			if (e.Data != null)
				errorBuilder.AppendLine(e.Data);
		};

		try
		{
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			// 等待退出或超时
			if (!process.WaitForExit(timeoutMilliseconds))
			{
				process.Kill();
				output = outputBuilder.ToString();
				error = "timeout";
				return false;
			}

			output = outputBuilder.ToString();
			error = errorBuilder.ToString();

			return process.ExitCode == 0;
		}
		catch (Exception ex)
		{
			output = outputBuilder.ToString();
			error = $"{ex.Message}";
			return false;
		}
	}
}