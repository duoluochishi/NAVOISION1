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
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums.AudioFileEnums;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface IRealtimeVoiceService
{
    #region Obsolete APIs
    [Obsolete("This API is obsolete. Use AddOrUpdateAudioFile instead.")]
    RealtimeCommandResult AddOrUpdate(ushort id, string filePath);

    [Obsolete("This API is obsolete. Use DeleteAudioFile instead.")]
    RealtimeCommandResult Delete(ushort id);

    [Obsolete("This API is obsolete. Use GetAudioFiles instead.")]
    RealtimeCommandResult GetAll(out ushort[] ids); 
    #endregion

    /// <summary>
    /// 播放语音文件
    /// </summary>
    /// <param name="id"></param>
    /// <param name="fileType">支持api扫描语音和呼吸训练语音</param>
    /// <param name="callback"></param>
    /// <returns></returns>
    bool PlayAudioFile(ushort id, AudioFileType fileType, Action<AudioPlaybackEventArgs> callback);

    /// <summary>
    /// 添加或者更新语音文件
    /// </summary>
    /// <param name="id"></param>
    /// <param name="filePath"></param>
    /// <param name="fileType">支持api扫描语音和呼吸训练语音</param>
    /// <returns></returns>
    RealtimeCommandResult AddOrUpdateAudioFile(ushort id, string filePath, AudioFileType fileType);

    /// <summary>
    /// 删除语音文件
    /// </summary>
    /// <param name="id"></param>
    /// <param name="fileType">支持api扫描语音和呼吸训练语音</param>
    /// <returns></returns>
    RealtimeCommandResult DeleteAudioFile(ushort id, AudioFileType fileType);

    /// <summary>
    /// 根据AudioFileType获取语音文件
    /// </summary>
    /// <param name="fileType">支持api扫描语音和呼吸训练语音</param>
    /// <param name="audioFileIDs"></param>
    /// <returns></returns>
    RealtimeCommandResult GetAudioFiles(AudioFileType fileType, out ushort[]? audioFileIDs);

    /// <summary>
    /// 设置呼吸训练语音音量
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    RealtimeCommandResult SetVolume(uint volume);

    /// <summary>
    /// 设置呼吸训练语音列表
    /// </summary>
    /// <param name="audioFileIDs"></param>
    /// <returns></returns>
    RealtimeCommandResult SetBreathTraningPlayList(IEnumerable<ushort> audioFileIDs);

    /// <summary>
    /// 清除呼吸训练语音列表
    /// </summary>
    /// <param name="audioFileIDs"></param>
    /// <returns></returns>
    RealtimeCommandResult ClearBreathTraningPlayList();

    /// <summary>
    /// 获取呼吸训练语音列表
    /// </summary>
    /// <param name="audioFileIDs"></param>
    /// <returns></returns>
    RealtimeCommandResult GetBreathTrainingPlayList(out ushort[]? audioFileIDs);
}
