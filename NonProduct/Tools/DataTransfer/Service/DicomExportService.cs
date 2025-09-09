using Microsoft.Extensions.Logging;
using NV.CT.NP.Tools.DataTransfer.Model;
using NV.CT.NP.Tools.DataTransfer.Utils;
using System.IO;

namespace NV.CT.NP.Tools.DataTransfer.Service
{
    public class DicomExportService : IExport
    {
        readonly ILogger<DicomExportService> _logger;
        private HashSet<string> _directoryList = new();
        private VStudyModel _studyModel;
        
        public long EstimateTotalFileSize { get; }

        public DicomExportService(VStudyModel vStudy)
        {
            _studyModel = vStudy;
            _logger = LogHelper<DicomExportService>.CreateLogger(nameof(DicomExportService));
            // Series
            foreach (var seriesModel in vStudy.Series)
            {
                var seriesPath = seriesModel.SeriesPath;
                string directory = FileHelper.GetStudyInstanceUIDDirectory(seriesPath, vStudy.StudyInstanceUID, ConstStrings.SERIES_ROOT_NAME);
                if (!string.IsNullOrEmpty(directory))
                    _directoryList.Add(directory);
            }
            EstimateTotalFileSize = FileHelper.CalculateTotalFileSize(_directoryList);
        }

        // base path: PatientId/StudyInstanceUID/
        // full path: TartgePath/PatientId/Dicom/StudyInstanceUID/SeriesInstaceUID/文件名
        public async Task<(bool result, string msg)> ExportAsync(string targetPath, CancellationToken cancellationToken)
        {
            (bool result, string msg) result = default;
            string destPath = Path.Combine(targetPath, _studyModel.PatientId, ConstStrings.DICOM_ROOT_NAME, _studyModel.StudyInstanceUID);

            try
            {
                foreach (var srcPath in _directoryList)
                {
                    result = await FileHelper.CopyAsync(srcPath, destPath, string.Empty, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                result.result = false;
                result.msg = ex.Message;
                _logger.LogError($"Copy failed: {ex.Message}");
            }

            return result;
        }
    }
}
