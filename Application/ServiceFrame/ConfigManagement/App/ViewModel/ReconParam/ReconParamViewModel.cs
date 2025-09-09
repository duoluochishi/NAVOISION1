//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.FacadeProxy.Common.Enums;
using System.Collections.Generic;

namespace NV.CT.ConfigManagement.ViewModel;

public class ReconParamViewModel : BaseViewModel
{
    private ObservableCollection<BaseItemViewModel<string>> _reconTypes = new ObservableCollection<BaseItemViewModel<string>>();
    public ObservableCollection<BaseItemViewModel<string>> ReconTypes
    {
        get => _reconTypes;
        set => SetProperty(ref _reconTypes, value);
    }

    private ObservableCollection<BaseItemViewModel<int>> _sliceThicknes = new ObservableCollection<BaseItemViewModel<int>>();
    public ObservableCollection<BaseItemViewModel<int>> SliceThicknes
    {
        get => _sliceThicknes;
        set => SetProperty(ref _sliceThicknes, value);
    }

    private ObservableCollection<BaseItemViewModel<int>> _imageIncrements = new ObservableCollection<BaseItemViewModel<int>>();
    public ObservableCollection<BaseItemViewModel<int>> ImageIncrements
    {
        get => _imageIncrements;
        set => SetProperty(ref _imageIncrements, value);
    }

    private ObservableCollection<BaseItemViewModel<int>> _reconMatrixs = new ObservableCollection<BaseItemViewModel<int>>();
    public ObservableCollection<BaseItemViewModel<int>> ReconMatrixs
    {
        get => _reconMatrixs;
        set => SetProperty(ref _reconMatrixs, value);
    }

	private double _definedMinPixelSpacing = 165;
	public double DefinedMinPixelSpacing
	{
		get => _definedMinPixelSpacing;
		set => SetProperty(ref _definedMinPixelSpacing, value);
	}

	public ReconParamViewModel()
    {
        InitReconTypeList();
        InitReconMatrixList();
        InitImageIncrements();
        InitSliceThickness();
    }

    private void InitReconTypeList()
    {
        foreach (var enumItem in Enum.GetValues(typeof(ReconType)))
        {
            if (enumItem is not null)
            {
                ReconTypes.Add(new BaseItemViewModel<string> { Display = enumItem.ToString(), Key = enumItem.ToString() });
            }
        }
    }

    private void InitReconMatrixList()
    {
        ReconMatrixs.Add(new BaseItemViewModel<int> { Key = 512, Display = "512" });
        ReconMatrixs.Add(new BaseItemViewModel<int> { Key = 768, Display = "768" });
        ReconMatrixs.Add(new BaseItemViewModel<int> { Key = 1024, Display = "1024" });
        ReconMatrixs.Add(new BaseItemViewModel<int> { Key = 1536, Display = "1536" });
        ReconMatrixs.Add(new BaseItemViewModel<int> { Key = 2048, Display = "2048" });
        ReconMatrixs.Add(new BaseItemViewModel<int> { Key = 3072, Display = "3072" });
    }

    private void InitSliceThickness()
    {
        SliceThicknes.Add(new BaseItemViewModel<int> { Key = 165, Display = "0.165" });
        SliceThicknes.Add(new BaseItemViewModel<int> { Key = 330, Display = "0.33" });
        SliceThicknes.Add(new BaseItemViewModel<int> { Key = 660, Display = "0.66" });
        SliceThicknes.Add(new BaseItemViewModel<int> { Key = 1000, Display = "1" });
        SliceThicknes.Add(new BaseItemViewModel<int> { Key = 2000, Display = "2" });
        SliceThicknes.Add(new BaseItemViewModel<int> { Key = 3000, Display = "3" });
        SliceThicknes.Add(new BaseItemViewModel<int> { Key = 5000, Display = "5" });
        SliceThicknes.Add(new BaseItemViewModel<int> { Key = 10000, Display = "10" });
    }

    private void InitImageIncrements()
    {
        ImageIncrements.Add(new BaseItemViewModel<int> { Key = 165, Display = "0.165" });
        ImageIncrements.Add(new BaseItemViewModel<int> { Key = 330, Display = "0.33" });
        ImageIncrements.Add(new BaseItemViewModel<int> { Key = 500, Display = "0.5" });
        ImageIncrements.Add(new BaseItemViewModel<int> { Key = 660, Display = "0.66" });
        ImageIncrements.Add(new BaseItemViewModel<int> { Key = 1000, Display = "1" });
        ImageIncrements.Add(new BaseItemViewModel<int> { Key = 1500, Display = "1.5" });
        ImageIncrements.Add(new BaseItemViewModel<int> { Key = 2000, Display = "2" });
        ImageIncrements.Add(new BaseItemViewModel<int> { Key = 3000, Display = "3" });
        ImageIncrements.Add(new BaseItemViewModel<int> { Key = 5000, Display = "5" });
        ImageIncrements.Add(new BaseItemViewModel<int> { Key = 10000, Display = "10" });
    }
}