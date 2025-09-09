//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/12/18 11:36:50     V1.0.0        an.hu
// </summary>
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.MPS.Environment;

namespace NV.CT.ConfigService.Impl
{
    public class PrintProtocolConfigService : IPrintProtocolConfigService
    {
        private readonly ConfigRepository<PrintProtocolConfig>? _repository;
        public event EventHandler? ConfigRefreshed;

        public PrintProtocolConfigService()
        {
            _repository = new ConfigRepository<PrintProtocolConfig>(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "ConfigService", "PrintProtocol.xml"), false);
            _repository.ConfigRefreshed += PrintProtocolConfig_Refreshed; ;
        }

        private void PrintProtocolConfig_Refreshed(object? sender, EventArgs e)
        {
            ConfigRefreshed?.Invoke(sender, e);
        }

        public void Save(PrintProtocolConfig printProtocolConfig)
        {
            _repository?.Save(printProtocolConfig);
        }

        public PrintProtocolConfig GetConfigs()
        {
            return _repository.GetConfigs();
        }

    }
}
