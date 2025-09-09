//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Autofac;

namespace NV.CT.PatientBrowser.ViewModel;

public class ViewModelModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PatientInfoViewModel>().SingleInstance();
        builder.RegisterType<WorkListViewModel>().SingleInstance();
        builder.RegisterType<FiltrationViewModel>().SingleInstance();
    }
}