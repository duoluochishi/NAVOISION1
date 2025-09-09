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

public class ArchiveNodeApplicationService : IArchiveNodeApplicationService
{
    public event EventHandler<EventArgs<(OperationType operation, ArchiveInfo archiveInfo)>>? ArchiveNodeChanged;
    public event EventHandler? ArchiveNodeReload;
    public void SetArchiveNode(OperationType operation, ArchiveInfo archiveInfo)
    {
        ArchiveNodeChanged?.Invoke(this, new EventArgs<(OperationType operation, ArchiveInfo archiveInfo)>((operation, archiveInfo)));
    }

    public void ReloadArchiveNodes()
    {
        ArchiveNodeReload?.Invoke(this, new EventArgs());
    }

    public List<ArchiveInfo> GetArchiveNodes()
    {
        return UserConfig.ArchiveConfig.Archives;
    }

    public bool Add(ArchiveInfo archiveInfo)
    {
        bool result = true;
        var model = UserConfig.ArchiveConfig.Archives.FirstOrDefault(t => t.Id.Equals(archiveInfo.Id));
        if (model is not null)
        {
            result = false;
        }
        else
        {
            UserConfig.ArchiveConfig.Archives.Add(archiveInfo);
            if (archiveInfo.IsDefault)
            {
                UserConfig.ArchiveConfig.Archives.FindAll(t => t.IsDefault && t.Id != archiveInfo.Id).ForEach(t => t.IsDefault = false);
            }
            result = UserConfig.SaveArchives();
        }
        return result;
    }

    public bool Update(ArchiveInfo archiveInfo)
    {

        bool result = true;
        var model = UserConfig.ArchiveConfig.Archives.FirstOrDefault(t => t.Id.Equals(archiveInfo.Id));
        if (model is null)
        {
            result = false;
        }
        else
        {
            model.Id = archiveInfo.Id;
            model.ServerAETitle = archiveInfo.ServerAETitle;
            model.IP = archiveInfo.IP;
            model.ClientAETitle = archiveInfo.ClientAETitle;
            model.Port = archiveInfo.Port;
            model.IncludesDeoseReportingByAuto = archiveInfo.IncludesDeoseReportingByAuto;
            model.IsDefault = archiveInfo.IsDefault;
            model.Remark = archiveInfo.Remark;
            if (archiveInfo.IsDefault)
            {
                UserConfig.ArchiveConfig.Archives.FindAll(t => t.IsDefault && t.Id != archiveInfo.Id).ForEach(t => t.IsDefault = false);
            }
            result = UserConfig.SaveArchives();
        }
        return result;
    }

    public bool Delete(string id)
    {
        if (UserConfig.ArchiveConfig.Archives.FirstOrDefault(t => t.Id.Equals(id)) is ArchiveInfo model)
        {
            UserConfig.ArchiveConfig.Archives.Remove(model);
            return UserConfig.SaveArchives();
        }
        return true;
    }
}