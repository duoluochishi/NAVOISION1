using CommandLine;

namespace NV.CT.NP.Tools.FileValidator;

/*
	生成 MD5 校验文件
	NV.CT.NP.Tools.FileValidator.exe check --dir "D:\work\mcs20.code\bin" --save "F:\SiteData\FileValidation\checksum.xml"
   
	验证 MD5 校验文件
	NV.CT.NP.Tools.FileValidator.exe validate --dir "D:\work\mcs20.code\bin" --save "F:\SiteData\FileValidation\checksum.xml" --validation "F:\SiteData\FileValidation\validation.xml"
   
 */

[Verb("check", HelpText = "生成 MD5 校验文件")]
public class CheckCommand
{
	[Option('d', "dir", Required = true, HelpText = "待验证的目录路径")]
	public string DirectoryPath { get; set; }

	[Option('s', "save", Required = true, HelpText = "保存结果的路径")]
	public string SavePath { get; set; }

	public void Execute()
	{
		FileValidationHelper.GenerateMD5CheckResult(DirectoryPath, SavePath);
		
		//var result = FileValidationHelper.GenerateMD5CheckResult(DirectoryPath, SavePath);
		//var resultStr = result ? "成功" : "失败";
		//MessageBox.Show($"验证文件生成完成，结果：{resultStr}, 保存路径：{SavePath}");
	}
}

[Verb("validate", HelpText = "验证 MD5 校验文件")]
public class ValidateCommand
{
	[Option('d', "dir", Required = true, HelpText = "待验证的目录路径")]
	public string DirectoryPath { get; set; }

	[Option('s', "save", Required = true, HelpText = "保存结果的路径")]
	public string SavePath { get; set; }

	[Option('v', "validation", Required = true, HelpText = "验证文件路径")]
	public string ValidationPath { get; set; }

	public void Execute()
	{
		FileValidationHelper.ValidateMD5CheckListAndSaveResult(DirectoryPath, SavePath, ValidationPath);

		//var result = FileValidationHelper.ValidateMD5CheckListAndSaveResult(DirectoryPath, SavePath, ValidationPath);
		//var resultStr = result ? "成功" : "失败";
		//MessageBox.Show($"验证完成，结果：{resultStr}, 结果文件路径：{ValidationPath}");
	}
}