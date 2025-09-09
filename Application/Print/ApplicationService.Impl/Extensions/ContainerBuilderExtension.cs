//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Autofac;


namespace NV.CT.Print.ApplicationService.Impl.Extensions
{
    public static class ContainerBuilderExtension
    {
        public static void AddApplicationServiceContainer(this ContainerBuilder builder)
        {
            builder.RegisterModule<ApplicationServiceModule>();
        }
    }
}
