using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NV.CT.Service.Common.Framework
{
    public class MaterialStyleDataGridTextColumn : DataGridTextColumn
    {
        public MaterialStyleDataGridTextColumn() : base()
        {
            ElementStyle = Application.Current.FindResource("MaterialDesignDataGridTextColumnStyle") as Style;
            EditingElementStyle = Application.Current.FindResource("MaterialDesignDataGridTextColumnEditingStyle") as Style;
        }
    }
}
