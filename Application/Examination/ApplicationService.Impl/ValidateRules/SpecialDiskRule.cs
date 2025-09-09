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

using NV.CT.SystemInterface.MCSRuntime.Contract;
using NV.MPS.Configuration;

namespace NV.CT.Examination.ApplicationService.Impl.ValidateRules;

public class SpecialDiskRule : IGoValidateRule
{
	private readonly ISpecialDiskService _specialDiskService;
	private readonly IGoValidateDialogService _goValidateDialogService;

	public SpecialDiskRule(ISpecialDiskService specialDiskService, IGoValidateDialogService goValidateDialogService)
	{
		_specialDiskService = specialDiskService;
		_goValidateDialogService = goValidateDialogService;
	}

	public void ValidateGo()
	{
		if (!IsCondition())
		{
			_goValidateDialogService.PopValidateMessageChanged("Disk space is not available!", RuleDialogType.CommonNotificationDialog);
		}
	}

	private bool IsCondition()
	{
		var eMax = UserConfig.DiskspaceSettingConfig.DiskspaceSetting.ImageDataWarningThreshold.Value;
		var fMax = UserConfig.DiskspaceSettingConfig.DiskspaceSetting.RawDataWarningThreshold.Value;
		return _specialDiskService.EFreeSpaceRate > (100 - eMax) && _specialDiskService.FFreeSpaceRate > (100 - fMax);
	}
}