//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/20 15:55:08     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.SystemInterface.MRSIntegration.Contract.Models;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface IHeatCapacityService
{
    List<Tube> Current { get; }

    event EventHandler<List<Tube>> HeatCapacityChanged;

    float MaxHeatCapacity { get; }

    float MinHeatCapacity { get; }
}
