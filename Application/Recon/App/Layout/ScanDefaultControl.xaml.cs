//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.UI.Exam.DynamicParameters.Impl;
using NV.CT.UI.Exam.DynamicParameters.Interfaces;

namespace NV.CT.Recon.Layout;

public partial class ScanDefaultControl
{
	private readonly IDynamicTemplateService? _dynamicTemplateService;

	public ScanDefaultControl()
	{
		InitializeComponent();
		DataContext = CTS.Global.ServiceProvider.GetRequiredService<ScanDefaultViewModel>();

		_dynamicTemplateService = CTS.Global.ServiceProvider?.GetRequiredService<IDynamicTemplateService>();
		if (_dynamicTemplateService != null)
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
			ScanParameterContentHolder.Content = result.ScanParameter;
			ReconParameterContentHolder.Content = result.ReconParameter;
		});
	}
}