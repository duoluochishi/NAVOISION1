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

using MySqlX.XDevAPI.Common;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.MPS.Configuration;
using System;

namespace NV.CT.ConfigManagement.ApplicationService.Impl;

public class WorklistNodeApplicationService : IWorklistNodeApplicationService
{
    public event EventHandler<EventArgs<(OperationType operation, WorklistInfo worklistInfo)>>? WorklistChanged;
    public event EventHandler? WorklistReload;

    public void SetWorklist(OperationType operation, WorklistInfo worklistInfo)
    {
        WorklistChanged?.Invoke(this, new EventArgs<(OperationType operation, WorklistInfo worklistInfo)>((operation, worklistInfo)));
    }

    public void ReloadWorklist()
    {
        WorklistReload?.Invoke(this, new EventArgs());
    }

    public List<WorklistInfo> GetWorklist()
    {
        return UserConfig.WorklistConfig.Worklists;
    }

    public bool Add(WorklistInfo worklistInfo)
    {
        bool result = true;
        var model = UserConfig.WorklistConfig.Worklists.FirstOrDefault(t => t.Id.Equals(worklistInfo.Id));
        if (model is not null)
        {
            result = false;
        }
        else
        {
            UserConfig.WorklistConfig.Worklists.Add(worklistInfo);
            if (worklistInfo.IsDefault)
            {
                UserConfig.WorklistConfig.Worklists.FindAll(t => t.IsDefault && t.Id != worklistInfo.Id).ForEach(t => t.IsDefault = false);
            }
            result = UserConfig.SaveWorklists();
        }
        return result;
    }

    public bool Update(WorklistInfo worklistInfo)
    {
        bool result = true;
        var model = UserConfig.WorklistConfig.Worklists.FirstOrDefault(t => t.Id.Equals(worklistInfo.Id));
        if (model is null)
        {
            result = false;
        }
        else
        {
            model.Id = worklistInfo.Id;
            model.AETitle = worklistInfo.AETitle;
            model.IP = worklistInfo.IP;
            model.Name = worklistInfo.Name;
            model.Port = worklistInfo.Port;
            model.IsMppsEnabled = worklistInfo.IsMppsEnabled;
            model.IsDefault = worklistInfo.IsDefault;
            model.Remark = worklistInfo.Remark;
            if (worklistInfo.IsDefault)
            {
                UserConfig.WorklistConfig.Worklists.FindAll(t => t.IsDefault && t.Id != worklistInfo.Id).ForEach(t => t.IsDefault = false);
            }
            result = UserConfig.SaveWorklists();
        }
        return result;
    }

    public bool Delete(string id)
    {
        if (UserConfig.WorklistConfig.Worklists.FirstOrDefault(t => t.Id.Equals(id)) is WorklistInfo model)
        {
            UserConfig.WorklistConfig.Worklists.Remove(model);
            return UserConfig.SaveWorklists();
        }
        return true;
    }
}