using NV.CT.UI.Exam.DynamicParameters.Interfaces;

namespace NV.CT.UI.Exam.DynamicParameters.Impl;

public class ContentControlResolver
{
    private static DynamicTemplates advancedSourceDynamicTemplate = DynamicTemplates.Default;

    public static ContentControlResult Resolve(DynamicTemplates dynamicTemplate)
    {
        var contentResult = new ContentControlResult();

        if (dynamicTemplate == DynamicTemplates.Default)
        {
            contentResult.ScanParameter = Global.ServiceProvider?.GetService<Templates.Default.ScanParameterControl>();
            contentResult.ReconParameter = Global.ServiceProvider?.GetService<Templates.Default.ReconParameterControl>();
            contentResult.ScanParameterDetail = Global.ServiceProvider?.GetService<Templates.Default.ScanParameterDetailControl>();
            contentResult.ReconParameterDetail = Global.ServiceProvider?.GetService<Templates.Default.ReconParameterDetailControl>();
        }
        else if (dynamicTemplate == DynamicTemplates.AxialDefault)
        {
            contentResult.ScanParameter = Global.ServiceProvider?.GetService<Templates.AxialDefault.ScanParameterControl>();
            contentResult.ReconParameter = Global.ServiceProvider?.GetService<Templates.AxialDefault.ReconParameterControl>();
            contentResult.ScanParameterDetail = Global.ServiceProvider?.GetService<Templates.AxialDefault.ScanParameterDetailControl>();
            contentResult.ReconParameterDetail = Global.ServiceProvider?.GetService<Templates.AxialDefault.ReconParameterDetailControl>();
        }
        else if (dynamicTemplate == DynamicTemplates.InterventionDefault)
        {
            contentResult.ScanParameter = Global.ServiceProvider?.GetService<Templates.InterventionDefault.ScanParameterControl>();
            contentResult.ReconParameter = Global.ServiceProvider?.GetService<Templates.InterventionDefault.ReconParameterControl>();
            contentResult.ScanParameterDetail = Global.ServiceProvider?.GetService<Templates.InterventionDefault.ScanParameterDetailControl>();
            contentResult.ReconParameterDetail = Global.ServiceProvider?.GetService<Templates.InterventionDefault.ReconParameterDetailControl>();
        }
        else if (dynamicTemplate == DynamicTemplates.BolusDefault)
        {
            contentResult.ScanParameter = Global.ServiceProvider?.GetService<Templates.Bolus.ScanParameterControl>();
            contentResult.ReconParameter = Global.ServiceProvider?.GetService<Templates.Bolus.ReconParameterControl>();
            contentResult.ScanParameterDetail = Global.ServiceProvider?.GetService<Templates.Bolus.ScanParameterDetailControl>();
            contentResult.ReconParameterDetail = Global.ServiceProvider?.GetService<Templates.Bolus.ReconParameterDetailControl>();
        }
        else if (dynamicTemplate == DynamicTemplates.AdvancedParamterDetail)
        {
            contentResult = Resolve(advancedSourceDynamicTemplate);
            contentResult.ScanParameterDetail = Global.ServiceProvider?.GetService<Templates.AdvancedScanParameterDetailControl>();
            contentResult.ReconParameterDetail = Global.ServiceProvider?.GetService<Templates.AdvancedReconParameterDetailControl>();
        }
        else if (dynamicTemplate == DynamicTemplates.SourceParamterDetail)
        {
            contentResult = Resolve(advancedSourceDynamicTemplate);
        }
        if (dynamicTemplate != DynamicTemplates.AdvancedParamterDetail && dynamicTemplate != DynamicTemplates.SourceParamterDetail)
        {
            advancedSourceDynamicTemplate = dynamicTemplate;
        }
        return contentResult;
    }
}