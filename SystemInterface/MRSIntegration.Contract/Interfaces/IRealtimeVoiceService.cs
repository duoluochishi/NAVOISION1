//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/8/7 9:36:14     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Models;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface IRealtimeVoiceService
{
    RealtimeCommandResult AddOrUpdate(ushort id, string filePath);

    RealtimeCommandResult Delete(ushort id);

    RealtimeCommandResult GetAll(out ushort[] ids);
}
