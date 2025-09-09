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

using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
namespace NV.CT.ConfigManagement.ApplicationService.Contract;

public interface IPrintProtocolApplicationService
{
    event EventHandler<EventArgs<(OperationType operation, PrintProtocol printProtocol)>> Changed;

    event EventHandler Reloaded;

    event EventHandler RowColmunChanged;

    void Set(OperationType operation, PrintProtocol printProtocol);

    void Reload();

    void RowClomunchange();

    List<PrintProtocol> Get();

    bool Add(PrintProtocol printProtocol);

    bool Update(PrintProtocol printProtocol);

    bool Delete(string id);
}