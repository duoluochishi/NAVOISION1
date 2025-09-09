using NV.CT.Service.AutoCali.DAL;

namespace NV.CT.Service.AutoCali.Logic
{
    /// <summary>
    /// 日常校准的ViewModel
    /// 与自动校准区别在于，配置来源不同，没有校准场景管理功能
    /// </summary>
    public class DailyCaliViewModel : AbstractCaliViewModel
    {
        private static readonly string ClassName = nameof(DailyCaliViewModel);

        protected override void InitDtoService()
        {
            ScenarioDtoService = DailyCaliScenarioServiceImpl.Instance;
            ScenarioConfigDtoService = DailyCaliScenarioServiceImpl.Instance;
            HistoryDtoService = DailyCaliHistoryServiceImpl.Instance;
        }
    }
}
