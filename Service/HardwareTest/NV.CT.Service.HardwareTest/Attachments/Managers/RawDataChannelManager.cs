using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NV.CT.Service.HardwareTest.Attachments.Managers
{
    public class RawDataChannelManager
    {        
        public static RawDataChannelManager Instance { set; get; } = new RawDataChannelManager();

        private RawDataChannelManager() { }

        #region Properties

        private Channel<IEnumerable<AbstractRawDataInfo>> Channel { get; set; } = null!;

        #endregion

        #region Events

        public event EventHandler<IEnumerable<AbstractRawDataInfo>>? RawDataAcquired;

        #endregion

        /// <summary>
        /// 开始监听
        /// </summary>
        public void StartListen()
        {
            //初始化Channel
            Channel = System.Threading.Channels.Channel.CreateUnbounded<IEnumerable<AbstractRawDataInfo>>();
            //获取Channel读通道
            var channelReader = Channel.Reader;
            //监听读通道 
            Task.Run(async () =>
            {
                //读取 
                while (await channelReader.WaitToReadAsync())
                {
                    if (channelReader.TryRead(out var rawDataSet))
                    {
                        RawDataAcquired?.Invoke(this, rawDataSet);
                    }
                }
            });
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        public async Task WriteToChannelAsync(IEnumerable<AbstractRawDataInfo> rawDataSet) 
        {
            //获取Channel写通道
            var channelWriter = Channel.Writer;
            //写入数据
            await channelWriter.WriteAsync(rawDataSet);
        }

    }
}
