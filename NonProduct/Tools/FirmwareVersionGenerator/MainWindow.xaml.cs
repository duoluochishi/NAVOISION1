using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace FirmwareVersionGenerator;

public partial class MainWindow : Window
{
	private DataTable _dataTable = new();
	private const string FIRMWARE_CONFIG_NAME = "FirmwareVersionConfig.xml";
	public MainWindow()
	{
		InitializeComponent();
	}

	// 粘贴按钮：解析剪贴板的 Excel 数据
	private void OnPasteClick(object sender, RoutedEventArgs e)
	{
		PasteLogic();
	}

	private void OnClearClick(object sender, RoutedEventArgs e)
	{
		DataGridExcel.ItemsSource = null;
	}

	private void OnWriteIntoConfigFile(object sender, RoutedEventArgs e)
	{
		var dict = ExtractDict();
		//return;
		UpdateXmlVersions(TxtFirmwareConfigFile.Text, dict);

		MessageBox.Show("write to config file success");
	}

	private void PasteLogic()
	{
		string clipboardText = Clipboard.GetText();

		if (string.IsNullOrWhiteSpace(clipboardText))
		{
			MessageBox.Show("剪贴板为空或不是表格数据");
			return;
		}

		string[] lines = clipboardText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		_dataTable = new DataTable();

		// 分析列数
		int columnCount = lines[0].Split('\t').Length;
		for (int i = 0; i < columnCount; i++)
		{
			_dataTable.Columns.Add($"列{i + 1}");
		}

		foreach (string line in lines)
		{
			var values = line.Split('\t');
			_dataTable.Rows.Add(values);
		}

		DataGridExcel.ItemsSource = _dataTable.DefaultView;
	}

	// 提取 Dictionary
	private void OnExtractDictionaryClick(object sender, RoutedEventArgs e)
	{
		var result = ExtractDict();

		// 显示结果
		ResultTextBox.Text = string.Join(Environment.NewLine, result.Select(kv => $"{kv.Key} => {kv.Value}"));
	}

	private Dictionary<string, string> ExtractDict()
	{
		var result = new Dictionary<string, string>();

		foreach (DataRow row in _dataTable.Rows)
		{
			if (_dataTable.Columns.Count >= 3)
			{
				string key = row[0]?.ToString()?.Trim();
				string value = row[2]?.ToString()?.Trim();

				if (!string.IsNullOrEmpty(key))
				{
					result[key] = value ?? string.Empty;
				}
			}
		}

		return result;
	}

	private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
		{
			if (e.Key == Key.V)
			{
				// 获取当前拥有焦点的元素
				var focusedElement = Keyboard.FocusedElement;

				// 如果是 TextBox 或其派生类，就跳过处理（允许默认粘贴）
				if (focusedElement is TextBox || focusedElement is PasswordBox || focusedElement is RichTextBox)
				{
					// 不处理，让 TextBox 默认处理 Ctrl+V
					return;
				}

				e.Handled = true;

				PasteLogic();
			}
		}
	}

	private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		DataGridExcel.Focus();
	}

	private void TxtFirmwareConfigFile_OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
		{
			if (e.Key == Key.V && Keyboard.FocusedElement is TextBox tb)
			{
				if (Clipboard.ContainsText())
				{
					e.Handled = true;
					string pastedText = Clipboard.GetText();

					var newText = "";
					if (!pastedText.Contains(FIRMWARE_CONFIG_NAME, StringComparison.OrdinalIgnoreCase))
					{
						newText = $"{pastedText}\\{FIRMWARE_CONFIG_NAME}";
					}

					// 插入自定义内容
					int start = tb.SelectionStart;
					int length = tb.SelectionLength;
					tb.Text = tb.Text.Remove(start, length).Insert(start, newText);
					tb.SelectionStart = start + newText.Length;
				}
			}
		}
	}

	public static void UpdateXmlVersions(string xmlPath, Dictionary<string, string> versionDict)
	{
		var doc = XDocument.Load(xmlPath);

		foreach (var item in doc.Descendants("FirmwareVersion"))
		{
			var typeAttr = item.Attribute("Type");
			var versionAttr = item.Attribute("Version");

			if (typeAttr != null && versionAttr != null)
			{
				var typeName = typeAttr.Value;
				if (versionDict.TryGetValue(typeName, out string newVersion))
				{
					if (newVersion.Contains("v", StringComparison.OrdinalIgnoreCase))
					{
						var findIndex=newVersion.IndexOf("v", StringComparison.OrdinalIgnoreCase);
						newVersion = newVersion.Remove(findIndex,1);
					}

					if (!newVersion.Equals(versionAttr.Value, StringComparison.OrdinalIgnoreCase))
					{
						versionAttr.Value = newVersion;
					}
				}
			}
		}

		doc.Save(xmlPath); // 保存覆盖原文件，可替换为另存
	}
}