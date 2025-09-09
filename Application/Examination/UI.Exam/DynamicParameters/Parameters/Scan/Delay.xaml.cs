using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.UI.Exam.DynamicParameters.Parameters.Scan;

/// <summary>
/// Delay.xaml 的交互逻辑
/// </summary>
public partial class Delay : UserControl
{
    public Delay()
    {
        InitializeComponent();
		this.txtDelayTime.Minimum = UnitConvert.Microsecond2Second(SystemConfig.ScanningParamConfig.ScanningParam.MinExposureDelayTime.Value);
	}
}