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

public class KvMaCoefficientApplicationService : IKvMaCoefficientApplicationService
{
    public event EventHandler<EventArgs<(OperationType operation, CategoryCoefficientInfo categoryCoefficentInfo)>>? ChangedHandler;
    public event EventHandler? ReloadHandler;

    public void Set(OperationType operation, CategoryCoefficientInfo categoryCoefficentInfo)
    {
        ChangedHandler?.Invoke(this, new EventArgs<(OperationType operation, CategoryCoefficientInfo categoryCoefficentInf)>((operation, categoryCoefficentInfo)));
    }

    public void Reload()
    {
        ReloadHandler?.Invoke(this, new EventArgs());
    }

    public List<CategoryCoefficientInfo> Get()
    {
        return SystemConfig.VoltageCurrentCoefficientConfig.Coefficients;
    }

    public bool Add(CategoryCoefficientInfo categoryCoefficentInfo)
    {
        bool result = true;
        var model = SystemConfig.VoltageCurrentCoefficientConfig.Coefficients.FirstOrDefault(t => t.KV.Equals(categoryCoefficentInfo.KV) && t.MA.Equals(categoryCoefficentInfo.MA));
        if (model is not null)
        {
            result = false;
        }
        else
        {
            SystemConfig.VoltageCurrentCoefficientConfig.Coefficients.Add(categoryCoefficentInfo);
            result = SystemConfig.SaveVoltageCurrentCoefficientConfig();
        }
        return result;
    }

    public bool Update(CategoryCoefficientInfo categoryCoefficentInfo)
    {
        bool result = true;
        var model = SystemConfig.VoltageCurrentCoefficientConfig.Coefficients.FirstOrDefault(t => t.KV.Equals(categoryCoefficentInfo.KV) && t.MA.Equals(categoryCoefficentInfo.MA));
        if (model is null)
        {
            result = false;
        }
        else
        {
            model.MA = categoryCoefficentInfo.MA;
            model.KV = categoryCoefficentInfo.KV;
            model.Sources = categoryCoefficentInfo.Sources;

            result = SystemConfig.SaveVoltageCurrentCoefficientConfig();
        }
        return result;
    }

    public bool Delete(int kv, int ma)
    {
        if (SystemConfig.VoltageCurrentCoefficientConfig.Coefficients.FirstOrDefault(t => t.KV.Equals(kv) && t.MA.Equals(ma)) is CategoryCoefficientInfo model)
        {
            SystemConfig.VoltageCurrentCoefficientConfig.Coefficients.Remove(model);
            return SystemConfig.SaveVoltageCurrentCoefficientConfig();
        }
        return true;
    }
}