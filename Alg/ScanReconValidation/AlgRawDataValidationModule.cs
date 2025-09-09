//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Autofac;

using NV.CT.Alg.ScanReconValidation.Scan.RawData;
using NV.CT.Alg.ScanReconValidation.Scan.RawData.Rule;

namespace NV.CT.Alg.ScanReconValidation;

/// <summary>
/// 算法-生数据-验证模块
/// </summary>
public class AlgRawDataValidationModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.Register(c =>
		{
			var validator = new RawDataValidator();
			//validator.AddValidationRule(new RawDataGeneratedFileNameValidationRule());
			//validator.AddValidationRule(new GeneratedFolderStructureValidateRule());
			//validator.AddValidationRule(new MCSParametersCompareValidationRule());
			validator.AddValidationRule(new RawDataHeaderInfoValidationRule());
			return validator;
		}).As<IRawDataValidator>().SingleInstance();
	}
}
