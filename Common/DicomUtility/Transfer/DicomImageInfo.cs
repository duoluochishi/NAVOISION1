//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.DicomUtility.Transfer
{
    public class DicomImageInfo
    {
        /// <summary>
        /// 实例号
        /// </summary>
        public string InstanceNumber { get; internal set; }
        /// <summary>
        /// SOPInstanceUid
        /// </summary>
        public string SOPInstanceUID { get; internal set; }
        /// <summary>
        /// 图片类型
        /// </summary>
        public string ImageType { get; internal set; }
        /// <summary>
        /// 图片曝光日期(字符串，不带时分秒，YYYYMMDD)
        /// </summary>
        public string ContentDate { get; internal set; }
        /// <summary>
        /// 图片曝光时间(字符串，不带时分秒，HHMMSS)
        /// </summary>
        public string ContentTime { get; internal set; }
        /// <summary>
        /// 图片曝光日期(DateTime，带时分秒)
        /// </summary>
        public DateTime? ContentDateTime { get; internal set; }
        /// <summary>
        /// 图像路径（不含根文件夹路径）
        /// </summary>
        public string Path { get; internal set; }

    }
}
