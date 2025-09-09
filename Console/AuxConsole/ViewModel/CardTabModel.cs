namespace NV.CT.AuxConsole.ViewModel;

public class CardTabModel : BindableBase
{
	private int _processId;
	public int ProcessId
	{
		get => _processId;
		set => SetProperty(ref _processId, value);
	}

	private string _patientName = string.Empty;
	public string PatientName
	{
		get => _patientName;
		set => SetProperty(ref _patientName, value);
	}

	private string _patientId = string.Empty;
	public string PatientId
	{
		get => _patientId;
		set => SetProperty(ref _patientId, value);
	}

	private int _age;
	public int Age
	{
		get => _age;
		set => SetProperty(ref _age, value);
	}

	private string _ageType = string.Empty;
	public string AgeType
	{
		get => _ageType;
		set => SetProperty(ref _ageType, value);
	}

	private int _itemStatus;
	public int ItemStatus
	{
		get => _itemStatus;
		set => SetProperty(ref _itemStatus, value);
	}

	private string _statusBackground = string.Empty;
	public string StatusBackground
	{
		get => _statusBackground;
		set => SetProperty(ref _statusBackground, value);
	}

	private string _iconGeometry = string.Empty;
	public string IconGeometry
	{
		get => _iconGeometry;
		set => SetProperty(ref _iconGeometry, value);
	}

	private string _itemName = string.Empty;
	public string ItemName
	{
		get => _itemName;
		set => SetProperty(ref _itemName, value);
	}

	private string _cardParameters = string.Empty;
	public string CardParameters
	{
		get => _cardParameters;
		set => SetProperty(ref _cardParameters, value);
	}

	private string _configName = string.Empty;
	public string ConfigName
	{
		get => _configName;
		set => SetProperty(ref _configName, value);
	}

	private bool _isConfig;
	public bool IsConfig
	{
		get => _isConfig;
		set => SetProperty(ref _isConfig, value);
	}

	private bool _isExamination;
	public bool IsExamination
	{
		get => _isExamination;
		set => SetProperty(ref _isExamination, value);
	}

}
