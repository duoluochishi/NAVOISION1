using System.Windows;

namespace NV.CT.NP.Tools.FileValidator;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}

	private void BtnCheck_OnClick(object sender, RoutedEventArgs e)
	{
		FileValidationHelper.GenerateMD5CheckResult(DirPath.Text, SavePath.Text);
		//var result = FileValidationHelper.GenerateMD5CheckResult(DirPath.Text, SavePath.Text);
		//var resultStr = result ? "成功" : "失败";
		//MessageBox.Show($"验证文件成成完成，生成结果：{resultStr},生成文件路径：{SavePath.Text}");
	}

	private void BtnValidate_OnClick(object sender, RoutedEventArgs e)
	{
		FileValidationHelper.ValidateMD5CheckListAndSaveResult(DirPath.Text, SavePath.Text, ValidationPath.Text);

		//var result = FileValidationHelper.ValidateMD5CheckListAndSaveResult(DirPath.Text, SavePath.Text, ValidationPath.Text);
		//var resultStr = result ? "成功" : "失败";
		//MessageBox.Show($"验证完成，验证结果：{resultStr},结果文件路径：{ValidationPath.Text}");
	}
}