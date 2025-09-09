//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:19    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
//using NV.CT.CTS.Models;
//using NV.CT.FacadeProxy.Common.Enums;

//namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

//public interface IOfflineShutdownService: IShutdownService
//{

//}

//public interface IEmbeddedSystemShutdownService: IShutdownService
//{

//}

///// <summary>
///// MRS shutdown interface
///// </summary>
//public interface IShutdownService
//{
//    event EventHandler<(ShutdownStatus Status, string ErrorCode)>? ShutdownStatusChanged;

//    event EventHandler<bool>? ConnectionStatusChanged;

//    BaseCommandResult CanShutdown(ShutdownScope scope);

//    BaseCommandResult Shutdown(ShutdownScope scope);

//	BaseCommandResult CanRestart(ShutdownScope scope);

//    BaseCommandResult Restart(ShutdownScope scope);
//}
