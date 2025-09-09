//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.CTS.Enums;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;

public interface IPageService
{
    ScanTaskAvailableLayout CurrentPage { get; }

    event EventHandler<EventArgs<ScanTaskAvailableLayout>>? CurrentPageChanged;

    void SetCurrentPage(ScanTaskAvailableLayout pageName);
}