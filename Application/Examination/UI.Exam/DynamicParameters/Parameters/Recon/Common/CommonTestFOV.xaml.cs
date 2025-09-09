
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.UI.Exam.DynamicParameters.Parameters.Recon;
/// <summary>
/// Kernel.xaml 的交互逻辑
/// </summary>
public partial class CommonTestFOV : UserControl
{
	public CommonTestFOV()
	{
		InitializeComponent();
		//int min = SystemConfig.OfflineReconParamConfig.OfflineReconParam.ReconMatrix.Ranges.Min();
		//int max = SystemConfig.OfflineReconParamConfig.OfflineReconParam.ReconMatrix.Ranges.Max();

		//this.CommonReconFov.Minimum = Math.Round(UnitConvert.Micron2Millimeter((double)((SystemConfig.OfflineReconParamConfig.OfflineReconParam.DefinedMinPixelSpacing.Value) * min)), 2);
		//this.CommonReconFov.Maximum = Math.Round(UnitConvert.Micron2Millimeter((double)((SystemConfig.OfflineReconParamConfig.OfflineReconParam.DefinedMinPixelSpacing.Value) * max)), 2);
		this.CommonReconFov.Minimum = -1000000;
		this.CommonReconFov.Maximum = 1000000;
	}
}