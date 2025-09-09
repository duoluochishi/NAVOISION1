//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Windows;

namespace NV.MPS.UI.Dialog.Helper;

public static class MvvmHelpers
{
    public static void ViewAndViewModelAction<T>(object view, Action<T> action) where T : class
    {
        if (view is T viewAsT)
            action(viewAsT);

        if (view is FrameworkElement element && element.DataContext is T viewModelAsT)
            action(viewModelAsT);
    }

    public static T GetImplementerFromViewOrViewModel<T>(object view) where T : class
    {
        if (view is T viewAsT)
            return viewAsT;

        if (view is FrameworkElement element && element.DataContext is T vmAsT)
            return vmAsT;

        return null;
    }
}