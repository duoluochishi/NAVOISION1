//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:43:11    V1.0.0       胡安
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
using NV.CT.UI.ViewModel;

namespace NV.CT.Print.Models
{
    public class StudyBodyPartModel : BaseViewModel
    {
        private string _studyId = string.Empty;
        public string StudyId
        {
            get => _studyId;
            set => SetProperty(ref _studyId, value);
        }

        private string _bodyPart = string.Empty;
        public string BodyPart
        {
            get => _bodyPart;
            set => SetProperty(ref _bodyPart, value);
        }

        private DateTime _reconEndDate;
        public DateTime ReconEndDate
        {
            get => _reconEndDate;
            set => SetProperty(ref _reconEndDate, value);
        }

    }
}
