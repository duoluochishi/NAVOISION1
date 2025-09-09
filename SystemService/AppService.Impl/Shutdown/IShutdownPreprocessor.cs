//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.AppService.Impl.Shutdown;

public interface IShutdownPreprocessor
{
    void Shutdown();

    void Restart();
}
