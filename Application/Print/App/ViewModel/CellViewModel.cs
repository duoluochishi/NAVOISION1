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
using NV.CT.Print.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Print.ViewModel
{
    public class CellViewModel : BaseViewModel
    {
        public const string COVERED_COLOR = "LightSkyBlue";
        public const string UNCOVERED_COLOR = "Gray";

        public int Id
        {
            get; set;
        }

        public int RowNumber
        {
            get; set;
        }

        public int ColumnNumber
        {
            get; set;
        }

        private string _cellColor = UNCOVERED_COLOR;
        public string CellColor
        {
            get
            {
                return _cellColor;
            }
            set
            {
                SetProperty(ref _cellColor, value);
            }
        }

        public CellViewModel()
        {
            Commands.Add("CellMouseUpCommand", new DelegateCommand(OnCellMouseUp));
            Commands.Add("CellMouseDownCommand", new DelegateCommand(OnCellMouseDown));
            Commands.Add("CellMouseMoveCommand", new DelegateCommand(OnCellMouseMove));
        }
        private void OnCellMouseUp()
        {
        }
        private void OnCellMouseDown()
        {
        }

        private void OnCellMouseMove()
        {
        }

    }
}
