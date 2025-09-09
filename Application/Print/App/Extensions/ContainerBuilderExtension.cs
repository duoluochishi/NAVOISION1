//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Autofac;
using NV.CT.Print.ViewModel;

namespace NV.CT.Print.Extensions
{
    public static class ContainerBuilderExtension
    {
        public static void AddViewModelContainer(this ContainerBuilder builder)
        {
            builder.RegisterModule<ViewModelModule>();
        }
    }
}
