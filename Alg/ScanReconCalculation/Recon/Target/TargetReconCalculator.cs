//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) $year$, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/14/04 14:02:21    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NV.CT.Alg.ScanReconCalculation.Recon.Target.Axial;
using NV.CT.Alg.ScanReconCalculation.Recon.Target.Helic;

namespace NV.CT.Alg.ScanReconCalculation.Recon.Target
{
    public class TargetReconCalculator:ITargetReconCalculator
    {
        private static readonly Lazy<TargetReconCalculator> _instance = new Lazy<TargetReconCalculator>(() => new TargetReconCalculator());

        public static TargetReconCalculator Instance => _instance.Value;

        private List<ITargetReconCalculator> Calculators { get; } = new List<ITargetReconCalculator>();

        private ILogger<TargetReconCalculator>? _logger;

        private TargetReconCalculator()
        {
            _logger = CTS.Global.ServiceProvider?.GetService<ILogger<TargetReconCalculator>>();
            AddTargetReconCalculator(new HelicTargetReconCalculator());
            AddTargetReconCalculator(new AxialTargetReconCalculator());
        }

        public void AddTargetReconCalculator(ITargetReconCalculator calculator)
        {
            if (Calculators.Contains(calculator))
                return;
            Calculators.Add(calculator);
        }

        public TargetReconOutput GetTargetReconParams(TargetReconInput input)
        {
            var calculator = GetCalculator(input);
            if (calculator == null) 
                return null;
            return calculator.GetTargetReconParams(input);
        }

        public bool CanAccept(TargetReconInput input)
        {
            return Calculators.Any(x => x.CanAccept(input));
        }

        private ITargetReconCalculator GetCalculator(TargetReconInput input)
        {
            var calculator = Calculators.FirstOrDefault(x => x.CanAccept(input));
            if (calculator is null)
            {
                _logger?.LogWarning($"NO TargetRecon calculator can handle recons for {input.ScanOption}");
                return null;
            }
            return calculator;
        }
    }

}
