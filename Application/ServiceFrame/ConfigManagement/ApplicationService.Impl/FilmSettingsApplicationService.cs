//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/7/5 16:35:51       V1.0.0      胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.CTS;
using NV.CT.CTS.Enums;

namespace NV.CT.ConfigManagement.ApplicationService.Impl;

public class FilmSettingsApplicationService : IFilmSettingsApplicationService
{
    public event EventHandler<EventArgs<(OperationType operation, FilmSettings filmSettings)>>? Changed;
    public event EventHandler? Reloaded;

    private readonly IFilmSettingsConfigService _filmSettingsConfigService;
    public FilmSettingsApplicationService(IFilmSettingsConfigService filmSettingsConfigService)
    {
        _filmSettingsConfigService = filmSettingsConfigService;
    }

    public void Set(OperationType operation, FilmSettings filmSettings)
    {
        Changed?.Invoke(this, new EventArgs<(OperationType operation, FilmSettings filmSettings)>((operation, filmSettings)));
    }

    public void Reload()
    {
        Reloaded?.Invoke(this, new EventArgs());
    }

    public List<FilmSettings> Get()
    {
        return _filmSettingsConfigService.GetConfigs().FilmSettingsList.ToList();
    }

    public bool Add(FilmSettings filmSettings)
    {
        bool result = true;
        var config = _filmSettingsConfigService.GetConfigs();
        var list = config.FilmSettingsList;
        var model = list.FirstOrDefault(t => t.Id.Equals(filmSettings.Id));
        if (model is not null)
        {
            result = false;
        }
        else
        {
            list.Add(filmSettings);
            _filmSettingsConfigService.Save(config);
            result = true;
        }
        return result;
    }

    public bool Update(FilmSettings filmSettings)
    {
        bool result = true;
        var config = _filmSettingsConfigService.GetConfigs();
        var model = config.FilmSettingsList.FirstOrDefault(t => t.Id.Equals(filmSettings.Id));
        if (model is null)
        {
            result = false;
        }
        else
        {
            model.Id = filmSettings.Id;
            model.IsPortrait = filmSettings.IsPortrait;
            model.NormalizedHeaderHeight = filmSettings.NormalizedHeaderHeight;
            model.NormalizedFooterHeight = filmSettings.NormalizedFooterHeight;
            model.HeaderLogo = filmSettings.HeaderLogo;
            model.FooterLogo = filmSettings.FooterLogo;
            model.HeaderCellsList = filmSettings.HeaderCellsList;
            model.FooterCellsList = filmSettings.FooterCellsList;
            _filmSettingsConfigService.Save(config);
            result = true;
        }
        return result;
    }

    public bool Delete(string id)
    {
        bool result = true;
        var config = _filmSettingsConfigService.GetConfigs();
        var list = config.FilmSettingsList;
        var model = list.FirstOrDefault(t => t.Id.Equals(id));
        if (model is null)
        {
            result = false;
        }
        else
        {
            list.Remove(model);
            _filmSettingsConfigService.Save(config);
            result = true;
        }
        return result;
    }
}