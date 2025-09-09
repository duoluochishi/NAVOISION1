//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/7/5 16:35:51    V1.0.0        胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
namespace NV.CT.ConfigManagement.ApplicationService.Contract;

public interface IFilmSettingsApplicationService
{
    event EventHandler<EventArgs<(OperationType operation, FilmSettings filmSettings)>> Changed;

    event EventHandler Reloaded;

    void Set(OperationType operation, FilmSettings filmSettings);

    void Reload();

    List<FilmSettings> Get();

    bool Add(FilmSettings filmSettings);

    bool Update(FilmSettings filmSettings);

    bool Delete(string id);
}