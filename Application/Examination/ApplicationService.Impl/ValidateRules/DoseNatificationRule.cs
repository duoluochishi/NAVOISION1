//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/31 14:45:22           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Language;
using NV.CT.Models;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.CT.WorkflowService.Contract;
using NV.MPS.Configuration;
using System.Text;

namespace NV.CT.Examination.ApplicationService.Impl.ValidateRules;

public class DoseNatificationRule : IGoValidateRule
{
	private readonly IGoValidateDialogService _goValidateDialogService;
	private readonly IProtocolHostService _protocolHostService;
	private readonly IAuthorization _authorization;
	private readonly IDoseEstimateService _doseEstimateService;
	private string showMessage = string.Empty;
	private DoseCheckModel _doseCheckModel = new DoseCheckModel();

	private List<DoseCheckModel> _doseCheckModels = new List<DoseCheckModel>();
	public DoseNatificationRule(IGoValidateDialogService goValidateDialogService,
		IProtocolHostService protocolHostService,
		IAuthorization authorization,
		IDoseEstimateService doseEstimateService)
	{
		_goValidateDialogService = goValidateDialogService;
		_protocolHostService = protocolHostService;
		_authorization = authorization;
		_doseEstimateService = doseEstimateService;
	}

	public void ValidateGo()
	{
		if (!IsCondition())
		{
			_goValidateDialogService.PopValidateMessageChanged(showMessage, RuleDialogType.DoseNotificationDialog, _doseCheckModels);
		}
	}

	private bool IsCondition()
	{
		_doseCheckModels = new List<DoseCheckModel>();
		if (!UserConfig.DoseSettingConfig.DoseSetting.NotificationEnabled.Value)
		{
			return true;
		}
		bool flag = true;
		var userName = string.Empty;
		if (_authorization.GetCurrentUser() is UserModel userModel)
		{
			userName = userModel.UserName;
		}
		StringBuilder stringBuilder = new StringBuilder();
		var _measurement = _protocolHostService.Models.FirstOrDefault(item => item.Measurement.Status == PerformStatus.Unperform);
		if (_measurement.Measurement is MeasurementModel measurementModel)
		{
			ScanDoseCheckHelper.GetDoseEstimatedInfoByUnperformMeasurement(_doseEstimateService, _protocolHostService, measurementModel);
			foreach (ScanModel scanModel in measurementModel.Children)
			{
				StringBuilder sb = new StringBuilder();
				if (scanModel.Status == PerformStatus.Unperform
					&& !(scanModel.ScanOption == ScanOption.Surview || scanModel.ScanOption == ScanOption.DualScout))
				{
					if (scanModel.DoseNotificationCTDI > 0 && scanModel.DoseEstimatedCTDI > scanModel.DoseNotificationCTDI)
					{
						sb.AppendLine(scanModel.Descriptor.Id + ":" + string.Format(LanguageResource.Message_CTDI_DoseNatification, scanModel.BodyPart, Math.Round(scanModel.DoseEstimatedCTDI, 2), Math.Round(scanModel.DoseNotificationCTDI, 2)));
					}
					if (scanModel.DoseNotificationDLP > 0 && scanModel.DoseEstimatedDLP > scanModel.DoseNotificationDLP)
					{
						sb.AppendLine(scanModel.Descriptor.Id + ":" + string.Format(LanguageResource.Message_DLP_DoseNatification, scanModel.BodyPart, Math.Round(scanModel.DoseEstimatedDLP, 2), Math.Round(scanModel.DoseNotificationDLP, 2)));
					}
				}
				if (sb.Length > 0)
				{
					_doseCheckModel = new DoseCheckModel();
					_doseCheckModel.Id = Guid.NewGuid().ToString();
					_doseCheckModel.DoseCheckType = DoseCheckType.Notification;
					_doseCheckModel.WarningDLP = scanModel.DoseNotificationDLP;
					_doseCheckModel.CurrentDLP = scanModel.DoseEstimatedDLP;
					_doseCheckModel.WarningCTDI = scanModel.DoseNotificationCTDI;
					_doseCheckModel.CurrentCTDI = scanModel.DoseEstimatedCTDI;

					_doseCheckModel.FrameOfReferenceId = scanModel.Parent.Parent.Descriptor.Id;
					_doseCheckModel.MeasurementId = scanModel.Parent.Descriptor.Id;
					_doseCheckModel.ScanId = scanModel.Descriptor.Id;
					_doseCheckModel.Operator = userName;

					_doseCheckModels.Add(_doseCheckModel);
					List<ParameterModel> parameterModels = new List<ParameterModel>
						{
							new ParameterModel
							{
								Name = ProtocolParameterNames.SCAN_DOSE_NOTIFICATION_ID,
								Value= _doseCheckModel.Id
							},
							new ParameterModel
							{
								Name = ProtocolParameterNames.SCAN_DOSE_NOTIFICATION_MESSAGE,
								Value= sb.ToString()
							},
							new ParameterModel
							{
								Name = ProtocolParameterNames.SCAN_DOSE_NOTIFICATION_OPERATOR,
								Value= userName,
							},
						};
					_protocolHostService.SetParameters(scanModel, parameterModels);
					flag &= false;
					stringBuilder.Append(sb);
				}
			}
		}
		if (!flag)
		{
			showMessage = stringBuilder.ToString();
		}
		return flag;
	}
}