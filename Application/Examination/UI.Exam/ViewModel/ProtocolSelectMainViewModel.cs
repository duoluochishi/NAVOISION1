//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.Examination.ApplicationService.Contract.Models;
using NV.CT.SyncService.Contract;
using NV.MPS.Configuration;

namespace NV.CT.UI.Exam.ViewModel;

public class ProtocolSelectMainViewModel : BaseViewModel
{
	public bool IsProtocolChangeFromRgt = false;
	private readonly IDataSync _dataSync;
	private readonly ILogger<ProtocolSelectMainViewModel> _logger;
	private readonly IProtocolOperation _protocolOperation;
	private readonly IProtocolHostService _protocolHostService;
	private readonly IDialogService _dialogService;
	private readonly IPatientConfigService _patientConfigService;
	private readonly IStudyHostService _studyHostService;

	private List<ProtocolViewModel> _sourceProtocolList = new();
	public List<ProtocolViewModel> SourceProtocolList
	{
		get => _sourceProtocolList;
		set => SetProperty(ref _sourceProtocolList, value);
	}

	private string _selectHumanBody = string.Empty;
	public string SelectHumanBody
	{
		get => _selectHumanBody;
		set => SetProperty(ref _selectHumanBody, value);
	}

	private string _selectEnhancedCustomized = string.Empty;
	public string SelectEnhancedCustomized
	{
		get => _selectEnhancedCustomized;
		set => SetProperty(ref _selectEnhancedCustomized, value);
	}

	private bool _isAdult = true;
	private bool IsAdult
	{
		get => _isAdult;
		set => SetProperty(ref _isAdult, value);
	}

	private CTS.Enums.BodyPart _selectHumanBodyPart = CTS.Enums.BodyPart.Abdomen;
	public CTS.Enums.BodyPart SelectHumanBodyPart
	{
		get => _selectHumanBodyPart;
		set => SetProperty(ref _selectHumanBodyPart, value);
	}

	private ObservableCollection<ProtocolViewModel>? _protocolList = new();
	public ObservableCollection<ProtocolViewModel>? ProtocolList
	{
		get => _protocolList;
		set => SetProperty(ref _protocolList, value);
	}

	private List<ProtocolSettingItem> settingItems { get; set; } = new();

	private ProtocolViewModel? _selectedProtocol;
	public ProtocolViewModel? SelectedProtocol
	{
		get => _selectedProtocol;
		set
		{
			if (value == _selectedProtocol)
			{
				return;
			}
			SetProperty(ref _selectedProtocol, value);
			if (value is not null)
			{
				NotifySelectProtocol(value.Id);
			}
		}
	}

	private string _studyDescription = string.Empty;
	public string StudyDescription
	{
		get => _studyDescription;
		set => SetProperty(ref _studyDescription, value);
	}

	/// <summary>
	/// 成人小孩tab选中项索引，0：成人索引，1：小孩索引
	/// </summary>
	private int _selectAdultChildtTabIndex = 0;
	public int SelectAdultChildTabIndex
	{
		get => _selectAdultChildtTabIndex;
		set
		{
			if (value == _selectAdultChildtTabIndex)
				return;

			if (SetProperty(ref _selectAdultChildtTabIndex, value))
			{
				if (value == 0) //TBA
				{
					IsAdult = true;
				}
				else
				{
					IsAdult = false;
				}

				////RGT
				//_dataSync.SelectPatientType(value.ToString());
			}
		}
	}

	public ProtocolSelectMainViewModel(ILogger<ProtocolSelectMainViewModel> logger,
		IProtocolOperation protocolOperation,
		IProtocolHostService protocolHostService,
		IDialogService dialogService,
		IPatientConfigService patientConfigService,
		IStudyHostService studyHostService,
		IDataSync dataSync)
	{
		_logger = logger;
		_dataSync = dataSync;
		_protocolOperation = protocolOperation;
		_protocolHostService = protocolHostService;
		_dialogService = dialogService;
		_patientConfigService = patientConfigService;
		_studyHostService = studyHostService;
		StudyDescription = _studyHostService.Instance.StudyDescription;
		settingItems = _protocolOperation.GetProtocolSettingItems();

		_dataSync.SelectHumanBodyStarted += DataSync_SelectHumanBodyStarted;
		_dataSync.SelectProtocolStarted += DataSync_SelectProtocolStarted;
		_dataSync.ReplaceProtocolStarted += DataSync_ReplaceProtocolStarted;
		_protocolOperation.ProtocolChanged += ProtocolOperation_ProtocolChanged;
		Commands.Add(CommandParameters.COMMAND_SEARCH, new DelegateCommand<string>(RearchClick, _ => true));
		Commands.Add(CommandParameters.COMMAND_HUMAN_BODY_CLICK, new DelegateCommand<string>(HumanBodyClick, _ => true));
		Commands.Add(CommandParameters.COMMAND_ENHANCED_CUSTOMIZED, new DelegateCommand<string>(EnhancedCustomizedClick, _ => true));
		Commands.Add(CommandParameters.COMMAND_ADD_TASK, new DelegateCommand(AddProtocolToTaskList, () => true));
		Commands.Add(CommandParameters.COMMAND_REPLACE_TASK, new DelegateCommand(ReplaceProtocolToTaskList, () => true));
		Commands.Add(CommandParameters.COMMAND_TOP, new DelegateCommand(TopProtocol, IsTopingProtocol));
		Commands.Add(CommandParameters.COMMAND_UNPIN, new DelegateCommand(UnpinProtocol, IsUnpinProtocol));
		Commands.Add(CommandParameters.COMMAND_PROTOCOL_ITEM_DOUBLE_CLICK, new DelegateCommand(ReplaceProtocolToTaskList, () => true));
		Commands.Add(CommandParameters.COMMAND_SAVE_CONFIG, new DelegateCommand(SaveConfigClick, () => true));
		LoadedProtocolPresentationModel();

		InitAdultChildTabIndex(studyHostService.Instance);

		if (settingItems.FirstOrDefault(t => t.OtherBodyPart.Equals(StudyDescription)) is ProtocolSettingItem protocolSettingItem)
		{
			HumanBodyClick(protocolSettingItem.BodyPart.ToString());
		}
	}

	/// <summary>
	/// 通知RGT客户端 当前选择协议变化
	/// </summary>
	private void NotifySelectProtocol(string protocolTemplateId)
	{
		_dataSync.NotifySelectProtocol(protocolTemplateId);
	}

	/// <summary>
	/// 协议替换
	/// </summary>
	[UIRoute]
	private void DataSync_ReplaceProtocolStarted(object? sender, string e)
	{
		IsProtocolChangeFromRgt = true;
		if (!string.IsNullOrEmpty(e))
		{
			var sourceProtocol = GetProtocol(e);
			_protocolHostService.ReplaceProtocol(sourceProtocol, GetNotEnableMeasurements(e));
		}
	}

	/// <summary>
	/// RGT选中协议变化事件
	/// </summary>
	[UIRoute]
	private void DataSync_SelectProtocolStarted(object? sender, string e)
	{
		//选中协议,广播出去
		_dataSync.NotifySelectProtocol(e);

		if (SelectedProtocol is not null && SelectedProtocol.Id == e)
		{
			return;
		}

		SelectedProtocol = ProtocolList?.FirstOrDefault(n => n.Id == e);
	}

	private void InitAdultChildTabIndex(StudyModel studyModel)
	{
		if (studyModel is null)
		{
			return;
		}
		int limitAge = UserConfig.PatientSettingConfig.PatientSetting.AdultAgeThreshold.Value;
		double age = studyModel.Age;
		int patientAge = studyModel.Age;
		switch ((AgeType)studyModel.AgeType)
		{
			case AgeType.Day:
				age = patientAge / 365;
				break;
			case AgeType.Month:
				age = patientAge * 30 / 365;
				break;
			case AgeType.Week:
				age = patientAge * 7 / 365;
				break;
			case AgeType.Year:
			default:
				age = patientAge;
				break;
		}

		int ageNumber = (int)(age * 100);
		if (ageNumber > (limitAge * 100))
		{
			SelectAdultChildTabIndex = 0; //0：标识选中成人Tab
		}
		else
		{
			SelectAdultChildTabIndex = 1; //1：标识选中儿童tab
		}
	}

	[UIRoute]
	private void ProtocolOperation_ProtocolChanged(object? sender, string e)
	{
		LoadedProtocolPresentationModel();
		HumanBodyClick(SelectHumanBody);
	}

	private void LoadedProtocolPresentationModel()
	{
		try
		{
			ConvertToModelList(_protocolOperation.GetPresentations());
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"ProtocolSelection exception: {ex.Message}");
		}
	}

	private void ConvertToModelList(List<ProtocolPresentationModel> list)
	{
		SourceProtocolList.Clear();

		list.Sort((item1, item2) => { return item1.IsTopping ? -1 : 1; });
		foreach (ProtocolPresentationModel protocolPresent in list)
		{
			//todo:急症协议为某个部位的协议，需要显示，取消此判断，2024/1/27
			//if (protocolPresent.IsEmergency)
			//{
			//    continue;
			//}
			ProtocolViewModel protocolModel = new ProtocolViewModel();
			protocolModel.Id = protocolPresent.Id;
			protocolModel.ProtocolName = protocolPresent.Name;
			protocolModel.BodyPart = protocolPresent.BodyPart;
			protocolModel.IsAdult = protocolPresent.IsAdult;
			protocolModel.IsEnhanced = protocolPresent.IsEnhanced;
			protocolModel.IsFactory = protocolPresent.IsFactory;
			protocolModel.IsEmergency = protocolPresent.IsEmergency;
			protocolModel.PatientPosition = protocolPresent.PatientPosition;
			protocolModel.Description = protocolPresent.Description;

			if (settingItems.FirstOrDefault(t => t.Id.Equals(protocolPresent.Id) && t.OtherBodyPart.Equals(StudyDescription)) is not null)
			{
				protocolModel.IsOnTop = true;
				protocolModel.IsDefaultMatch = true;
			}
			else
			{
				protocolModel.IsOnTop = false;
				protocolModel.IsDefaultMatch = false;
			}
			foreach (MeasurementPresentationModel measurement in protocolPresent.Measurements)
			{
				int index = 0;
				foreach (ScanPresentationModel scanItem in measurement.Scans)
				{
					ScanRangeViewModel scanRangeModel = new ScanRangeViewModel();
					if (index == 0)
					{
						scanRangeModel.ScanRangeId = measurement.Id;
					}
					else
					{
						scanRangeModel.ScanRangeId = scanItem.Id;
						scanRangeModel.IsShowScanCheckBox = false;
					}
					scanRangeModel.ScanRangeIsChecked = true;
					scanRangeModel.ScanRange = scanItem.Name;
					scanRangeModel.Description = scanItem.Description;
					scanRangeModel.ReconIdList = "";
					int i = 0;
					scanRangeModel.ReconNameList = "";
					foreach (ReconPresentationModel reconItem in scanItem.Recons)
					{
						if (i == 0)
						{
							scanRangeModel.ReconIdList = reconItem.Id;
							scanRangeModel.ReconNameList = reconItem.Name;
						}
						else
						{
							scanRangeModel.ReconIdList = scanRangeModel.ReconIdList + "," + reconItem.Id;
							scanRangeModel.ReconNameList = scanRangeModel.ReconNameList + " " + reconItem.Name;
						}
						i++;
					}
					protocolModel.ScanRangeList.Add(scanRangeModel);
					index += 1;
				}
			}
			SourceProtocolList.Add(protocolModel);
		}
		SourceProtocolList.Sort((item1, item2) => { return item1.IsOnTop ? -1 : 1; });
	}

	/// <summary>
	/// RGT选中人体部位变化事件
	/// </summary>
	[UIRoute]
	private void DataSync_SelectHumanBodyStarted(object? sender, string e)
	{
		if (SelectHumanBody == e)
		{
			return;
		}
		HumanBodyClick(e);
	}

	private void HumanBodyClick(string bodyPartName)
	{
		SelectHumanBody = bodyPartName;
		switch (bodyPartName)
		{
			case CommandParameters.HUMAN_BODY_HEAD:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Head;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_NECK:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Neck;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_SHOULDER:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Shoulder;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_BREAST:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Chest;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_ABDOMEN:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Abdomen;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_HAND:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Arm;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_PELVIS:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Pelvis;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_LEG:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Leg;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_SPINE:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Spine;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_VEINHEAD:
				SelectHumanBodyPart = CTS.Enums.BodyPart.BHead;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_VEINNECK:
				SelectHumanBodyPart = CTS.Enums.BodyPart.BNeck;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_VEINBREAST:
				SelectHumanBodyPart = CTS.Enums.BodyPart.BChest;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_VEINABDOMEN:
				SelectHumanBodyPart = CTS.Enums.BodyPart.BAbdomen;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_VEINLEG:
				SelectHumanBodyPart = CTS.Enums.BodyPart.BLeg;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_VEINHAND:
				SelectHumanBodyPart = CTS.Enums.BodyPart.BArm;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_CHILDHEAD:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Head;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDNECK:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Neck;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDSHOULDER:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Shoulder;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDBREAST:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Chest;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDABDOMEN:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Abdomen;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDHAND:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Arm;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDPELVIS:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Pelvis;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDLEG:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Leg;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDSPINE:
				SelectHumanBodyPart = CTS.Enums.BodyPart.Spine;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDVEINHEAD:
				SelectHumanBodyPart = CTS.Enums.BodyPart.BHead;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDVEINNECK:
				SelectHumanBodyPart = CTS.Enums.BodyPart.BNeck;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDVEINBREAST:
				SelectHumanBodyPart = CTS.Enums.BodyPart.BChest;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDVEINABDOMEN:
				SelectHumanBodyPart = CTS.Enums.BodyPart.BAbdomen;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDVEINLEG:
				SelectHumanBodyPart = CTS.Enums.BodyPart.BLeg;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDVEINHAND:
				SelectHumanBodyPart = CTS.Enums.BodyPart.BArm;
				IsAdult = false;
				break;
		}

		LoadProtocols(SelectHumanBodyPart);

		if (IsAdult)
		{
			if (SelectAdultChildTabIndex == 1)
			{
				SelectAdultChildTabIndex = 0;
			}
		}
		else
		{
			if (SelectAdultChildTabIndex == 0)
			{
				SelectAdultChildTabIndex = 1;
			}
		}

		//RGT通知所有客户端 
		var dto = ProtocolList?.Select(n => new SyncProtocolModel()
		{
			Id = n.Id,
			ProtocolName = n.ProtocolName,
			BodyPart = n.BodyPart,
			IsAdult = n.IsAdult,
			IsEnhanced = n.IsEnhanced,
			IsEmergency = n.IsEmergency,
			PatientPosition = n.PatientPosition.ToString(),
			IsDefaultMatch = n.IsDefaultMatch,
			IsFactory = n.IsFactory,
			IsOnTop = n.IsOnTop,
			Description = n.Description,
			ScanMode = n.ScanMode
		}).ToList();
		_dataSync.NotifySelectHumanBody(new SyncProtocolResponse()
		{
			BodyPart = bodyPartName,
			ProtocolList = dto
		});
	}

	private void EnhancedCustomizedClick(string parameter)
	{
		SelectEnhancedCustomized = parameter;
	}

	public void DoSelectProtocol(string templateId)
	{
		SelectedProtocol = ProtocolList?.FirstOrDefault(t => t.Id.Equals(templateId));
	}

	private ProtocolModel GetProtocol(string templateId)
	{
		var protocolTemplate = _protocolOperation.GetProtocolTemplate(templateId);
		if (protocolTemplate is null)
		{
			return new ProtocolModel();
		}
		return protocolTemplate.Protocol.Clone();
	}

	private void AddProtocolToTaskList()
	{
		if (!(SelectedProtocol is not null && SelectedProtocol.Id.IsNotNullOrEmpty()))
		{
			return;
		}
		var sourceProtocol = GetProtocol(SelectedProtocol.Id);
		_protocolHostService.AddProtocol(sourceProtocol, GetNotEnableMeasurements(SelectedProtocol.Id));
	}

	public void ReplaceProtocolToTaskList()
	{
		IsProtocolChangeFromRgt = false;
		if (SelectedProtocol is null)
		{
			return;
		}
		var sourceProtocol = GetProtocol(SelectedProtocol.Id);
		_protocolHostService.ReplaceProtocol(sourceProtocol, GetNotEnableMeasurements(SelectedProtocol.Id));
	}

	private bool IsTopingProtocol()
	{
		bool topProtocol = true;
		if (SelectedProtocol is not null && SelectedProtocol.IsOnTop)
		{
			topProtocol = false;
		}
		return topProtocol;
	}

	private void TopProtocol()
	{
		if (!(SelectedProtocol is not null && SelectedProtocol.Id.IsNotNullOrEmpty()))
		{
			return;
		}
		var protocolTemplate = _protocolOperation.GetProtocolTemplate(SelectedProtocol.Id);
		_protocolOperation.Save(protocolTemplate, true);
	}

	private bool IsUnpinProtocol()
	{
		bool topProtocol = true;
		if (SelectedProtocol is not null && SelectedProtocol.IsOnTop == false)
		{
			topProtocol = false;
		}
		return topProtocol;
	}

	private void UnpinProtocol()
	{
		if (!(SelectedProtocol is not null && SelectedProtocol.Id.IsNotNullOrEmpty()))
		{
			return;
		}
		var protocolTemplate = _protocolOperation.GetProtocolTemplate(SelectedProtocol.Id);
		_protocolOperation.Save(protocolTemplate, false);
	}

	private void RearchClick(string name)
	{
		LoadProtocols(SelectHumanBodyPart, name);
	}

	private void LoadProtocols(CTS.Enums.BodyPart bodyPart, string name = null)
	{
		var bodyParts = new List<CTS.Enums.BodyPart> { bodyPart };
		switch (bodyPart)
		{
			case CTS.Enums.BodyPart.BHead:
				bodyParts.Add(CTS.Enums.BodyPart.Head);
				break;
			case CTS.Enums.BodyPart.BNeck:
				bodyParts.Add(CTS.Enums.BodyPart.Neck);
				break;
			case CTS.Enums.BodyPart.BChest:
				bodyParts.Add(CTS.Enums.BodyPart.Chest);
				break;
			case CTS.Enums.BodyPart.BAbdomen:
				bodyParts.Add(CTS.Enums.BodyPart.Abdomen);
				break;
			case CTS.Enums.BodyPart.BArm:
				bodyParts.Add(CTS.Enums.BodyPart.Arm);
				break;
			case CTS.Enums.BodyPart.BLeg:
				bodyParts.Add(CTS.Enums.BodyPart.Leg);
				break;
			default:
				break;
		}

		if (string.IsNullOrEmpty(name))
		{
			ProtocolList = SourceProtocolList.Where(t => bodyParts.Contains(t.BodyPart) && t.IsAdult == IsAdult).ToList().ToObservableCollection();
		}
		else
		{
			ProtocolList = SourceProtocolList.Where(t => bodyParts.Contains(t.BodyPart) && t.IsAdult == IsAdult && t.ProtocolName.ToLower().Contains(name.ToLower())).ToList().ToObservableCollection();
		}
	}

	private List<string> GetNotEnableMeasurements(string templateId)
	{
		List<string> ids = new List<string>();
		var protocol = SourceProtocolList.FirstOrDefault(t => t.Id == templateId);
		if (protocol is not null)
		{
			var li = protocol.ScanRangeList.Where(t => !t.ScanRangeIsChecked).Select(t => t.ScanRangeId).ToList();
			if (li is not null && li.Any())
			{
				ids = li;
			}
		}
		return ids;
	}

	private void SaveConfigClick()
	{
		if (string.IsNullOrEmpty(StudyDescription))
		{
			_dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", "The requester default scheme cannot be empty!", arg => { }, ConsoleSystemHelper.WindowHwnd);
			return;
		}
		List<ProtocolSettingItem> protocolSettingItems = new List<ProtocolSettingItem>();
		foreach (var item in SourceProtocolList.FindAll(t => t.IsDefaultMatch))
		{
			ProtocolSettingItem protocolSettingItem = new ProtocolSettingItem();
			protocolSettingItem.Id = item.Id;
			protocolSettingItem.Name = item.ProtocolName;
			protocolSettingItem.BodyPart = item.BodyPart;
			protocolSettingItem.OtherBodyPart = StudyDescription;
			protocolSettingItems.Add(protocolSettingItem);
		}
		if (protocolSettingItems.Count == 0)
		{
			_dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", "Please tick the match protocol!", arg => { }, ConsoleSystemHelper.WindowHwnd);
			return;
		}
		if (protocolSettingItems.Count > 0)
		{
			_protocolOperation.SaveSettingItemList(protocolSettingItems);
		}
		_dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", LanguageResource.Message_Saved_Successfully, arg => { }, ConsoleSystemHelper.WindowHwnd);
	}
}