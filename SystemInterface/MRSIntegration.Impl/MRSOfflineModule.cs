//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/1/8 17:31:54    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Autofac;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class MRSOfflineModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<OfflineProxyService>().As<IOfflineProxyService>().SingleInstance();

        builder.RegisterType<OfflineConnectionService>().As<IOfflineConnectionService>().SingleInstance();

        builder.RegisterType<OfflineTaskProxyService>().As<IOfflineTaskProxyService>().SingleInstance();
    }
}
