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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NV.CT.Print.Models
{
    public class PrintProtocolModel : BaseViewModel
    {
        private string _name = string.Empty;
        public string Name 
        {
            get
            {
                return _name;
            }
            set
            {
                SetProperty(ref _name, value);
            }
        }

        private string _bodyPart = string.Empty;
        public string BodyPart
        {
            get
            {
                return _bodyPart;
            }
            set
            {
                SetProperty(ref _bodyPart, value);
            }
        }

        private bool _isDefault = false;
        public bool IsDefault
        {
            get
            {
                return _isDefault;
            }
            set
            {
                SetProperty(ref _isDefault, value);
            }
        }

        private bool _isSystem = false;
        public bool IsSystem
        {
            get => _isSystem;
            set
            {
                SetProperty(ref _isSystem, value);
            }
        }

        private int _row = 1;
        public int Row
        {
            get
            {
                return _row;
            }
            set
            {
                SetProperty(ref _row, value);
            }
        }

        private int _column = 1;
        public int Column
        {
            get
            {
                return _column;
            }
            set
            {
                SetProperty(ref _column, value);
            }
        }
    }
}
