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
    public class DicomPatientInfo
    {
        /// <summary>
        /// 病人id
        /// </summary>
        public string PatientId { get; internal set; }
        /// <summary>
        /// 病人姓名
        /// </summary>
        public string PatientName { get; internal set; }
        /// <summary>
        /// 病人名
        /// </summary>
        public string PatientFirstName { get; internal set; }
        /// <summary>
        /// 病人姓
        /// </summary>
        public string PatientLastName { get; internal set; }
        /// <summary>
        /// 病人性别
        /// </summary>
        public string PatientSex { get; internal set; }
        /// <summary>
        /// 病人体重
        /// </summary>
        public string PatientWeight { get; internal set; }
        /// <summary>
        /// 病人身高
        /// </summary>
        public string PatientSize { get; internal set; }
        /// <summary>
        /// 病房
        /// </summary>
        public string CurrentPatientLocation { get; internal set; }
        /// <summary>
        /// 病人地址
        /// </summary>
        public string PatientAddress { get; internal set; }
        /// <summary>
        /// 医疗警报
        /// </summary>
        public string MedicalAlerts { get; internal set; }
        /// <summary>
        /// 病人生日(字符串，不带时分秒，YYYYMMDD)
        /// </summary>
        public string PatientBirthDate { get; internal set; }
        /// <summary>
        /// 病人生日(DateTime，带时分秒)
        /// </summary>
        public DateTime? PatientBirthDateTime { get; internal set; }
        /// <summary>
        /// 登记日期(字符串，不带时分秒，YYYYMMDD)
        /// </summary>
        public string AdmittingDate { get; internal set; }
        /// <summary>
        /// 登记时间(字符串，时分秒，HHMMSS)
        /// </summary>
        public string AdmittingTime { get; internal set; }
        /// <summary>
        /// 登记时间(DateTime，带时分秒)
        /// </summary>
        public DateTime? AdmittingDateTime { get; internal set; }
        /// <summary>
        /// 检查列表
        /// </summary>
        public List<DicomStudyInfo> StudyList { get; internal set; }
    }
}
