//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
namespace NV.CT.SyncService.Contract;

public interface IScreenSync
{
    string GetPreviousLayout();
    string GetCurrentLayout();
    void SwitchTo(string syncScreens);
    void Back();

    void Go();

    void Resume();

    event EventHandler<string>? ScreenChanged;
}