using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Service.AutoCali.Logic;
using NV.CT.Service.AutoCali.Model;
using NV.MPS.Environment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NV.CT.Service.AutoCali.UI.Logic
{
    /// <summary>
    /// 以校准项目的一个参数协议为单位，执行一个任务
    /// </summary>
    public class CaliScanTaskViewModel : CaliTaskViewModel<Handler>
    {
        public static readonly string ServiceDataPath = RuntimeConfig.Console.ServiceData.Path;// "F:\\AppData\\DataMRS\\ServiceData";
        public static readonly string AfterGlowFolderName = "Gain_AfterGlow";

        public CaliScanTaskViewModel(Handler argProtocol) : base(argProtocol) { }

        public string T03_RawDataPath
        {
            get => mT03_RawDataPath;
            set
            {
                mT03_RawDataPath = value;
                OnPropertyChanged("T03_RawDataPath");
            }
        }


        public string S03_RawDataPath
        {
            get => mS03_RawDataPath;
            set
            {
                mS03_RawDataPath = value;
                OnPropertyChanged("S03_RawDataPath");

                SetTaskParams(mS03_RawDataPath);
            }
        }

        public bool IsAfterGlow
        {
            get
            {
                if (mIsAfterGlow.HasValue)
                {
                    return mIsAfterGlow.Value;
                }

                mIsAfterGlow = string.Equals(RawDataType.AfterGlow.ToString(),
                    Inner?.Parameters?.FirstOrDefault(p => p.Name == "RawDataType")?.Value,
                    StringComparison.OrdinalIgnoreCase);
                return mIsAfterGlow.Value;
            }
        }

        public string ConvertRawDataPath_From_T03_To_S03(string T03RawDataPath)
        {
            var partTaskTaskms = CaliScanTaskViewModel.Parse_RawDataPath(T03RawDataPath);

            string destPath = this.IsAfterGlow
                ? Path.Combine(ServiceDataPath, $"{partTaskTaskms.studyInstanceUID}\\{AfterGlowFolderName}\\{partTaskTaskms.scanUID}")
                : Path.Combine(ServiceDataPath, $"{partTaskTaskms.studyInstanceUID}\\{partTaskTaskms.scanUID}");
            return destPath;
        }

        public static (string studyInstanceUID, string scanUID) Parse_RawDataPath(string rawDataPath, bool isForS03AfterGlow = false)
        {
            if (string.IsNullOrEmpty(rawDataPath))
            {
                return (null, null);
            }

            string[] folders = rawDataPath.Split('\\');
            string studyUID = null;
            string scanUID = null;
            string middle = null;
            for (int i = folders.Length - 1; i >= 0; i--)
            {
                string folder = folders[i].Trim();
                if (string.Empty == folder)
                {
                    continue;
                }

                if (scanUID == null)
                {
                    scanUID = folder;
                }
                else if (isForS03AfterGlow && middle == null)
                {
                    //余晖校准的生数据目录，多了一个中间目录Gain_AfterGlow，比如\ServiceData\S03\Gain_AfterGlow\484
                    //在解析的时候，S03解析为{StudyUID}，484解析为{ScanUID}
                    middle = folder;
                }
                else if (studyUID == null)
                {
                    studyUID = folder;
                }

                if (studyUID != null && scanUID != null)
                {
                    break;
                }
            }

            return (studyUID, scanUID);
        }

        private void SetTaskParams(string scanedRawDataPath)
        {
            var partTaskTaskms = Parse_RawDataPath(scanedRawDataPath, IsAfterGlow);
            SetTaskParams(this, partTaskTaskms.studyInstanceUID, partTaskTaskms.scanUID, scanedRawDataPath);

        }

        private void SetTaskParams(CaliScanTaskViewModel caliStepTaskVM, string studyInstanceUID, string scanUID, string scanedRawDataPath)
        {
            if (null == caliStepTaskVM.TaskParams)
            {
                caliStepTaskVM.TaskParams = new Dictionary<string, object>();
            }

            if (!caliStepTaskVM.TaskParams.TryAdd(CaliScenarioTaskViewModel.Common_StudyInstanceUID, studyInstanceUID))
            {
                caliStepTaskVM.TaskParams[CaliScenarioTaskViewModel.Common_StudyInstanceUID] = studyInstanceUID;
            }

            if (!caliStepTaskVM.TaskParams.TryAdd(CaliScenarioTaskViewModel.Common_ScanUID, scanUID))
            {
                caliStepTaskVM.TaskParams[CaliScenarioTaskViewModel.Common_ScanUID] = scanUID;
            }

            if (!caliStepTaskVM.TaskParams.TryAdd(CaliScenarioTaskViewModel.Common_RawDataDirectory, scanedRawDataPath))
            {
                caliStepTaskVM.TaskParams[CaliScenarioTaskViewModel.Common_RawDataDirectory] = scanedRawDataPath;
            }
        }

        private string mT03_RawDataPath;
        private string mS03_RawDataPath;
        private bool? mIsAfterGlow;
    }
}
