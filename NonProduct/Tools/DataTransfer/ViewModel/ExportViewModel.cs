using Microsoft.Extensions.Logging;
using NV.CT.NP.Tools.DataTransfer.Model;
using NV.CT.NP.Tools.DataTransfer.Service;
using NV.CT.NP.Tools.DataTransfer.Utils;
using NV.CT.UI.ViewModel;
using System.IO;

namespace NV.CT.NP.Tools.DataTransfer.ViewModel
{
    public class ExportViewModel : BaseViewModel
    {
        readonly CancellationTokenSource _cts;
        readonly CancellationToken _token;
        readonly ILogger<ExportViewModel> _logger;


        IEnumerable<VStudyModel> _selectedVStudies;
        private string _targetPath;
        private IDictionary<ExportType, bool> _exportType;
        private Action _action;

        public ExportViewModel(IEnumerable<VStudyModel> selectedVStudies, string targetPath, IDictionary<ExportType, bool> dataExportType, Action action)
        {
            _selectedVStudies = selectedVStudies;
            _targetPath = targetPath;
            _exportType = dataExportType;
            _patientName = selectedVStudies.First().PatientName;
            _remainCount = selectedVStudies.Count();
            _totalCount = _remainCount;

            _cts = new CancellationTokenSource();
            _token = _cts.Token;

            _action = action;

            _logger = LogHelper<ExportViewModel>.CreateLogger(nameof(ExportViewModel));

            BeginExport();
        }

        private void BeginExport()
        {
            _logger.LogInformation("BeginExport------------------------->");
            Task.Run(async () =>
            {
                try
                {
                    foreach (var vStudyModel in _selectedVStudies)
                    {
                        _token.ThrowIfCancellationRequested();
                        GlobalService.Instance.RunOnUI(() =>
                        {
                            PatientName = vStudyModel.PatientName;
                        });

                        var exportStatus = ExportStatus.None;
                        string errorMessage = string.Empty;

                        IExport export = null;

                        foreach (var item in _exportType)
                        {
                            if (item.Key == ExportType.Dicom && item.Value)
                            {
                                export = new DicomExportService(vStudyModel);
                                if (vStudyModel.Series == null || vStudyModel.Series.Count == 0)
                                {
                                    exportStatus = ExportStatus.Fail;
                                    errorMessage = "Series is empty";
                                    break;
                                }
                            }
                            else if (item.Key == ExportType.RawData && item.Value)
                            {
                                export = new RawDataExportService(vStudyModel);
                                if (vStudyModel.RawData == null || vStudyModel.RawData.Count == 0)
                                {
                                    exportStatus = ExportStatus.Fail;
                                    errorMessage = "RawData is empty";
                                    break;
                                }
                            }
                            else
                                continue;

                            // Calculate total bytes
                            long totalBytes = export == null ? 0 : export.EstimateTotalFileSize;
                            if (totalBytes > 0 && totalBytes < GetDriveTotalFreeSpace())
                            {
                                var result = await export.ExportAsync(_targetPath, _token);
                                if (result.result)
                                {
                                    exportStatus = ExportStatus.Success;
                                    errorMessage = string.Empty;
                                }
                                else
                                {
                                    exportStatus = ExportStatus.Fail;
                                    errorMessage = $"Exporting {item.Key.ToString()} failed: {result.msg}";
                                }
                            }
                            else
                            {
                                exportStatus = ExportStatus.Fail;
                                if (totalBytes == 0)
                                    errorMessage = "Data does not exist";
                                else
                                    errorMessage = "Insufficient space";
                            }

                            if (exportStatus == ExportStatus.Fail)
                            {
                                _logger.LogError($"Copy {vStudyModel.Id} {vStudyModel.PatientName} {vStudyModel.StudyInstanceUID} failed:\r\n {exportStatus.ToString()}\r\n {errorMessage}");
                                break;
                            }
                        }

                        // Todo: Batch update to improve performance
                        GlobalService.Instance.SqliteService.InsertExportRecord(vStudyModel.PatientId, vStudyModel.StudyInstanceUID, exportStatus.ToString(), errorMessage);

                        GlobalService.Instance.RunOnUI(() =>
                        {
                            vStudyModel.ExportStatus = exportStatus;
                            vStudyModel.ErrorMessage = errorMessage;
                            RemainCount--;
                        });
                    }
                }
                catch (OperationCanceledException e)
                {
                    _logger.LogInformation(e, $"BeginExport Cancelled!");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in BeginExport");
                }
            }, _token).ContinueWith(res =>
            {
                GlobalService.Instance.RunOnUI(_action);
                _logger.LogInformation("BeginExport finished.<-------------------------");
            });
        }

        private long GetDriveTotalFreeSpace()
        {
            if (string.IsNullOrEmpty(_targetPath))
                return 0;

            string rootPath = Path.GetPathRoot(_targetPath);
            var driveInfo = new DriveInfo(rootPath);
            return driveInfo.TotalFreeSpace;
        }

        #region Binding Properties
        private string _patientName = string.Empty;
        public string PatientName
        {
            get { return _patientName; }
            set
            {
                SetProperty(ref _patientName, value);
            }
        }

        private int _remainCount = 0;
        public int RemainCount
        {
            get { return _remainCount; }
            set
            {
                SetProperty(ref _remainCount, value);
            }
        }

        private int _totalCount = 0;
        public int TotalCount
        {
            get { return _totalCount; }
            set
            {
                SetProperty(ref _totalCount, value);
            }
        }
        #endregion

        public void CancelExport()
        {
            _cts.Cancel();
        }
    }
}
