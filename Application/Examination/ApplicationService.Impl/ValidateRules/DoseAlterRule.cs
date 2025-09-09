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

public class DoseAlterRule : IGoValidateRule
{
	private readonly IGoValidateDialogService _goValidateDialogService;
	private readonly IProtocolHostService _protocolHostService;
	private readonly IAuthorization _authorization;
	private readonly IDoseEstimateService _doseEstimateService;
	private string showMessage = string.Empty;
	private DoseCheckModel _doseCheckModel = new DoseCheckModel();
	private List<DoseCheckModel> _doseCheckModels = new List<DoseCheckModel>();
	private double SettingDLP = 0.0;
	private double SettingCTDI = 0.0;
	public DoseAlterRule(IGoValidateDialogService goValidateDialogService,
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
			_goValidateDialogService.PopValidateMessageChanged(showMessage, RuleDialogType.DoseAlertDialog, _doseCheckModels);
		}
	}

	private bool IsCondition()
	{
		_doseCheckModels = new List<DoseCheckModel>();
		bool flag = true;
		StringBuilder stringBuilder = new StringBuilder();
		switch (_protocolHostService.Instance.BodySize)
		{
			case BodySize.Child:
				SettingDLP = UserConfig.DoseSettingConfig.DoseSetting.ChildAlertDLPThreshold.Value;
				SettingCTDI = UserConfig.DoseSettingConfig.DoseSetting.ChildAlertCTDIThreshold.Value;
				break;
			case BodySize.Adult:
			default:
				SettingDLP = UserConfig.DoseSettingConfig.DoseSetting.AdultAlertDLPThreshold.Value;
				SettingCTDI = UserConfig.DoseSettingConfig.DoseSetting.AdultAlertCTDIThreshold.Value;
				break;
		}
		var userName = string.Empty;
		if (_authorization.GetCurrentUser() is UserModel userModel)
		{
			userName = userModel.UserName;
		}
		var _measurement = _protocolHostService.Models.FirstOrDefault(item => item.Measurement.Status == PerformStatus.Unperform);
		if (_measurement.Measurement is MeasurementModel measurementModel)
		{
			ScanDoseCheckHelper.GetDoseEstimatedInfoByUnperformMeasurement(_doseEstimateService, _protocolHostService, measurementModel);
			foreach (ScanModel scanModel in measurementModel.Children)
			{
				StringBuilder sb = new StringBuilder();
				float CTDI = scanModel.AccumulatedDoseEstimatedCTDI;
				float DLP = scanModel.AccumulatedDoseEstimatedDLP;
				if (scanModel.Status == PerformStatus.Unperform
					&& !(scanModel.ScanOption == ScanOption.Surview || scanModel.ScanOption == ScanOption.DualScout))
				{
					if (SettingCTDI > 0 && CTDI > SettingCTDI)
					{
						sb.AppendLine(scanModel.Descriptor.Id + ":" + string.Format(LanguageResource.Message_CTDI_DoseAlert, Math.Round(CTDI, 2), Math.Round(SettingCTDI, 2), _protocolHostService.Instance.BodySize));
					}
					if (SettingDLP > 0 && DLP > SettingDLP)
					{
						sb.AppendLine(scanModel.Descriptor.Id + ":" + string.Format(LanguageResource.Message_DLP_DoseAlert, Math.Round(DLP, 2), Math.Round(SettingDLP, 2), _protocolHostService.Instance.BodySize));
					}
				}
				if (sb.Length > 0)
				{
					_doseCheckModel = new DoseCheckModel();
					_doseCheckModel.Id = Guid.NewGuid().ToString();
					_doseCheckModel.DoseCheckType = DoseCheckType.Alert;
					_doseCheckModel.WarningDLP = SettingDLP;
					_doseCheckModel.CurrentDLP = DLP;
					_doseCheckModel.WarningCTDI = SettingCTDI;
					_doseCheckModel.CurrentCTDI = CTDI;
					_doseCheckModel.FrameOfReferenceId = scanModel.Parent.Parent.Descriptor.Id;
					_doseCheckModel.MeasurementId = scanModel.Parent.Descriptor.Id;
					_doseCheckModel.ScanId = scanModel.Descriptor.Id;
					_doseCheckModel.Operator = userName;
					_doseCheckModels.Add(_doseCheckModel);
					List<ParameterModel> parameterModels = new List<ParameterModel>
						{
							new ParameterModel
							{
								Name = ProtocolParameterNames.SCAN_DOSE_ALERT_ID,
								Value= _doseCheckModel.Id
							},
							 new ParameterModel
							{
								Name = ProtocolParameterNames.SCAN_DOSE_ALERT_MESSAGE,
								Value= sb.ToString()
							},
							new ParameterModel
							{
								Name = ProtocolParameterNames.SCAN_DOSE_ALERT_OPERATOR,
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