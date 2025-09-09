using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.UI.Exam.DynamicParameters.Parameters.Recon;
/// <summary>
/// Kernel.xaml 的交互逻辑
/// </summary>
public partial class TestFOV : UserControl
{
	public TestFOV()
	{
		InitializeComponent();
		//int min = SystemConfig.OfflineReconParamConfig.OfflineReconParam.ReconMatrix.Ranges.Min();
		//int max = SystemConfig.OfflineReconParamConfig.OfflineReconParam.ReconMatrix.Ranges.Max();

		//this.ReconFov.Minimum = Math.Round(UnitConvert.Micron2Millimeter((double)((SystemConfig.OfflineReconParamConfig.OfflineReconParam.DefinedMinPixelSpacing.Value) * min)), 2);
		//this.ReconFov.Maximum = Math.Round(UnitConvert.Micron2Millimeter((double)((SystemConfig.OfflineReconParamConfig.OfflineReconParam.DefinedMinPixelSpacing.Value) * max)), 2);
		this.ReconFov.Minimum = -1000000;
		this.ReconFov.Maximum = 1000000;
	}
}