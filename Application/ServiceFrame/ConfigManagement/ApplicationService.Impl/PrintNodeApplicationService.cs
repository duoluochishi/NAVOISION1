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

public class PrintNodeApplicationService : IPrintNodeApplicationService
{
    public event EventHandler<EventArgs<(OperationType operation, PrinterInfo printInfo)>>? PrintNodeChanged;
    public event EventHandler? PrintNodeReload;

    public void SetPrintNode(OperationType operation, PrinterInfo printInfo)
    {
        PrintNodeChanged?.Invoke(this, new EventArgs<(OperationType operation, PrinterInfo printInfo)>((operation, printInfo)));
    }

    public void ReloadPrintNode()
    {
        PrintNodeReload?.Invoke(this, new EventArgs());
    }

    public List<PrinterInfo> GetPrintNodes()
    {
        return UserConfig.PrinterConfig.Printers;
    }

    public bool Add(PrinterInfo printInfo)
    {
        bool result = true;
        var model = UserConfig.PrinterConfig.Printers.FirstOrDefault(t => t.Id.Equals(printInfo.Id));
        if (model is not null)
        {
            result = false;
        }
        else
        {
            UserConfig.PrinterConfig.Printers.Add(printInfo);
            if (printInfo.IsDefault)
            {
                UserConfig.PrinterConfig.Printers.FindAll(t => t.IsDefault && t.Id != printInfo.Id).ForEach(t => t.IsDefault = false);
            }
            result = UserConfig.SavePrinters();
        }
        return result;
    }

    public bool Update(PrinterInfo printInfo)
    {
        bool result = true;
        var model = UserConfig.PrinterConfig.Printers.FirstOrDefault(t => t.Id.Equals(printInfo.Id));
        if (model is null)
        {
            result = false;
        }
        else
        {
            model.Id = printInfo.Id;
            model.AETitle = printInfo.AETitle;
            model.IP = printInfo.IP;
            model.Name = printInfo.Name;
            model.Port = printInfo.Port;
            model.CreateTime = printInfo.CreateTime;
            model.AECaller = printInfo.AECaller;
            model.Layout = printInfo.Layout;
            model.Resolution = printInfo.Resolution;
            model.IsDefault = printInfo.IsDefault;
            model.Remark = printInfo.Remark;
            model.Creator = printInfo.Creator;
            model.IsUsing = printInfo.IsUsing;
            model.PaperSize = printInfo.PaperSize;
            if (printInfo.IsDefault)
            {
                UserConfig.PrinterConfig.Printers.FindAll(t => t.IsDefault && t.Id != printInfo.Id).ForEach(t => t.IsDefault = false);
            }
            result = UserConfig.SavePrinters();
        }
        return result;
    }

    public bool Delete(string id)
    {
        if (UserConfig.PrinterConfig.Printers.FirstOrDefault(t => t.Id.Equals(id)) is PrinterInfo model)
        {
            UserConfig.PrinterConfig.Printers.Remove(model);
            return UserConfig.SavePrinters();
        }
        return true;
    }
}