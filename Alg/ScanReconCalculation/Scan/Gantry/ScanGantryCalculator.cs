//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) $year$, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/02/02 09:42:22    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using NV.CT.Alg.ScanReconCalculation.Scan.Gantry.Axial;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry.DualTopo;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry.Helic;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry.Topo;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry.Bolus;

namespace NV.CT.Alg.ScanReconCalculation.Scan.Gantry;

public class ScanGantryCalculator : IScanGantryCalculator
{

    private static readonly Lazy<ScanGantryCalculator> _instance = new Lazy<ScanGantryCalculator>(() => new ScanGantryCalculator());

    public static ScanGantryCalculator Instance => _instance.Value;


    private List<IScanGantryCalculator> Calculators { get; } = new List<IScanGantryCalculator>();

    ScanGantryCalculator()
    {
        AddTableCalculator(new TopoGantryCalculator());
        AddTableCalculator(new DualTopoGantryCalculator());
        AddTableCalculator(new AxialGantryCalculator());
        AddTableCalculator(new HelicGantryCalculator());
        AddTableCalculator(new BolusGantryCalculator());
        AddTableCalculator(new TestBolusGantryCalculator());
    }

    public void AddTableCalculator(IScanGantryCalculator calculator)
    {
        if (Calculators.Contains(calculator))
            return;
        Calculators.Add(calculator);
    }

    public bool CanAccept(GantryControlInput input)
    {
        return Calculators.Any(x => x.CanAccept(input));
    }

    public GantryControlOutput GetGantryControlOutput(GantryControlInput input)
    {
        return GetCalculator(input).GetGantryControlOutput(input);
    }
    private IScanGantryCalculator GetCalculator(GantryControlInput input)
    {
        var calculator = Calculators.FirstOrDefault(x => x.CanAccept(input));
        if (calculator is null)
        {
            throw new InvalidOperationException($"No calculator can accept the input: {input.ScanOption}/{input.ScanMode}");
        }
        return calculator;
    }
}
