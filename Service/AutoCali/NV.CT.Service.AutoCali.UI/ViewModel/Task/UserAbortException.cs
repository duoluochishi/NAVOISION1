using System;

namespace NV.CT.Service.AutoCali.Logic
{
    internal class AbstractAbortException : Exception { }

    /// <summary>
    /// 用户选择 终止运行，抛出定制异常，用来快速简单跳出控制流程
    /// </summary>
    internal class UserAbortException : AbstractAbortException { }
    internal class SystemAbortException : AbstractAbortException { }
}
