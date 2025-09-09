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
using Autofac;

namespace NV.CT.SystemInterface.MRSIntegration.Impl.Extensions;

public static class ContainerBuilderExtension
{
    public static void AddRealtimeContainer(this ContainerBuilder builder)
    {
        builder.RegisterModule<MRSRealtimeModule>();
    }

    public static void AddOfflineContainer(this ContainerBuilder builder)
    {
        builder.RegisterModule<MRSOfflineModule>();
    }
}