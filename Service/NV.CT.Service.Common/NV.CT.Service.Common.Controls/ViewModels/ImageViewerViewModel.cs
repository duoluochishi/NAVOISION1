using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NV.CT.Service.Common.Controls.Attachments.Helpers;
using NV.CT.Service.Common.Controls.Attachments.Managers;
using NV.CT.Service.Common.Controls.Attachments.Messages;
using NV.CT.Service.Common.Controls.Integrations;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Common.Utils;
using NV.CT.Service.Enums;
using NV.CT.Service.Helpers;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NV.CT.Service.Common.Controls.ViewModels
{
    public partial class ImageViewerViewModel : ObservableObject, IMessengerToken
    {
        private readonly ILogService logService;
        private IMessagePrintService messagePrintService;
        public string MessengerToken { get; set; }

        public ImageViewerViewModel(ILogService logService, IMessagePrintService messagePrintService)
        {
            //Get from DI
            this.logService = logService;
            this.messagePrintService = messagePrintService;

            MessengerToken = $"{DateTime.Now.ToString("yyyyMMdd.HHmmss.fff")}.{this.GetHashCode()}";
            //Initialize
            InitializeProperties();

            InitializeEvents();
        }

        #region Initialize

        private void InitializeProperties()
        {
            //图像控件Manager
            ImageViewerManager = new(MessengerToken);
        }

        #endregion

        #region Properties

        #region Image Viewer

        public ImageViewerManager ImageViewerManager { get; set; } = null!;

        #endregion

        #endregion

        #region Events

        private void InitializeEvents()
        {
            WeakReferenceMessenger.Default.Unregister<LoadRawDataDirectoryMessage, string>(this, MessengerToken);
            WeakReferenceMessenger.Default.Register<LoadRawDataDirectoryMessage, string>(this, MessengerToken, OnLoadRawDataDirectory);

            WeakReferenceMessenger.Default.Unregister<LoadRawDataSeriesMessage, string>(this, MessengerToken);
            WeakReferenceMessenger.Default.Register<LoadRawDataSeriesMessage, string>(this, MessengerToken, OnLoadRawDataSeries);
        }

        #region Events Registration

        #endregion

        /// <summary>
        /// 图像加载进度回调
        /// </summary>
        /// <param name="count"></param>
        /// <param name="total"></param>
        private void RawDataReadProgressChanging(int count, int total)
        {

        }

        #region Data Loader

        [RelayCommand]
        private void LoadRawDataSeriesFolder()
        {
            //打开文件夹对话框
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            //显示
            var dialogResult = folderBrowserDialog.ShowDialog();
            //选择后加载数据
            if (dialogResult == DialogResult.OK)
            {
                LoadRawDataCommon(folderBrowserDialog.SelectedPath);
            }
        }

        #endregion

        #region Image Actions

        [RelayCommand]
        private void ExecuteImageCut()
        {
            //执行ImageCut
            var response = ImageViewerManager.CutImage();
            //显示反馈
            if (response.status)
            {
                messagePrintService.PrintLoggerInfo(response.message);
            }
            else
            {
                messagePrintService.PrintLoggerError(response.message);
            }
        }

        [RelayCommand]
        private void ExecuteImageSort(object dataContext)
        {
            WindowHelper.Show<ImageSortView>(1035, 750, dataContext);
        }

        [RelayCommand]
        private void ExecuteAnalysisTool(object dataContext)
        {
            //string rawFolder = @"F:\AppData\DataMRS\ServiceData\2024_0823_105001_AirBowtie\105001_Bowtie_00";
            string rawFolder = RawDataDirectory;
            DataAnalysisToolUtil.OpenFolder(rawFolder);
        }

        #endregion

        #region Scan Control

        /// <summary>
        /// 生数据目录
        /// </summary>
        public string RawDataDirectory { get; set; }

        /// <summary>
        /// 载入生数据Command
        /// </summary>
        [RelayCommand]
        private void LoadRawData()
        {
            LoadRawDataCommon(RawDataDirectory);
        }

        /// <summary>
        /// 载入生数据 Common
        /// </summary>
        private void LoadRawDataCommon(string directory)
        {
            ReleaseRawData();

            //读取生数据
            var response = RawDataReadWriteHelper.Instance.Read(directory, RawDataReadProgressChanging);
            //校验
            if (!response.status)
            {
                MessagePrinterHelper.MessageWrapper($"Failed to read raw data, error code: {response.message}, directory: {directory}", PrintLevel.Error);

                return;
            }

            RawDataDirectory = directory;
            //显示数据
            ImageViewerManager.LoadRawDataList(response.data);
            //打印消息
            MessagePrinterHelper.MessageWrapper("Raw data has been loaded.", PrintLevel.Info);
        }

        /// <summary>
        /// 释放RawData（由Native库读取的），避免内存泄露，7-8次后内存被爆掉
        /// </summary>
        private void ReleaseRawData()
        {
            //数据有效性校验
            if (ImageViewerManager.RawDataPool is not null)
            {
                //释放
                RawDataReadWriteHelper.Instance.Release();
            }
        }

        private void OnLoadRawDataDirectory(object sender, LoadRawDataDirectoryMessage message)
        {
            if (message == null || !Directory.Exists(message.Directory))
            {
                //先清空已有数据
                ReleaseRawData();
                //清空图像控件状态
                ImageViewerManager.ClearImageViewer();

                return;
            }

            LoadRawDataCommon(message.Directory);
        }

        private void OnLoadRawDataSeries(object sender, LoadRawDataSeriesMessage message)
        {
            var dataSeries = message?.ImageRawDataSeries;
            int? count = dataSeries?.Count;
            if (count < 1)
            {
                //先清空已有数据
                ReleaseRawData();
                //清空图像控件状态
                ImageViewerManager.ClearImageViewer();

                return;
            }

            ReleaseRawData();

            //显示数据
            ImageViewerManager.LoadImageRawDataSeries(dataSeries);
            //打印消息
            MessagePrinterHelper.MessageWrapper($"Raw data series({count}) has been loaded.", PrintLevel.Info);
        }

        #endregion

        #endregion
    }
}