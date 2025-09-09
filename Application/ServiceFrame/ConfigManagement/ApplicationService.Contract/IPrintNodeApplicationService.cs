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

using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.MPS.Configuration;
namespace NV.CT.ConfigManagement.ApplicationService.Contract;

public interface IPrintNodeApplicationService
{
    event EventHandler<EventArgs<(OperationType operation, PrinterInfo printInfo)>> PrintNodeChanged;

    event EventHandler PrintNodeReload;

    void SetPrintNode(OperationType operation, PrinterInfo printInfo);

    void ReloadPrintNode();

    List<PrinterInfo> GetPrintNodes();

    bool Add(PrinterInfo printInfo);

    bool Update(PrinterInfo printInfo);

    bool Delete(string id);
}