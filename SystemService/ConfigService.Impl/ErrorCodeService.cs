//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/6 16:16:34     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Impl;
using NV.CT.ConfigService.Models.SystemConfig;
using NV.MPS.Environment;

namespace NV.CT.ConfigService.Server.Services;

public class ErrorCodeService : IErrorCodeService
{
    private readonly ConfigRepository<ErrorConfig> _repository;

    public ErrorCodeService()
    {
        _repository = new ConfigRepository<ErrorConfig>(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "ConfigService", "ErrorCodes.xml"), false);
        _repository.ConfigRefreshed += ErrorCode_ConfigRefreshed;
    }

    private void ErrorCode_ConfigRefreshed(object? sender, EventArgs e)
    {
        ConfigRefreshed?.Invoke(sender, e);
    }

    public event EventHandler ConfigRefreshed;

    public ErrorConfig GetConfigs()
    {
        return _repository.GetConfigs();
    }

    public void Save(ErrorConfig errorConfig)
    {
        _repository.Save(errorConfig);
    }
}
