using NV.CT.DmsTest.Model;
using NV.CT.DmsTest.ViewModel;
using System.Windows;
using System.Windows.Controls;


namespace NV.CT.DmsTest.View
{
    /// <summary>
    /// DmsTestView.xaml 的交互逻辑
    /// </summary>
    public partial class DmsTestView : UserControl
    {
        public DmsTestView()
        {
            InitializeComponent();
            DmsTestVM dmsTestVM = new DmsTestVM();
  
            this.DataContext = dmsTestVM;

            
        }
        public void SelectRawDataPath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = "D:\\";
            folderBrowserDialog.ShowDialog();
            
            if (CoreParamShow.SelectedItem is CoreScanParam coreScanParam)
            {
                coreScanParam.RawDataPath = folderBrowserDialog.SelectedPath;
            }
        }
        
        private void DebugModeChange_Click(object sender, RoutedEventArgs e)
        {
            CheckBox? checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                if (checkBox.IsChecked == true)
                {
                    RawDataPathSelecte.Visibility = Visibility.Visible;
                } else
                {
                    RawDataPathSelecte.Visibility = Visibility.Collapsed;   
                }
            }

        }

        private void TextBlock_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RunLogGrid.ScrollToEnd();
        }
    }
}
