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


using NV.CT.Alg.ScanReconCalculation.Scan.Table.Axial;
using NV.CT.Alg.ScanReconCalculation.Scan.Table.DualTopo;
using NV.CT.Alg.ScanReconCalculation.Scan.Table.Helic;
using NV.CT.Alg.ScanReconCalculation.Scan.Table.Topo;
using NV.CT.Alg.ScanReconCalculation.Scan.Table.Bolus;

namespace NV.CT.Alg.ScanReconCalculation.Scan.Table;


/// <summary>
/// 扫描检查床控制计算类，根据扫描检查床输入参数计算实际控制参数。
/// </summary>
public class ScanTableCalculator: IScanTableCalculator
{
    private static readonly Lazy<ScanTableCalculator> _instance = new Lazy<ScanTableCalculator>(() => new ScanTableCalculator());

    public static ScanTableCalculator Instance => _instance.Value;

    private List<IScanTableCalculator> Calculators { get; } = new List<IScanTableCalculator>();


    ScanTableCalculator()
    {
        AddTableCalculator(new TopoTableCalculator());
        AddTableCalculator(new DualTopoTableCalculator());
        AddTableCalculator(new AxialTableCalculator());
        AddTableCalculator(new HelicTableCalculator());
        AddTableCalculator(new BolusTableCalculator());
    }

    public void AddTableCalculator(IScanTableCalculator calculator)
    {
        if (Calculators.Contains(calculator))
            return;
        Calculators.Add(calculator);
    }

    public bool CanAccept(TableControlInput input)
    {
        return Calculators.Any(x=>x.CanAccept(input));
    }

    public TableControlOutput CalculateTableControlInfo(TableControlInput input)
    {
        return GetCalculator(input).CalculateTableControlInfo(input);
    }

    public double GetPreOffsetD2V(TableControlInput input)
    {
        return GetCalculator(input).GetPreOffsetD2V(input);
    }

    public double GetPostOffsetD2V(TableControlInput input)
    {
        return GetCalculator(input).GetPreOffsetD2V(input);
    }

    public double GetPreOffsetT2D(TableControlInput input)
    {
        return GetCalculator(input).GetPreOffsetT2D(input);
    }

    public double GetPostOffsetT2D(TableControlInput input)
    {
        return GetCalculator(input).GetPostOffsetT2D(input);
    }
    public double TryCorrectReconVolumnLength(TableControlInput input)
    {
        return GetCalculator(input).TryCorrectReconVolumnLength(input);
    }
    public double TryCorrectReconVolumnLength(TableControlInput input, double newScanLength)
    {
        return GetCalculator(input).TryCorrectReconVolumnLength(input,newScanLength);
    }

    private IScanTableCalculator GetCalculator(TableControlInput input)
    {
        var calculator = Calculators.FirstOrDefault(x => x.CanAccept(input));
        if (calculator is null)
        {
            throw new InvalidOperationException($"No calculator can accept the input: {input.ScanOption}/{input.ScanMode}");
        }
        return calculator;
    }

}
