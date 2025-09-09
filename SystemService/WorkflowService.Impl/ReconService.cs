using Microsoft.Extensions.Logging;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.ErrorCodes;
using NV.CT.Protocol;
using NV.CT.Protocol.Models;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.WorkflowService.Impl;

public class ReconService : IReconService
{
	private readonly ILogger _logger;
	private readonly IStudyService _studyService;
	private readonly IReconTaskService _reconTaskService;
	private readonly ISeriesService _seriesService;

	private List<string> _openReconStudies;

	public ReconService(ILogger<ReconService> logger, IStudyService studyService, IReconTaskService reconTaskService, ISeriesService seriesService)
	{
		_logger = logger;
		_studyService = studyService;
		_seriesService = seriesService;
		_reconTaskService = reconTaskService;
        _openReconStudies = new List<string>();
	}

	public void OpenReconApplication(string studyId)
	{
		if (_openReconStudies.Contains(studyId)) return;
		_openReconStudies.Add(studyId);
	}

	public void CloseReconApplication(string studyId)
	{
		if (_openReconStudies.Contains(studyId))
		{
			_openReconStudies.Remove(studyId);
		}
	}

	public bool CheckReconApplication(string studyId)
	{
		return _openReconStudies.Contains(studyId);
	}

	public void ResumeReconStates(string studyId)
	{
		try
		{
			var parameterChanged = false;

			//var content = File.ReadAllText(@"D:\downloads\protocol.xml");
			var (studyModel, _) = _studyService.Get(studyId);
			if (studyModel != null && !string.IsNullOrEmpty(studyModel.Protocol))
			{
				//var protocolModel = ProtocolHelper.Deserialize(content);
				var protocolModel = ProtocolHelper.Deserialize(studyModel.Protocol);
				if (protocolModel is null)
					return;

				//针对没有恢复正常状态的recon
				//var notRightRecons = ProtocolHelper.ExpandRecons(protocolModel)
				//	.Where(n => n.Recon.Status == PerformStatus.Performing)
				//	.Select(n => n).ToList();

				var allRecons = ProtocolHelper.ExpandRecons(protocolModel).Select(n => n).ToList();

				foreach (var item in allRecons)
				{
					var reconTask = _reconTaskService.Get2(studyId, item.Scan.Descriptor.Id, item.Recon.Descriptor.Id);

					//拿recon id查到的对应的序列
					var seriesTask = _seriesService.GetSeriesByReconId(item.Recon.Descriptor.Id);

					//先处理 performed的,避免因为后面的逻辑设置后导致进入
					//如果是正常状态的,但是有序列被删除了,当前Recon需要修改
					if (item.Recon.Status == PerformStatus.Performed)
					{
						//如果发现这个Recon对应的序列被删除了
						if (seriesTask is null || seriesTask.IsDeleted)
						{
							var targetRecon = protocolModel.Children
								.SelectMany(n => n.Children)
								.SelectMany(m => m.Children)
								.SelectMany(p => p.Children)
								.FirstOrDefault(j => j.Descriptor.Id == item.Recon.Descriptor.Id);
							if (targetRecon != null)
							{
								_logger.LogInformation($"ResumeReconState find target recon: {targetRecon.Descriptor.Id}, {targetRecon.Status.ToString()}");

								//该序列被用户删除,则将重建状态设为 UserDeletion 
								targetRecon.FailureReason = FailureReasonType.UserDeletion;
								// MCS014000003
								var errorCodes = new List<string> { ErrorCodeResource.MCS_Series_UserDeletion_Code };
								ProtocolHelper.SetParameter(targetRecon, ProtocolParameterNames.ERROR_CODE, errorCodes.ToJson());

								parameterChanged = true;
							}
						}
					}
					//对于是 Unperform Performing  Waiting 的这样处理
					else
					{
						//检查这几个异常的recon id 在数据库里面是否已经完成了,然后确认已经完成的id集合 
						_logger.LogInformation($"OfflineRecon status not correct, parameters: {studyId}, {item.Frame.Descriptor.Id}, {item.Measurement.Descriptor.Id}, {item.Scan.Descriptor.Id}, {item.Recon.Descriptor.Id}, {item.Recon.Status.ToString()}");

						if (reconTask is not null && (
							reconTask.TaskStatus == (int)OfflineTaskStatus.Finished ||
							reconTask.TaskStatus == (int)OfflineTaskStatus.Error))
						{
							var targetRecon = protocolModel.Children
								.SelectMany(n => n.Children)
								.SelectMany(m => m.Children)
								.SelectMany(p => p.Children)
								.FirstOrDefault(j => j.Descriptor.Id == item.Recon.Descriptor.Id);
							if (targetRecon != null)
							{
								targetRecon.Status = PerformStatus.Performed;
							}

							//重建错误,没有序列
							if (seriesTask != null && targetRecon != null)
							{
								ProtocolHelper.SetParameter(targetRecon, ProtocolParameterNames.RECON_IMAGE_PATH,
									seriesTask.SeriesPath);

								parameterChanged = true;
							}
						}

						//cancelled的归类为 Unperformed 让它可以继续去重建
						if (reconTask is not null && (reconTask.TaskStatus == (int)OfflineTaskStatus.Cancelled || reconTask.TaskStatus == (int)OfflineTaskStatus.Created || reconTask.TaskStatus == (int)OfflineTaskStatus.None))
						{
							var targetRecon = protocolModel.Children
								.SelectMany(n => n.Children)
								.SelectMany(m => m.Children)
								.SelectMany(p => p.Children)
								.FirstOrDefault(j => j.Descriptor.Id == item.Recon.Descriptor.Id);
							if (targetRecon != null)
							{
								targetRecon.Status = PerformStatus.Unperform;

								parameterChanged = true;
							}
						}

						if (reconTask is not null && reconTask.TaskStatus == (int)OfflineTaskStatus.Waiting)
						{
							var targetRecon = protocolModel.Children
								.SelectMany(n => n.Children)
								.SelectMany(m => m.Children)
								.SelectMany(p => p.Children)
								.FirstOrDefault(j => j.Descriptor.Id == item.Recon.Descriptor.Id);
							if (targetRecon != null)
							{
								targetRecon.Status = PerformStatus.Waiting;

								parameterChanged = true;
							}
						}

						if (reconTask is not null && reconTask.TaskStatus == (int)OfflineTaskStatus.Executing)
						{
							var targetRecon = protocolModel.Children
								.SelectMany(n => n.Children)
								.SelectMany(m => m.Children)
								.SelectMany(p => p.Children)
								.FirstOrDefault(j => j.Descriptor.Id == item.Recon.Descriptor.Id);
							if (targetRecon != null)
							{
								targetRecon.Status = PerformStatus.Performing;

								parameterChanged = true;
							}
						}
					}

				}

				if (parameterChanged)
				{
					var newProtocol = ProtocolHelper.Serialize(protocolModel);
					_logger.LogInformation($"OfflineRecon resume recon status with study_id : {studyId}");
					_studyService.UpdateProtocolByStudyId(new UpdateStudyProtocolReq()
					{
						StudyId = studyId,
						Protocol = newProtocol
					});
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError($"OfflineRecon resume recon status with error : {ex.Message}-{ex.StackTrace}");
		}
	}

}