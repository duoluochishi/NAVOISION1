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

public interface IKvMaCoefficientApplicationService
{
    event EventHandler<EventArgs<(OperationType operation, CategoryCoefficientInfo categoryCoefficentInfo)>> ChangedHandler;

    event EventHandler ReloadHandler;

    void Set(OperationType operation, CategoryCoefficientInfo categoryCoefficentInfo);

    void Reload();

    List<CategoryCoefficientInfo> Get();

    bool Add(CategoryCoefficientInfo categoryCoefficentInfo);

    bool Update(CategoryCoefficientInfo categoryCoefficentInfo);

    bool Delete(int kv, int ma);
}