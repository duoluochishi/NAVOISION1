using NV.CT.UI.Exam.DynamicParameters.Impl;
using NV.CT.UI.Exam.DynamicParameters.Interfaces;
using System.Windows.Input;

namespace NV.CT.UI.Exam.View;

public partial class ParameterDetailWindow
{
    private readonly IDynamicTemplateService? _dynamicTemplateService;
    public ParameterDetailWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        MouseDown += (_, _) =>
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        };
        DataContext = Global.ServiceProvider.GetRequiredService<ParameterDetailViewModel>();

        _dynamicTemplateService = Global.ServiceProvider?.GetRequiredService<IDynamicTemplateService>();
        if (_dynamicTemplateService is not null)
            _dynamicTemplateService.TemplateNameChanged += DynamicTemplate_TemplateNameChanged;

        InitContentHolder();
    }

    private void DynamicTemplate_TemplateNameChanged(object? sender, DynamicTemplates e)
    {
        InitContentHolder();
    }

    private void InitContentHolder()
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            var templateName = _dynamicTemplateService?.GetTemplate();
            if (templateName is null)
                return;

            var result = ContentControlResolver.Resolve((DynamicTemplates)templateName);
            ScanParameterDetailContentHolder.Content = result.ScanParameterDetail;
            ReconParameterDetailContentHolder.Content = result.ReconParameterDetail;
        });
    }
}