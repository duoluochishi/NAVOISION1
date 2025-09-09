//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.AppService.Contract;

public class ApplicationRequest
{
    /// <summary>
    /// a class should have a default empty constructor nor that grpc communication fails
    /// </summary>
    public ApplicationRequest()
    {
    }
    public ApplicationRequest(string applicationName)
    {
        ApplicationName=applicationName;
        Parameters = string.Empty;
        NeedConfirm = true;
    }

    public ApplicationRequest(string applicationName,string parameters,bool needConfirm=true)
    {
        ApplicationName = applicationName;
        Parameters= parameters;
        NeedConfirm = needConfirm;
    }

	public ApplicationRequest(string applicationName, string parameters , object extraParameter,bool needConfirm=true)
    {
        ApplicationName = applicationName;
        Parameters = parameters;
        ExtraParameter = extraParameter;
        NeedConfirm = needConfirm;
    }


	public string ApplicationName { get; set; }
    public string Parameters { get; set; }
    public ProcessStatus Status { get; set; } = ProcessStatus.None;
    public bool NeedConfirm { get; set; }
    /// <summary>
    /// 额外参数
    /// </summary>
    public object? ExtraParameter { get; set; }
}
