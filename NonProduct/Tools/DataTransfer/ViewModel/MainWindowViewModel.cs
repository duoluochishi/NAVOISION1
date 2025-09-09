using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.NP.Tools.DataTransfer.Model;
using NV.CT.NP.Tools.DataTransfer.Service;
using NV.CT.NP.Tools.DataTransfer.Utils;
using NV.CT.NP.Tools.DataTransfer.View;
using NV.CT.UI.ViewModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace NV.CT.NP.Tools.DataTransfer.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        readonly ILogger<MainWindowViewModel> _logger;
        private bool _isAllDaySelected = true;
        private VStudyModel? _selectedItem;
        private string _targetPath = string.Empty;
        private const string DEFAULT_PATH = "D:\\";
        private ObservableCollection<VStudyModel> _cacheStudies = new();

        public MainWindowViewModel()
        {
            BeginDate = DateTime.Now.AddDays(-1);
            EndDate = DateTime.Now;
            _targetPath += $"{DriveInfo.GetDrives().Last().Name}test_{Guid.NewGuid().ToString()}";
            _logger = LogHelper<MainWindowViewModel>.CreateLogger(nameof(MainWindowViewModel));
        }

        #region Binding properties and commands
        public bool IsAllDaySelected
        {
            get => _isAllDaySelected;
            set
            {
                SetProperty(ref _isAllDaySelected, value);
            }
        }

        private DateTime _beginDate;
        public DateTime BeginDate
        {
            get => _beginDate;
            set => SetProperty(ref _beginDate, value);
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        private ObservableCollection<VStudyModel> _vStudies = new();
        public ObservableCollection<VStudyModel> VStudies
        {
            get => _vStudies;
            set
            {
                SetProperty(ref _vStudies, value);
            }
        }

        public VStudyModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
            }
        }

        private bool _isSelectedAll;
        public bool IsSelectedAll
        {
            get => _isSelectedAll;
            set
            {
                SetProperty(ref _isSelectedAll, value);
                OnCheckAll(value);
            }
        }

        private bool _isShowLoading = false;
        public bool IsShowLoading
        {
            get => _isShowLoading;
            set
            {
                SetProperty(ref _isShowLoading, value);
            }
        }

        private void OnCheckAll(bool isChecked)
        {
            VStudies?.ForEach(v =>
            {
                if (v.ExportStatus != ExportStatus.Success)
                    v.IsSelected = isChecked;
            });
        }

        public string TargetPath
        {
            get => _targetPath;
            set => SetProperty(ref _targetPath, value);
        }

        private ExportType _dataExportType = ExportType.Dicom;
        public ExportType DataExportType
        {
            get => _dataExportType;
            set => SetProperty(ref _dataExportType, value);
        }

        private bool _isAllChecked;
        public bool IsAllChecked
        {
            get => _isAllChecked;
            set
            {
                SetProperty(ref _isAllChecked, value);
                IsDicomChecked = value;
                IsRawDataChecked = value;
            }
        }

        private bool _isDicomChecked;
        public bool IsDicomChecked
        {
            get => _isDicomChecked;
            set => SetProperty(ref _isDicomChecked, value);
        }

        private bool _isRawDataChecked;
        public bool IsRawDataChecked
        {
            get => _isRawDataChecked;
            set => SetProperty(ref _isRawDataChecked, value);
        }

        public ICommand SearchCommand => new DelegateCommand(async () =>
        {
            IsShowLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        List<(PatientModel, StudyModel)> result;
                        if (IsAllDaySelected)
                            result = GetAllPatientStudies();
                        else
                            result = GetPatientStudyListWithEndStudyDate();
                        ConstructVStudies(result);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("搜索或构造数据时发生错误", ex);
                    }
                });
            }
            catch (AggregateException agEx)
            {
                MessageBox.Show($"Search database error: {agEx.InnerException?.Message ?? agEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Search database error: {ex.Message}");
            }
            finally
            {
                IsShowLoading = false;
            }
        });

        public ICommand BeginxportCommand => new DelegateCommand(() =>
        {
            var types = GetSelectedExportType();
            if (types.Count(va=>va.Value) < 1)
            {
                MessageBox.Show("请选择要导出的类型！");
                return;
            }

            var selectedStudies = GetSelectedStudies();
            if (selectedStudies.Count < 1)
            {
                MessageBox.Show("请选择要导出的信息！");
                return;
            }

            ExportWindow exportWindow = new ExportWindow();
            ExportViewModel exportWindowViewModel = new ExportViewModel(selectedStudies, _targetPath, types, () => exportWindow.Close());
            exportWindow.DataContext = exportWindowViewModel;
            exportWindow.Owner = Application.Current.MainWindow;
            exportWindow.ShowDialog();
        });

        public ICommand SelectPathCommand => new DelegateCommand(() =>
        {
            var folderBrowser = new OpenFolderDialog();
            folderBrowser.InitialDirectory = DEFAULT_PATH;
            if (folderBrowser.ShowDialog() == true)
            {
                TargetPath = folderBrowser.FolderName;
            }
        });

        public ICommand FilterStudiesCommand => new DelegateCommand<string>((string keyword) =>
        { 
            if(string.IsNullOrEmpty(keyword))
            {
                VStudies = _cacheStudies;
                return;
            }

            var key = keyword.ToLower();
            Func<VStudyModel, bool> func = r =>
            {
                if (string.IsNullOrEmpty(r.PatientName))
                {
                    r.PatientName = string.Empty;
                }

                if (string.IsNullOrEmpty(r.PatientId))
                {
                    r.PatientId = string.Empty;
                }
                return (r.PatientName.ToLower().Contains(key) || r.PatientId.ToLower().Contains(key));
            };
            VStudies = new ObservableCollection<VStudyModel>(_cacheStudies.Where(func));
        });
        #endregion

        private List<(PatientModel, StudyModel)> GetAllPatientStudies()
        {
            return GlobalService.Instance.StudyService.GetPatientStudyListWithEnd();
        }

        private List<(PatientModel, StudyModel)> GetPatientStudyListWithEndStudyDate()
        {
            var beginDate = BeginDate.ToString("yyyy-MM-dd HH:mm:ss");
            var endDate = EndDate.ToString("yyyy-MM-dd HH:mm:ss");
            return GlobalService.Instance.StudyService.GetPatientStudyListWithEndStudyDate(beginDate, endDate);
        }

        private void ConstructVStudies(List<(PatientModel, StudyModel)> result)
        {
            _cacheStudies = new ObservableCollection<VStudyModel>(
                result.Select(x =>
                {
                    try
                    {
                        var study = new VStudyModel
                        {
                            Id = x.Item2.Id,
                            StudyInstanceUID = x.Item2.StudyInstanceUID,
                            PatientName = x.Item1.PatientName,
                            PatientId = x.Item1.PatientId,
                            StudyTime = x.Item2.StudyTime,
                            BodyPart = x.Item2.BodyPart,
                            Series = GlobalService.Instance.SeriesService.GetSeriesByStudyId(x.Item2.Id),
                            RawData = GlobalService.Instance.RawDataService.GetRawDataListByStudyId(x.Item2.Id)
                        };
                        var result = GlobalService.Instance.SqliteService.GetJobStatusByPatientIdAndStudyUid(study.PatientId, study.StudyInstanceUID);
                        study.ExportStatus = ExportStatusConvert(result.jobStatus);
                        study.ErrorMessage = result.errorMessage;
                        return study;
                    }
                    catch (Exception ex)
                    {
                        var study = new VStudyModel
                        {
                            Id = x.Item2.Id,
                            StudyInstanceUID = x.Item2.StudyInstanceUID,
                            PatientName = x.Item1.PatientName,
                            PatientId = x.Item1.PatientId,
                            ErrorMessage = $"Loading data failed: {ex.Message}"
                        };
                        _logger.LogError($"Loading data:\r\n {JsonConvert.SerializeObject(study)} \r\n failed: {ex.Message}");
                        return study;
                    }
                }));

            GlobalService.Instance.RunOnUI(() =>
            {
                VStudies = _cacheStudies;
            });
        }

        private ExportStatus ExportStatusConvert(string str)
        {
            if (str == "Success")
            {
                return ExportStatus.Success;
            }
            else if (str == "Fail")
            {
                return ExportStatus.Fail;
            }
            else
            {
                return ExportStatus.None;
            }
        }

        private ObservableCollection<VStudyModel> GetSelectedStudies()
        {
            return new ObservableCollection<VStudyModel>(
                VStudies.Where(item => item.IsSelected));
        }

        private IDictionary<ExportType, bool> GetSelectedExportType()
        {
            return new Dictionary<ExportType, bool>() { [ExportType.Dicom] = IsDicomChecked, [ExportType.RawData] = IsRawDataChecked };
        }
    }
}
