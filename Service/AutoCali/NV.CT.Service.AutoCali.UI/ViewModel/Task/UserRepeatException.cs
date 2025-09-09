using System;

namespace NV.CT.Service.AutoCali.Logic
{
    /// <summary>
    /// 用户选择重新运行，抛出定制异常，用来快速简单跳出控制流程
    /// </summary>
    internal class UserRepeatException : Exception { }
}
