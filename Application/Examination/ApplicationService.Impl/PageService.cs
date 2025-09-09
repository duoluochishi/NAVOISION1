//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS;

namespace NV.CT.Examination.ApplicationService.Impl;

public class PageService : IPageService
{
    public ScanTaskAvailableLayout CurrentPage { get; private set; }

    public event EventHandler<EventArgs<ScanTaskAvailableLayout>>? CurrentPageChanged;

    public void SetCurrentPage(ScanTaskAvailableLayout pageName)
    {
        CurrentPage = pageName;
        CurrentPageChanged?.Invoke(this, new CTS.EventArgs<ScanTaskAvailableLayout>(pageName));
    }
}