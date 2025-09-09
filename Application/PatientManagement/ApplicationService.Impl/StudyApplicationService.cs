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
// <key>
//
// </key>
//-----------------------------------------------------------------------
using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NV.CT.AppService.Contract;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Helpers;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.PatientManagement.ApplicationService.Contract.Interfaces;
using NV.CT.PatientManagement.ApplicationService.Contract.Models;
using NV.CT.Protocol;
using NV.CT.Protocol.Interfaces;
using NV.CT.Protocol.Models;
using NV.MPS.Environment;
using NV.MPS.Exception;
using System.IO;
using System.Text;

namespace NV.CT.PatientManagement.ApplicationService.Impl;

public class StudyApplicationService : IStudyApplicationService
{
	private readonly IMapper _mapper;
	private readonly ILogger<StudyApplicationService> _logger;
	private readonly IStudyService _studyService;
	private readonly ISeriesService _seriesService;
	private readonly IReconTaskService _reconTaskService;
	private readonly IScanTaskService _scanTaskService;
	private readonly IRawDataService _rawDataService;

	private readonly IPatientService _patientService;
	
	private readonly IApplicationCommunicationService _applicationCommunicationService;
	private readonly IProtocolModificationService _protocolModificationService;

	public event EventHandler<EventArgs<string>>? SelectItemChanged;
	public event EventHandler<EventArgs<(PatientModel, StudyModel, DataOperateType)>>? RefreshPatientManagementStudyList;
	public event EventHandler<EventArgs<(PatientModel, StudyModel)>>? GotoExam;
	public event EventHandler<EventArgs<SearchTimeType>>? RefreshRearchDateType;
	public event EventHandler<ApplicationResponse>? ApplicationClosing;

	public StudyApplicationService(ILogger<StudyApplicationService> logger,
								   IMapper mapper,
								   IStudyService studyService,
								   ISeriesService seriesService,
								   IReconTaskService reconTaskService,
								   IScanTaskService scanTaskService,
								   IRawDataService rawDataService,
								   IPatientService patientService,
								   IApplicationCommunicationService applicationCommunicationService,
								   IProtocolModificationService protocolModificationService)
	{
		_logger = logger;
		_mapper = mapper;
		_studyService = studyService;
		_seriesService = seriesService;
		_reconTaskService = reconTaskService;
		_scanTaskService = scanTaskService;
		_rawDataService = rawDataService;
		_patientService = patientService;
		_protocolModificationService = protocolModificationService;
		_applicationCommunicationService = applicationCommunicationService;

		_studyService.UpdateStudyInformation += OnStudyInformationUpdated;
		_studyService.GotoExam += OnGotoExam;
	}
	private void OnGotoExam(object? sender, EventArgs<(DatabaseService.Contract.Models.PatientModel, DatabaseService.Contract.Models.StudyModel)> e)
	{
		Contract.Models.PatientModel patientModel = _mapper.Map<Contract.Models.PatientModel>(e.Data.Item1);
		Contract.Models.StudyModel studyModel = _mapper.Map<Contract.Models.StudyModel>(e.Data.Item2);
		GotoExam?.Invoke(this, new EventArgs<(Contract.Models.PatientModel, Contract.Models.StudyModel)>((patientModel, studyModel)));
	}

	private void OnStudyInformationUpdated(object? sender, EventArgs<(DatabaseService.Contract.Models.PatientModel, DatabaseService.Contract.Models.StudyModel, DataOperateType)> e)
	{
		Contract.Models.PatientModel patientModel = _mapper.Map<Contract.Models.PatientModel>(e.Data.Item1);
		Contract.Models.StudyModel studyModel = _mapper.Map<Contract.Models.StudyModel>(e.Data.Item2);
		RefreshPatientManagementStudyList?.Invoke(this, new EventArgs<(Contract.Models.PatientModel, Contract.Models.StudyModel, DataOperateType)>((patientModel, studyModel, e.Data.Item3)));
	}
	public void RaiseSelectItemChanged(string studyId)
	{
		SelectItemChanged?.Invoke(this, new EventArgs<string>(studyId));
	}
	public List<(Contract.Models.PatientModel, Contract.Models.StudyModel)> GetPatientStudyListWithEnd()
	{
		try
		{
			var result = _studyService.GetPatientStudyListWithEnd();
			var list = new List<(Contract.Models.PatientModel, Contract.Models.StudyModel)>();
			list = _mapper.Map<List<(DatabaseService.Contract.Models.PatientModel, DatabaseService.Contract.Models.StudyModel)>, List<(Contract.Models.PatientModel, Contract.Models.StudyModel)>>(result);
			return list;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, $"Failed to query study list with exception: {ex.Message}");
			throw new NanoException("MCS006000001", "Failed to query study list."); // Will be replaced with the following line code later.
																					//throw new NanoException(ErrorCodeResource.MCS_PatientManagementQueryFailed, "Failed to query study list.", ex); 
		}
	}

	public bool Delete(StudyModel studyModel)
	{
		//var result = _studyService.Delete(_mapper.Map<DatabaseService.Contract.Models.StudyModel>(studyModel));
		//return result;
		return DeleteByGuid(studyModel.Id,studyModel);
	}

	public StudyModel GetStudyModelByPatientIdAndStudyStatus(string patientId, string studyStatus)
	{
		try
		{
			var result = _studyService.GetStudyModelByPatientIdAndStudyStatus(patientId, studyStatus);
			return _mapper.Map<StudyModel>(result);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, $"Failed to query study list with exception: {ex.Message}");
			throw new NanoException("MCS006000001", "Failed to query study list."); // Will be replaced with the following line code later.
																					//throw new NanoException(ErrorCodeResource.MCS_PatientManagementQueryFailed, "Failed to query study list.", ex); 
		}
	}

	public bool DeleteByGuid(string studyGuid, StudyModel studyModel)
	{
		var studyInfo = _studyService.GetStudyById(studyGuid);
		if (studyInfo is null || studyInfo.IsProtected) return false;

        //todo:待检测离线重建、图像浏览、胶片打印是否打开此检查

        var patientStudies = _studyService.GetStudiesByPatient(studyInfo.InternalPatientId);

		var reconTasks = _reconTaskService.GetAll(studyGuid);
		var scanTasks = _scanTaskService.GetAll(studyGuid);
		var serieses = _seriesService.GetSeriesByStudyId(studyGuid);
		var rawList = _rawDataService.GetRawDataListByStudyId(studyGuid);

		if (serieses is not null && serieses.Count > 0)
		{
			foreach(var seriesInfo in serieses)
			{
				var tempPath = seriesInfo.SeriesPath;
				if (File.Exists(tempPath))
				{
					tempPath = Path.Combine(tempPath, "..");
				}
				DirectoryHelper.DeleteDirectory(tempPath);
				_seriesService.Delete(seriesInfo);
			}
		}

		if (reconTasks is not null && reconTasks.Count > 0)
		{
			foreach(var reconTask in reconTasks)
			{
				_reconTaskService.DeleteByGuid(reconTask.Id);
			}
		}

		if (scanTasks is not null && scanTasks.Count > 0)
		{
			foreach(var scanTask in scanTasks)
			{
				_scanTaskService.Delete(scanTask);
			}
		}

		if (rawList is not null && rawList.Count > 0)
		{
			foreach(var rawItem in rawList)
			{
				DirectoryHelper.DeleteDirectory(rawItem.Path);
				_rawDataService.Delete(rawItem.Id);
			}
		}

		_studyService.DeleteByGuid(_mapper.Map<DatabaseService.Contract.Models.StudyModel>(studyModel));

		if (patientStudies is not null && patientStudies.Count < 2)
		{
			_patientService.DeleteByGuid(studyInfo.InternalPatientId);
		}

		return true;
	}
	public bool GetStudyByStudyInstanceUID(string studyInstanceUID)
	{
	  var result= _studyService.GetWithUID(studyInstanceUID);
		return !(result.Study is null);
    }
    public bool GetSeriesBySeriesInstanceUID(string seriesnstanceUID)
	{
        var result = _seriesService.GetSeriesBySeriesInstanceUID(seriesnstanceUID);
		return !(string.IsNullOrEmpty(result?.Id));

    }
    public PatientModel GetPatientModelById(string patientId)
	{
		var result = _studyService.GetPatientModelById(patientId);
		return _mapper.Map<PatientModel>(result);
	}

	public bool ResumeExamination(StudyModel studyModel, string StudyId)
	{
		return _studyService.ResumeExamination(_mapper.Map<DatabaseService.Contract.Models.StudyModel>(studyModel), StudyId);
	}

	public bool SwitchLockStatus(string studyId)
	{
		return _studyService.SwitchLockStatus(studyId);
	}

	public bool UpdateArchiveStatus(List<StudyModel> studyModels)
	{
		return _studyService.UpdateArchiveStatus(_mapper.Map<List<StudyModel>, List<DatabaseService.Contract.Models.StudyModel>>(studyModels));

	}

	public void RefreshRearchDateTyped(SearchTimeType searchTimeType)
	{
		RefreshRearchDateType?.Invoke(this, new EventArgs<SearchTimeType>(searchTimeType));
	}

	public List<(PatientModel, StudyModel)> GetPatientStudyListWithEnd(string startDate, string endDate)
	{
		var result = _studyService.GetPatientStudyListWithEndStudyDate(startDate, endDate);
		var list = new List<(PatientModel, StudyModel)>();
		list = _mapper.Map<List<(DatabaseService.Contract.Models.PatientModel, DatabaseService.Contract.Models.StudyModel)>, List<(Contract.Models.PatientModel, Contract.Models.StudyModel)>>(result);

		return list;
	}

	public List<(PatientModel, StudyModel)> GetPatientStudyListByFilter(StudyListFilterModel filter)
	{
		var result = _studyService.GetPatientStudyListByFilter(filter);
		var list = new List<(PatientModel, StudyModel)>();
		list = _mapper.Map<List<(DatabaseService.Contract.Models.PatientModel, DatabaseService.Contract.Models.StudyModel)>, List<(Contract.Models.PatientModel, Contract.Models.StudyModel)>>(result);

		return list;
	}

	public StudyModel[] GetStudiesByIds(string[] studyIdList)
	{
		var result = _studyService.GetStudiesByIds(studyIdList);
		var list = _mapper.Map<DatabaseService.Contract.Models.StudyModel[], Contract.Models.StudyModel[]>(result);
		return list.ToArray();
    }

	public bool Update(bool isGotoExam, PatientModel patientModel, StudyModel studyModel)
	{
		_logger.LogInformation("Update patient and study.");
		var _patientModel = _mapper.Map<DatabaseService.Contract.Models.PatientModel>(patientModel);
		var _studyModel = _mapper.Map<DatabaseService.Contract.Models.StudyModel>(studyModel);
		try
		{
			return _studyService.Update(isGotoExam, _patientModel, _studyModel);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, $"Failed to update patient with exception: {ex.Message}");
			throw new NanoException("MCS005000003", "Failed to update patient."); // Will be replaced with the following line code later.
																				  //throw new NanoException(ErrorCodeResource.MCS_PatientBrowserUpdateFailed, "Failed to update patient.", ex); 
		}

	}

	public bool Correct(PatientModel patientModel, StudyModel studyModel, string editor)
	{
		_logger.LogInformation("Correct patient and study.");
		var _patientModel = _mapper.Map<DatabaseService.Contract.Models.PatientModel>(patientModel);
		var _studyModel = _mapper.Map<DatabaseService.Contract.Models.StudyModel>(studyModel);
		try
		{
			return _studyService.Correct(_patientModel, _studyModel, editor);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, $"Failed to correct patient and study with exception: {ex.Message}");
			throw new NanoException("MCS005000004", "Failed to update patient and study."); // Will be replaced with the following line code later.
																							//throw new NanoException(ErrorCodeResource.MCS_PatientBrowserUpdateFailed, "Failed to update patient.", ex); 
		}
	}

	public List<(PatientModel, StudyModel)> GetCorrectionHistoryList(string studyId)
	{
		var result = _studyService.GetCorrectionHistoryList(studyId);
		var list = new List<(PatientModel, StudyModel)>();
		list = _mapper.Map<List<(DatabaseService.Contract.Models.PatientModel, DatabaseService.Contract.Models.StudyModel)>, List<(Contract.Models.PatientModel, Contract.Models.StudyModel)>>(result);

		return list;

	}
	#region oldMethod
	/// <summary>
	/// 废弃的第一版Raw数据导入方法；
	/// </summary>
	/// <param name="dataPath"></param>
	/// <returns></returns>
	//  public bool ImportRaw(string dataPath)
	//  {
	//      if (!CheckRawData(dataPath))
	//      {
	//          return false;
	//      }

	//      var result = GetRawDataModels(dataPath);
	//      //if (result.Study is null || result.Patient is null || result.Scan is null || result.Recon is null)
	//      //{
	//      //	return false;
	//      //}

	//      if (result is null)
	//      {
	//          return false;
	//      }

	//      //定位像直接退出
	//      if (result.ScanParameter.ScanOption is ScanOption.Surview)
	//          return false;


	////todo:创建检查数据
	//var patientModel = ConstructPatientModel(result.ScanParameter);
	//var studyModel = ConstructStudyModel(result.ScanParameter, patientModel);
	//studyModel.InternalPatientId = patientModel.Id;

	////todo:创建默认扫描协议
	//var protocolModel = ConstructProtocolModel(result.ScanParameter);
	//var protocolStr = ProtocolHelper.Serialize(protocolModel);
	//studyModel.Protocol = protocolStr;
	//studyModel.StudyStatus = WorkflowStatus.ExaminationClosed.ToString();

	////插入数据库
	//_studyService.Insert(false, false, patientModel, studyModel);


	////TODO:导入一份生数据，写一份协议文件 ，后面可以取消这里
	//var rawDataImport = @"F:\RawDataImport";
	//      if (!Directory.Exists(rawDataImport))
	//      {
	//          Directory.CreateDirectory(rawDataImport);
	//      }
	//      File.WriteAllText(Path.Combine(rawDataImport, $"{result.ScanParameter.ScanUID}_protocol.xml"), protocolStr);


	//      //todo:生数据复制到指定目录
	//      var rawDataPath = RuntimeConfig.Console.RawData.Path;
	//      if (!Directory.Exists(rawDataPath))
	//      {
	//          Directory.CreateDirectory(rawDataPath);
	//      }
	//      var studyPath = Path.Combine(Path.Combine(rawDataPath, studyModel.StudyInstanceUID), result.ScanParameter.ScanUID);
	//      if (!Directory.Exists(studyPath))
	//      {
	//          Directory.CreateDirectory(studyPath);
	//      }

	//      _logger.LogInformation($"copy from source path {dataPath} to dest path {studyPath}");
	//      CopyDirectory(dataPath, studyPath, true);

	//      return true;
	//  }
	#endregion

	#region constructDataRecord
	/// <summary>
	/// 通过Raw数据json文件恢复数据记录的方法；
	/// </summary>
	/// <param name="dataPath"></param>
	/// <returns></returns>
	public (string,string) ImportRaw(string dataPath)
    {
		//if (!CheckRawData(dataPath))
		//{
		//    return false;
		//}
		(string, string) result=("","");
        foreach (var item in Directory.GetDirectories(dataPath))
		{
		    result=InsertData(item);
        }
        return result; 
    }
	/// <summary>
	/// 插入数据库记录;
	/// </summary>
	/// <param name="dataPath"></param>
	/// <returns></returns>
	private (string,string)  InsertData(string dataPath)
	{
        //var result = GetRawDataModels(dataPath);
        //if (result.Study is null || result.Patient is null || result.Scan is null || result.Recon is null)
        //{
        //	return false;
        //}

        //if (result is null)
        //{
        //    return ("","");
        //}
        return ("", ""); //临时添加，保存编译时没有警告信息，该方法使用时去除这一行；
        //定位像直接退出
        //if (result.ScanParameter.ScanOption is ScanOption.Surview)
        //    return false;


        //var uidData = _studyService.GetWithUID(result.Study.StudyInstanceUID);
        //if (uidData.Study is null)
        //{
        //    var patientModel = ConstructPatientModel(result.Patient, result.Study);
        //    var studyModel = ConstructStudyModel(result.Study, result.ScanParameter, patientModel, result.Patient, result.ReconSeriesParams[0]);
        //    studyModel.InternalPatientId = patientModel.Id;

        //    //todo:创建默认扫描协议
        //    var protocolModel = ConstructProtocolModel(result.Study, result.ScanParameter, result.ReconSeriesParams[0]);
        //    var protocolStr = ProtocolHelper.Serialize(protocolModel.Item1);
        //    studyModel.Protocol = protocolStr;
        //    studyModel.StudyStatus = WorkflowStatus.ExaminationClosed.ToString();

        //    //插入数据库
        //    _studyService.Insert(false, false, patientModel, studyModel);
        //    var seriesItem = ConstuctSeriesItem(result, studyModel);
        //    _seriesService.Add(seriesItem);

        //    var rawDataItem = ConstuctRawDataItem(result, studyModel);
        //    _rawDataService.Add(rawDataItem);

        //    var scanTaskItem = ConstuctScanTaskItem(result, studyModel, protocolModel.Item4, patientModel,protocolModel.Item2);
        //    _scanTaskService.Insert(scanTaskItem);
        //    var reconTaskItem = ConstuctReconTaskItem(result, studyModel, patientModel, protocolModel.Item3);
        //    _reconTaskService.Insert(reconTaskItem);
        //    return (result.Study.StudyInstanceUID, studyModel.Id);
        //}
        //else
        //{
        //    var protocolModel = ProtocolHelper.Deserialize(uidData.Study.Protocol);
        //    var addStudyProtocol = ConstructStudyProtocolModel(protocolModel, result.Study, result.ScanParameter, result.ReconSeriesParams[0]);
        //    var protocolStr = ProtocolHelper.Serialize(addStudyProtocol.Item1);
        //    uidData.Study.Protocol = protocolStr;
        //    uidData.Study.StudyStatus = WorkflowStatus.ExaminationClosed.ToString();
        //    _studyService.UpdateStudyProtocol(uidData.Study);
        //    var seriesItem = ConstuctSeriesItem(result, uidData.Study);
        //    _seriesService.Add(seriesItem);
        //    var rawDataItem = ConstuctRawDataItem(result, uidData.Study);
        //    _rawDataService.Add(rawDataItem);
        //    var scanTaskItem = ConstuctScanTaskItem(result, uidData.Study, addStudyProtocol.Item4, uidData.Patient, addStudyProtocol.Item2);
        //    _scanTaskService.Insert(scanTaskItem);
        //    var reconTaskItem = ConstuctReconTaskItem(result, uidData.Study, uidData.Patient, addStudyProtocol.Item3);
        //    _reconTaskService.Insert(reconTaskItem);
        //    return (result.Study.StudyInstanceUID, uidData.Study.Id);
        //}



    }
	/// <summary>
	/// 根据SeriesModel构造图像序列记录
	/// </summary>
	/// <param name="seriesModel"></param>
	/// <returns></returns>
	public bool AddSeries(SeriesModel seriesModel)
	{
        DatabaseService.Contract.Models.SeriesModel databaseSeriesModel = new DatabaseService.Contract.Models.SeriesModel();

        databaseSeriesModel.Id = Guid.NewGuid().ToString();
        databaseSeriesModel.InternalStudyId = seriesModel.InternalStudyId;
        databaseSeriesModel.SeriesDescription = seriesModel.SeriesDescription;
		databaseSeriesModel.SeriesNumber = seriesModel.SeriesNumber;
		databaseSeriesModel.SeriesInstanceUID = seriesModel.SeriesInstanceUID;
        databaseSeriesModel.WindowType = "Custom";
        databaseSeriesModel.SeriesType = Constants.SERIES_TYPE_IMAGE;
		databaseSeriesModel.ImageCount = seriesModel.ImageCount;

        databaseSeriesModel.BodyPart = seriesModel.BodyPart;

        databaseSeriesModel.ImageType = seriesModel.ImageType;
        databaseSeriesModel.ReconEndDate = seriesModel.ReconEndDate.GetValueOrDefault();
		databaseSeriesModel.PatientPosition = seriesModel.PatientPosition;
        databaseSeriesModel.ScanId = seriesModel.ScanId; 
        databaseSeriesModel.ReconId = seriesModel.ReconId;
        databaseSeriesModel.SeriesPath = seriesModel.SeriesPath;
        databaseSeriesModel.SeriesDate = seriesModel.SeriesDate;
		databaseSeriesModel.SeriesTime = seriesModel.SeriesTime;
        return _seriesService.Add(databaseSeriesModel);
    }
	/// <summary>
	/// 创建扫描任务；
	/// </summary>
	/// <param name="scanReconParam"></param>
	/// <param name="studyModel"></param>
	/// <param name="measurementModel"></param>
	/// <param name="patientModel"></param>
	/// <param name="scanMode"></param>
	/// <returns></returns>
  //  private DatabaseService.Contract.Models.ScanTaskModel ConstuctScanTaskItem(ScanReconParam scanReconParam, DatabaseService.Contract.Models.StudyModel studyModel, MeasurementModel measurementModel, DatabaseService.Contract.Models.PatientModel patientModel, NV.CT.Protocol.Models.ScanModel scanMode)
  //  {
  //      DatabaseService.Contract.Models.ScanTaskModel scanTaskModel = new DatabaseService.Contract.Models.ScanTaskModel();

  //      scanTaskModel.Id = Guid.NewGuid().ToString();
  //      scanTaskModel.InternalStudyId = studyModel.Id;
  //      scanTaskModel.MeasurementId= measurementModel.Descriptor.Id;
  //      scanTaskModel.FrameOfReferenceUid = scanReconParam.ReconSeriesParams[0].FrameOfReferenceUID;
  //      scanTaskModel.ScanId = scanReconParam.ScanParameter.ScanUID;
  //      scanTaskModel.BodyPart= scanReconParam.ScanParameter.BodyPart.ToString();
  //      scanTaskModel.ScanOption= scanReconParam.ScanParameter.ScanMode.ToString();
		//scanTaskModel.ScanEndDate= scanReconParam.ReconSeriesParams[0].AcquisitionDate.GetValueOrDefault();
  //      scanTaskModel.IssuingParameters= JsonConvert.SerializeObject(scanMode.Parameters);
  //      scanTaskModel.ActuralParameters= string.Empty;
  //      scanTaskModel.Description = scanReconParam.Study.StudyDescription;
  //      scanTaskModel.InternalPatientId = patientModel.Id;
		//scanTaskModel.IsLinkScan = false;
		//scanTaskModel.IsInject = false;
  //      scanTaskModel.TopoScanId=string.Empty;
		//scanTaskModel.TaskStatus =0;
  //      scanTaskModel.CreateTime = scanReconParam.ReconSeriesParams[0].AcquisitionDate.GetValueOrDefault();
  //      scanTaskModel.Creator = scanReconParam.Study.OperatorsName;

  //      return scanTaskModel;

  //  }
	/// <summary>
	/// 创建重建任务；
	/// </summary>
	/// <param name="scanReconParam"></param>
	/// <param name="studyModel"></param>
	/// <param name="patientModel"></param>
	/// <param name="reconModel"></param>
	/// <returns></returns>
  //  private DatabaseService.Contract.Models.ReconTaskModel ConstuctReconTaskItem(ScanReconParam scanReconParam, DatabaseService.Contract.Models.StudyModel studyModel, DatabaseService.Contract.Models.PatientModel patientModel,ReconModel reconModel)
  //  {
  //      DatabaseService.Contract.Models.ReconTaskModel reconTaskModel = new DatabaseService.Contract.Models.ReconTaskModel();

  //      reconTaskModel.Id = Guid.NewGuid().ToString();
  //      reconTaskModel.InternalStudyId = studyModel.Id;
		//reconTaskModel.IsRTD = true;
  //      reconTaskModel.FrameOfReferenceUid = scanReconParam.ReconSeriesParams[0].FrameOfReferenceUID;
  //      reconTaskModel.ReconId = scanReconParam.ReconSeriesParams[0].ReconID;
  //      reconTaskModel.ScanId = scanReconParam.ScanParameter.ScanUID;
  //      reconTaskModel.SeriesNumber = scanReconParam.ReconSeriesParams[0].SeriesNumber.GetValueOrDefault();
  //      reconTaskModel.SeriesDescription = scanReconParam.ReconSeriesParams[0].SeriesDescription;
		//reconTaskModel.WindowWidth = scanReconParam.ReconSeriesParams[0].WindowWidth[0].ToString();
		//reconTaskModel.WindowLevel = scanReconParam.ReconSeriesParams[0].WindowCenter[0].ToString();
		//reconTaskModel.ReconEndDate = scanReconParam.ReconSeriesParams[0].SeriesDate.GetValueOrDefault();
  //      reconTaskModel.IssuingParameters = JsonConvert.SerializeObject(reconModel.Parameters);
  //      reconTaskModel.ActuralParameters = string.Empty;
		//reconTaskModel.Description = scanReconParam.ReconSeriesParams[0].SeriesDescription;
  //      reconTaskModel.InternalPatientId = patientModel.Id;
		//reconTaskModel.TaskStatus = 5;
  //      reconTaskModel.CreateTime = scanReconParam.ReconSeriesParams[0].AcquisitionDate.GetValueOrDefault();
  //      reconTaskModel.Creator = scanReconParam.Study.OperatorsName;

  //      return reconTaskModel;

  //  }
	/// <summary>
	/// 创建Raw数据记录
	/// </summary>
	/// <param name="scanReconParam"></param>
	/// <param name="studyModel"></param>
	/// <returns></returns>
 //   private DatabaseService.Contract.Models.RawDataModel ConstuctRawDataItem(ScanReconParam scanReconParam, DatabaseService.Contract.Models.StudyModel studyModel)
	//{
 //       DatabaseService.Contract.Models.RawDataModel rawDataModel=new DatabaseService.Contract.Models.RawDataModel();

 //       rawDataModel.Id= Guid.NewGuid().ToString();
 //       rawDataModel.InternalStudyId = studyModel.Id;
 //       rawDataModel.FrameOfReferenceUID= scanReconParam.ReconSeriesParams[0].FrameOfReferenceUID;
 //       rawDataModel.ScanId= scanReconParam.ScanParameter.ScanUID;
 //       rawDataModel.ScanName= scanReconParam.ReconSeriesParams[0].ProtocolName;
 //       rawDataModel.PatientPosition= scanReconParam.ReconSeriesParams[0].PatientPosition.ToString();
	//	rawDataModel.ScanModel = string.Empty;
 //       rawDataModel.ScanEndTime= scanReconParam.ReconSeriesParams[0].AcquisitionDate.GetValueOrDefault();
 //       rawDataModel.Path= Path.Combine(RuntimeConfig.Console.RawData.Path, scanReconParam.Study.StudyInstanceUID, scanReconParam.ScanParameter.ScanUID);
 //       rawDataModel.CreateTime= scanReconParam.ReconSeriesParams[0].AcquisitionDate.GetValueOrDefault();
 //       rawDataModel.Creator= scanReconParam.Study.OperatorsName;

 //       return rawDataModel;

    //}
	/// <summary>
	/// 构造图像序列
	/// </summary>
	/// <param name="scanReconParam"></param>
	/// <param name="studyModel"></param>
	/// <returns></returns>
 //   private DatabaseService.Contract.Models.SeriesModel  ConstuctSeriesItem(ScanReconParam scanReconParam, DatabaseService.Contract.Models.StudyModel studyModel)
	//{
 //       DatabaseService.Contract.Models.SeriesModel seriesModel=new DatabaseService.Contract.Models.SeriesModel();

 //       seriesModel.Id=Guid.NewGuid().ToString();
 //       seriesModel.InternalStudyId= studyModel.Id;
 //       seriesModel.SeriesDescription= scanReconParam.ReconSeriesParams[0].SeriesDescription;
	//	seriesModel.SeriesNumber = scanReconParam.ReconSeriesParams[0].SeriesNumber.GetValueOrDefault().ToString();
	//	seriesModel.SeriesInstanceUID = scanReconParam.ReconSeriesParams[0].SeriesInstanceUID;
 //       seriesModel.WindowType = "Custom";
 //       seriesModel.SeriesType=Constants.SERIES_TYPE_IMAGE;
	//	string rootdir = $"{seriesModel.SeriesNumber}_{scanReconParam.ReconSeriesParams[0].SeriesInstanceUID}";  

 //       DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(RuntimeConfig.Console.MCSAppData.Path, scanReconParam.Study.StudyInstanceUID,rootdir ));

	//	if (directoryInfo.Exists)
	//	{
 //           seriesModel.ImageCount = directoryInfo.GetFiles("*.dcm").Length;
 //       }else
	//	{
	//		seriesModel.ImageCount = 1;
 //       }

 //       seriesModel.FrameOfReferenceUID= scanReconParam.ReconSeriesParams[0].FrameOfReferenceUID;
	//	seriesModel.ProtocolName = scanReconParam.ReconSeriesParams[0].ProtocolName;
	//	seriesModel.BodyPart = scanReconParam.ScanParameter.BodyPart.ToString();
	//	if (seriesModel.ImageCount>1)
	//	{
 //           seriesModel.ImageType = Constants.IMAGE_TYPE_TOMO;
 //       }else
	//	{
 //           seriesModel.ImageType = Constants.IMAGE_TYPE_TOPO;
 //       }

 //       seriesModel.WindowWidth= scanReconParam.ReconSeriesParams[0].WindowWidth[0].ToString();
 //       seriesModel.WindowLevel = scanReconParam.ReconSeriesParams[0].WindowCenter[0].ToString();
 //       seriesModel.ReconEndDate= scanReconParam.ReconSeriesParams[0].AcquisitionDate.GetValueOrDefault();
 //       seriesModel.PatientPosition= scanReconParam.ReconSeriesParams[0].PatientPosition.ToString();
 //       seriesModel.ScanId= scanReconParam.ScanParameter.ScanUID;
 //       seriesModel.ReconId = scanReconParam.ReconSeriesParams[0].ReconID;
	//	seriesModel.SeriesPath = Path.Combine(RuntimeConfig.Console.MCSAppData.Path, scanReconParam.Study.StudyInstanceUID, rootdir);

 //       return seriesModel;
 //   }
	/// <summary>
	/// 构造ProtocolModel
	/// </summary>
	/// <param name="study"></param>
	/// <param name="scanParam"></param>
	/// <param name="reconSeriesParam"></param>
	/// <returns></returns>
 //   private (ProtocolModel, NV.CT.Protocol.Models.ScanModel, ReconModel, MeasurementModel) ConstructProtocolModel(Study study,ScanParam scanParam,ReconSeriesParam reconSeriesParam)
	//{
	//	var pm = new ProtocolModel();
	//	var frame = new FrameOfReferenceModel();
	//	var measurement = new MeasurementModel();
	//	var scanModel = new ScanModel();
	//	var reconModel = new ReconModel();

	//	var scan = scanParam;

	//	pm.Descriptor = new DescriptorModel()
	//	{
	//		Id = IdGenerator.Next(1),
	//		Name = "Protocol",
	//		Type = "Protocol"
	//	};
	//	pm.Status = PerformStatus.Performed;

	//	var patientPosition = "HFS";

	//	_protocolModificationService.SetParameter<string>(pm, ProtocolParameterNames.PATIENT_POSITION, patientPosition);
	//	_protocolModificationService.SetParameter<string>(pm, ProtocolParameterNames.BODY_PART, scan.BodyPart.ToString());
	//	_protocolModificationService.SetParameter<string>(pm, ProtocolParameterNames.SCAN_OPTION, scan.ScanOption.ToString());

	//	frame.Descriptor = new DescriptorModel()
	//	{
	//		Id = IdGenerator.Next(2),
	//		Name = "FrameOfReference",
	//		Type = "FrameOfReference"
	//	};
	//	frame.Status = PerformStatus.Performed;

	//	_protocolModificationService.SetParameter<string>(frame, ProtocolParameterNames.PATIENT_POSITION, patientPosition);

	//	measurement.Descriptor = new DescriptorModel()
	//	{
	//		Id = IdGenerator.Next(3),
	//		Name = "Measurement",
	//		Type = "Measurement"
	//	};
	//	measurement.Status = PerformStatus.Performed;

	//	reconModel.Descriptor = new DescriptorModel()
	//	{
	//		Id = reconSeriesParam.ReconID,
	//		Name = "Recon",
	//		Type = "Recon"
	//	};
	//	reconModel.Status = PerformStatus.Performed;
	//	string rootdir = $"{reconSeriesParam.SeriesNumber}_{reconSeriesParam.SeriesInstanceUID}";  
 //       var listParameters = new List<ParameterModel>()
	//	{
	//		new (){Name = ProtocolParameterNames.RECON_RECON_TYPE,Value = ReconType.HCT.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_FILTER_TYPE,Value = reconSeriesParam.FilterType.ToString()},
	//		//new (){Name = ProtocolParameterNames.RECON_BINNING,Value = ReconBinning.Bin11.ToString()},
	//		//new (){Name = ProtocolParameterNames.RECON_BM3D_DENOISE_COEF,Value = 2.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_BONE_ARITIFACT_ENABALE,Value = 0.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_INTERP_TYPE,Value = InterpType.InterpNone.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_AIR_CORRECTION_MODE,Value = AirCorrectionMode.None.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_PRE_DENOISE_TYPE,Value = PreDenoiseType.BM3D.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_PRE_DENOISE_COEF,Value = 0.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_POST_DENOISE_TYPE,Value = PostDenoiseType.BM3D.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_POST_DENOISE_COEF,Value = 0.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_METAL_ARITIFACT_ENABLE,Value = 0.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_SLICE_THICKNESS,Value = reconSeriesParam.SliceThickness.ToString()},
	//		//new (){Name = ProtocolParameterNames.RECON_TV_DENOISE_COEF,Value = 2.ToString()},
	//		//new (){Name = ProtocolParameterNames.RECON_BOWTIE,Value = 1.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_WINDOW_TYPE,Value ="Custom" },
	//		new (){Name = ProtocolParameterNames.RECON_VIEW_WINDOW_WIDTH,Value ="-1" },
	//		new (){Name = ProtocolParameterNames.RECON_VIEW_WINDOW_CENTER,Value ="0" },

	//		new (){Name = ProtocolParameterNames.RECON_WINDOW_CENTER,Value = ToArrString(new int[]{reconSeriesParam.WindowCenter[0]})},
	//		new (){Name = ProtocolParameterNames.RECON_WINDOW_WIDTH,Value = ToArrString(new int[]{reconSeriesParam.WindowWidth[0]})},
	//		new (){Name = ProtocolParameterNames.RECON_CENTER_FIRST_X,Value = 0.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y,Value = 0.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z,Value = (reconSeriesParam.CenterFirstZ * -1).ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_CENTER_LAST_X,Value = 0.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_CENTER_LAST_Y,Value = 0.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_CENTER_LAST_Z,Value = (reconSeriesParam.CenterLastZ * -1).ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X,Value = 1.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y,Value = 0.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z,Value = 0.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X,Value = 0.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y,Value = 1.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z,Value = 0.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_FOV_LENGTH_HORIZONTAL,Value = reconSeriesParam.FoVLengthHor.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_FOV_LENGTH_VERTICAL,Value = reconSeriesParam.FoVLengthVert.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_HORIZONTAL,Value = reconSeriesParam.ImageMatrixHor.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_VERTICAL,Value = reconSeriesParam.ImageMatrixVert.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_IMAGE_INCREMENT,Value = reconSeriesParam.ImageIncrement.ToString()},
	//		new (){Name = ProtocolParameterNames.RECON_IS_RTD,Value = "True"},
 //           new (){Name = ProtocolParameterNames.RECON_PRE_BINNING,Value = reconSeriesParam.PreBinning.ToString()},
 //           new (){Name = ProtocolParameterNames.RECON_IMAGE_PATH,Value = Path.Combine(RuntimeConfig.Console.MCSAppData.Path,study.StudyInstanceUID,rootdir)}
 //       };

	//	_protocolModificationService.SetParameters(reconModel, listParameters);

 //       scanModel.Descriptor = new DescriptorModel()
	//	{
	//		Id = scan.ScanUID,
	//		Name = scan.ScanOption.ToString(),
	//		Type = "Scan"
	//	};
	//	scanModel.Status = PerformStatus.Performed;

	//	var tubePositions = ToArrString(scan.TubePositions.Select(n => (int)n).ToArray());

	//	var scanParameters = new List<ParameterModel>()
	//	{
	//		new (){Name = ProtocolParameterNames.SCAN_BINNING,Value = scan.ScanBinning.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_PRE_VOICE_ID,Value = scan.PreVoiceID.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_POST_VOICE_ID,Value = scan.PostVoiceID.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_AUTO_SCAN,Value = scan.AutoScan.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_KILOVOLT,Value =ToArrString( scan.kV)},
	//		new (){Name = ProtocolParameterNames.SCAN_MILLIAMPERE,Value = ToArrString(DivideArray( scan.mA))},
	//		new (){Name = ProtocolParameterNames.SCAN_EXPOSURE_TIME,Value = scan.ExposureTime.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_FRAME_TIME,Value = scan.FrameTime.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_FRAMES_PER_CYCLE,Value = scan.FramesPerCycle.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_OPTION,Value = scan.ScanOption.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_MODE,Value = scan.ScanMode.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_RAW_DATA_TYPE,Value = scan.RawDataType.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_TRIGGER_MODE,Value = scan.TriggerMode.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_TRIGGER_END,Value = scan.TriggerEnd.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_EXPOSURE_MODE,Value = scan.ExposureMode.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_TABLE_DIRECTION,Value = scan.TableDirection.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_BOWTIE_ENABLE,Value = scan.BowtieEnable.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_GAIN,Value = scan.Gain.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_TABLE_HEIGHT,Value = scan.TableHeight.ToString()},
	//		new (){Name = ProtocolParameterNames.BODY_PART,Value = scan.BodyPart.ToString()},


	//		new (){Name = ProtocolParameterNames.SCAN_TUBE_POSITIONS,Value = tubePositions},
	//		new (){Name = ProtocolParameterNames.SCAN_TUBE_NUMBERS,Value =ToArrString(scan.TubeNumbers)},

	//		new (){Name = ProtocolParameterNames.SCAN_DOSE_CURVE,Value = "null"},
	//		new (){Name = ProtocolParameterNames.SCAN_TABLE_SPEED,Value = scan.TableSpeed.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_TABLE_FEED,Value = scan.TableFeed.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_PITCH,Value = scan.Pitch.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_PRE_OFFSET_FRAMES,Value = scan.PreOffsetFrames.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_POST_OFFSET_FRAMES,Value = scan.PostOffsetFrames.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_LOOPS,Value = scan.Loops.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_LOOP_TIME,Value = scan.LoopTime.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_PRE_VOICE_DELAY_TIME,Value = scan.PreVoiceDelayTime.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_PRE_VOICE_PLAY_TIME,Value = scan.PreVoicePlayTime.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_PRE_VOICE_ID,Value = scan.PreVoiceID.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_POST_VOICE_ID,Value = scan.PostVoiceID.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_FOV,Value = scan.ScanFOV.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_TRIGGER_START,Value = scan.TriggerStart.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_TRIGGER_END,Value = scan.TriggerEnd.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_TRIGGER_MODE,Value = scan.TriggerMode.ToString()},

	//		new (){Name = ProtocolParameterNames.SCAN_FOCAL_TYPE,Value = scan.Focal.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_EXPOSURE_DELAY_TIME,Value = scan.ExposureDelayTime.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_EXPOSURE_START_POSITION,Value = (scan.ExposureStartPosition*-1).ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_EXPOSURE_END_POSITION,Value = (scan.ExposureEndPosition*-1).ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION,Value = (scan.ReconVolumeStartPosition * -1).ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION,Value = (scan.ReconVolumeEndPosition * -1).ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_TABLE_START_POSITION,Value = (scan.TableStartPosition * -1).ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_TABLE_END_POSITION,Value = (scan.TableEndPosition * -1).ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_TABLE_ACCELERATION,Value = scan.TableAcceleration.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_TABLE_ACCELERATION_TIME,Value = scan.TableAccelerationTime.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_GANTRY_START_POSITION,Value = scan.GantryStartPosition.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_GANTRY_END_POSITION,Value = scan.GantryEndPosition.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_GANTRY_DIRECTION,Value = scan.GantryDirection.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_GANTRY_ACCELERATION,Value = scan.GantryAcceleration.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_GANTRY_ACCELERATION_TIME,Value = scan.GantryAccelerationTime.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_GANTRY_SPEED,Value = scan.GantrySpeed.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_WARM_UP,Value = scan.WarmUp.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_WARM_UP_TUBE_NUMBER,Value = scan.WarmUpTubeNumber.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_AUTO_DELETE_NUMBER,Value = scan.AutoDeleteNum.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_TOTAL_FRAMES,Value = scan.TotalFrames.ToString()},
	//		//new (){Name = ProtocolParameterNames.SCAN_COLLIMATOR_X,Value = scan.CollimatorX.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_COLLIMATOR_Z,Value = scan.CollimatorZ.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_COLLIMATOR_SLICE_WIDTH,Value = scan.CollimatorSliceWidth.ToString()},
	//		new (){Name = ProtocolParameterNames.SCAN_FUNCTION_MODE,Value = FunctionMode.Clinical.ToString()},
 //           new (){Name = ProtocolParameterNames.SCAN_SMALL_ANGLE_DELETE_LENGTH,Value = scan.SmallAngleDeleteLength.ToString()},
 //           new (){Name = ProtocolParameterNames.SCAN_LARGE_ANGLE_DELETE_LENGTH,Value = scan.LargeAngleDeleteLength.ToString()}

 //       };
	//	_protocolModificationService.SetParameters(scanModel, scanParameters);


	//	scanModel.Children = new List<ReconModel>();
	//	scanModel.Children.Add(reconModel);
	//	measurement.Children = new List<ScanModel>();
	//	measurement.Children.Add(scanModel);
	//	frame.Children = new List<MeasurementModel>();
	//	frame.Children.Add(measurement);
	//	pm.Children = new List<FrameOfReferenceModel>();
	//	pm.Children.Add(frame);

	//	return (pm,scanModel,reconModel, measurement);
	//}
	/// <summary>
	/// 合并Study下的ProtocolModel
	/// </summary>
	/// <param name="pm"></param>
	/// <param name="study"></param>
	/// <param name="scanParam"></param>
	/// <param name="reconSeriesParam"></param>
	/// <returns></returns>
   // private (ProtocolModel, NV.CT.Protocol.Models.ScanModel, ReconModel, MeasurementModel) ConstructStudyProtocolModel(ProtocolModel pm,Study study, ScanParam scanParam, ReconSeriesParam reconSeriesParam)
   // {

   //     var scanModel = new ScanModel();
   //     var reconModel = new ReconModel();
   //     var measurement = new MeasurementModel();

   //     var scan = scanParam;
   //     measurement.Descriptor = new DescriptorModel()
   //     {
   //         Id = IdGenerator.Next(3),
   //         Name = "Measurement",
   //         Type = "Measurement"
   //     };
   //     measurement.Status = PerformStatus.Performed;

   //     reconModel.Descriptor = new DescriptorModel()
   //     {
   //         Id = reconSeriesParam.ReconID,
   //         Name = "Recon",
   //         Type = "Recon"
   //     };
   //     reconModel.Status = PerformStatus.Performed;
   //     string rootdir = $"{reconSeriesParam.SeriesNumber}_{reconSeriesParam.SeriesInstanceUID}";
   //     var listParameters = new List<ParameterModel>()
   //     {
   //         new (){Name = ProtocolParameterNames.RECON_RECON_TYPE,Value = ReconType.HCT.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_FILTER_TYPE,Value = reconSeriesParam.FilterType.ToString()},
			////new (){Name = ProtocolParameterNames.RECON_BINNING,Value = ReconBinning.Bin11.ToString()},
			////new (){Name = ProtocolParameterNames.RECON_BM3D_DENOISE_COEF,Value = 2.ToString()},
			//new (){Name = ProtocolParameterNames.RECON_BONE_ARITIFACT_ENABALE,Value = 0.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_INTERP_TYPE,Value = InterpType.InterpNone.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_AIR_CORRECTION_MODE,Value = AirCorrectionMode.None.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_PRE_DENOISE_TYPE,Value = PreDenoiseType.BM3D.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_PRE_DENOISE_COEF,Value = 0.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_POST_DENOISE_TYPE,Value = PostDenoiseType.BM3D.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_POST_DENOISE_COEF,Value = 0.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_METAL_ARITIFACT_ENABLE,Value = 0.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_SLICE_THICKNESS,Value = reconSeriesParam.SliceThickness.ToString()},
			////new (){Name = ProtocolParameterNames.RECON_TV_DENOISE_COEF,Value = 2.ToString()},
			////new (){Name = ProtocolParameterNames.RECON_BOWTIE,Value = 1.ToString()},
			//new (){Name = ProtocolParameterNames.RECON_WINDOW_TYPE,Value ="Custom" },
   //         new (){Name = ProtocolParameterNames.RECON_VIEW_WINDOW_WIDTH,Value ="-1" },
   //         new (){Name = ProtocolParameterNames.RECON_VIEW_WINDOW_CENTER,Value ="0" },

   //         new (){Name = ProtocolParameterNames.RECON_WINDOW_CENTER,Value = ToArrString(new int[]{reconSeriesParam.WindowCenter[0]})},
   //         new (){Name = ProtocolParameterNames.RECON_WINDOW_WIDTH,Value = ToArrString(new int[]{reconSeriesParam.WindowWidth[0]})},
   //         new (){Name = ProtocolParameterNames.RECON_CENTER_FIRST_X,Value = 0.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y,Value = 0.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z,Value = (reconSeriesParam.CenterFirstZ*-1).ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_CENTER_LAST_X,Value = 0.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_CENTER_LAST_Y,Value = 0.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_CENTER_LAST_Z,Value = (reconSeriesParam.CenterLastZ*-1).ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X,Value = 1.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y,Value = 0.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z,Value = 0.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X,Value = 0.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y,Value = 1.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z,Value = 0.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_FOV_LENGTH_HORIZONTAL,Value = reconSeriesParam.FoVLengthHor.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_FOV_LENGTH_VERTICAL,Value = reconSeriesParam.FoVLengthVert.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_HORIZONTAL,Value = reconSeriesParam.ImageMatrixHor.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_VERTICAL,Value = reconSeriesParam.ImageMatrixVert.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_IMAGE_INCREMENT,Value = reconSeriesParam.ImageIncrement.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_IS_RTD,Value = "True"},
   //         new (){Name = ProtocolParameterNames.RECON_PRE_BINNING,Value = reconSeriesParam.PreBinning.ToString()},
   //         new (){Name = ProtocolParameterNames.RECON_IMAGE_PATH,Value = Path.Combine(RuntimeConfig.Console.MCSAppData.Path,study.StudyInstanceUID,rootdir)}
   //     };

   //     _protocolModificationService.SetParameters(reconModel, listParameters);

   //     scanModel.Descriptor = new DescriptorModel()
   //     {
   //         Id = scan.ScanUID,
   //         Name = scan.ScanOption.ToString(),
   //         Type = "Scan"
   //     };
   //     scanModel.Status = PerformStatus.Performed;

   //     var tubePositions = ToArrString(scan.TubePositions.Select(n => (int)n).ToArray());

   //     var scanParameters = new List<ParameterModel>()
   //     {
   //         new (){Name = ProtocolParameterNames.SCAN_BINNING,Value = scan.ScanBinning.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_PRE_VOICE_ID,Value = scan.PreVoiceID.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_POST_VOICE_ID,Value = scan.PostVoiceID.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_AUTO_SCAN,Value = scan.AutoScan.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_KILOVOLT,Value =ToArrString( scan.kV)},
   //         new (){Name = ProtocolParameterNames.SCAN_MILLIAMPERE,Value = ToArrString(DivideArray( scan.mA))},
   //         new (){Name = ProtocolParameterNames.SCAN_EXPOSURE_TIME,Value = scan.ExposureTime.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_FRAME_TIME,Value = scan.FrameTime.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_FRAMES_PER_CYCLE,Value = scan.FramesPerCycle.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_OPTION,Value = scan.ScanOption.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_MODE,Value = scan.ScanMode.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_RAW_DATA_TYPE,Value = scan.RawDataType.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_TRIGGER_MODE,Value = scan.TriggerMode.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_TRIGGER_END,Value = scan.TriggerEnd.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_EXPOSURE_MODE,Value = scan.ExposureMode.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_TABLE_DIRECTION,Value = scan.TableDirection.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_BOWTIE_ENABLE,Value = scan.BowtieEnable.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_GAIN,Value = scan.Gain.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_TABLE_HEIGHT,Value = scan.TableHeight.ToString()},
   //         new (){Name = ProtocolParameterNames.BODY_PART,Value = scan.BodyPart.ToString()},


   //         new (){Name = ProtocolParameterNames.SCAN_TUBE_POSITIONS,Value = tubePositions},
   //         new (){Name = ProtocolParameterNames.SCAN_TUBE_NUMBERS,Value =ToArrString(scan.TubeNumbers)},

   //         new (){Name = ProtocolParameterNames.SCAN_DOSE_CURVE,Value = "null"},
   //         new (){Name = ProtocolParameterNames.SCAN_TABLE_SPEED,Value = scan.TableSpeed.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_TABLE_FEED,Value = scan.TableFeed.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_PITCH,Value = scan.Pitch.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_PRE_OFFSET_FRAMES,Value = scan.PreOffsetFrames.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_POST_OFFSET_FRAMES,Value = scan.PostOffsetFrames.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_LOOPS,Value = scan.Loops.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_LOOP_TIME,Value = scan.LoopTime.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_PRE_VOICE_DELAY_TIME,Value = scan.PreVoiceDelayTime.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_PRE_VOICE_PLAY_TIME,Value = scan.PreVoicePlayTime.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_PRE_VOICE_ID,Value = scan.PreVoiceID.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_POST_VOICE_ID,Value = scan.PostVoiceID.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_FOV,Value = scan.ScanFOV.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_TRIGGER_START,Value = scan.TriggerStart.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_TRIGGER_END,Value = scan.TriggerEnd.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_TRIGGER_MODE,Value = scan.TriggerMode.ToString()},

   //         new (){Name = ProtocolParameterNames.SCAN_FOCAL_TYPE,Value = scan.Focal.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_EXPOSURE_DELAY_TIME,Value = scan.ExposureDelayTime.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_EXPOSURE_START_POSITION,Value = (scan.ExposureStartPosition*-1).ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_EXPOSURE_END_POSITION,Value = (scan.ExposureEndPosition*-1).ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION,Value = (scan.ReconVolumeStartPosition * -1).ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION,Value = (scan.ReconVolumeEndPosition * -1).ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_TABLE_START_POSITION,Value = (scan.TableStartPosition * -1).ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_TABLE_END_POSITION,Value = (scan.TableEndPosition * -1).ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_TABLE_ACCELERATION,Value = scan.TableAcceleration.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_TABLE_ACCELERATION_TIME,Value = scan.TableAccelerationTime.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_GANTRY_START_POSITION,Value = scan.GantryStartPosition.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_GANTRY_END_POSITION,Value = scan.GantryEndPosition.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_GANTRY_DIRECTION,Value = scan.GantryDirection.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_GANTRY_ACCELERATION,Value = scan.GantryAcceleration.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_GANTRY_ACCELERATION_TIME,Value = scan.GantryAccelerationTime.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_GANTRY_SPEED,Value = scan.GantrySpeed.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_WARM_UP,Value = scan.WarmUp.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_WARM_UP_TUBE_NUMBER,Value = scan.WarmUpTubeNumber.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_AUTO_DELETE_NUMBER,Value = scan.AutoDeleteNum.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_TOTAL_FRAMES,Value = scan.TotalFrames.ToString()},
			////new (){Name = ProtocolParameterNames.SCAN_COLLIMATOR_X,Value = scan.CollimatorX.ToString()},
			//new (){Name = ProtocolParameterNames.SCAN_COLLIMATOR_Z,Value = scan.CollimatorZ.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_COLLIMATOR_SLICE_WIDTH,Value = scan.CollimatorSliceWidth.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_FUNCTION_MODE,Value = FunctionMode.Clinical.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_SMALL_ANGLE_DELETE_LENGTH,Value = scan.SmallAngleDeleteLength.ToString()},
   //         new (){Name = ProtocolParameterNames.SCAN_LARGE_ANGLE_DELETE_LENGTH,Value = scan.LargeAngleDeleteLength.ToString()}

   //     };
   //     _protocolModificationService.SetParameters(scanModel, scanParameters);


   //     scanModel.Children = new List<ReconModel>();
   //     scanModel.Children.Add(reconModel);

   //     measurement.Children = new List<ScanModel>();
   //     measurement.Children.Add(scanModel);
   //     pm.Children[0].Children.Add(measurement);

   //     return (pm,scanModel,reconModel, measurement);
   // }



    private string ToArrString<T>(T[] arr)
	{
		if (arr is null)
			return $"[]";

		return $"[{string.Join(',', arr)}]";
	}

	private uint[] DivideArray(uint[] arr, int divider = 1000)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			arr[i] = (uint)(arr[i] / divider);
		}

		return arr;
	}
	/// <summary>
	/// 构造检查记录
	/// </summary>
	/// <param name="study"></param>
	/// <param name="scanParam"></param>
	/// <param name="patientModel"></param>
	/// <param name="patient"></param>
	/// <param name="reconSeriesParam"></param>
	/// <returns></returns>
	//private DatabaseService.Contract.Models.StudyModel ConstructStudyModel(Study study,ScanParam scanParam, DatabaseService.Contract.Models.PatientModel patientModel,Patient patient,ReconSeriesParam reconSeriesParam)
	//{
	//	var sm = new DatabaseService.Contract.Models.StudyModel();
	//	sm.StudyInstanceUID = study.StudyInstanceUID;
	//	sm.Id = Guid.NewGuid().ToString();
	//	sm.InternalPatientId = patientModel.Id;

	//	sm.BodyPart = scanParam.BodyPart.ToString();
	//	sm.StudyId = study.StudyID;
	//	sm.Age = GetPatientAge(patient.Age);
 //       sm.AgeType = AgeType.Year;
	//	sm.StudyDescription = study.StudyDescription;
	//	sm.StudyDate=study.StudyDate.GetValueOrDefault();
	//	sm.StudyTime = study.StudyTime.GetValueOrDefault();
	//	sm.PatientSex = _mapper.Map<Gender>(patient.Sex);
	//	sm.BodyPart= scanParam.BodyPart.ToString();
	//	sm.PatientType = 3;
	//	sm.Creator=study.OperatorsName;
 //       sm.CreateTime = study.StudyDate.GetValueOrDefault();


 //       return sm;
	//}
	private int GetPatientAge(string age)
	{
		if (age.Contains("Y"))
		{
			int index = age.IndexOf("Y");
			string filterage= age.Remove(index);
			return Convert.ToInt32(filterage);
		}
		return 0;
	}
	/// <summary>
	/// 构造患者数据模型
	/// </summary>
	//private DatabaseService.Contract.Models.PatientModel ConstructPatientModel(Patient patient,Study study)
	//{
	//	var pm = new DatabaseService.Contract.Models.PatientModel();
	//	pm.Id = Guid.NewGuid().ToString();
	//	pm.PatientName = patient.Name;
	//	pm.PatientId = patient.ID;
	//	pm.PatientSex = _mapper.Map<Gender>(patient.Sex);
	//	pm.PatientBirthDate = patient.BirthDate.GetValueOrDefault();
	//	pm.CreateTime = study.StudyDate.GetValueOrDefault();
	//	pm.Creator=study.OperatorsName;

	//	return pm;
	//}

	private bool CheckRawData(string dataPath)
	{
		bool result = File.Exists(Path.Combine(dataPath, "ScanReconParameter.json"));
		return result;
	}


	//private ScanReconParam GetRawDataModels(string dataPath)
	//{
	//	var sb = new StringBuilder();

 //       ScanReconParam scanreconparam = null;
	//	try
	//	{
 //           scanreconparam = JsonConvert.DeserializeObject<ScanReconParam>(File.ReadAllText(Path.Combine(dataPath, "ScanReconParameter.json")));
	//	}
	//	catch (Exception ex)
	//	{
	//		sb.Append($"Scan recon parameter deserialize failed: {ex.Message}");
	//	}


	//	return scanreconparam;
	//}


	private void CopyDirectory(string source_dir, string destinationDir, bool recursive)
	{
		var sourceDir = new DirectoryInfo(source_dir);

		if (!sourceDir.Exists)
			throw new DirectoryNotFoundException($"Source directory not found: {sourceDir.FullName}");

		DirectoryInfo[] dirs = sourceDir.GetDirectories();

		Directory.CreateDirectory(destinationDir);

		foreach (FileInfo file in sourceDir.GetFiles())
		{
			string targetFilePath = Path.Combine(destinationDir, file.Name);
			file.CopyTo(targetFilePath);
		}

		// If recursive and copying subdirectories, recursively call this method
		if (recursive)
		{
			foreach (DirectoryInfo subDir in dirs)
			{
				string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
				CopyDirectory(subDir.FullName, newDestinationDir, true);
			}
		}
	}

	private bool CheckRawData2(string dataPath)
	{
		var result = true;
		if (!File.Exists(Path.Combine(dataPath, "study.xml")))
		{
			result &= false;
		}
		if (!File.Exists(Path.Combine(dataPath, "patient.xml")))
		{
			result &= false;
		}
		if (!File.Exists(Path.Combine(dataPath, "ScanParameter.json")))
		{
			result &= false;
		}
		if (!File.Exists(Path.Combine(dataPath, "ReconSeriesParams.xml")))
		{
			result &= false;
		}
		return true;
	}
	//private DatabaseService.Contract.Models.StudyModel ConstructStudyModel_bak((Study Study, Patient Patient, ScanParam Scan, ReconSeriesParam Recon) result)
	//{
	//	var study = result.Study;
	//	var patient = result.Patient;
	//	var scan = result.Scan;
	//	var recon = result.Recon;

	//	var sm = new DatabaseService.Contract.Models.StudyModel();
	//	sm.StudyInstanceUID = study.StudyInstanceUID;
	//	sm.Id = Guid.NewGuid().ToString();
	//	sm.InternalPatientId = patient.ID;
	//	//sm.Age = patient.Age;
	//	sm.BodyPart = scan.BodyPart.ToString();
	//	sm.StudyId = study.StudyID;
	//	//sm.StudyStatus = ExaminationStatus

	//	return sm;
	//}	
	//private DatabaseService.Contract.Models.PatientModel ConstructPatientModel2(Patient patient)
	//{
	//	var pm = new DatabaseService.Contract.Models.PatientModel();
	//	pm.Id = Guid.NewGuid().ToString();
	//	pm.PatientName = patient.Name;
	//	pm.PatientId = patient.ID;
	//	if (patient.Sex is not null)
	//	{
	//		pm.PatientSex = (Gender)(int)(patient.Sex);
	//	}

	//	if (patient.BirthDate != null)
	//		pm.PatientBirthDate = (DateTime)patient.BirthDate;

	//	pm.CreateTime = DateTime.Now;

	//	return pm;
	//}
    #endregion
}