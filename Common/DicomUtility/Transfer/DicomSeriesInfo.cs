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
    public class DicomSeriesInfo
    {
        /// <summary>
        /// 序列号
        /// </summary>
        public int? SeriesNumber { get; internal set; }
        /// <summary>
        /// SeriesInstanceUid
        /// </summary>
        public string SeriesInstanceUID { get; internal set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public string Modality { get; internal set; }
        /// <summary>
        /// 序列开始日期(字符串，不带时分秒，YYYYMMDD)
        /// </summary>
        public string SeriesDate { get; internal set; }
        /// <summary>
        /// 序列开始时间(字符串，不带时分秒，HHMMSS)
        /// </summary>
        public string SeriesTime { get; internal set; }
        /// <summary>
        /// 序列开始日期(DateTime，带时分秒)
        /// </summary>
        public DateTime? SeriesDateTime { get; internal set; }
        public string PatientPosition { get; set; }
        public string BodyPartExamined { get; set; }
        public string ImageType { get; set; }
        /// <summary>
        /// 序列描述
        /// </summary>
        public string SeriesDescription { get; internal set; }

        /// <summary>
        /// 窗宽
        /// </summary>
        public string WindowWidth { get; internal set; }

        /// <summary>
        /// 窗位
        /// </summary>
        public string WindowLevel { get; internal set; }

        /// <summary>
        /// 图像列表
        /// </summary>
        public List<DicomImageInfo> ImageList { get; internal set; }
    }
}
