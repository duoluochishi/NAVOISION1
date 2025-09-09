//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Autofac;

namespace NV.MPS.UI.Dialog.Extension;

public static class ContainerBuilderExtension
{
    public static void AddDialogServiceContianer(this ContainerBuilder builder)
    {
        builder.RegisterModule<DialogServiceModule>();
    }
}