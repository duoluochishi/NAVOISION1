//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Autofac;

namespace NV.CT.SystemInterface.MCSRuntime.Impl.Extensions;

public static class ContainerBuilderExtension
{
    public static void AddMCSContainer(this ContainerBuilder builder)
    {
        builder.RegisterModule<MCSRuntimeModule>();
    }
}