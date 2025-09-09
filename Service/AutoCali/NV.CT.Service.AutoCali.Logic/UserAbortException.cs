using System;

namespace NV.CT.Service.AutoCali.Logic
{
    /// <summary>
    /// 用户选择 终止运行，抛出定制异常，用来快速简单跳出控制流程
    /// </summary>
    public class UserAbortException : Exception { }
}
