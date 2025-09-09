//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/10/19 9:52:14           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.EventArguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Common.Enums.SelfCheck;
using NV.CT.FacadeProxy.Common.Models.SelfCheck;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface ISelfCheckingProxyService
{
	event EventHandler<SelfCheckEventArgs>? SelfCheckStatusChanged;

	List<SelfCheckInfo> GetResults();

	[Obsolete("这里的实现逻辑有问题")]
	BaseCommandResult StartSelfChecking(SelfCheckPartType partType);
}
