using NV.CT.FacadeProxy.Common.Models;
using System;

namespace NV.CT.Service.AutoCali.UI.ViewModel
{
    /// <summary>
    /// 通知图片控件更新的事件参数
    /// </summary>
    internal class RawImageChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 通知图像控件如何更新:新增图像 / 清空
        /// </summary>
        public ImageChangedType ImageChangedType { get; set; }

        /// <summary>
        /// 文件路径（支持多个）
        /// </summary>
        public string[] FilePaths { get; set; }

        /// <summary>
        /// 使用的扫描重建参数
        /// </summary>
        public ScanReconParam ScanReconParam { get; set; }
    }

    /// <summary>
    /// 通知图像控件如何更新:新增图像 / 清空
    /// </summary>
    internal enum ImageChangedType
    {
        /// <summary>
        /// 新增图像
        /// </summary>
        Add = 0,

        /// <summary>
        /// 清空
        /// </summary>
        Clear = 1
    }

}
