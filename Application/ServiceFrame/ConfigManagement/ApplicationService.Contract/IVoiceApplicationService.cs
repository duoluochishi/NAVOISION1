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
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.FacadeProxy.Common.Arguments;
namespace NV.CT.ConfigManagement.ApplicationService.Contract;

public interface IVoiceApplicationService
{
    event EventHandler<EventArgs<(OperationType operation, VoiceModel voiceModel)>> VoiceInfoChanged;

    event EventHandler VoiceListReload;

    void SetVoiceInfo(OperationType operation, VoiceModel voiceModel);

    void ReloadVoiceList();

    List<VoiceModel> GetVoiceInfo(string front);

    List<VoiceModel> GetVoiceModels();

    bool SetDefault(VoiceModel voiceModel);

    bool Add(VoiceModel voiceModel);

    bool Update(VoiceModel voiceModel);

    bool Delete(VoiceModel voiceModel);

    VoiceModel GetVoiceInfoByID(string id);

    ushort GetMaxInternalId();

    /// <summary>
    /// 添加或者更新语音到Auxboard
    /// </summary>
    /// <param name="voiceModel"></param>
    /// <returns>执行结果</returns>
    bool AddOrUpdate(VoiceModel voiceModel);

    /// <summary>
    /// 获取Auxboard中的所有语音的Internalid
    /// </summary>
    /// <returns>语音的Internalid数组</returns>
    ushort[] GetAll();

    /// <summary>
    /// 删除Auxboard中的语音
    /// </summary>
    /// <param name="internalId"></param>
    /// <returns></returns>
    bool Delete(ushort internalId);

    /// <summary>
    /// 让控制盒播放指定语音
    /// </summary>
    /// <param name="internalId"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    bool PlayAudioFile(ushort internalId, Action<AudioPlaybackEventArgs> callback);
}