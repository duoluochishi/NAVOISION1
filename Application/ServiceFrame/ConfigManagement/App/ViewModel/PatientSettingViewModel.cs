//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using NV.CT.ConfigService.Models.UserConfig;

namespace NV.CT.ConfigManagement.ViewModel
{
    public class PatientSettingViewModel : BaseViewModel
    {       
        public PatientSettingViewModel()
        {           
            List<string> limits = new List<string>();
            for (int i = 0; i < 20; i++)
            {
                limits.Add((i + 1).ToString());
            }
            AgeLimits = new ObservableCollection<string>(limits);

            IntervalCheckStates.Add("10", false);
            IntervalCheckStates.Add("30", false);
            IntervalCheckStates.Add("60", false);
            IntervalCheckStates.Add("600", false);
            IntervalCheckStates.Add("1800", false);

            Commands.Add("SaveCommand", new DelegateCommand(SaveCommand));
            Commands.Add("SelectIntervalCommand", new DelegateCommand<string>(SelectIntervalCommand));
            Commands.Add("SelectSuffixTypeCommand", new DelegateCommand<object>(SelectSuffixTypeCommand));
            Commands.Add("LoadCommand", new DelegateCommand<PatientConfig>(LoadCommand));
        }
        private OperationType operationType;
        public OperationType OperationType
        {
            get => operationType;
            set => SetProperty(ref operationType, value);
        }
        public void SaveCommand()
        {
            SavePatientConfigCommand();
        }
        public void SelectIntervalCommand(string interval)
        {
          
        }

        public void SelectSuffixTypeCommand(object suffixType)
        {
           
        }

        public void SavePatientConfigCommand()
        {

        }
        public void LoadCommand(PatientConfig patientConfig)
        {
            if (null != patientConfig)
            {
              
            }
        }

        public void LoadAllList()
        {
          
        }

        public void SetCurrentPreview()
        {
           
        }

        private PatientConfig currentPatientConfig;
        public PatientConfig CurrentPatientConfig
        {
            get => currentPatientConfig;
            set => SetProperty(ref currentPatientConfig, value);
        }

        private ObservableCollection<string> ageLimits;
        public ObservableCollection<string> AgeLimits
        {
            get => ageLimits;
            set => SetProperty(ref ageLimits, value);
        }

        private string currentAgeLimit = string.Empty;
        public string CurrentAgeLimit
        {
            get => currentAgeLimit;
            set => SetProperty(ref currentAgeLimit, value);
        }
        private string currentInterval = string.Empty;
        public string CurrentInterval
        {
            get => currentInterval;
            set => SetProperty(ref currentInterval, value);
        }
        private ObservableDictionary<string, bool> intervalCheckStates = new ObservableDictionary<string, bool>();
        public ObservableDictionary<string, bool> IntervalCheckStates
        {
            get => intervalCheckStates;
            set => SetProperty(ref intervalCheckStates, value);
        }

        private string currentPrefix = string.Empty;
        public string CurrentPrefix
        {
            get => currentPrefix;
            set
            {
                SetProperty(ref currentPrefix, value);
                SetCurrentPreview();
            }
        }
        private string currentInfix = string.Empty;
        public string CurrentInfix
        {
            get => currentInfix;
            set
            {
                SetProperty(ref currentInfix, value);
                SetCurrentPreview();
            }
        }

        private string currentSuffixType = string.Empty;
        public string CurrentSuffixType
        {
            get => currentSuffixType;
            set
            {
                SetProperty(ref currentSuffixType, value);
                SetCurrentPreview();
            }
        }
        private string currentPreview = string.Empty;
        public string CurrentPreview
        {
            get => currentPreview;
            set => SetProperty(ref currentPreview, value);
        }
        private ObservableDictionary<string, bool> suffixTypeCheckStates = new ObservableDictionary<string, bool>();
        public ObservableDictionary<string, bool> SuffixTypeCheckStates
        {
            get => suffixTypeCheckStates;
            set
            {
                SetProperty(ref suffixTypeCheckStates, value);
                SetCurrentPreview();
            }
        }
    }
}