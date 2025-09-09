using CommunityToolkit.Mvvm.Messaging;
using NV.CT.FacadeProxy.Common.Models.Generic;
using NV.CT.Service.Common;
using NV.CT.Service.HardwareTest.Attachments.Messages;
using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition.Abstractions;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.Share.Utils;
using NV.MPS.Native.AcqImage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NV.CT.Service.HardwareTest.Attachments.Helpers
{
    public static class LocalRawDataIOHelper
    {
        //图像宽
        private readonly static int ImageSizeX = 10240;
        //图像高
        private readonly static int ImageSizeY = 288;
        //图像头文件读取偏移
        private readonly static int ImageHeadReadReadOffset = Marshal.SizeOf(typeof(CommonHeader));
        //图像大小
        private readonly static int ImageSize = ImageSizeX * ImageSizeY * sizeof(ushort);
        //图像头大小
        private readonly static int ImageHeadSize = Marshal.SizeOf(typeof(RawDataHeader));

        /// <summary>
        /// 读取实时采集数据（.head & .raw）
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="interval"></param>
        /// <param name="finishCount"></param>
        /// <param name="framesPerCycle"></param>
        /// <returns></returns>
        public static async Task<GenericResponse<bool, IEnumerable<AbstractRawDataInfo>?>> ReadAcquringDataDirectoryAsync(
            string directory, int interval, int finishCount, int framesPerCycle = 1080)
        {
            //路径校验 
            if (!Directory.Exists(directory))
            {
                LogService.Instance.Error(ServiceCategory.HardwareTest,
                    $"[{ComponentDefaults.DataAcquisition}] The acquistion raw data directory path does not exist, path: [{directory}].");

                return new (false, default);
            }
            //获取路径下的RawData文件(.raw和.head) 
            string[] acquiringDataFileNames = Directory.GetFiles(directory, "*.raw");
            string[] acquiringDataHeadFileNames = Directory.GetFiles(directory, "*.head");
            //根据帧计算对应的数据offset和文件Index 
            int fileIndex = (finishCount - 1) / framesPerCycle;
            long dataOffset = (finishCount - interval) - (fileIndex * framesPerCycle);
            //显示记录 
            LogService.Instance.Info(ServiceCategory.HardwareTest,
                $"[{ComponentDefaults.DataAcquisition}] Acquiring raw data head & data, file index: {fileIndex}, data offset: {dataOffset}.");
            //有效数据文件校验 
            if (acquiringDataFileNames.Length == 0 || acquiringDataFileNames.Length <= fileIndex)
            {
                LogService.Instance.Error(ServiceCategory.HardwareTest,
                    $"[{ComponentDefaults.DataAcquisition}] The acquistion raw data folder path does not have valid raw data, " +
                    $"current raw data file index: {fileIndex}, current raw data offset: {dataOffset}.");

                return new(false, default);
            }
            //有效数据头文件校验 
            if (acquiringDataHeadFileNames.Length == 0 || acquiringDataHeadFileNames.Length <= fileIndex)
            {
                LogService.Instance.Error(ServiceCategory.HardwareTest,
                    $"[{ComponentDefaults.DataAcquisition}] The acquistion raw data folder path does not have valid head file, " +
                    $"current raw data head file index: {fileIndex}, current raw data head offset: {dataOffset}.");

                return new(false, default);
            }
            //获取对应的文件路径 
            string dataFilePath = Path.Combine(directory, acquiringDataFileNames[fileIndex]);
            string dataHeadFilePath = Path.Combine(directory, acquiringDataHeadFileNames[fileIndex]);
            //初始化多帧数据buffer 
            byte[] bufferForDataFrames = new byte[ImageSize * interval];
            byte[] bufferForHeadFrames = new byte[ImageHeadSize * interval];
            //初始化RawDataInfo集合 
            var rawDataSet = new List<AbstractRawDataInfo>();
            //初始化单帧数据tempData和单帧数据头tempHead
            byte[] tempHead = new byte[ImageHeadSize];
            //读取 
            try
            {
                //FileStream(开启FileShare)
                using FileStream dataFileStream = new FileStream(dataFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                using FileStream headFileStream = new FileStream(dataHeadFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                //移动读位置 
                dataFileStream.Seek(dataOffset * ImageSize, SeekOrigin.Begin);
                headFileStream.Seek(dataOffset * ImageHeadSize + ImageHeadReadReadOffset, SeekOrigin.Begin);
                //显示记录 
                LogService.Instance.Info(ServiceCategory.HardwareTest,
                    $"[{ComponentDefaults.DataAcquisition}] " +
                    $"Acquring data offset: {dataOffset * ImageSize}, fileStreamLength: {dataFileStream.Length}," +
                    $"Acquring head offset: {dataOffset * ImageHeadSize + ImageHeadReadReadOffset}, headStreamLength: {headFileStream.Length}");
                //读取imageSize * interval长度的数据 
                int readDataLength = await dataFileStream.ReadAsync(bufferForDataFrames, 0, bufferForDataFrames.Length);
                int readHeadLength = await headFileStream.ReadAsync(bufferForHeadFrames, 0, bufferForHeadFrames.Length);
                //遍历写入 
                for (int i = 0; i < interval; i++)
                {
                    //生成 UshortRawDataInfo
                    UshortRawDataInfo ushortRawDataInfo = new UshortRawDataInfo(ImageSizeX, ImageSizeY);
                    //获取单帧头数据 
                    Buffer.BlockCopy(bufferForHeadFrames, i * ImageHeadSize, tempHead, 0, ImageHeadSize);
                    RawDataHeader imageHead = ByteUtils.BytesToStruct<RawDataHeader>(tempHead)!.Value;
                    //获取单帧数据 
                    Buffer.BlockCopy(bufferForDataFrames, i * ImageSize, ushortRawDataInfo.Data, 0, ImageSize);
                    //更新 UshortRawDataInfo
                    ushortRawDataInfo.Width = imageHead.ImageSizeX;
                    ushortRawDataInfo.Height = imageHead.ImageSizeY;
                    ushortRawDataInfo.SupportInfo.SourceID = imageHead.SourceId;
                    ushortRawDataInfo.SupportInfo.FrameSeriesNumber = imageHead.FrameNoInSeries;
                    ushortRawDataInfo.SupportInfo.GantryRotateAngle = imageHead.GantryAngle;
                    ushortRawDataInfo.SupportInfo.TablePosition = imageHead.TablePosition; 
                    ushortRawDataInfo.SupportInfo.Slope0 = imageHead.Slope0;
                    ushortRawDataInfo.SupportInfo.Slope1 = imageHead.Slope1;
                    //添加 
                    rawDataSet.Add(ushortRawDataInfo);
                }
                LogService.Instance.Info(ServiceCategory.HardwareTest,
                    $"[{ComponentDefaults.DataAcquisition}] From No.{finishCount - interval + 1} to No.{finishCount} raw data been loaded.");

                return new(true, rawDataSet);
            }
            catch (Exception ex)
            {
                LogService.Instance.Error(ServiceCategory.HardwareTest, 
                    $"[{ComponentDefaults.DataAcquisition}] Something wrong when extracting raw data, [Stack]: {ex}");

                return new(false, default);
            }
        }

        /// <summary>
        /// 读取序列文件
        /// </summary>
        /// <param name="headPath"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<GenericResponse<bool,IEnumerable<AbstractRawDataInfo>?>> ReadImageSeriesFileAsync(
            string headPath, string filePath, bool withHeadFile = false)
        {
            /** 初始化RawDataInfo集合 **/
            List<AbstractRawDataInfo> rawDataSet = new List<AbstractRawDataInfo>();
            /** Reader **/
            using FileStream dataFileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            /** 数据张数 **/
            long frameCount = dataFileStream.Length / ImageSize;
            /** bytes **/
            byte[] tempData = new byte[ImageSize];
            /** 循环读 **/
            for (long i = 0; i < frameCount; i++)
            {
                /** 移动头位置 **/
                dataFileStream.Seek(i * ImageSize, SeekOrigin.Begin);
                /** 读取 **/
                int readDataLength = await dataFileStream.ReadAsync(tempData, 0, tempData.Length);
                /** 实例 **/
                UshortRawDataInfo ushortRawDataInfo = new UshortRawDataInfo(ImageSizeX, ImageSizeY);
                /** 更新单帧数据 **/
                Buffer.BlockCopy(tempData, 0, ushortRawDataInfo.Data, 0, ImageSize);
                /** 添加 **/
                rawDataSet.Add(ushortRawDataInfo);
            }
            /** 若需要解析头文件 **/
            if (withHeadFile)
            {
                /** Reader **/
                using FileStream headFileStream = new FileStream(headPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                /** 数据头张数 **/
                long headCount = (headFileStream.Length - ImageHeadReadReadOffset) / ImageHeadSize;
                /** 校验 **/
                if (headCount != frameCount)
                {
                    string message = $"[{ComponentDefaults.DataAcquisition}] " +
                        $"Unmatched head count - {headCount} and data - {frameCount} count, headPath: {headPath}, filePath: {filePath}.";
                    /** 记录 **/
                    LogService.Instance.Error(ServiceCategory.HardwareTest, message);
                    /** 发送消息 **/
                    WeakReferenceMessenger.Default.Send(new RawDataSetLoadedLoggerMessage(message));

                    return new(false, default);
                }
                /** 初始化单帧数据tempData和单帧数据头tempHead * */
                byte[] tempHead = new byte[ImageHeadSize];
                /** 循环匹配 **/
                for (long i = 0; i < headCount; i++)
                {
                    /** 移动头位置 **/
                    headFileStream.Seek(i * ImageHeadSize + ImageHeadReadReadOffset, SeekOrigin.Begin);
                    /** 读取 **/
                    int readHeadLength = await headFileStream.ReadAsync(tempHead, 0, tempHead.Length);
                    /** 更新信息 **/
                    UshortRawDataInfo ushortRawDataInfo = (UshortRawDataInfo)rawDataSet[(int)i];
                    /** 获取单帧头数据 **/
                    Buffer.BlockCopy(tempHead, 0, tempHead, 0, ImageHeadSize);
                    /** 转换 **/
                    RawDataHeader imageHead = ByteUtils.BytesToStruct<RawDataHeader>(tempHead)!.Value;
                    /** 更新ushortRawDataInfo**/
                    ushortRawDataInfo.Width = imageHead.ImageSizeX;
                    ushortRawDataInfo.Height = imageHead.ImageSizeY;
                    ushortRawDataInfo.SupportInfo.SourceID = imageHead.SourceId;
                    ushortRawDataInfo.SupportInfo.FrameSeriesNumber = imageHead.FrameNoInSeries;
                    ushortRawDataInfo.SupportInfo.GantryRotateAngle = imageHead.GantryAngle;
                    ushortRawDataInfo.SupportInfo.TablePosition = imageHead.TablePosition;
                    ushortRawDataInfo.SupportInfo.Slope0 = imageHead.Slope0;
                    ushortRawDataInfo.SupportInfo.Slope1 = imageHead.Slope1;        
                }
            }

            return new(true, rawDataSet);
        }

    }
}
