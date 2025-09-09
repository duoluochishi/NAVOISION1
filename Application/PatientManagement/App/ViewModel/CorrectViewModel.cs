//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.JobService.Contract;
using NV.CT.JobService.Contract.Model;
using NV.CT.Language;
using NV.CT.PatientManagement.ApplicationService.Contract.Interfaces;
using NV.CT.PatientManagement.Models;
using NV.MPS.UI.Dialog.Service;

namespace NV.CT.PatientManagement.ViewModel;

public class CorrectViewModel : BaseViewModel
{
    #region Members 
    private readonly IDialogService _dialogService;
    private readonly IMapper _mapper;
    private readonly ILogger<CorrectViewModel> _logger;
    private readonly PatientConfig _patientConfig;
    private readonly IStudyApplicationService _studyApplicationService;
    private readonly ISeriesApplicationService _seriesApplicationService;
    private readonly IDicomFileService _dicomFileService;
    private const string ICON_ORIGINAL_STATUS_STYLE = "IconOriginalRecord";

    #endregion

    #region Properties

    private List<AgeType> _ageTypes = new List<AgeType>();
    public List<AgeType> AgeTypes
    {
        get => _ageTypes;
        set => SetProperty(ref _ageTypes, value);
    }

    /// <summary>
    /// 保持StudyList选择对象的引用
    /// </summary>
    private VStudyModel _originalStudyModel;

    public VStudyModel OriginalStudyModel
    {
        get
        {
            return _originalStudyModel;
        }
        set
        {
            this.SetProperty(ref _originalStudyModel, value);
        }
    }

    /// <summary>
    /// 本窗体修改的副本，避免直接修改StudyList选择对象的引用
    /// </summary>
    private VStudyModel _selectedStudyModel = new VStudyModel();
    public VStudyModel SelectedStudyModel
    {
        get 
        { 
            return _selectedStudyModel; 
        }
        set
        {
            if (this.SetProperty(ref _selectedStudyModel, value))
            {
                // 创建编辑区副本，避免影响CorrectionHistory
                MapStudyModel(this.SelectedStudyModel, this.EditableStudyModel);

                //如果没有校正时间，则是原记录，可以编辑，否则不允许编辑
                IsEditable = this.SelectedStudyModel.CreateTime is null ? true : false;
                this.MessageContent = string.Empty;
            }
        }
    
    }

    /// <summary>
    /// 提供编辑区绑定，避免影响CorrectionHistory列表
    /// </summary>
    private VStudyModel _editableStudyModel = new VStudyModel();

    public VStudyModel EditableStudyModel
    {
        get
        {
            return _editableStudyModel;
        }
        set
        {
            this.SetProperty(ref _editableStudyModel, value);
        }
    }

    private Dictionary<int, string> _genders = new Dictionary<int, string>();
    public Dictionary<int, string> Genders
    {
        get => _genders;
        set => SetProperty(ref _genders, value);
    }

    private string _messageContent = string.Empty;
    public string MessageContent
    {
        get
        { 
            return this._messageContent;
        }
        set
        {
            this.SetProperty(ref this._messageContent, value);
        }
    
    }

    private string _editor = string.Empty;
    public string Editor
    {
        get => _editor;
        set => SetProperty(ref _editor, value);
    }

    private bool _isEditable = false;
    public bool IsEditable
    {
        get => _isEditable;
        set => SetProperty(ref _isEditable, value);
    }

    private ObservableCollection<VStudyModel> _correctionHistoryStudies = new ObservableCollection<VStudyModel>();
    public ObservableCollection<VStudyModel> CorrectionHistoryStudies
    {
        get => _correctionHistoryStudies;
        set
        {
            SetProperty(ref _correctionHistoryStudies, value);
        }
    }

    #endregion

    #region Constructor

    public CorrectViewModel(IDialogService dialogService,
                            IMapper mapper,
                            ILogger<CorrectViewModel> logger,
                            IPatientConfigService patientConfigService,
                            IStudyApplicationService studyApplicationService,
                            ISeriesApplicationService seriesApplicationService,
                            IDicomFileService dicomFileService
                            )
    {
        _mapper = mapper;
        _dialogService = dialogService;
        _logger = logger;
        _patientConfig = patientConfigService.GetConfigs();
        _studyApplicationService = studyApplicationService;
        _seriesApplicationService = seriesApplicationService;
        _dicomFileService = dicomFileService;

        Commands.Add("SaveCommand", new DelegateCommand(OnSaveClick)); //, () => this.EditableStudyModel.IsValid && this.IsEditable)
        Commands.Add("ClickPatientIdCommand", new DelegateCommand(OnClickPatientID));
        Commands.Add("CloseCommand", new DelegateCommand<object>(OnClose, _ => true));

        this.InitData();
    }

    #endregion

    #region Public methods

    public void SetSelectedStudy(VStudyModel studyModel)
    {   
        this.OriginalStudyModel = studyModel;

        if (this.SelectedStudyModel is null)
        {
            this.SelectedStudyModel = new VStudyModel();
        }

        // 从StudyList选择对象的引用创建绑定CorrectionHistory的副本，避免直接修改StudyList选择对象的引用
        MapStudyModel(this.OriginalStudyModel, this.EditableStudyModel);
        MapStudyModel(this.OriginalStudyModel, this.SelectedStudyModel);

        this.ClearData();
        this.FetchCorrectionHistory(studyModel.StudyId);
    }

    #endregion

    #region Private methods   

    private void InitData()
    {
        //TODO   这是临时用法，以后抽空用Converter实现
        var dictionaryGender = EnumExtension.ToDictionary<int>(typeof(Gender));
        Genders.Clear();
        foreach (var gender in dictionaryGender)
        {
            switch (gender.Key)
            {
                case (int)Gender.Male:
                    Genders.Add(gender.Key, LanguageResource.Content_GenderMale);
                    break;
                case (int)Gender.Female:
                    Genders.Add(gender.Key, LanguageResource.Content_GenderFemale);
                    break;
                case (int)Gender.Other:
                    Genders.Add(gender.Key, LanguageResource.Content_GenderOther);
                    break;
                default:
                    Genders.Add(gender.Key, LanguageResource.Content_GenderOther);
                    break;
            }
        }

        var ageTypeStringList = Enum.GetNames(typeof(AgeType));
        foreach (var ageTypeString in ageTypeStringList)
        {
            AgeTypes.Add((AgeType)Enum.Parse(typeof(AgeType), ageTypeString, true));
        }
    }

    private void OnSaveClick()
    {
        MessageContent = "Correcting...";
        var patient = _mapper.Map<ApplicationService.Contract.Models.PatientModel>(EditableStudyModel);
        patient.PatientBirthDate = EditableStudyModel.Birthday.Value;

        var study = _mapper.Map<ApplicationService.Contract.Models.StudyModel>(EditableStudyModel);
        if (string.IsNullOrEmpty(EditableStudyModel.FirstName))
        {
            patient.PatientName = EditableStudyModel.LastName;
        }
        else
        {
            patient.PatientName = EditableStudyModel.FirstName + "^" + EditableStudyModel.LastName;
        }

        study.CorrectStatus = (int)CorrectStatus.Corrected;
        EditableStudyModel.CorrectStatus = (int)CorrectStatus.Corrected;
        EditableStudyModel.PatientName = patient.PatientName;

        bool sucessful = _studyApplicationService.Correct(patient, study, EditableStudyModel.Editor);
        if (!sucessful)
        {
            this._logger.LogDebug($"Failed to correct study with patientID:{patient.PatientId}");
            MessageContent = "Failed to correct study! ";
            return;
        }
        sucessful = UpdateDICOM();
        if (sucessful)
        {
            this.EditableStudyModel.Editor = string.Empty;
            this.MapStudyModel(this.EditableStudyModel, this.SelectedStudyModel);
            this.MapStudyModel(this.EditableStudyModel, this.OriginalStudyModel);
            this.FetchCorrectionHistory(EditableStudyModel.StudyId);

            MessageContent = "Correct the study successfully! ";
        }
        else
        {
            this._logger.LogDebug($"Failed to update DICOM with patientID:{patient.PatientId}");
            MessageContent = "Failed to update DICOM! ";
            return;
        }

    }

    private void OnClickPatientID()
    {
        if (string.IsNullOrWhiteSpace(SelectedStudyModel.PatientId))
        {
            SelectedStudyModel.PatientId = this.GetPatientId();
        }
    }

    private string GetPatientId()
    {
        if (_patientConfig is not null && _patientConfig.PatientIdConfig is not null)
        {
            return $"{_patientConfig.PatientIdConfig.Prefix}_{_patientConfig.PatientIdConfig.Infix}{IdGenerator.Next()}";

        }
        return IdGenerator.Next();
    }

    private void OnClose(object parameter)
    {
        this.MessageContent = string.Empty;
        if (parameter is Window window)
        {           
            window.Hide();
        }
    }

    private bool UpdateDICOM()
    {
        var seriesArray = this._seriesApplicationService.GetSeriesByStudyId(this.OriginalStudyModel.StudyId).Select(s => s.SeriesPath).ToArray();
        if (seriesArray is null || seriesArray.Length == 0)
        {
            return true;        
        }

        var request = new UpdateDicomRequest();        
        request.SeriesFolders.AddRange(seriesArray);
        request.PatientID = this.EditableStudyModel.PatientId;
        request.PatientName = this.EditableStudyModel.PatientName;
        request.PatientBirthDate = this.EditableStudyModel.Birthday.Value.ToString("yyyyMMdd");
        request.PatientBirthTime = this.EditableStudyModel.Birthday.Value.ToString("hhmmss");
        request.PatientSex = this.ConvertSexString(this.EditableStudyModel.Gender);
        request.PatientAge = $"{this.EditableStudyModel.Age.Value.ToString("000")}{ ConvertAgeTypeString((int)this.EditableStudyModel.AgeType) }";
        request.PatientSize = this.EditableStudyModel.Height.HasValue ? this.EditableStudyModel.Height.Value.ToString("0.00") : string.Empty;
        request.PatientWeight = this.EditableStudyModel.Weight.HasValue ? this.EditableStudyModel.Weight.Value.ToString("0.00") : string.Empty;
        request.AccessionNumber = this.EditableStudyModel.AccessionNo;
        request.ReferringPhysicianName = this.EditableStudyModel.ReferringPhysician;
        request.StudyDescription = this.EditableStudyModel.StudyDescription;

        var result = _dicomFileService.UpdateDICOM(request);

        return result.Status == CommandExecutionStatus.Success;
    }

    private void FetchCorrectionHistory(string studyId)
    {        
        var result = _studyApplicationService.GetCorrectionHistoryList(studyId);
        var list = result.Select(r =>
        {
            var firstName = string.Empty;
            string lastName;
            if (r.Item1.PatientName.Contains("^"))
            {
                string[] arr = r.Item1.PatientName.Split('^');
                firstName = arr[0];
                lastName = arr[1];
            }
            else
            {
                lastName = r.Item1.PatientName;
            }

            int bodyPartKey = -1;
            if (Enum.TryParse<BodyPart>(r.Item2.BodyPart, true, out var bodyPart))
            {
                bodyPartKey = (int)bodyPart;
            }

            return new VStudyModel
            {
                PatientName = r.Item1.PatientName,
                Age = r.Item2.Age,
                AgeType = (AgeType)r.Item2.AgeType,
                Weight = r.Item2.PatientWeight,
                Height = r.Item2.PatientSize,
                Pid = r.Item1.Id,
                StudyId = r.Item2.Id,
                PatientId = r.Item1.PatientId,
                LastName = lastName,
                FirstName = firstName,
                Gender = ((int)r.Item1.PatientSex),
                CreateTime = r.Item1.CreateTime,
                AdmittingDiagnosis = r.Item2.AdmittingDiagnosisDescription,
                Ward = r.Item2.Ward,
                BodyPart = r.Item2.BodyPart,
                BodyPartKey = bodyPartKey,
                HisStudyId = r.Item2.StudyId,
                AccessionNo = r.Item2.AccessionNo,
                Comments = r.Item2.Comments,
                InstitutionName = r.Item2.InstitutionName,
                PatientType = r.Item2.PatientType,
                InstitutionAddress = r.Item2.InstitutionAddress,
                Birthday = r.Item1.PatientBirthDate,
                StudyDescription = r.Item2.StudyDescription,
                StudyStatus = r.Item2.StudyStatus,
                IsProtected = r.Item2.IsProtected,
                ArchiveStatus = (JobTaskStatus)r.Item2.ArchiveStatus,
                PrintStatus = (JobTaskStatus)r.Item2.PrintStatus,
                CorrectStatus = r.Item2.CorrectStatus,
                StudyDate = r.Item2.StudyDate,
                StudyTime = r.Item2.StudyTime,
                ReferringPhysician = r.Item2.ReferringPhysicianName,
                ExamStartTime = r.Item2.ExamStartTime,
                ExamEndTime = r.Item2.ExamEndTime,
                Editor = r.Item1.Editor,
            };
        }).ToList();

        list.Insert(0, this.SelectedStudyModel);
        CorrectionHistoryStudies = list.ToObservableCollection();
        this.ResetOriginalRecordStyle();
    }

    private void MapStudyModel(VStudyModel sourceModel, VStudyModel targetModel)
    {

        targetModel.Pid = sourceModel.Pid;
        targetModel.FirstName = sourceModel.FirstName;
        targetModel.LastName = sourceModel.LastName;
        targetModel.PatientName = sourceModel.PatientName;
        targetModel.PatientId = sourceModel.PatientId;
        targetModel.Birthday = sourceModel.Birthday;
        targetModel.Age = sourceModel.Age;
        targetModel.AgeType = sourceModel.AgeType;
        targetModel.Gender = sourceModel.Gender;
        targetModel.PatientType = sourceModel.PatientType;
        targetModel.CreateTime = sourceModel.CreateTime;
        targetModel.Editor = sourceModel.Editor;
        targetModel.StudyId = sourceModel.StudyId;
        targetModel.StudyId_Dicom = sourceModel.StudyId_Dicom;
        targetModel.BodyPart = sourceModel.BodyPart;
        targetModel.BodyPartKey = sourceModel.BodyPartKey;
        targetModel.Height = sourceModel.Height;
        targetModel.HeightUnit = sourceModel.HeightUnit;
        targetModel.Weight = sourceModel.Weight;
        targetModel.WeightUnit = sourceModel.WeightUnit;
        targetModel.AdmittingDiagnosis = sourceModel.AdmittingDiagnosis;
        targetModel.Ward = sourceModel.Ward;
        targetModel.FieldStrenght = sourceModel.FieldStrenght;
        targetModel.AccessionNo = sourceModel.AccessionNo;
        targetModel.ExamStartTime = sourceModel.ExamStartTime;
        targetModel.ExamEndTime = sourceModel.ExamEndTime;
        targetModel.Technician = sourceModel.Technician;
        targetModel.ReferringPhysician = sourceModel.ReferringPhysician;
        targetModel.StudyStatus = sourceModel.StudyStatus;
        targetModel.PatientStatus = sourceModel.PatientStatus;
        targetModel.PatientAddress = sourceModel.PatientAddress;
        targetModel.MedicalAlerts = sourceModel.MedicalAlerts;
        targetModel.PerformingPhysician = sourceModel.PerformingPhysician;
        targetModel.StudyInstanceUID = sourceModel.StudyInstanceUID;
        targetModel.StudyDescription = sourceModel.StudyDescription;
        targetModel.ArchiveStatus = sourceModel.ArchiveStatus;
        targetModel.PrintStatus = sourceModel.PrintStatus;
        targetModel.CorrectStatus = sourceModel.CorrectStatus;
        targetModel.InstitutionName = sourceModel.InstitutionName;
        targetModel.InstitutionAddress = sourceModel.InstitutionAddress;
        targetModel.Comments = sourceModel.Comments;
        targetModel.HisStudyId = sourceModel.HisStudyId;
        targetModel.IsProtected = sourceModel.IsProtected;
        targetModel.StudyDate = sourceModel.StudyDate;
        targetModel.StudyTime = sourceModel.StudyTime;
        targetModel.IsOriginalRecord = sourceModel.IsOriginalRecord;
        targetModel.OriginalRecordStyle = sourceModel.OriginalRecordStyle;

    }

    private string ConvertSexString(int gender)
    {
        string dicomSex;
        switch (gender)
        {
            case (int)Gender.Male:
                dicomSex = Constants.DICOM_SEX_MALE;
                break;
            case (int)Gender.Female:
                dicomSex = Constants.DICOM_SEX_FEMALE;
                break;
            case (int)Gender.Other:
                dicomSex = Constants.DICOM_SEX_OTHER;
                break;
            default:
                dicomSex = string.Empty;
                break;
        }

        return dicomSex;
    }
    private string ConvertAgeTypeString(int ageType)
    {
        string dicomAgeType;
        switch (ageType)
        {
            case (int)AgeType.Year:
                dicomAgeType = Constants.DICOM_AGETYPE_YEAR;
                break;
            case (int)AgeType.Month:
                dicomAgeType = Constants.DICOM_AGETYPE_MONTH;
                break;
            case (int)AgeType.Week:
                dicomAgeType = Constants.DICOM_AGETYPE_WEEK;
                break;
            case (int)AgeType.Day:
                dicomAgeType = Constants.DICOM_AGETYPE_DAY;
                break;
            default:
                dicomAgeType = string.Empty;
                break;
        }

        return dicomAgeType;
    }

    private void RefreshIsvalidStatus()
    {
        this.SelectedStudyModel.CalcIsValid();
        this.EditableStudyModel.CalcIsValid();    
    }

    private void ClearData()
    {
        this.MessageContent = string.Empty;
        this.SelectedStudyModel.CreateTime = null;
        this.SelectedStudyModel.Editor = string.Empty;
        this.EditableStudyModel.CreateTime = null;
        this.EditableStudyModel.Editor = string.Empty;
        this.IsEditable = true;
        this.RefreshIsvalidStatus();
    }

    private void ResetOriginalRecordStyle()
    {
        CorrectionHistoryStudies.ForEach(s => { s.IsOriginalRecord = false; s.OriginalRecordStyle = null; });
        var lastRecord = CorrectionHistoryStudies.Last();
        lastRecord.IsOriginalRecord = true;
        lastRecord.OriginalRecordStyle = Application.Current.Resources[ICON_ORIGINAL_STATUS_STYLE] as Style;
    }

    #endregion
}



