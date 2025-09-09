//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.MPS.Configuration;

namespace NV.CT.ConfigManagement.ApplicationService.Impl;

public class TabletApplicationService : ITabletApplicationService
{
    public event EventHandler<EventArgs<(OperationType operation, TabletInfo tabletModel)>>? TabletInfoChanged;
    public event EventHandler? TabletListReload;

    public TabletApplicationService()
    {
    }

    public void SetTabletInfo(OperationType operation, TabletInfo tabletModel)
    {
        TabletInfoChanged?.Invoke(this, new EventArgs<(OperationType operation, TabletInfo tabletModel)>((operation, tabletModel)));
    }

    public void ReloadTabletList()
    {
        TabletListReload?.Invoke(this, new EventArgs());
    }

    public List<TabletInfo> GetAllTabletInfo()
    {
        return UserConfig.TabletConfig.Tablets;
    }

    public bool Add(TabletInfo tabletModel)
    {
        bool result = true;
        var model = UserConfig.TabletConfig.Tablets.FirstOrDefault(t => t.Id.Equals(tabletModel.Id));
        if (model is not null)
        {
            result = false;
        }
        else
        {
            UserConfig.TabletConfig.Tablets.Add(tabletModel);
            result = UserConfig.SaveTablets();
        }
        return result;
    }

    public bool Update(TabletInfo tabletModel)
    {
        bool result = true;
        var model = UserConfig.TabletConfig.Tablets.FirstOrDefault(t => t.Id.Equals(tabletModel.Id));
        if (model is null)
        {
            result = false;
        }
        else
        {
            model.Id = tabletModel.Id;
            model.SerialNumber = tabletModel.SerialNumber;
            model.IP = tabletModel.IP;
            result = UserConfig.SaveTablets();
        }
        return result;
    }

    public bool Delete(string id)
    {
        if (UserConfig.TabletConfig.Tablets.FirstOrDefault(t => t.Id.Equals(id)) is TabletInfo model)
        {
            UserConfig.TabletConfig.Tablets.Remove(model);
            return UserConfig.SaveTablets();
        }
        return true;
    }
}