using System.Windows;
using CommandLine;

namespace NV.CT.NP.Tools.FileValidator;

public class App : Application
{
	[STAThread]
	public static void Main(string[] args)
	{
		if (args.Length == 0)
		{
			var app = new App();
			app.Run(new MainWindow());
		}
		else
		{
			// 解析命令行参数
			var exitCode = Parser.Default.ParseArguments<CheckCommand, ValidateCommand>(args)
				.MapResult(
					(CheckCommand cmd) => RunCheckCommand(cmd),
					(ValidateCommand cmd) => RunValidateCommand(cmd),
					errs =>
					{
						Console.Error.WriteLine("无效的命令或参数");
						return 1;
					});
			Environment.Exit(exitCode); // ✅ 明确退出
		}
	}

	// 处理 Check 命令
	private static int RunCheckCommand(CheckCommand cmd)
	{
		cmd.Execute();
		return 0;
	}

	// 处理 Validate 命令
	private static int RunValidateCommand(ValidateCommand cmd)
	{
		cmd.Execute();
		return 0;
	}
}