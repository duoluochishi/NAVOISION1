//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/4/10 15:25:35     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface IDoorStatusService
{
    bool IsClosed { get; }

    event EventHandler<bool> StatusChanged;
}
