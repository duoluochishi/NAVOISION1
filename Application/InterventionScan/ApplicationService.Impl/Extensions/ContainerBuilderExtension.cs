//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/10/23 15:38:52           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Autofac;
using NV.CT.InterventionScan.ApplicationService.Contract;

namespace NV.CT.InterventionScan.ApplicationService.Impl.Extensions;
public static class ContainerBuilderExtension
{
    public static void AddInterventionApplicationServiceContainer(this ContainerBuilder builder)
    {
        builder.RegisterType<InterventionService>().As<IInterventionService>().SingleInstance();
    }
}