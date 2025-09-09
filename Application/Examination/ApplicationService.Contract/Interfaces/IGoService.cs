//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/28 9:48:00           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.CTS;
namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;

public interface IGoService
{
    event EventHandler<EventArgs<bool>> Validated;

    event EventHandler<EventArgs<bool>> ReconAllValidated;

    event EventHandler<EventArgs<bool>> ParameterValidated;

    event EventHandler<EventArgs<bool>> ParameterLogicValidated;

    IList<IGoValidateRule> ParameterValidateModels { get; set; }

    IList<IGoValidateRule> ReconAllValidateModels { get; set; }

    bool IsStopValidated { get; set; }

    void GoValidate();

    void ReconAllValidate();

    void StopValidated(bool isStop);

    void ParamsValidate();

    void ParamsLogicValidate();
}