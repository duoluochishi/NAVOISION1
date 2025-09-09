//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
//     2024/8/13 10:08:58    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Models;
using NV.MPS.Configuration;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface IDetectorTemperatureService
{
	bool IsTemperatureNormalStatus { get; }

	event EventHandler<bool> TemperatureStatusChanged;

	Contract.Models.Detector CurrentDetector { get; set; }

	event EventHandler<List<Contract.Models.DetectorModule>> DetectorTemperatureOvershot;

	BaseCommandResult SetDetectorTargetTemperature(DetectorModuleTemperatureInfo temperature);

	BaseCommandResult SetDetectorTargetTemperature(List<DetectorModuleTemperatureInfo> temperatures, bool continueWithError = false);
}
