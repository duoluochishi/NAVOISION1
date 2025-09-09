namespace NV.CT.UI.Exam.Extensions;

public static class BaseViewModelExtensions
{
    public static T GetViewModel<T>(this BaseViewModel viewModel) where T : BaseViewModel
    {
        var result = Global.ServiceProvider.GetRequiredService<T>();
        if (result is null)
            return Activator.CreateInstance<T>();

        return result;
    }
}