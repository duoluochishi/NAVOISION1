using NV.CT.FacadeProxy.Common.Enums.SelfCheck;

namespace NV.CT.NanoConsole.ViewModel;

public class FileValidationViewModel : BaseViewModel
{
	private static readonly string CheckSum1 = "CheckSum.xml";
	private static readonly string CheckSum2 = "CheckSum_Remote.xml";
	private static readonly string Validation1 = "Validation.xml";
	private static readonly string Validation2 = "Validation_Remote.xml";

	private static readonly string McsRootPath = Path.GetFullPath(Path.Combine(RuntimeConfig.Console.MCSBin.Path, "../.."));
	private static readonly string MrsRootPath = @$"\\{RuntimeConfig.OfflineMachineIP}\D\NV";
	//private static readonly string MrsRootPath = @$"\\10.0.0.11\公共文件\软件部\ym";

	private readonly string _mcsChecksum = Path.GetFullPath(Path.Combine( McsRootPath, CheckSum1));
	private readonly string _mcsValidation = Path.GetFullPath(Path.Combine(McsRootPath,Validation1));
	private readonly string _mrsChecksum = Path.GetFullPath(Path.Combine(McsRootPath, CheckSum2));
	private readonly string _mrsValidation = Path.GetFullPath(Path.Combine(McsRootPath,Validation2));

	private readonly string _fileValidator = Path.GetFullPath(Path.Combine(RuntimeConfig.Console.MCSBin.Path, "./FileValidator/NV.CT.NP.Tools.FileValidator.exe"));

	private readonly ILogger<FileValidationViewModel> _logger;
	public FileValidationViewModel(ILogger<FileValidationViewModel> logger)
	{
		_logger = logger;

		Task.Run(() =>
		{
			var list1=GetFileValidationResult(McsRootPath, _mcsChecksum, _mcsValidation);
			var list2=GetFileValidationResult(MrsRootPath, _mrsChecksum, _mrsValidation);

			Application.Current?.Dispatcher?.Invoke(() =>
			{
				McsFileValidationList.Clear();
				McsFileValidationList = list1.ToObservableCollection();

				MrsFileValidationList.Clear();
				MrsFileValidationList=list2.ToObservableCollection();
			});
		});

		//FakeList();
	}

	private List<FileValidationInfo> GetFileValidationResult(string checkPath, string checkSumFile, string validationFile)
	{
		try
		{
			var arguments = $"validate --dir \"{checkPath}\" --save \"{checkSumFile}\" --validation \"{validationFile}\"";
			if (!File.Exists(_fileValidator))
			{
				_logger.LogError($"FileValidation error with FileValidator '{_fileValidator}' not exist");
				return new();
			}
			if (!File.Exists(checkSumFile))
			{
				_logger.LogError($"FileValidation error with '{checkSumFile}' not exist");
				return new();
			}

			//这里有一个路径 D:\NV\ 这样多了一个\导致这里产生了一个bug,导致cmd一直执行失败
			var lastResult = $"{_fileValidator} {arguments}";
			_logger.LogInformation($"FileValidation validate exec last path:{lastResult}");
			var isOk = ProcessHelper.Execute(_fileValidator, arguments, out string _, out string error);
			if (isOk)
			{
				_logger.LogInformation("FileValidation ok");

				//验证结果
				var validationResult = FileValidationHelper.ValidateMD5CheckList(checkPath, checkSumFile);
				var tmpList = new List<FileValidationInfo>();
				if (validationResult!=null)
				{
					AddToList(validationResult.ModifiedItems, FileValidateStatus.Modified, tmpList);
					AddToList(validationResult.AdditionalItems, FileValidateStatus.Added, tmpList);
					AddToList(validationResult.MissingItems, FileValidateStatus.Deleted, tmpList);
					AddToList(validationResult.MatchItems, FileValidateStatus.Normal, tmpList);
				}

				return tmpList;
			}
			else
			{
				_logger.LogInformation($"FileValidation error : {error}");
				return new();
			}
		}
		catch (Exception ex)
		{
			_logger.LogError($"FileValidation error : {ex.Message}-{ex.StackTrace}");
			return new();
		}
	}

	private void AddToList(List<string> allFiles, FileValidateStatus status, List<FileValidationInfo> list)
	{
		var excludedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			CheckSum1,CheckSum2,Validation1,Validation2
		};

		var filteredFiles = allFiles
			.Where(path => !excludedFiles.Any(ex => path.EndsWith(ex, StringComparison.OrdinalIgnoreCase)))
			.ToList();

		filteredFiles.ForEach(item =>
		{
			list.Add(new FileValidationInfo()
			{
				Name = item,
				Status = status
			});
		});
	}

	private ObservableCollection<FileValidationInfo> _mcsFileValidationList = new();
	public ObservableCollection<FileValidationInfo> McsFileValidationList
	{
		get => _mcsFileValidationList;
		set => SetProperty(ref _mcsFileValidationList, value);
	}

	private ObservableCollection<FileValidationInfo> _mrsFileValidationList = new();
	public ObservableCollection<FileValidationInfo> MrsFileValidationList
	{
		get => _mrsFileValidationList;
		set => SetProperty(ref _mrsFileValidationList, value);
	}

	public bool IsValidationOk()
	{
		return McsFileValidationList.Count(n => n.Status == FileValidateStatus.Modified || n.Status==FileValidateStatus.Deleted || n.Status==FileValidateStatus.Added)==0 && MrsFileValidationList.Count(n => n.Status == FileValidateStatus.Modified || n.Status == FileValidateStatus.Deleted || n.Status==FileValidateStatus.Added) == 0;
	}

	#region not used
	//private void FakeList()
	//{
	//	var list = new List<FileValidationInfo>();

	//	for (int i = 1; i <= 50; i++)
	//	{
	//		list.Add(new FileValidationInfo()
	//		{
	//			Name = $"Recon_Item{i.ToString().PadLeft(2, '0')}.so",
	//			Status = FileValidateStatus.Normal,
	//			CreateTime = DateTime.Now,
	//			OperationTime = DateTime.Now
	//		});
	//	}

	//	McsFileValidationList = list.ToObservableCollection();
	//}
	#endregion
}

public class FileValidationInfo
{
	public string Name { get; set; } = string.Empty;

	public FileValidateStatus Status { get; set; }

	public string StatusString => Status.ToString();
	/// <summary>
	/// 创建时间
	/// </summary>
	public DateTime? CreateTime { get; set; }
	/// <summary>
	/// 最新操作时间
	/// </summary>
	public DateTime? OperationTime { get; set; }
}

public enum FileValidateStatus
{
	Added,
	Modified,
	Deleted,
	Normal
}