//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Autofac;
using NV.CT.PatientManagement.ViewModel;

namespace NV.CT.PatientManagement.Extensions;

public static class ContainerBuilderExtension
{
    public static void AddViewModelContainer(this ContainerBuilder builder)
    {
        builder.RegisterModule<ViewModelModule>();
    }
}