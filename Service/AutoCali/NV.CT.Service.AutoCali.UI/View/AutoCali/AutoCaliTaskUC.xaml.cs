using NV.CT.Service.AutoCali.Logic;
using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.AutoCali.UI.Logic;
using NV.CT.Service.AutoCali.UI.ViewModel;
using NV.CT.Service.Common;
using NV.CT.Service.UI.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;

namespace NV.CT.Service.AutoCali.UI
{
    /// <summary>
    /// 校准场景任务UI
    /// </summary>
    public partial class AutoCaliTaskUC : UserControl
    {
        private CaliScenarioTaskViewModel scenarioViewModel;

        public AutoCaliTaskUC()
        {
            InitializeComponent();
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            //注册图片发生变化的事件
            EventMediator.UnRegister(CaliScenarioTaskViewModel.KEY_RAW_IMAGE_CHANGED_EVENT);
            EventMediator.Register(CaliScenarioTaskViewModel.KEY_RAW_IMAGE_CHANGED_EVENT, OnRawImageChanged);

            DataGridUtil.AddRowHeader(this.argProtocolDataGrid2);
            //Console.WriteLine("[DataGrid2]"+ argProtocolDataGrid2.GetHashCode());
        }

        private void Root_Unloaded(object sender, RoutedEventArgs e)
        {
            EventMediator.UnRegister(CaliScenarioTaskViewModel.KEY_RAW_IMAGE_CHANGED_EVENT);
        }

        private void UserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            string block = "AutoCaliTaskUC.UserControl_DataContextChanged";
            LogService.Instance.Debug(ServiceCategory.AutoCali, $"Beginning {block}");

            LogService.Instance.Debug(ServiceCategory.AutoCali, $"DataContext={this.DataContext}({this.DataContext?.GetHashCode()})");

            if (this.DataContext is CaliScenarioTaskViewModel)
            {
                scenarioViewModel = this.DataContext as CaliScenarioTaskViewModel;

                if (scenarioViewModel?.CaliItemTaskViewModels?.Count < 1)
                {
                    return;
                }
                scenarioViewModel.SelectedCaliItemTaskViewModel = scenarioViewModel.CaliItemTaskViewModels?[0];

                isDebugModeGlobal = scenarioViewModel.IsDebugMode;
                if (scenarioViewModel.IsAutoConfirmResult)
                {
                    //grid-row=*的方式下，对应布局的控件即使设置collapsed也会占用空间
                    //所以，当自动确认结果（即不需要显示扫描结果）时，需要将布局方式修改为auto（修改前xaml中定义为*）
                    this.imageRow.Height = GridLength.Auto;
                }

                SwitchWhenGlobalDebugMode();
            }

            LogService.Instance.Debug(ServiceCategory.AutoCali, $"Ended {block}");
        }

        private void SwitchWhenGlobalDebugMode()
        {
            if (isDebugModeGlobal)
            {
                debugModeContainer.Visibility = Visibility.Visible;
                switchDebugModeCheckBox.IsChecked = true;
            }
        }

        private void protocolContainer_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as Control;
            string block = "AutoCaliTaskUC.protocolContainer_DataContextChanged";
            LogService.Instance.Debug(ServiceCategory.AutoCali, $"Beginning {block}");
            LogService.Instance.Debug(ServiceCategory.AutoCali, $"DataContext={control?.DataContext}({control?.DataContext?.GetHashCode()})");

            ListView_SelectionChanged(this, null);

            LogService.Instance.Debug(ServiceCategory.AutoCali, $"Ended {block}");
        }

        /// <summary>
        /// 切换选择校准项目，同步生成参数协议控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string block = "AutoCaliTaskUC.ListView_SelectionChanged";
            LogService.Instance.Debug(ServiceCategory.AutoCali, $"Beginning {block}");

            var aCaliItemTaskViewModel = scenarioViewModel.SelectedCaliItemTaskViewModel;

            #region 动态生成协议参数表，1-保留固定列，清除动态列

            //this.argProtocolDataGrid2.Columns.Clear();
            for (int i = this.argProtocolDataGrid2.Columns.Count - 1; i >= 0; i--)
            {
                var column = this.argProtocolDataGrid2.Columns[i];
                if (column == this.RawDataDirectoryDataGridColumn)
                {
                    continue;
                }

                this.argProtocolDataGrid2.Columns.RemoveAt(i);
            }
            SwitchColumnWhenDebugMode(scenarioViewModel.IsDebugMode);

            #endregion

            GenDynamicalDataTable(aCaliItemTaskViewModel);
            foreach (var column in CustomBoundColumns)
            {
                this.argProtocolDataGrid2.Columns.Add(column);
            }

            LogService.Instance.Debug(ServiceCategory.AutoCali, $"Ended {block}");
        }

        public ObservableCollection<CustomBoundColumn> CustomBoundColumns { get; private set; } = new ObservableCollection<CustomBoundColumn>();
        private void GenDynamicalDataTable(CaliItemTaskViewModel aCaliItemTaskViewModel)
        {
            CustomBoundColumns.Clear();
            var SelectedCaliItemTaskViewModel = aCaliItemTaskViewModel;
            bool hasHeader = false;
            if (null == SelectedCaliItemTaskViewModel || null == SelectedCaliItemTaskViewModel.SubTaskViewModels)
            {
                return;
            }

            var data = new List<ICaliTaskViewModel>();
            foreach (var caliProtocolTaskViewModel in SelectedCaliItemTaskViewModel.SubTaskViewModels)
            {
                foreach (var stepTaskVM in caliProtocolTaskViewModel.SubTaskViewModels)
                {
                    data.Add(stepTaskVM);

                    #region 生成表头
                    if (hasHeader)
                    {
                        continue;
                    }

                    var column = new CustomBoundColumn();
                    column.IsReadOnly = true;
                    //column.Header = "Status";//状态列，不需要列头名称
                    //column.Binding = new Binding("CaliTaskState");
                    column.Binding = new Binding(".");
                    column.TemplateName = "processStatusTemplate";
                    column.Width = DataGridLength.Auto;
                    CustomBoundColumns.Add(column);

                    int k = 0;
                    var stepDto = ((CaliScanTaskViewModel)stepTaskVM).Inner;
                    foreach (var argItem in stepDto.Parameters)
                    {
                        column = new CustomBoundColumn();
                        column.IsReadOnly = true;
                        column.Header = $"{argItem.Name}";
                        //column.Binding = new Binding($"Inner.Args[{k++}].Value");
                        column.Binding = new Binding($"Inner.Parameters[{k++}].Value");

                        column.TemplateName = "dgTextBlockColumnDataTemplate";
                        column.Width = DataGridLength.Auto;
                        CustomBoundColumns.Add(column);
                    }
                    hasHeader = true;

                    #endregion
                }
            }

            //根据父节点分组
            ICollectionView collectionView = CollectionViewSource.GetDefaultView(data);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Parent"));

            this.argProtocolDataGrid2.ItemsSource = collectionView;
        }

        /// <summary>
        /// 响应图像发生变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnRawImageChanged(object sender, EventArgs eventArgs)
        {
            //if (null == serisImageControlHelper)
            //{
            //    serisImageControlHelper = new SerisImageControlHelper(this.rawImageControl);
            //}
            //serisImageControlHelper.OnRawImageChanged(sender, eventArgs);
            //return;

            var imageChangedEventArg = eventArgs as RawImageChangedEventArgs;
            string directory = imageChangedEventArg?.ScanReconParam?.ScanParameter?.RawDataDirectory;
            LogService.Instance.Debug(ServiceCategory.AutoCali,$"收到消息：加载生数据目录，Path:{directory}");

            //directory = @"F:\AppData\DataMRS\ServiceData\2024_0418_130240_SourceXZ\130240_inDynamic_AfterDelete";
            //Console.WriteLine($"[DebugTest] 指定加载生数据目录，Path:{directory}");
            this.rawImageControl.LoadRawDataDirectory(directory);
        }

        //private SerisImageControlHelper serisImageControlHelper;

        private void Analysis_Click(object sender, RoutedEventArgs e)
        {
            //DataAnalysisToolsHelper helper = new();
            //helper.ViewRawDataByFiles(@"F:\AppData\DataMRS\ServiceData\2024_0418_130240_SourceXZ\130240_inDynamic_AfterDelete");

            //helper.ViewRawDataByFolder(@"F:\AppData\DataMRS\ServiceData\2024_0418_130240_SourceXZ\130240_inDynamic_AfterDelete");

        }

        private void AnalysisConfig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OnFilter(object sender, RoutedEventArgs e)
        {
            var Prepare = ((object sender) =>
            {
                (bool Passed, bool IsChecked, IEnumerable<ICaliTaskViewModel> Children) result = (false, false, null);

                CheckBox checkBox = sender as CheckBox;
                if (checkBox == null)
                {
                    return result;
                }

                var aCaliItemTaskViewModel = scenarioViewModel.SelectedCaliItemTaskViewModel;
                if (aCaliItemTaskViewModel == null)
                {
                    return result;
                }

                var subTaskViewModels = aCaliItemTaskViewModel.SubTaskViewModels;
                int count = subTaskViewModels.Count();
                if (count < 1)
                {
                    return result;
                }

                bool isChecked = checkBox.IsChecked.Value;
                result = (true, isChecked, subTaskViewModels);
                return result;
            });

            var result = Prepare(sender);
            if (!result.Passed) return;

            bool isChecked = result.IsChecked;
            var protocolViewModels = result.Children;

            string commandParameter = (sender as ButtonBase)?.CommandParameter?.ToString();
            var paras = commandParameter?.Split(":");

            string propertyName = paras?.First()?.Trim();// "kV";
            string propertyValue = paras?.Last()?.Trim();// "120";

            LogService.Instance.Debug(ServiceCategory.AutoCali, $"[OnFilter] Parsed to propertyName '{propertyName}', propertyValue '{propertyValue}' from the CommandParameter '{commandParameter}'");

            Func<Model.Parameter, bool> paramIsMatched = null;
            if (string.Equals(propertyName, "all", StringComparison.OrdinalIgnoreCase))
            {
                paramIsMatched = (param =>
                {
                    return true;
                });
            }
            else
            {
                paramIsMatched = (param =>
                {
                    return string.Equals(param.Name, propertyName, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(param.Value, propertyValue, StringComparison.OrdinalIgnoreCase);
                });
            }

            OnProtocolIsCheckedChanged(protocolViewModels, isChecked, paramIsMatched);
        }

        private bool OnProtocolIsCheckedChanged(
            IEnumerable<ICaliTaskViewModel> protocolViewModels,
            bool isChecked,
            Func<Model.Parameter, bool> paramIsMatched)
        {
            //string propertyName = "kV";
            //string propertyValue = "120";
            foreach (var child in protocolViewModels)
            {
                if (!(child is CaliProtocolTaskViewModel protocolViewModel)) break;

                CalibrationProtocol calibrationProtocol = protocolViewModel.Inner;

                bool propertyMatched = false;
                //遍历子项处理流程，寻找包含目标属性，并且属性值符合，视为这个协议符合筛选要求，勾选它
                foreach (var handler in calibrationProtocol.HandlerGroup)
                {
                    propertyMatched = handler.Parameters.Any(p => paramIsMatched(p));
                    if (propertyMatched) break;
                }

                if (propertyMatched)
                {
                    protocolViewModel.IsChecked = isChecked;
                }
            }

            return true;
        }
    }

    public partial class AutoCaliTaskUC : UserControl
    {
        #region 内部调试模式

        /// <summary>
        /// 内部调试模式
        /// </summary>
        private bool isDebugModeGlobal = false;

        private static readonly string TestData_T03_Path = "E:\\TestData_T03";

        private void selectS03_RawDataPathButton_Click(object sender, RoutedEventArgs e)
        {
            var caliStepTaskVM = GetCaliStepTaskVM(sender);

            //test
            string rawDataType = caliStepTaskVM.Inner.Parameters.FirstOrDefault(p => p.Name == "RawDataType")?.Value;
            string rawDataPath = GetRawDataPath_S03_AsDefault(rawDataType);
            if (SelectFolder(rawDataPath, out var selectedFolder))
            {
                caliStepTaskVM.T03_RawDataPath = string.Empty;

                caliStepTaskVM.S03_RawDataPath = selectedFolder;
                lastSelectedRawDataPath_S03 = selectedFolder;
            }
        }

        private string lastSelectedRawDataPath_T03 = string.Empty;
        private string lastSelectedRawDataPath_S03 = string.Empty;
        private string GetRawDataPath_T03_AsDefault(string rawDataType)
        {
            string rawDataPath = string.Empty;
            if (string.IsNullOrEmpty(lastSelectedRawDataPath_T03))
            {
                rawDataPath = Path.Combine(TestData_T03_Path, $"{rawDataType}");
            }
            else
            {
                string parent = Directory.GetParent(lastSelectedRawDataPath_T03)?.FullName;
                rawDataPath = Path.Combine(parent, rawDataType);
            }
            return rawDataPath;
        }

        private string GetRawDataPath_S03_AsDefault(string rawDataType)
        {
            string rawDataPath = string.Empty;
            if (string.IsNullOrEmpty(lastSelectedRawDataPath_S03))
            {
                rawDataPath = Path.Combine(CaliScanTaskViewModel.ServiceDataPath, $"S03\\{rawDataType}");
            }
            else
            {
                string parent = Directory.GetParent(lastSelectedRawDataPath_S03)?.FullName;
                rawDataPath = Path.Combine(parent, rawDataType);
            }
            return rawDataPath;
        }

        /// <summary>
        /// 选择文件夹
        /// </summary>
        /// <param name="defaultPath"></param>
        /// <param name="selectedFolder"></param>
        /// <returns></returns>
        public static bool SelectFolder(string defaultPath, out string selectedFolder)
        {
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.SelectedPath = defaultPath;
            if (System.Windows.Forms.DialogResult.OK == folderDialog.ShowDialog())
            {
                selectedFolder = folderDialog.SelectedPath;
                return true;
            }

            selectedFolder = string.Empty;
            return false;
        }

        private CaliScanTaskViewModel GetCaliStepTaskVM(object sender)
        {
            var element = sender as FrameworkElement;
            var caliStepTaskVM = (element.DataContext as CaliScanTaskViewModel);
            return caliStepTaskVM;
        }

        private void SwitchDebugModeCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                scenarioViewModel.IsDebugMode = checkBox.IsChecked.Value;
            }

            SwitchColumnWhenDebugMode(scenarioViewModel.IsDebugMode);
        }

        private void SwitchColumnWhenDebugMode(bool isOn)
        {
            this.RawDataDirectoryDataGridColumn.Visibility = isOn ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ConvertRawData_RootPathButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            object dataContext = element.DataContext;

        }

        private void selectT03_RawData_RootPathButton_Click(object sender, RoutedEventArgs e)
        {

            //var protocolTaskViewModel = ((sender as FrameworkElement).DataContext as MS.Internal.Data.CollectionViewGroupInternal).Name as CaliProtocolTaskViewModel;

        }

        #endregion 内部调试模式
    }
    //public class SerisImageControlHelper
    //{
    //    private SeriesImageControl rawImageControl;
    //    public SerisImageControlHelper(SeriesImageControl rawImageControl)
    //    {
    //        this.rawImageControl = rawImageControl;
    //    }

    //    /// <summary>
    //    /// 响应图像发生变化
    //    /// </summary>
    //    /// <param name="sender"></param>
    //    /// <param name="eventArgs"></param>
    //    public void OnRawImageChanged(object sender, EventArgs eventArgs)
    //    {
    //        LogService.Instance.Debug(ServiceCategory.AutoCali, "响应图像发生变化, Begin");
    //        var imageChangedEventArg = eventArgs as RawImageChangedEventArgs;
    //        if (null == imageChangedEventArg)
    //        {
    //            LogService.Instance.Warn(ServiceCategory.AutoCali, "响应图像发生变化, 参数为空，End(Non-normal)");
    //            return;
    //        }

    //        if (imageChangedEventArg.ImageChangedType == ImageChangedType.Clear)
    //        {
    //            LogService.Instance.Debug(ServiceCategory.AutoCali, "响应图像发生变化, 清空上次图像");
    //            this.rawImageControl.ClearImages();
    //            return;
    //        }

    //        LogService.Instance.Debug(ServiceCategory.AutoCali, "响应图像发生变化, 加载并显示新的图像");
    //        //this.rawImageControl.AddImage(imageChangedEventArg.FilePath);
    //        AddImageAsync(imageChangedEventArg.FilePaths, imageChangedEventArg.ScanReconParam);

    //        LogService.Instance.Debug(ServiceCategory.AutoCali, "响应图像发生变化, End");
    //    }

    //    /// <summary>
    //    /// 手动添加图像，并附加扫描参数作为DicomTag
    //    /// </summary>
    //    /// <param name="rawImageControl"></param>
    //    /// <param name="rawFilePath"></param>
    //    /// <param name="scanReconParam"></param>
    //    public async Task AddImageAsync(string[] rawFilePaths, ScanReconParam scanReconParam = null)
    //    {
    //        if (rawImageControl == null || rawFilePaths == null || rawFilePaths.Length == 0)
    //        {
    //            return;
    //        }

    //        List<ImageData> imageDataList = await ParseRawFiles(rawFilePaths, scanReconParam);
    //        if (imageDataList?.Count < 1)
    //        {
    //            LogService.Instance.Warn(ServiceCategory.AutoCali, $"未能解析到图像数据，其中，输入生数据文件：\n{string.Join("\n", rawFilePaths)}");
    //            return;
    //        }

    //        rawImageControl.AddImage(imageDataList);
    //    }

    //    private async Task<List<ImageData>> ParseRawFiles(string[] rawFilePaths, ScanReconParam scanReconParam = null)
    //    {
    //        List<ImageData> imageDataList = new List<ImageData>();
    //        await Task.Factory.StartNew(() =>
    //        {
    //            foreach (string rawFilePath in rawFilePaths)
    //            {
    //                var curImageDataList = SeriesRawImageReader.Read(rawFilePath);
    //                if (curImageDataList == null || curImageDataList.Count == 0)
    //                {
    //                    continue;
    //                }

    //                imageDataList.AddRange(curImageDataList);

    //                //todo:暂时只读取第1个文件的数据，遇到非线性的4320张图像的文件，共4个文件，在研发自身机器上报错内存耗尽。 
    //                break;
    //            }

    //            if (imageDataList.Count > 0)
    //            {
    //                FillDicomTagInfo(imageDataList, scanReconParam);
    //            }
    //        });

    //        return imageDataList;
    //    }

    //    /// <summary>
    //    /// 给（多个）图像附加扫描参数作为DicomTag
    //    /// </summary>
    //    /// <param name="imageDataList"></param>
    //    /// <param name="scanReconParam"></param>
    //    private void FillDicomTagInfo(List<ImageData> imageDataList, ScanReconParam scanReconParam)
    //    {
    //        if (scanReconParam != null)
    //        {
    //            string kv = scanReconParam.ScanParameter.kV[0].ToString();
    //            string ma = scanReconParam.ScanParameter.mA[0].ToString();
    //            for (int i = 0; i < imageDataList.Count; i++)
    //            {
    //                var imageData = imageDataList[i];
    //                imageData[DicomTag.KVP] = kv;
    //                imageData[DicomTag.XRayTubeCurrent] = ma;
    //                imageData[DicomTag.InstanceNumber] = (i + 1).ToString();
    //            }
    //        }
    //    }
    //}
}