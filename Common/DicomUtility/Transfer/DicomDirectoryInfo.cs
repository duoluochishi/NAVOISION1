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
    public class DicomDirectoryInfo
    {

        /// <summary>
        /// 目录路径
        /// </summary>
        public string TargetDirectoryPath { get; internal set; }

        /// <summary>
        /// 病人列表
        /// </summary>
        public List<DicomPatientInfo> PatientList { get; internal set; }
    }
}
