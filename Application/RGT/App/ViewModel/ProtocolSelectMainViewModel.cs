using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.ProtocolService.Contract;
using BodyPart = NV.CT.CTS.Enums.BodyPart;
using StudyModel = NV.CT.DatabaseService.Contract.Models.StudyModel;

namespace NV.CT.RGT.ViewModel;

public class ProtocolSelectMainViewModel:BaseViewModel
{
	private readonly IDataSync _dataSync;
	private readonly ILogger<ProtocolSelectMainViewModel> _logger;

	private ObservableCollection<ProtocolViewModel> _sourceProtocolList = new();
	public ObservableCollection<ProtocolViewModel> SourceProtocolList
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

	private BodyPart _selectHumanBodyPart = BodyPart.Abdomen;
	public BodyPart SelectHumanBodyPart
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

	/// <summary>
	/// 选中协议
	/// </summary>
	private ProtocolViewModel? _selectedProtocol = new();
	public ProtocolViewModel? SelectedProtocol
	{
		get => _selectedProtocol;
		set
		{
			if (value == _selectedProtocol)
				return;

			SetProperty(ref _selectedProtocol, value);
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
	private int _selectAdultChildTabIndex = 0;
	public int SelectAdultChildTabIndex
	{
		get => _selectAdultChildTabIndex;
		set
		{
			if (value == _selectAdultChildTabIndex)
				return;

			if (SetProperty(ref _selectAdultChildTabIndex, value))
			{
				if (value == 0)
				{
					IsAdult = true;
				}
				else
				{
					IsAdult = false;
				}
			}
		}
	}

	public ProtocolSelectMainViewModel(IDataSync dataSync, ILogger<ProtocolSelectMainViewModel> logger)
	{
		_dataSync = dataSync;
		_logger = logger;

		_dataSync.SelectHumanBodyFinished += DataSync_SelectHumanBodyFinished;
		_dataSync.SelectProtocolFinished += DataSync_SelectProtocolFinished;

		Commands.Add(CommandParameters.COMMAND_SEARCH, new DelegateCommand<string>(ResearchClick, _ => true));
		Commands.Add(CommandParameters.COMMAND_HUMAN_BODY_CLICK, new DelegateCommand<string>(HumanBodyClick, _ => true));
		Commands.Add(CommandParameters.COMMAND_TOP, new DelegateCommand(TopProtocol, IsTopingProtocol));
		Commands.Add(CommandParameters.COMMAND_UNPIN, new DelegateCommand(UnpinProtocol, IsUnpinProtocol));
	}

	/// <summary>
	/// RGT端选中某条协议之后,发送通知到MCS
	/// </summary>
	public void SendSelectedProtocolToMcs(string templateId)
	{
		if (!string.IsNullOrEmpty(templateId))
		{
			_dataSync.SelectProtocol(templateId);
		}
	}

	/// <summary>
	/// MCS通知过来的选中协议变化事件
	/// </summary>
	[UIRoute]
	private void DataSync_SelectProtocolFinished(object? sender, string e)
	{
		if (SelectedProtocol is not null && SelectedProtocol.Id == e)
			return;

		SelectedProtocol = ProtocolList?.FirstOrDefault(n => n.Id == e);
	}

	private void InitAdultChildTabIndex(StudyModel studyModel)
	{
		//if (studyModel is null)
		//{
		//    return;
		//}
		//StudyDescription = studyModel.StudyDescription;
		//int limitAge = _patientConfigService.GetConfigs().ChildrenAgeLimit.UpperLimit;
		//double age = studyModel.Age;

		//int patientAge = studyModel.Age;
		//switch ((AgeType)studyModel.AgeType)
		//{
		//    case AgeType.Day:
		//        age = patientAge / 365;
		//        break;
		//    case AgeType.Month:
		//        age = patientAge * 30 / 365;
		//        break;
		//    case AgeType.Week:
		//        age = patientAge * 7 / 365;
		//        break;
		//    case AgeType.Year:
		//    default:
		//        age = patientAge;
		//        break;
		//}

		//if (age >= limitAge)
		//{
		//    SelectAdultChildTabIndex = 0; //0：标识选中成人Tab
		//}
		//else
		//{
		//    SelectAdultChildTabIndex = 1; //1：标识选中儿童tab
		//}
	}

	private void ConvertToModelList(List<ProtocolPresentationModel> list)
	{
		SourceProtocolList.Clear();

		list.Sort((item1, item2) => { return item1.IsTopping ? -1 : 1; });
		foreach (ProtocolPresentationModel protocolPresent in list)
		{
			if (protocolPresent.IsEmergency)
			{
				continue;
			}
			ProtocolViewModel protocolModel = new ProtocolViewModel();
			protocolModel.Id = protocolPresent.Id;
			protocolModel.ProtocolName = protocolPresent.Name;
			protocolModel.BodyPart = protocolPresent.BodyPart;
			protocolModel.IsAdult = protocolPresent.IsAdult;
			protocolModel.IsOnTop = protocolPresent.IsTopping;
			protocolModel.IsEnhanced = protocolPresent.IsEnhanced;
			protocolModel.IsFactory = protocolPresent.IsFactory;
			protocolModel.IsEmergency = protocolPresent.IsEmergency;
			protocolModel.PatientPosition = protocolPresent.PatientPosition;
			protocolModel.Description = protocolPresent.Description;

			foreach (var measurement in protocolPresent.Measurements)
			{
				foreach (ScanPresentationModel scanItem in measurement.Scans)
				{
					ScanRangeViewModel scanRangeModel = new ScanRangeViewModel();
					scanRangeModel.ScanRangeId = scanItem.Id;
					scanRangeModel.ScanRange = scanItem.Name;
					scanRangeModel.Description = scanItem.Description;
					scanRangeModel.ReconIdList = "";
					scanRangeModel.ReconNameList = "";
					int i = 0;
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
				}
				SourceProtocolList.Add(protocolModel);
			}
		}
	}

	/// <summary>
	/// MCS通知过来的选中人体部位
	/// </summary>
	private void DataSync_SelectHumanBodyFinished(object? sender, SyncProtocolResponse res)
	{
		//无论什么情况转变协议列表为本地列表
		ConvertSyncProtocolToProtocolList(res.ProtocolList);

		if (SelectHumanBody == res.BodyPart)
			return;

		SetHumanBodyData(res.BodyPart);
	}

	/// <summary>
	/// 将MCS发送过来的协议数据转为本地列表
	/// </summary>
	[UIRoute]
	private void ConvertSyncProtocolToProtocolList(List<SyncProtocolModel> list)
	{
		SourceProtocolList.Clear();

		foreach (var protocolPresent in list)
		{
			var protocolModel = new ProtocolViewModel();
			protocolModel.Id = protocolPresent.Id;
			protocolModel.ProtocolName = protocolPresent.ProtocolName;
			protocolModel.BodyPart = protocolPresent.BodyPart;
			protocolModel.IsAdult = protocolPresent.IsAdult;
			protocolModel.IsOnTop = protocolPresent.IsOnTop;
			protocolModel.IsEnhanced = protocolPresent.IsEnhanced;
			protocolModel.IsFactory = protocolPresent.IsFactory;
			protocolModel.IsEmergency = protocolPresent.IsEmergency;
			protocolModel.PatientPosition = System.Enum.Parse<PatientPosition>(protocolPresent.PatientPosition);
			protocolModel.Description = protocolPresent.Description;

			//foreach (var measurement in protocolPresent.Measurements)
			//{
			//	foreach (ScanPresentationModel scanItem in measurement.Scans)
			//	{
			//		ScanRangeViewModel scanRangeModel = new ScanRangeViewModel();
			//		scanRangeModel.ScanRangeId = scanItem.Id;
			//		scanRangeModel.ScanRange = scanItem.Name;
			//		scanRangeModel.Description = scanItem.Description;
			//		scanRangeModel.ReconIdList = "";
			//		scanRangeModel.ReconNameList = "";
			//		int i = 0;
			//		foreach (ReconPresentationModel reconItem in scanItem.Recons)
			//		{
			//			if (i == 0)
			//			{
			//				scanRangeModel.ReconIdList = reconItem.Id;
			//				scanRangeModel.ReconNameList = reconItem.Name;
			//			}
			//			else
			//			{
			//				scanRangeModel.ReconIdList = scanRangeModel.ReconIdList + "," + reconItem.Id;
			//				scanRangeModel.ReconNameList = scanRangeModel.ReconNameList + " " + reconItem.Name;
			//			}
			//			i++;
			//		}
			//		protocolModel.ScanRangeList.Add(scanRangeModel);
			//	}
			//}

			SourceProtocolList.Add(protocolModel);
		}

		ProtocolList = SourceProtocolList;
	}

	private void SetHumanBodyData(string bodyPartName)
	{
		SelectHumanBody = bodyPartName;
		switch (bodyPartName)
		{
			case CommandParameters.HUMAN_BODY_HEAD:
				SelectHumanBodyPart = BodyPart.Head;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_NECK:
				SelectHumanBodyPart = BodyPart.Neck;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_SHOULDER:
				SelectHumanBodyPart = BodyPart.Shoulder;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_BREAST:
				SelectHumanBodyPart = BodyPart.Breast;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_ABDOMEN:
				SelectHumanBodyPart = BodyPart.Abdomen;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_HAND:
				SelectHumanBodyPart = BodyPart.Arm;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_PELVIS:
				SelectHumanBodyPart = BodyPart.Pelvis;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_LEG:
				SelectHumanBodyPart = BodyPart.Leg;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_SPINE:
				SelectHumanBodyPart = BodyPart.Spine;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_VEINHEAD:
				SelectHumanBodyPart = BodyPart.Head;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_VEINNECK:
				SelectHumanBodyPart = BodyPart.Neck;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_VEINBREAST:
				SelectHumanBodyPart = BodyPart.Breast;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_VEINABDOMEN:
				SelectHumanBodyPart = BodyPart.Abdomen;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_VEINLEG:
				SelectHumanBodyPart = BodyPart.Leg;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_VEINHAND:
				SelectHumanBodyPart = BodyPart.Arm;
				IsAdult = true;
				break;
			case CommandParameters.HUMAN_BODY_CHILDHEAD:
				SelectHumanBodyPart = BodyPart.Head;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDNECK:
				SelectHumanBodyPart = BodyPart.Neck;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDSHOULDER:
				SelectHumanBodyPart = BodyPart.Shoulder;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDBREAST:
				SelectHumanBodyPart = BodyPart.Breast;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDABDOMEN:
				SelectHumanBodyPart = BodyPart.Abdomen;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDHAND:
				SelectHumanBodyPart = BodyPart.Arm;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDPELVIS:
				SelectHumanBodyPart = BodyPart.Pelvis;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDLEG:
				SelectHumanBodyPart = BodyPart.Leg;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDSPINE:
				SelectHumanBodyPart = BodyPart.Spine;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDVEINHEAD:
				SelectHumanBodyPart = BodyPart.Head;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDVEINNECK:
				SelectHumanBodyPart = BodyPart.Neck;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDVEINBREAST:
				SelectHumanBodyPart = BodyPart.Breast;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDVEINABDOMEN:
				SelectHumanBodyPart = BodyPart.BAbdomen;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDVEINLEG:
				SelectHumanBodyPart = BodyPart.Leg;
				IsAdult = false;
				break;
			case CommandParameters.HUMAN_BODY_CHILDVEINHAND:
				SelectHumanBodyPart = BodyPart.Arm;
				IsAdult = false;
				break;
		}

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
	}

	/// <summary>
	/// RGT点击人体部位触发的方法
	/// </summary>
	[UIRoute]
	private void HumanBodyClick(string bodyPartName)
	{
		SetHumanBodyData(bodyPartName);

		//通知SyncService
		_dataSync.SelectHumanBody(bodyPartName);
	}

	public void DoSelectProtocol(string templateId)
	{
		SelectedProtocol = ProtocolList?.FirstOrDefault(t => t.Id.Equals(templateId));
	}

	private bool IsTopingProtocol()
	{
		bool topProtocol = true;
		//if (SelectedProtocol is not null && SelectedProtocol.IsOnTop)
		//{
		//	topProtocol = false;
		//}
		return topProtocol;
	}

	private void TopProtocol()
	{
		//if (!(SelectedProtocol is not null && SelectedProtocol.Id.IsNotNullOrEmpty()))
		//{
		//	return;
		//}
		//var protocolTemplate = _protocolOperation.GetProtocolTemplate(SelectedProtocol.Id);
		//_protocolOperation.Save(protocolTemplate, true);
	}

	private bool IsUnpinProtocol()
	{
		var topProtocol = true;
		if (SelectedProtocol is not null && SelectedProtocol.IsOnTop == false)
		{
			topProtocol = false;
		}
		return topProtocol;
	}

	private void UnpinProtocol()
	{
		//if (!(SelectedProtocol is not null && SelectedProtocol.Id.IsNotNullOrEmpty()))
		//{
		//	return;
		//}
		//var protocolTemplate = _protocolOperation.GetProtocolTemplate(SelectedProtocol.Id);
		//_protocolOperation.Save(protocolTemplate, false);
	}

	private void ResearchClick(string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			ProtocolList = SourceProtocolList.Where(t => t.BodyPart == SelectHumanBodyPart && t.IsAdult == IsAdult && t.ProtocolName.ToLower().Contains(name.ToLower())).ToList().ToObservableCollection();
		}
		else
		{
			ProtocolList = SourceProtocolList.Where(t => t.BodyPart == SelectHumanBodyPart && t.IsAdult == IsAdult).ToList().ToObservableCollection();
		}
	}
}