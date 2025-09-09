//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/8/30 14:31:08           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Models;

namespace NV.CT.DatabaseService.Contract;

public interface IDoseCheckService
{
    bool Add(DoseCheckModel doseCheckModel);

	bool AddList(List<DoseCheckModel> doseCheckModels);

	bool Update(DoseCheckModel doseCheckModel);

	bool UpdateList(List<DoseCheckModel> doseCheckModels);

	bool Delete(DoseCheckModel doseCheckModel);

    DoseCheckModel Get(string doseCheckId);

    List<DoseCheckModel> GetAll();
}