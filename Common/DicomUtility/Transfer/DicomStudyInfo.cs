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
    public class DicomStudyInfo
    {
        /// <summary>
        /// 访问号
        /// </summary>
        public string AccessionNumber { get; internal set; }
        /// <summary>
        /// StudyInstanceUid
        /// </summary>
        public string StudyInstanceUID { get; internal set; }
        /// <summary>
        /// 检查id
        /// </summary>
        public string StudyID { get; internal set; }
        /// <summary>
        /// 检查日期(字符串，不带时分秒，YYYYMMDD)
        /// </summary>
        public string StudyDate { get; internal set; }
        /// <summary>
        /// 检查时间(字符串，不带时分秒，HHMMSS)
        /// </summary>
        public string StudyTime { get; internal set; }
        /// <summary>
        /// 检查日期(DateTime，带时分秒)
        /// </summary>
        public DateTime? StudyDateTime { get; internal set; }
        /// <summary>
        /// 病人年龄
        /// </summary>
        public string PatientAge { get; internal set; }
        /// <summary>
        /// 医疗机构名称
        /// </summary>
        public string InstitutionName { get; internal set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public string Modalities { get; internal set; }
        /// <summary>
        /// 制造商
        /// </summary>
        public string Manufacturer { get; internal set; }
        /// <summary>
        /// 型号
        /// </summary>
        public string ManufacturerModelName { get; internal set; }
        /// <summary>
        /// 协议名称
        /// </summary>
        public string ProtocolName { get; internal set; }
        /// <summary>
        /// 检查描述
        /// </summary>
        public string StudyDescription { get; internal set; }
        /// <summary>
        /// 检查部位
        /// </summary>
        public string BodyPartExamined { get; internal set; }
        /// <summary>
        /// 医疗警报
        /// </summary>
        public string MedicalAlerts { get; internal set; }
        /// <summary>
        /// 执行医生
        /// </summary>
        public string PerformingPhysicianName { get; internal set; }
        /// <summary>
        /// 转诊医生
        /// </summary>
        public string ReferringPhysicianName { get; internal set; }
        /// <summary>
        /// 入院诊断
        /// </summary>
        public string AdmittingDiagnosesDescription { get; internal set; }
        /// <summary>
        /// 病房
        /// </summary>
        public string CurrentPatientLocation { get; internal set; }
        /// <summary>
        /// 病人地址
        /// </summary>
        public string PatientAddress { get; internal set; }
        /// <summary>
        /// 病人体重
        /// </summary>
        public string PatientWeight { get; internal set; }
        /// <summary>
        /// 病人身高
        /// </summary>
        public string PatientSize { get; internal set; }
        /// <summary>
        /// 序列列表
        /// </summary>
        public List<DicomSeriesInfo> SeriesList { get; internal set; }
    }
}
