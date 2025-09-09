using ConfigurationTool.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Runtime;
using System.Windows;

namespace ConfigurationTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string[] _configs = new string[] { "NV.MPS.Configuration.SystemConfig", "NV.MPS.Configuration.UserConfig", "NV.MPS.Configuration.ProductConfig" };
        private string _destPath = AppDomain.CurrentDomain.BaseDirectory;
        private SettingsModel _settings;

        public MainWindow()
        {
            InitializeComponent();
            InitProperties();
        }

        private void InitProperties()
        {
            string settingsPath = System.IO.Path.Combine(_destPath, "Assets") + "\\appsettings.json";
            if (File.Exists(settingsPath))
            {
                _settings = JsonConvert.DeserializeObject<SettingsModel>(File.ReadAllText(settingsPath));
                txt_AssemblyPath.Text = _settings?.ConfigurationAssemblyPath;
                txt_StorePath.Text = _settings?.NewConfigFolder;
                txt_BasePath.Text = _settings?.BaseConfigFolder;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var assemblyPath = txt_AssemblyPath.Text.Trim();
            var storePath = txt_StorePath.Text.Trim();
            
            if (string.IsNullOrEmpty(assemblyPath) || !File.Exists(assemblyPath))
            {
                MessageBox.Show("Please input the AssemblyPath, \r\n eg: D:\\CodeHere\\mcs20.code\\bin\\NV.MPS.Configuration.dll");
                return;
            }

            if (string.IsNullOrEmpty(storePath))
            {
                MessageBox.Show("Please input your destination path!");
                return;
            }

            foreach (var typeName in _configs)
            {
                try
                {
                    var config = ConfigAssemblyReflectionHelper.GetConfigurationAsJson(assemblyPath, typeName);
                    string filePath = System.IO.Path.Combine(storePath, typeName + ".json");

                    if (File.Exists(filePath))
                    {
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to delete file: {filePath}, Exception :{ex.Message}, \r\n Please delete the file manually and then continue!");
                        }
                    }

                    string directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    try
                    {
                        File.WriteAllText(filePath, config, System.Text.Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Saving configuration failed: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Converting configuration failed: {ex.Message}");
                    return;
                }
            }

            MessageBox.Show("Convert configuration successfully");
            SaveAppSetings();
        }

        private void ComprareButton_Click(object sender, RoutedEventArgs e)
        {
            string baseConfigPath = txt_BasePath.Text.Trim();//@"C:\Users\DELL\Desktop\Test\Config_v01";
            string newConfigPath = txt_StorePath.Text.Trim();//@"C:\Users\DELL\Desktop\Test\Config_v02";
            string diffString = string.Empty;

            if (string.IsNullOrEmpty(baseConfigPath) || string.IsNullOrEmpty(newConfigPath))
            {
                MessageBox.Show("Please input the correct Path");
                return;
            }

            foreach (var name in _configs)
            {
                diffString += $"{name}:\r\n";
                string jsonPath1 = Path.Combine(baseConfigPath, name) + ".json";
                string jsonPath2 = Path.Combine(newConfigPath, name) + ".json";
                if(!File.Exists(jsonPath1) || !File.Exists(jsonPath2))
                {
                    MessageBox.Show("Please make sure the configuration file exists in the folder!");
                    return;
                }

                string json1 = File.ReadAllText(jsonPath1);
                string json2 = File.ReadAllText(jsonPath2);

                //If no changes, skip
                if (json1.GetHashCode() == json2.GetHashCode())
                {
                    diffString += "No changes\r\n";
                    continue;
                }

                // Compare json
                JToken token1 = JToken.Parse(json1);
                JToken token2 = JToken.Parse(json2);

                var differences = new List<string>();
                JsonComparer jsonComparer = new JsonComparer();
                jsonComparer.CompareJson(token1, token2, "", differences);
                if (differences.Count > 0)
                    differences.ForEach(x => diffString += $"{x}\r\n");
                else
                    diffString += "No changes\r\n";
            }
            MessageWindow messageWindow = new MessageWindow();
            messageWindow.SetMessage(diffString);
            messageWindow.Owner = this;
            if (messageWindow.ShowDialog() == true)
            {
                if (!File.Exists(_settings.BeyondCompareFullPath))
                    _settings.BeyondCompareFullPath = $"{Path.Combine(_destPath, "Assets", "Beyond Compare 5")}\\BCompare.exe";
                if (!File.Exists(_settings.BeyondCompareFullPath))
                {
                    MessageBox.Show("Beyond Compare not found, Please input the correct path!");
                    return;
                }
                // More details
                BCHelper.StartBeyondCompare(baseConfigPath, newConfigPath, _settings.BeyondCompareFullPath);
            }
            SaveAppSetings();
        }

        private void SaveAppSetings()
        {
            if (_settings == null)
            {
                _settings = new SettingsModel();
                _settings.NewConfigFolder = txt_StorePath.Text.Trim();
                _settings.BaseConfigFolder = txt_BasePath.Text.Trim();
                _settings.ConfigurationAssemblyPath = txt_AssemblyPath.Text.Trim();
                _settings.BeyondCompareFullPath = $"{Path.Combine(_destPath, "Assets", "Beyond Compare 5")}\\BCompare.exe";
            }
            else
            {
                _settings.NewConfigFolder = txt_StorePath.Text.Trim();
                _settings.BaseConfigFolder = txt_BasePath.Text.Trim();
                _settings.ConfigurationAssemblyPath = txt_AssemblyPath.Text.Trim();
                if (string.IsNullOrEmpty(_settings.BeyondCompareFullPath))
                    _settings.BeyondCompareFullPath = $"{Path.Combine(_destPath, "Assets", "Beyond Compare 5")}\\BCompare.exe";
            }
            try
            {
                var settingsContent = JsonConvert.SerializeObject(_settings);
                string filePath = System.IO.Path.Combine(_destPath, "Assets") + "\\appsettings.json";
                if (File.Exists(filePath))
                    File.Delete(filePath);
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(filePath, settingsContent, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Saving appsettings.json failed: {ex.Message}");
            }
        }
    }
}