//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
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

namespace NV.CT.PatientManagement.Models
{
    public class RawDataModel: BaseViewModel
    {
        public event EventHandler<bool>? SelectionChanged;

        private string _id = string.Empty;
        public string Id
        {
            get => _id;
            set
            {
                SetProperty(ref _id, value);
            }
        }

        private string _studyId = string.Empty;
        public string StudyId
        {
            get => _studyId;
            set
            {
                SetProperty(ref _studyId, value);
            }
        }

        private string _scanRange = string.Empty;
        public string ScanRange
        {
            get => _scanRange;
            set
            {
                SetProperty(ref _scanRange, value);
            }
        }

        private DateTime _createdTime;
        public DateTime CreatedTime
        {
            get => _createdTime;
            set
            {
                _createdTime = value;
                SetProperty(ref _createdTime, value);
            }
        }

        private DateTime _scanEndtime;
        public DateTime ScanEndtime
        {
            get => _scanEndtime;
            set
            {
                SetProperty(ref _scanEndtime, value);
            }
        }

        private string _path = string.Empty;
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                SetProperty(ref _path, value);
            }
        }

        private bool _isExoprted = false;
        public bool IsExoprted
        {
            get => _isExoprted;
            set
            {
                SetProperty(ref _isExoprted, value);
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    this.SelectionChanged?.Invoke(this, value);
                }
            }
        }

    }
}
