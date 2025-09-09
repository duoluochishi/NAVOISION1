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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Print.Models
{
    public class OptionsBindableModel : BaseViewModel
    {
        private string _displayText;
        public string DisplayText
        {
            get => _displayText ?? string.Empty;
            set => SetProperty(ref _displayText, value);
        }

        private string _valueText;
        public string ValueText
        {
            get => _valueText ?? string.Empty;
            set => SetProperty(ref _valueText, value);
        }

    }


}
