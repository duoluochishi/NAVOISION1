using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.Common.Framework
{
    public class EditableObject : ViewModelBase, IEditableObject
    {
        //DataGrid默认调用两侧下列函数，所以需要使用_isEditable过滤
        private bool _isEditable = false;
        public void BeginEdit()
        {
            if (!_isEditable)
            {
                _isEditable = true;
                BeginEditCore();
            }
        }

        public void CancelEdit()
        {
            if (_isEditable)
            {
                _isEditable = false;
            }
        }

        public void EndEdit()
        {
            if (_isEditable)
            {
                _isEditable = false;
                EndEditCore();
            }
        }
        protected virtual void BeginEditCore() { }
        protected virtual void EndEditCore() { }
    }
}
