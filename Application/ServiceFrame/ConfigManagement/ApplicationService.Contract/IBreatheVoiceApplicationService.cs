using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.MPS.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.ConfigManagement.ApplicationService.Contract
{
    public interface IBreatheVoiceApplicationService
    {
        public event EventHandler<EventArgs<(OperationType operation, BreatheVoiceGroup breatheVoiceGroup)>>? BreatheVoiceGroupChanged;
        public event EventHandler<EventArgs<(OperationType operation, int groupId, BreatheVoiceInfo breatheVoice)>>? BreatheVoiceChanged;
        public event EventHandler<EventArgs<BreatheVoiceInfo>>? AddEditResultChanged;

        /// <summary>
        /// 配合BreatheVoiceGroupChanged，在添加或修改分组时，设置分组信息
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="breatheVoiceGroup"></param>
        void SetBreatheVoiceGroup(OperationType operation, BreatheVoiceGroup breatheVoiceGroup);

        /// <summary>
        /// 配合BreatheVoiceChanged，在添加或修改语音，设置语音信息
        /// </summary>
        /// <param name="operation">Add or Edit</param>
        /// <param name="groupId">当前Group id</param>
        /// <param name="breatheVoice">当前要操作的语音信息对象</param>
        void SetBreatheVoice(OperationType operation, int groupId, BreatheVoiceInfo breatheVoice);

        /// <summary>
        /// 配合AddEditResultChanged，用来在添加语音结束后传递Add、Edit breathevoice的结果
        /// </summary>
        /// <param name="breatheVoice">不为空更新，空不做处理</param>
        void SetAddEditResult(BreatheVoiceInfo breatheVoice);

        List<BreatheVoiceGroup> GetAll();

        BreatheVoiceGroup? GetBreatheVoiceGroupById(int id);

        int GetMaxItemCount();

        int GetLatestGroupId();

        bool AddBreatheVoiceGroup(BreatheVoiceGroup voiceGroup);
        
        /// <summary>
        /// 删除BreatheVoiceGroup，以及该组下的所有的auxboard和本地音频文件
        /// </summary>
        /// <param name="voiceGroupId"></param>
        /// <returns></returns>
        bool DeleteBreatheVoiceGroup(int voiceGroupId);

        /// <summary>
        /// 更新BreatheVoiceGroup，以及删除该组下的旧音频文件
        /// </summary>
        /// <param name="voiceGroup"></param>
        /// <returns></returns>
        bool UpdateBreatheVoiceGroup(BreatheVoiceGroup voiceGroup);

        /// <summary>
        /// 设置默认的BreatheVoiceGroup,同时设置呼吸语音列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool SetDefaultVoiceGroup(int id);

        /// <summary>
        /// 删除本地音频文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool DeleteVoiceFile(string filePath);

        /// <summary>
        /// 更新auxboard中的语音语音文件
        /// </summary>
        /// <param name="voices"></param>
        /// <returns></returns>
        bool AddOrUpdateAudioFile(IEnumerable<BreatheVoiceInfo> voices);
    }
}
