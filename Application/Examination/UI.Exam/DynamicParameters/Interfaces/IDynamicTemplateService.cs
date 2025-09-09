namespace NV.CT.UI.Exam.DynamicParameters.Interfaces;
public interface IDynamicTemplateService
{
    DynamicTemplates? GetTemplate();

    event EventHandler<DynamicTemplates>? TemplateNameChanged;

    void SetTemplate(DynamicTemplates dynamicTemplates);
}