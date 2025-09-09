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
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.MPS.Configuration;

namespace NV.CT.ConfigManagement.ApplicationService.Impl;

public class WindowingApplicationService : IWindowingApplicationService
{
    public event EventHandler<EventArgs<(OperationType operation, WindowingInfo windowingInfo)>>? WindowingChanged;
    public event EventHandler? WindowingReload;

    public void SetWindowing(OperationType operation, WindowingInfo windowingInfo)
    {
        WindowingChanged?.Invoke(this, new EventArgs<(OperationType operation, WindowingInfo windowingInfo)>((operation, windowingInfo)));
    }

    public void ReloadWindowing()
    {
        WindowingReload?.Invoke(this, new EventArgs());
    }

    public List<WindowingInfo> GetWindowings()
    {
        return UserConfig.WindowingConfig.Windowings;
    }

    public bool Add(WindowingInfo windowingInfo)
    {
        bool result = true;
        var model = UserConfig.WindowingConfig.Windowings.FirstOrDefault(t => t.Id is not null && t.Id.Equals(windowingInfo.Id));
        if (model is not null)
        {
            result = false;
        }
        else
        {
            //保证快捷键唯一
            var shortcut = UserConfig.WindowingConfig.Windowings.FirstOrDefault(t => t.Shortcut is not null && t.Shortcut.Equals(windowingInfo.Shortcut));
            if (shortcut is not null)
            {
                result = false;
            }
            else
            {
                UserConfig.WindowingConfig.Windowings.Add(windowingInfo);
                result = UserConfig.SaveWindowings();
            }
        }
        return result;
    }

    public bool Update(WindowingInfo windowingInfo)
    {
        bool result = true;
        var model = UserConfig.WindowingConfig.Windowings.FirstOrDefault(t => t.Id.Equals(windowingInfo.Id));
        if (model is null)
        {
            result = false;
        }
        else
        {
            //保证快捷键唯一
            var shortcut = UserConfig.WindowingConfig.Windowings.FirstOrDefault(t => t.Shortcut is not null && t.Id is not null && t.Shortcut.Equals(windowingInfo.Shortcut) && !t.Id.Equals(windowingInfo.Id));
            if (shortcut is not null)
            {
                result = false;
            }
            else
            {
                model.Id = windowingInfo.Id;
                model.BodyPart = windowingInfo.BodyPart;
                model.Shortcut = windowingInfo.Shortcut;
                model.IsFactory = windowingInfo.IsFactory;
                model.Width.Value = windowingInfo.Width.Value;
                model.Level.Value = windowingInfo.Level.Value;
                model.Description = windowingInfo.Description;
                result = UserConfig.SaveWindowings();
            }
        }
        return result;
    }

    public bool Delete(string id)
    {
        if (UserConfig.WindowingConfig.Windowings.FirstOrDefault(t => t.Id is not null && t.Id.Equals(id)) is WindowingInfo model)
        {
            UserConfig.WindowingConfig.Windowings.Remove(model);
            return UserConfig.SaveWindowings();
        }
        return true;
    }
}