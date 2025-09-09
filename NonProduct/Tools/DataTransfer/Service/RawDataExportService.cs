using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.NP.Tools.DataTransfer.Model;
using NV.CT.NP.Tools.DataTransfer.Utils;
using NV.CT.NP.Tools.DataTransfer.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.NP.Tools.DataTransfer.Service
{
    public class RawDataExportService : IExport
    {
        readonly ILogger<RawDataExportService> _logger;
        private HashSet<string> _directoryList = new();
        private VStudyModel _studyModel;

        public long EstimateTotalFileSize { get; }

        public RawDataExportService(VStudyModel vStudy)
        {
            _studyModel = vStudy;
            _logger = LogHelper<RawDataExportService>.CreateLogger(nameof(RawDataExportService));
            // Series
            foreach (var seriesModel in vStudy.Series)
            {
                var seriesPath = seriesModel.SeriesPath;
                string directory = FileHelper.GetStudyInstanceUIDDirectory(seriesPath, vStudy.StudyInstanceUID, ConstStrings.SERIES_ROOT_NAME);
                if (!string.IsNullOrEmpty(directory))
                    _directoryList.Add(directory);
            }
            // RawData
            foreach (var rawDataModel in vStudy.RawData)
            {
                var rawDataPath = rawDataModel.Path;
                string directory = FileHelper.GetStudyInstanceUIDDirectory(rawDataPath, vStudy.StudyInstanceUID);
                if (!string.IsNullOrEmpty(directory))
                    _directoryList.Add(directory);
            }

            EstimateTotalFileSize = FileHelper.CalculateTotalFileSize(_directoryList);
        }

        // base path: PatientId/StudyInstanceUID/
        // full path: TartgePath/PatientId/RawData/StudyInstanceUID/StudyInstanceUID/文件名
        public async Task<(bool result, string msg)> ExportAsync(string targetPath, CancellationToken cancellationToken)
        {
            (bool result, string msg) result = default;
            string destPath = Path.Combine(targetPath, _studyModel.PatientId, ConstStrings.RAW_DATA_ROOT_NAME, _studyModel.StudyInstanceUID);

            try
            {
                foreach (var srcPath in _directoryList)
                {
                    result = await FileHelper.CopyAsync(srcPath, destPath, string.Empty, cancellationToken);
                }

                if (result.result)
                {
                    SaveReconJson(_studyModel.Id, destPath);
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


        private void SaveReconJson(string studyId, string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var studyEntity = GlobalService.Instance.StudyService.GetStudyById(studyId);
            var patientEntity = GlobalService.Instance.PatientService.GetPatientById(studyEntity.InternalPatientId);
            var rawDataModelList = GlobalService.Instance.RawDataService.GetRawDataListByStudyId(studyId);
            var scanTaskModelList = GlobalService.Instance.ScanTaskService.GetAll(studyId);
            var reconTaskModelList = GlobalService.Instance.ReconTaskService.GetAll(studyId);//.FindAll(r => r.IsRTD == true);
            var seriesModelList = GlobalService.Instance.SeriesService.GetTopoTomoSeriesByStudyId(studyId);
            WriteJson(dir, "study.json", studyEntity);
            WriteJson(dir, "patient.json", patientEntity);
            WriteJson(dir, "rawdata.json", rawDataModelList);
            WriteJson(dir, "scantask.json", scanTaskModelList);
            WriteJson(dir, "recontask.json", reconTaskModelList);
            WriteJson(dir, "series.json", seriesModelList);
        }

        private void WriteJson(string dir, string fileName, object obj)
        {
            string fileFullName = Path.Combine(dir, fileName);
            File.WriteAllText(fileFullName, JsonConvert.SerializeObject(obj));
        }
    }
}
