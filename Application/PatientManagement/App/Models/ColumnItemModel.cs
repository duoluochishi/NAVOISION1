//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/8/2 11:01:27    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using Prism.Mvvm;

namespace NV.CT.PatientManagement.Models
{
    public class ColumnItemModel : BindableBase
    {
        private StudyListColumn _itemName;
        public StudyListColumn ItemName 
        { 
            get => _itemName;
            set => this.SetProperty(ref _itemName, value);
        }

        private bool _isFixed = false;
        public bool IsFixed 
        { 
            get => _isFixed; 
            set => this.SetProperty(ref _isFixed, value); 
        
        }

        private bool _isChecked = false;
        public bool IsChecked 
        { 
            get => _isChecked; 
            set => this.SetProperty(ref _isChecked, value); 
        }

        private int _sortNumber = 0;
        public int SortNumber 
        { 
            get => _sortNumber; 
            set => this.SetProperty(ref _sortNumber, value); 
        }

    }

}
