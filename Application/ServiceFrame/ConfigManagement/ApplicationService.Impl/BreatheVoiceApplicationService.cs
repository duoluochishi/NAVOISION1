using Microsoft.Extensions.Logging;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums.AudioFileEnums;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.ConfigManagement.ApplicationService.Impl
{
    public class BreatheVoiceApplicationService : IBreatheVoiceApplicationService
    {
        private ILogger<BreatheVoiceApplicationService> _logger;
        private IRealtimeVoiceService _realtimeVoiceService;

        public event EventHandler<EventArgs<(OperationType operation, BreatheVoiceGroup breatheVoiceGroup)>>? BreatheVoiceGroupChanged;
        public event EventHandler<EventArgs<(OperationType operation, int groupId, BreatheVoiceInfo breatheVoice)>>? BreatheVoiceChanged;
        public event EventHandler<EventArgs<BreatheVoiceInfo>>? AddEditResultChanged;

        public BreatheVoiceApplicationService(ILogger<BreatheVoiceApplicationService> logger, IRealtimeVoiceService realtimeVoiceService)
        { 
            _logger = logger;
            _realtimeVoiceService = realtimeVoiceService;
        }

        /// <summary>
        /// 配合BreatheVoiceGroupChanged，在添加或修改分组时，设置分组信息
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="breatheVoiceGroup"></param>
        public void SetBreatheVoiceGroup(OperationType operation, BreatheVoiceGroup breatheVoiceGroup)
        {
            BreatheVoiceGroupChanged?.Invoke(this, new EventArgs<(OperationType operation, BreatheVoiceGroup breatheVoiceGroup)>((operation, breatheVoiceGroup)));
        }

        /// <summary>
        /// 配合BreatheVoiceChanged，在添加或修改语音，设置语音信息
        /// </summary>
        /// <param name="operation">Add or Edit</param>
        /// <param name="groupId">当前Group id</param>
        /// <param name="breatheVoice">当前要操作的语音信息对象</param>
        public void SetBreatheVoice(OperationType operation, int groupId, BreatheVoiceInfo breatheVoice)
        {
            BreatheVoiceChanged?.Invoke(this, new EventArgs<(OperationType operation, int groupId, BreatheVoiceInfo breatheVoice)>((operation, groupId, breatheVoice)));
        }

        /// <summary>
        /// 配合AddEditResultChanged，用来在添加语音结束后传递Add、Edit breathevoice的结果
        /// </summary>
        /// <param name="breatheVoice">不为空更新，空不做处理</param>
        public void SetAddEditResult(BreatheVoiceInfo breatheVoice)
        {
            AddEditResultChanged?.Invoke(this, new EventArgs<BreatheVoiceInfo>(breatheVoice));
        }

        public List<BreatheVoiceGroup> GetAll()
        {
            return UserConfig.BreatheVoiceConfig.BreatheVoiceGroups;
        }

        public BreatheVoiceGroup? GetBreatheVoiceGroupById(int id)
        {
            return UserConfig.BreatheVoiceConfig.BreatheVoiceGroups.FirstOrDefault(x => x.Id == id);
        }

        public int GetMaxItemCount()
        {
            return UserConfig.BreatheVoiceConfig.MaxGroups;
        }

        public int GetLatestGroupId()
        {
            var group = UserConfig.BreatheVoiceConfig.BreatheVoiceGroups.MaxBy(x => x.Id);
            if (group != null)
                return group.Id;
            else
                return 0;
        }

        public bool AddBreatheVoiceGroup(BreatheVoiceGroup voiceGroup)
        {
            if (!UserConfig.BreatheVoiceConfig.BreatheVoiceGroups.Any(v => v.Id == voiceGroup.Id))
            {
                UserConfig.BreatheVoiceConfig.BreatheVoiceGroups.Add(voiceGroup);
                return UserConfig.SaveBreatheVoices();
            }
            return false;
        }

        /// <summary>
        /// 删除BreatheVoiceGroup，以及该组下的所有音频文件
        /// </summary>
        /// <param name="voiceGroupId"></param>
        /// <returns></returns>
        public bool DeleteBreatheVoiceGroup(int voiceGroupId)
        {
            var voiceGroup = UserConfig.BreatheVoiceConfig.BreatheVoiceGroups.FirstOrDefault(v => v.Id == voiceGroupId);

            if (voiceGroup == null)
            {
                _logger.LogError($"DeleteBreatheVoiceGroup -> Cannot find the group: {voiceGroupId}");
                return false;
            }

            RealtimeCommandResult commandResult;
            // Clear the play list if the group is default.
            if (voiceGroup.IsDefault)
            {
                commandResult = _realtimeVoiceService.ClearBreathTraningPlayList();
                {
                    if (commandResult.Status != CommandExecutionStatus.Success)
                    {
                        _logger.LogError($"DeleteBreatheVoiceGroup -> Failed to clear breath training play list: {commandResult.Details}");
                        return false;
                    }
                }
            }

            // Delete voice file from the auxboard
            commandResult = _realtimeVoiceService.DeleteAudioFile((ushort)voiceGroup.Id, AudioFileType.BreathTraining);
            if (commandResult.Status != CommandExecutionStatus.Success)
            {
                _logger.LogError($"DeleteBreatheVoiceGroup -> Failed to DeleteAudioFile from auxboard: {commandResult.Details}");
                return false;
            }

            // Delete voice file from xml and local storage
            if (voiceGroup is not null && voiceGroup.IsFactory == false)
            {
                UserConfig.BreatheVoiceConfig.BreatheVoiceGroups.Remove(voiceGroup);
                bool res = UserConfig.SaveBreatheVoices();
                if(res)
                {
                    foreach (var voice in voiceGroup.BreatheVoices)
                    {
                        DeleteVoiceFile(voice.FilePath);
                    }
                }
                return res;
            }
            return false;
        }

        /// <summary>
        /// 更新BreatheVoiceGroup，以及删除该组下的旧音频文件
        /// </summary>
        /// <param name="voiceGroup"></param>
        /// <returns></returns>
        public bool UpdateBreatheVoiceGroup(BreatheVoiceGroup voiceGroup)
        {
            try
            {
                var groups = UserConfig.BreatheVoiceConfig.BreatheVoiceGroups;
                var index = groups.FindIndex(v => v.Id == voiceGroup.Id);

                if (index == -1) return false;

                // 找出改组下不再使用的voice file,，然后删除
                var oldVoiceFilePaths = new HashSet<string>(groups[index].BreatheVoices.Select(v => v.FilePath));
                var newVoiceFilePaths = new HashSet<string>(voiceGroup.BreatheVoices.Select(v => v.FilePath));
                var diffFilePaths = oldVoiceFilePaths.Except(newVoiceFilePaths);
                foreach (var filePath in diffFilePaths)
                {
                    // 暂时Ignore删除失败的情况下
                    DeleteVoiceFile(filePath);
                }
                // 更新group
                groups[index] = voiceGroup;
                return UserConfig.SaveBreatheVoices();
            }
            catch
            {
                return false;
            }
        }

        public bool SetDefaultVoiceGroup(int id)
        {
            var allGroups = GetAll();
            var current = allGroups.Where(x => x.Id == id).First();
            if (current is null)
            {
                _logger.LogError($"SetDefaultVoiceGroup -> Cannot find the group id:{id}");
                return false;
            }

            // 1, Set play list
            var currentVoiceIds = current.BreatheVoices.Select(x => (ushort)x.Id);
            var commandResult = _realtimeVoiceService.SetBreathTraningPlayList(currentVoiceIds);
            if (commandResult.Status != CommandExecutionStatus.Success)
            {
                _logger.LogError($"SetDefaultVoiceGroup -> SetBreathTraningPlayList failed, group id:{current.Id} group name:{current.Name}");
                return false;
            }

            // 2, Copmpare local voice files
            commandResult = _realtimeVoiceService.GetAudioFiles(AudioFileType.BreathTraining, out var audioFiles);
            if (commandResult.Status != CommandExecutionStatus.Success)
            {
                _logger.LogError($"SetDefaultVoiceGroup -> GetAudioFiles failed, group id:{current.Id} group name:{current.Name}");
                return false;
            }

            if (audioFiles is not null)
            {

                var diffFiles = currentVoiceIds.Except(audioFiles);
                if (diffFiles.Count() != 0)
                {
                    _logger.LogError($"SetDefaultVoiceGroup -> Comparing audio files failed, group id:{current.Id} group name:{current.Name}");
                    return false;
                }
            }
            else
                _logger.LogError($"SetDefaultVoiceGroup -> audioFiles is null");

            // 3, Compare playlist
            commandResult = _realtimeVoiceService.GetBreathTrainingPlayList(out var audioFileIDs);
            if (commandResult.Status != CommandExecutionStatus.Success)
            {
                _logger.LogError($"SetDefaultVoiceGroup -> GetBreathTrainingPlayList failed, group id:{current.Id} group name:{current.Name}");
                return false;
            }
            if (audioFileIDs is not null)
            {

                var diffPlaylist = currentVoiceIds.Except(audioFileIDs);
                if (diffPlaylist.Count() != 0)
                {
                    _logger.LogError($"SetDefaultVoiceGroup -> Comparing playlist failed, group id:{current.Id} group name:{current.Name}");
                    return false;
                }
            }
            else
                _logger.LogError($"SetDefaultVoiceGroup -> audioFileIDs is null");

            // 4, Clear all default groups
            var preGroups = allGroups.Where(x => x.IsDefault == true).ToList();
            foreach (var group in preGroups)
            {
                group.IsDefault = false;
                bool isSuccess = UpdateBreatheVoiceGroup(group);
                if (!isSuccess)
                {
                    _logger.LogError($"SetDefaultVoiceGroup -> Step 2, Clear all defaults, UpdateBreatheVoiceGroup failed, group id:{group.Id} group name:{group.Name}");
                    return false;
                }
            }

            // 5, Set current default
            if (current != null)
            {
                current.IsDefault = true;
                bool isSuccess = UpdateBreatheVoiceGroup(current);
                if (!isSuccess)
                {
                    _logger.LogError($"SetDefaultVoiceGroup -> Step 3, Set current default, UpdateBreatheVoiceGroup failed, group id:{current.Id} group name:{current.Name}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 删除音频文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool DeleteVoiceFile(string filePath)
        {
            string fullPath = Path.Combine(RuntimeConfig.Console.MCSBreatheVoices.Path, filePath);
            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Failed to delete the breathe voice file: {filePath}");
                    return false;
                }
            }
            return true;
        }

        #region Auxboard APIs

        public bool AddOrUpdateAudioFile(IEnumerable<BreatheVoiceInfo> voices)
        {
            foreach (var voice in voices)
            {
                string fullPath = Path.Combine(RuntimeConfig.Console.MCSBreatheVoices.Path, voice.FilePath);
                if (File.Exists(fullPath))
                {
                    var commandResult = _realtimeVoiceService.AddOrUpdateAudioFile((ushort)voice.Id, voice.FilePath, AudioFileType.BreathTraining);
                    if (commandResult.Status != CommandExecutionStatus.Success)
                    {
                        _logger.LogError($"AddOrUpdateAudioFile failed: Id:{voice.Id} filePath:{voice.FilePath} AudioFileType:BreathTraning \r\nerror:{commandResult.Details}");
                        return false;
                    }
                }
                else
                {
                    _logger.LogError($"AddOrUpdateAudioFile failed: Id:{voice.Id} filePath:{voice.FilePath} does not exist.");
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
