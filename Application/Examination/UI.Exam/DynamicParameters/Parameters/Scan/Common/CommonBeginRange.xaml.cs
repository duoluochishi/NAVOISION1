using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.UI.Exam.DynamicParameters.Parameters.Scan;

public partial class CommonBeginRange : UserControl
{
    public CommonBeginRange()
    {
        InitializeComponent();

        this.CommonBenginRange.Minimum = Math.Round((float)UnitConvert.Micron2Millimeter(SystemConfig.TableConfig.Table.MinZ.Value), 2);
        this.CommonBenginRange.Maximum = Math.Round((float)UnitConvert.Micron2Millimeter(SystemConfig.TableConfig.Table.MaxZ.Value), 2);
    }
}