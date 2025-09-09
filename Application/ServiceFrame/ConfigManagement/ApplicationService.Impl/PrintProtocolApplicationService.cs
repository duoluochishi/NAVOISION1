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
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.CTS;
using NV.CT.CTS.Enums;

namespace NV.CT.ConfigManagement.ApplicationService.Impl;

public class PrintProtocolApplicationService : IPrintProtocolApplicationService
{
    public event EventHandler<EventArgs<(OperationType operation, PrintProtocol printProtocol)>>? Changed;
    public event EventHandler? Reloaded;
    public event EventHandler? RowColmunChanged;

    private readonly IPrintProtocolConfigService _printProtocolConfigService;
    public PrintProtocolApplicationService(IPrintProtocolConfigService printProtocolConfigService)
    {
        _printProtocolConfigService = printProtocolConfigService;
    }

    public void Set(OperationType operation, PrintProtocol printProtocol)
    {
        Changed?.Invoke(this, new EventArgs<(OperationType operation, PrintProtocol printProtocol)>((operation, printProtocol)));
    }

    public void Reload()
    {
        Reloaded?.Invoke(this, new EventArgs());
    }

    public void RowClomunchange()
    {
        RowColmunChanged?.Invoke(this, new EventArgs());
    }

    public List<PrintProtocol> Get()
    {
        return _printProtocolConfigService.GetConfigs().PrintProtocols.ToList();
    }

    public bool Add(PrintProtocol printProtocol)
    {
        bool result = true;
        var config = _printProtocolConfigService.GetConfigs();
        var list = config.PrintProtocols;
        var model = list.FirstOrDefault(t => t.Id.Equals(printProtocol.Id));
        if (model is not null)
        {
            result = false;
        }
        else
        {
            list.Add(printProtocol);
            if (printProtocol.IsDefault)
            {
                list.ToList().FindAll(t => t.IsDefault && t.Id != printProtocol.Id && t.BodyPart.Equals(printProtocol.BodyPart)).ForEach(t => t.IsDefault = false);
            }
            _printProtocolConfigService.Save(config);
            result = true;
        }
        return result;
    }

    public bool Update(PrintProtocol printProtocol)
    {
        bool result = true;
        var config = _printProtocolConfigService.GetConfigs();
        var list = config.PrintProtocols;
        var model = list.FirstOrDefault(t => t.Id.Equals(printProtocol.Id));
        if (model is null)
        {
            result = false;
        }
        else
        {
            model.Id = printProtocol.Id;
            model.Name = printProtocol.Name;
            model.BodyPart = printProtocol.BodyPart;
            model.IsSystem = printProtocol.IsSystem;
            model.IsDefault = printProtocol.IsDefault;
            model.Row = printProtocol.Row;
            model.Column = printProtocol.Column;
            if (printProtocol.IsDefault)
            {
                list.ToList().FindAll(t => t.IsDefault && t.Id != printProtocol.Id && t.BodyPart.Equals(printProtocol.BodyPart)).ForEach(t => t.IsDefault = false);
            }
            _printProtocolConfigService.Save(config);
            result = true;
        }
        return result;
    }

    public bool Delete(string id)
    {
        bool result = true;
        var config = _printProtocolConfigService.GetConfigs();
        var list = config.PrintProtocols;
        var model = list.FirstOrDefault(t => t.Id.Equals(id));
        if (model is null)
        {
            result = false;
        }
        else
        {
            list.Remove(model);
            _printProtocolConfigService.Save(config);
            result = true;
        }
        return result;
    }
}