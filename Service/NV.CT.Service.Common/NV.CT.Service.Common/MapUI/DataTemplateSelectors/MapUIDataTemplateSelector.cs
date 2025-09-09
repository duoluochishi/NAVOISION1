using System.Windows;
using System.Windows.Controls;
using NV.CT.Service.Common.MapUI.Templates;

namespace NV.CT.Service.Common.MapUI.DataTemplateSelectors
{
    public class MapUIDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SimpleDataTemplate { get; set; } = null!;
        public DataTemplate EnumDataTemplate { get; set; } = null!;
        public DataTemplate CollectionSetTemplate { get; set; } = null!;

        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            if (item is AbstractMapUITemplate)
            {
                var type = item.GetType();
                var definitionType = type.GetGenericTypeDefinition();

                if (definitionType == typeof(SimpleMapUITemplate<,>))
                {
                    return SimpleDataTemplate;
                }

                if (definitionType == typeof(EnumMapUITemplate<,>) || definitionType == typeof(EnumNullableMapUITemplate<,>))
                {
                    return EnumDataTemplate;
                }

                var genericTypeArguments = type.GetGenericArguments();

                if (genericTypeArguments.Length >= 3)
                {
                    var collectionSetType = typeof(AbstractCollectionMapUITemplate<,,>).MakeGenericType(genericTypeArguments[0], genericTypeArguments[1], genericTypeArguments[2]);

                    if (type.IsAssignableTo(collectionSetType))
                    {
                        return CollectionSetTemplate;
                    }
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}