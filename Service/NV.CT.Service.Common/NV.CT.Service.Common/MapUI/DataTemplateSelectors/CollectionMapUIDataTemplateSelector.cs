using System.Windows;
using System.Windows.Controls;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.MapUI.Templates;

namespace NV.CT.Service.Common.MapUI.DataTemplateSelectors
{
    public class CollectionMapUIDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SimpleCollectionTemplate { get; set; } = null!;
        public DataTemplate EnumCollectionTemplate { get; set; } = null!;

        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            var itemsControl = container.FindParent<ItemsControl>();

            if (itemsControl is { DataContext: AbstractMapUITemplate template })
            {
                var definitionType = template.GetType().GetGenericTypeDefinition();

                if (definitionType == typeof(SimpleCollectionMapUITemplate<,,>))
                {
                    return SimpleCollectionTemplate;
                }

                if (definitionType == typeof(EnumCollectionMapUITemplate<,,>))
                {
                    return EnumCollectionTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}