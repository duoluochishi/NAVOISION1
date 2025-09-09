//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.CTS.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.JobService.Contract.Model
{
    public class UpdateDicomRequest
    {
        public List<string> SeriesFolders { get; set; }

        public string PatientID { get; set; } = string.Empty;

        public string PatientName{ get; set; } = string.Empty;

        public string PatientBirthDate { get; set; } = string.Empty;

        public string PatientBirthTime { get; set; } = string.Empty;

        public string PatientSex { get; set; } = string.Empty;

        public string PatientAge { get; set; } = string.Empty;

        public string PatientSize { get; set; } = string.Empty;

        public string PatientWeight { get; set; } = string.Empty;

        public string AccessionNumber { get; set; } = string.Empty;

        public string ReferringPhysicianName { get; set; } = string.Empty;

        public string StudyDescription { get; set; } = string.Empty;


        public UpdateDicomRequest() 
        {
            SeriesFolders = new List<string>();
        }

    }
}
