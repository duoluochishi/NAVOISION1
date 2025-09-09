//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.ViewModel;

public class FooterViewModel : BaseViewModel
{
	private ProductSettingInfo _productInfo = new();
	public ProductSettingInfo ProductInfo
	{
		get => _productInfo;
		set => SetProperty(ref _productInfo, value);
	}

	public FooterViewModel()
	{
		ProductInfo = ProductConfig.ProductSettingConfig.ProductSetting;
	}
}