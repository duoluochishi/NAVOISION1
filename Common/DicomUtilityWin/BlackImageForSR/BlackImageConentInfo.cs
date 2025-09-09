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

namespace NV.CT.DicomUtility.BlackImageForSR
{
    public class BlackImageConentInfo
    {
        public string StudyInstanceUID { get; set; } = string.Empty;

        public string StudyID { get; set; } = string.Empty;

        public double TotalDLP { get; set; }


        public DateTime StudyTime { get; set; }

        public List<ACQDoseInfoForBlackImage> AcqDoseInfos { get; } = new List<ACQDoseInfoForBlackImage>();


    }
}
