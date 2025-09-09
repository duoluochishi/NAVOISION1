//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Autofac;
using NV.CT.Print.ApplicationService.Contract.Interfaces;

namespace NV.CT.Print.ApplicationService.Impl
{
    public class ApplicationServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PrintApplicationService>().As<IPrintApplicationService>().SingleInstance();
        }
    }
}
