using NV.CT.UI.Exam.DynamicParameters.Interfaces;

namespace NV.CT.UI.Exam.DynamicParameters.Impl;

public class DynamicTemplateService : IDynamicTemplateService
{
    private DynamicTemplates? _currentTemplate = DynamicTemplates.Default;
    private readonly ISelectionManager _selectionManager;
    public DynamicTemplateService(ISelectionManager selectionManager)
    {
        _selectionManager = selectionManager;
        _selectionManager.SelectionScanChanged -= SelectionManager_SelectionScanChanged;
        _selectionManager.SelectionScanChanged += SelectionManager_SelectionScanChanged;

        //emit at ctor
        if (_currentTemplate is not null)
        {
            TemplateNameChanged?.Invoke(this, (DynamicTemplates)_currentTemplate);
        }
    }

    private void SelectionManager_SelectionScanChanged(object? sender, EventArgs<ScanModel> e)
    {
        if (e is null || e.Data is null)
        {
            return;
        }
        DynamicTemplates? newTemplateName = DynamicTemplates.Default;

        if (e.Data.IsIntervention)
        {
            newTemplateName = DynamicTemplates.InterventionDefault;
        }
        else if (!e.Data.IsIntervention && (e.Data.ScanOption == FacadeProxy.Common.Enums.ScanOption.NVTestBolusBase
            || e.Data.ScanOption == FacadeProxy.Common.Enums.ScanOption.NVTestBolus
            || e.Data.ScanOption == FacadeProxy.Common.Enums.ScanOption.TestBolus
            || e.Data.ScanOption == FacadeProxy.Common.Enums.ScanOption.BolusTracking))
        {
            newTemplateName = DynamicTemplates.BolusDefault;
        }
        else if (!e.Data.IsIntervention && e.Data.ScanImageType == ScanImageType.Topo)
        {
            newTemplateName = DynamicTemplates.Default;
        }
        else if (!e.Data.IsIntervention && e.Data.ScanImageType == ScanImageType.Tomo)
        {
            newTemplateName = DynamicTemplates.AxialDefault;
        }
        if (!newTemplateName.Equals(_currentTemplate))
        {
            _currentTemplate = newTemplateName;
            TemplateNameChanged?.Invoke(this, (DynamicTemplates)_currentTemplate);
        }
    }

    public DynamicTemplates? GetTemplate()
    {
        return _currentTemplate;
    }

    public event EventHandler<DynamicTemplates>? TemplateNameChanged;

    public void SetTemplate(DynamicTemplates dynamicTemplates)
    {
        if (!dynamicTemplates.Equals(_currentTemplate))
        {
            _currentTemplate = dynamicTemplates;
            TemplateNameChanged?.Invoke(this, dynamicTemplates);
        }
    }
}