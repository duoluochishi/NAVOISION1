using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NV.CT.Service.UI.Util
{
    public class CustomBoundColumn : DataGridBoundColumn
    {
        public string TemplateName { get; set; }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var content = new ContentControl();
            content.ContentTemplate = (DataTemplate)cell.FindResource(TemplateName);

            if (Binding != null)
            {
                var binding = new Binding(((Binding)Binding).Path.Path);
                binding.Source = dataItem;

                content.SetBinding(ContentControl.ContentProperty, binding);
            }

            return content;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return GenerateElement(cell, dataItem);
        }
    }
}
